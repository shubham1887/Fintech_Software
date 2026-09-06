using Fino.Pay1Api.Interfaces;
using Fino.Pay1Api.Repositories;
using Fino.Pay1Api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fino.Pay1Api.Extensions
{
    // One-call DI registration for the Pay1 Utility library:  builder.Services.AddPay1Api();
    public static class Pay1ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddPay1Api(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<IPay1ApiClient, Pay1ApiClient>();
            services.AddScoped<IPay1ApiService, Pay1ApiService>();
            return services;
        }
    }
}
