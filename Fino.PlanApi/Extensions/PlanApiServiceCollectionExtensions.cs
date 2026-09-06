using Fino.PlanApi.Interfaces;
using Fino.PlanApi.Repositories;
using Fino.PlanApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fino.PlanApi.Extensions
{
    // One-call DI registration for the Plan API library:  builder.Services.AddPlanApi();
    public static class PlanApiServiceCollectionExtensions
    {
        public static IServiceCollection AddPlanApi(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<IPlanApiClient, PlanApiClient>();
            services.AddScoped<IPlanApiService, PlanApiService>();
            return services;
        }
    }
}
