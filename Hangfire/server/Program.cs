using Hangfire;
using Hangfire.SqlServer;
using server.jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        "Data Source=localhost\\SQLEXPRESS;" +
        "Initial Catalog=HangfireDemo;" +
        "Integrated Security=True;" +
        "TrustServerCertificate=True;"));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<DemoJobs>();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire");


// 1. FIRE AND FORGET
app.MapPost("/fire-and-forget", (IBackgroundJobClient jobs) =>
{
    jobs.Enqueue<DemoJobs>(x => x.SendEmail());

    return Results.Ok("Job queued");
});


// 2. DELAYED
app.MapPost("/delayed", (IBackgroundJobClient jobs) =>
{
    jobs.Schedule<DemoJobs>(
        x => x.SendReminder(),
        TimeSpan.FromSeconds(30));

    return Results.Ok("Reminder scheduled for 30 seconds");
});


// 3. RECURRING
RecurringJob.AddOrUpdate<DemoJobs>(
    "cleanup-job",
    x => x.Cleanup(),
    Cron.Minutely);


// 4. CONTINUATION
app.MapPost("/pipeline", (IBackgroundJobClient jobs) =>
{
    var firstJob = jobs.Enqueue<DemoJobs>(
        x => x.ProcessFile());

    var secondJob = jobs.ContinueJobWith<DemoJobs>(
        firstJob,
        x => x.ResizeFile());

    jobs.ContinueJobWith<DemoJobs>(
        secondJob,
        x => x.OptimizeFile());

    return Results.Ok("Pipeline created");
});

app.Run();