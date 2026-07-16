using GraphQL.Schema;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddGraphQLServer().AddQueryType<Query>().AddMutationType<Mutation>().AddSubscriptionType<Subscription>();
builder.Services.AddInMemorySubscriptions();
    var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.UseWebSockets();   // required for subscriptions
app.MapGraphQL();   // default path is /graphql

app.Run();
