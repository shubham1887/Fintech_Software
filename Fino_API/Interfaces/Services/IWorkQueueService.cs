using FintechApp.Requests;
using FintechApp.Responses.WorkQueue;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// "What is waiting for me?" — the counts behind the strip that sits on every screen.
    /// One call, role-scoped on the server, so the client never has to know which queues a
    /// role owns.
    /// </summary>
    public interface IWorkQueueService
    {
        Task<WorkQueueResponse> GetCounts(KeyDataModel caller);
    }
}
