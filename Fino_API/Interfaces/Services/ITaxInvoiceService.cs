using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses.TaxInvoice;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Generates sale documents from what already happened, rather than from anybody typing.
    ///
    /// Both methods are safe to re-run: a source already covered by an annexure row cannot be
    /// picked up again, enforced by a unique index rather than by the code remembering. A batch
    /// that stops halfway is simply run again.
    /// </summary>
    public interface ITaxInvoiceService
    {
        /// <summary>
        /// Wallet top-ups on gross-basis wallets become tax invoices for the full amount —
        /// "jitna wallet load, utna bill".
        ///
        /// Registered parties get one invoice per top-up. Unregistered parties get one
        /// consolidated invoice per day, which is what GST allows for B2C and what keeps a
        /// counter with forty top-ups a day from generating forty documents.
        /// </summary>
        Task<InvoiceGenerationResult> GenerateWalletTopUpInvoices(InvoiceGenerationRequest request);

        /// <summary>
        /// Commission and charge earned on transactions becomes one consolidated invoice per party
        /// for the period, with the underlying transactions in the annexure.
        /// </summary>
        Task<InvoiceGenerationResult> GenerateCommissionInvoices(InvoiceGenerationRequest request);

        /// <summary>
        /// Raise our sale invoice for ONE outward exchange leg.
        ///
        /// Here rather than in the exchange service so that every document this system issues is
        /// written by the same code: one place that allocates a number, builds the header, the
        /// line and the annexure, and links the source. A second writer would drift from this one,
        /// and a document that drifts is a document that does not reconcile.
        /// </summary>
        Task<(GeneratedDocument? Document, string FailureMessage)> IssueExchangeLegInvoice(
            int exchangePartnerLegID, InvoiceGenerationRequest request);

        /// <summary>
        /// Raise a self-invoice for a supply we owe the tax on under reverse charge.
        ///
        /// The supply is somebody else's — typically an unregistered supplier, who cannot issue a
        /// tax invoice — but the liability is ours, and GST requires us to document it ourselves.
        /// `ServiceMaster.HSNCodeForSelfInvoice` exists for exactly this, and it takes its own
        /// series so the sequence stays separate from ordinary sales.
        ///
        /// Which inward supplies attract reverse charge is a question for the CA (blueprint #4);
        /// this issues the document, it does not decide that the document is due.
        /// </summary>
        Task<(GeneratedDocument? Document, string FailureMessage)> IssueSelfInvoice(
            SelfInvoiceRequest request);
    }
}
