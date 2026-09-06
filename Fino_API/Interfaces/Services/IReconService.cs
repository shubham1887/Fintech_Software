using FintechApp.Requests.Reconciliation;

namespace FintechApp.Interfaces.Services
{
    // Reconciliation engine: parses a provider file, matches each row against TransactionMaster by
    // ProviderRequestID, and settles (SUCCESS → finalise, FAILED → refund) via AccountProcessing.
    public interface IReconService
    {
        Task<(string failureMessage, int successfulUpdates, int totalRecords)> Upload(ReconUploadRequest request, int currentUserID);
    }
}
