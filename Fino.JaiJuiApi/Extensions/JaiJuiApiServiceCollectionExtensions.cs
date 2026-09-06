using Fino.JaiJuiApi.Interfaces;
using Fino.JaiJuiApi.Repositories;
using Fino.JaiJuiApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fino.JaiJuiApi.Extensions
{
    // One-call DI registration for the JaiJui DMT API library:  builder.Services.AddJaiJuiApi();
    public static class JaiJuiApiServiceCollectionExtensions
    {
        public static IServiceCollection AddJaiJuiApi(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<IJaiJuiApiClient, JaiJuiApiClient>();
            services.AddScoped<IJaiJuiApiService, JaiJuiApiService>();
            return services;
        }
    }
}
