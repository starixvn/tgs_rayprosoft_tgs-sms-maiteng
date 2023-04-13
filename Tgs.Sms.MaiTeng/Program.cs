using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Sms.MaiTeng;
using Tgs.Sms.MaiTeng.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp!;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddOtlpExporter())
    .Services
    .Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/health")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !new[] { "html", "ico", "png", "jpg" }.Any(
                ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

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

        dbContextBuilder.UseNpgsql(config.GetConnectionString("Tgs")!);
    });

builder.Services.AddHealthChecks();

builder.Services.AddMaiTengSms((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    config.GetSection("MaiTengSms").Bind(options);
});

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Hello Sms MaiTeng");

app.MapHealthChecks("/healthz");

app.Run();
