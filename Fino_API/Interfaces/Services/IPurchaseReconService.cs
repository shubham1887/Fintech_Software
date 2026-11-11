using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Reconciles captured purchases against the active GSTR-2B snapshot for a period, and lines
    /// both up against what we paid the provider (the three-way). The tax comparison carries a
    /// paisa of tolerance because two suppliers derive the same tax two different ways.
    /// </summary>
    public interface IPurchaseReconService
    {
        /// <summary>Run the reconciliation. When <paramref name="commit"/> is true the match result
        /// is written back onto the 2B lines and the ITC register; otherwise it is a preview that
        /// changes nothing.</summary>
        Task<ReconResult> Reconcile(ReconRequest request, bool commit);
    }
}
