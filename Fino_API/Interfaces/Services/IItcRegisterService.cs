using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The input-tax-credit ledger. Builds an entry per purchase that carries claimable credit,
    /// tracks claim and reversal, and raises the two deadlines that can take a credit away: Rule 37
    /// (pay the supplier within 180 days) and Section 16(4) (claim by 30 November of the next year).
    /// </summary>
    public interface IItcRegisterService
    {
        Task<ItcBuildResult> BuildRegister(ItcBuildRequest request);
        Task<List<ItcRegisterRow>> GetRegister(string periodKey);

        Task<string> MarkSupplierPaid(ItcMarkPaidRequest request);
        Task<(int Claimed, string Failure)> Claim(ItcClaimRequest request);
        Task<string> Reverse(ItcReverseRequest request);

        /// <summary>Claimed-but-unpaid credits whose 180-day Rule 37 date is near or past.</summary>
        Task<List<ItcDeadlineAlert>> GetRule37Alerts(DateTime asOf, int daysAhead);

        /// <summary>Eligible-but-unclaimed credits whose 16(4) window is near or past.</summary>
        Task<List<ItcDeadlineAlert>> GetSection16_4Alerts(DateTime asOf, int daysAhead);
    }
}
