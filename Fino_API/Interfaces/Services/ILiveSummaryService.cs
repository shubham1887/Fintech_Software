using FintechApp.Requests;
using FintechApp.Responses.Live;

namespace FintechApp.Interfaces.Services
{
    public interface ILiveSummaryService
    {
        /// <summary>
        /// The caller's work-queue counts and wallet balances in one answer.
        /// </summary>
        /// <param name="caller">Identity from the validated session — never from the request body.</param>
        /// <param name="knownVersion">
        /// The version the client already holds. When it matches what the server would send, the
        /// result comes back as "not modified" with no payload.
        /// </param>
        Task<LiveSummaryResponse> GetSummary(KeyDataModel caller, string knownVersion);

        /// <summary>
        /// Drop this user's cached summary so the very next poll rebuilds it.
        ///
        /// Called the moment something changes the numbers — money moved, a request was accepted —
        /// which is what makes the strip feel immediate without anybody polling faster.
        /// </summary>
        void Invalidate(int userId);
    }
}
