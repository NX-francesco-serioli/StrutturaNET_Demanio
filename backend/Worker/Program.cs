using AdSPMdS.DemanioDigitale.Application.Options;
using AdSPMdS.DemanioDigitale.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;

    if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
    {
        logging.AddOtlpExporter();
    }
});

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection("RabbitMq"))
    .ValidateDataAnnotations();

builder.Services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();
    configurator.AddConsumer<UserRegisteredConsumer>();

    configurator.UsingRabbitMq((context, cfg) =>
    {
        var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        cfg.Host(options.Host, options.VirtualHost, h =>
        {
            h.Username(options.Username);
            h.Password(options.Password);
        });

        cfg.ReceiveEndpoint(options.UserRegisteredQueue, endpoint =>
        {
            endpoint.UseMessageRetry(r =>
            {
                // Exponential backoff: up to 3 retries, starting at 1s, capping at 30s
                r.Exponential(retryLimit: 3, minInterval: TimeSpan.FromSeconds(1), maxInterval: TimeSpan.FromSeconds(30), intervalDelta: TimeSpan.FromSeconds(5));
            });

            endpoint.ConfigureConsumer<UserRegisteredConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
