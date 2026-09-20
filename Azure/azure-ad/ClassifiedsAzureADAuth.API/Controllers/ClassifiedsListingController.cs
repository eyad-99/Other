using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace ClassifiedsAzureADAuth.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClassifiedsListingController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Shoes", "Cars", "Jackets", "Houses", "Apartments", "Gear", "Sports", "Technology", "Phones", "Laptops"
        };

        private readonly ILogger<ClassifiedsListingController> _logger;
        private readonly IHttpClientFactory httpClientFactory;

        public ClassifiedsListingController(ILogger<ClassifiedsListingController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Authorize(Policy = "AllAccess")]

       public async Task<IEnumerable<ClassifiedsListing>> GetAsync()
        {
            var currentToken = await HttpContext.GetTokenAsync("access_token");
            var httpClient = httpClientFactory.CreateClient();
            var tokenEndpointResponse = await httpClient.RequestTokenAsync(
                new TokenRequest 
                { 
                    Address = "https://login.microsoftonline.com/70c07c26-601e-415b-9a91-c351a5ad357b/oauth2/v2.0/token",
                    GrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ClientId = "bc33e163-b67b-470b-8ebc-a40c86e3e3db",
                    ClientSecret = "AxC8Q~dSza0SbaiJKGxHaypWJXj_-HG.6CaaOdai",
                    Parameters =
                    {
                        {"assertion",currentToken},
                        {"scope","api://d52c2b15-dbf6-4664-8b84-482bb5aa5777/.default"},
                        {"requested_token_use","on_behalf_of"},
                    }
                });

            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44319/weatherforecast");
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, tokenEndpointResponse.AccessToken);

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(response.StatusCode.ToString());
            }

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new ClassifiedsListing
            {
                Date = DateTime.Now.AddDays(index),
                Sold = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
