using FintechApp.Responses.Dashboard;

namespace FintechApp.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<(string failureMessage, DashboardViewModel? data)> Select(int userId, string date);
        Task<(string failureMessage, DashboardViewModel? data)> SelectForRetailer(int userId, string date);
        Task<(string failureMessage, DashboardViewModel? data)> SelectForDistributor(int userId, string date);
    }
}
