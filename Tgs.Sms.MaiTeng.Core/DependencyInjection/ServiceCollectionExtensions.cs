using Microsoft.Extensions.Options;
using Tgs.Sms.MaiTeng;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaiTengSms(
        this IServiceCollection services,
        Action<MaiTengSmsOptions, IServiceProvider> configure)
    => services.AddOptions<MaiTengSmsOptions>()
        .Configure(configure)
        .Services
        .AddHttpClient<MaiTengClient>()
        .ConfigureHttpClient((sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<MaiTengSmsOptions>>().Value;

            httpClient.BaseAddress = options.SmsApiUrl;
        })
        .Services;
}
