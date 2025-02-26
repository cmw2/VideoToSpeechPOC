using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        // Register VideoIndexerClient as a singleton
        services.AddSingleton<VideoIndexerClient>();

        return services;
    }
}

