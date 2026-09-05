using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using Mvc.StorageAccount.Demo.Services;

var builder = WebApplication.CreateBuilder(args);






// Add services to the container.
var storageConnectionString = builder.Configuration["AzureStorage:ConnectionString"];

builder.Services.AddAzureClients(b =>
{
    // Blob service client → used in BlobStorageService
    b.AddBlobServiceClient(storageConnectionString);

    // Queue client → used in QueueService
    b.AddClient<QueueClient, QueueClientOptions>((_, _, _) =>
    {
        return new QueueClient(storageConnectionString,
            builder.Configuration["AzureStorage:QueueName"],
            new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
    });

    // Table client → used in TableStorageService
    b.AddClient<TableClient, TableClientOptions>((_, _, _) =>
    {
        return new TableClient(storageConnectionString,
            builder.Configuration["AzureStorage:TableStorage"]);
    });
});

// Register your custom services
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IQueueService, QueueService>();

builder.Services.AddControllersWithViews();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 200 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
