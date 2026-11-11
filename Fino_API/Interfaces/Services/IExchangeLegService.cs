using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses.TaxInvoice;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Finds both halves of a reciprocal wallet swap and makes sure each one has a document.
    ///
    /// "He puts 50k in my wallet, I put 50k in his." The party balance nets to zero, which is why
    /// it feels like an adjustment — but under GST Section 7(1)(a) barter and exchange ARE supply:
    /// each side is a taxable supply at open market value, and the turnover is the SUM of the two
    /// legs, not the difference. Five partners swapping Rs 50 lakh a month is Rs 3 crore of
    /// turnover that disappears entirely if only the incoming credit was recorded, which is the
    /// single largest suspect behind 18 crore of sales against 19 crore of purchases.
    /// </summary>
    public interface IExchangeLegService
    {
        /// <summary>
        /// Record a leg for every wallet movement between us and a party flagged as an exchange
        /// partner. Safe to re-run: a movement already carrying a leg is skipped, enforced by a
        /// unique index on (FundTransactionID, Direction) rather than by the code remembering.
        /// </summary>
        Task<ExchangeLegDetectionResult> DetectLegs(InvoiceGenerationRequest request);

        /// <summary>
        /// Raise our sale invoice for every outward leg still awaiting one. Inward legs are left
        /// alone on purpose — those are the partner's supplies and their invoice has to be
        /// obtained, not invented.
        /// </summary>
        Task<InvoiceGenerationResult> InvoiceOutwardLegs(InvoiceGenerationRequest request);

        /// <summary>
        /// Partner by partner: what we supplied, what they supplied, the net that makes it look
        /// like nothing happened, the turnover that actually has to be reported, and how many legs
        /// are still missing a document.
        /// </summary>
        Task<ExchangeRegisterResult> GetRegister(DateTime periodFrom, DateTime periodTo);
    }
}
