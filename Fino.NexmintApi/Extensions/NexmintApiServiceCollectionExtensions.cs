using Fino.NexmintApi.Interfaces;
using Fino.NexmintApi.Repositories;
using Fino.NexmintApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fino.NexmintApi.Extensions
{
    // One-call DI registration for the Nexmint payout API library:  builder.Services.AddNexmintApi();
    public static class NexmintApiServiceCollectionExtensions
    {
        public static IServiceCollection AddNexmintApi(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<INexmintApiClient, NexmintApiClient>();
            services.AddScoped<INexmintApiService, NexmintApiService>();
            return services;
        }
    }
}
