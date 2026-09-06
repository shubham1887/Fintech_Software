using FintechApp.Responses.TodaySummary;

namespace FintechApp.Interfaces.Services
{
    public interface ITodaySummaryService
    {
        Task<(string failureMessage, List<TodaySummaryViewModel> data)> Select(int userId, string date);
        Task<(string failureMessage, List<TodaySummaryViewModel> data)> SelectForRetailer(int userId, string date);
        Task<(string failureMessage, List<TodaySummaryViewModel> data)> SelectForDistributor(int userId, string date);
    }
}
