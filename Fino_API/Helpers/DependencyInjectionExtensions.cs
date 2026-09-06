using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Services;
using System.Reflection;

namespace FintechApp.Helpers
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Automatically registers all services and repositories
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register all repositories
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Register all services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }

        /// <summary>
        /// Automatically registers all services and repositories by scanning assemblies
        /// This is a more dynamic approach
        /// </summary>
        public static IServiceCollection AddDynamicDependencyInjection(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Get all interfaces from Interfaces.Services
            var serviceInterfaces = assembly.GetTypes()
                .Where(t => t.IsInterface && t.Namespace == "FintechApp.Interfaces.Services")
                .ToList();

            // Get all service implementations
            var serviceImplementations = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "FintechApp.Services")
                .ToList();

            // Register services
            foreach (var serviceInterface in serviceInterfaces)
            {
                var implementation = serviceImplementations
                    .FirstOrDefault(t => serviceInterface.IsAssignableFrom(t));

                if (implementation != null)
                {
                    services.AddScoped(serviceInterface, implementation);
                }
            }

            // Get all interfaces from Interfaces.Repositories
            var repositoryInterfaces = assembly.GetTypes()
                .Where(t => t.IsInterface && t.Namespace == "FintechApp.Interfaces.Repositories")
                .ToList();

            // Get all repository implementations
            var repositoryImplementations = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "FintechApp.Repositories")
                .ToList();

            // Register repositories
            foreach (var repositoryInterface in repositoryInterfaces)
            {
                var implementation = repositoryImplementations
                    .FirstOrDefault(t => repositoryInterface.IsAssignableFrom(t));

                if (implementation != null)
                {
                    services.AddScoped(repositoryInterface, implementation);
                }
            }

            return services;
        }
    }
}
