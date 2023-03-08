using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Tgs.Sms.MaiTeng;
using Tgs.Sms.MaiTeng.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);

builder.Services.AddOpenTelemetryTracing(builder =>
    builder
        .AddAspNetCoreInstrumentation()
        .AddSource("TGS.*")
        .AddSource("Tgs.*")
        .AddProcessor(new SimpleActivityExportProcessor(new PerformanceCounterExporter())));

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(queueConfig =>
    {
        queueConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                config.GetSection("Sms").Bind(options);

                options.OnSetupQueueAndExchange = channel =>
                {
                    _ = channel.QueueDeclare(
                        "tgs.sms.maiteng",
                        true,
                        false,
                        false,
                        null);

                    channel.QueueBind(
                        "tgs.sms.maiteng",
                        "amq.direct",
                        "tgs.sms.maiteng",
                        null);
                };
            })
            .AddHandler<MaiTengReceiveHandler>("tgs.sms.maiteng", false);
    })
    .AddRabbitGlobPatternExchange("*", "amq.direct");

builder.Services
    .AddSingleton<MaiTengSerializer>()
    .AddMaiTengRepositories(
    (sp, dbContextBuilder) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        dbContextBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
    });

builder.Services.AddHealthChecks();

builder.Services.AddMaiTengSms((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    config.GetSection("MaiTengSms").Bind(options);
});

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
});

app.MapHealthChecks("/healthz");

app.Run();