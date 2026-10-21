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
    }
}
