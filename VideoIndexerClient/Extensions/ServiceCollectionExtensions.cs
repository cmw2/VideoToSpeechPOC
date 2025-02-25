using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoIndexer.Auth;
using VideoIndexer.Options;

namespace VideoIndexer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureVideoIndexer(this IServiceCollection services, IConfiguration namedConfigurationSection)
    {
        services.Configure<AzureVideoIndexerOptions>(namedConfigurationSection);

        services.AddHttpClient<AccountTokenProvider>();

        // Register HttpClient with custom handler
        services.AddHttpClient<VideoIndexerClient>(client =>
        {
            // Configure the HttpClient if needed
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
        });

        return services;
    }
}

