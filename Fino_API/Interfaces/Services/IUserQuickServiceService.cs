using FintechApp.Requests.QuickService;
using FintechApp.Responses.QuickService;

namespace FintechApp.Interfaces.Services
{
    public interface IUserQuickServiceService
    {
        /// <summary>
        /// The caller's own shortcut tiles, already resolved against the catalogue they are
        /// allowed to see — a pinned service that was later withdrawn from them simply drops out.
        /// </summary>
        Task<(string failureMessage, List<QuickServiceViewModel> data)> Select(int userId, int userTypeId);

        /// <summary>Replace the caller's shortcuts with the given services, in the given order.</summary>
        Task<string> Save(int userId, int userTypeId, SaveQuickServiceRequest request);
    }
}
