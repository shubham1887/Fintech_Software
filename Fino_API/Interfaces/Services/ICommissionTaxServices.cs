using FintechApp.Entities;
using FintechApp.Requests.CommissionTax;
using FintechApp.Responses.CommissionTax;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// What a commission figure actually means, resolved from the configured profiles.
    ///
    /// Every number in this result was previously a literal somewhere in the code: tax-inclusive
    /// was hardcoded true, the base was inferred from the billing basis alone, and TDS was never
    /// deducted. Nothing here decides policy — it reads what the admin set, most specific first.
    /// </summary>
    public interface ICommissionTaxResolver
    {
        /// <summary>
        /// The treatment for one commission, at one moment. <paramref name="slabMasterId"/> and
        /// <paramref name="providerId"/> may be 0; the most specific row that matches wins, and
        /// the GLOBAL default always matches.
        /// </summary>
        Task<CommissionTaxTreatment> Resolve(
            int serviceId, int operatorId, int providerId, int slabMasterId, DateTime asOf);

        /// <summary>Split a commission figure into its net and tax parts under a treatment.</summary>
        CommissionSplit Split(decimal commission, CommissionTaxTreatment treatment, decimal grossAmount);
    }

    /// <summary>The configured profiles: read them, write them, and try one out.</summary>
    public interface ICommissionTaxProfileService
    {
        Task<List<CommissionTaxProfileRow>> GetAll(string scope);
        Task<(int ProfileID, string Failure)> Save(SaveCommissionTaxProfileRequest request);
        Task<string> Delete(int profileId);

        /// <summary>
        /// Work a figure through the resolved treatment and show every step. The same reason the
        /// tax simulator exists: a non-technical admin has to be able to see WHY a commission
        /// split the way it did before a month of them is booked on it.
        /// </summary>
        Task<(CommissionTaxPreview? Preview, string Failure)> Preview(CommissionTaxPreviewRequest request);
    }

    /// <summary>Deducts TDS on commission, when the profile says to.</summary>
    public interface ITdsDeductionService
    {
        /// <summary>
        /// Work out and record the TDS on each tier's commission for a settled transaction.
        /// Returns what was deducted so the caller can debit it; records nothing and returns zero
        /// when TDS is switched off, which is the default.
        /// </summary>
        Task<TdsDeductionResult> DeductForTransaction(int transactionMasterId);
    }
}
