using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses.TaxInvoice;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Credit notes — the only correct way to undo an issued document.
    ///
    /// An issued invoice is never edited and never deleted. Its number has gone into GSTR-1, and
    /// changing what that number stands for makes the return and the books disagree from then on;
    /// deleting it leaves a hole in a sequence that GST requires to be consecutive, which is a red
    /// flag in any audit. So a mistake is reversed with a note and, where needed, a fresh invoice.
    /// </summary>
    public interface ICreditNoteService
    {
        /// <summary>
        /// Raise a credit note against an issued document, in full or in part.
        ///
        /// The tax is pro-rated from the ORIGINAL document's own figures, not resolved again from
        /// today's masters. A reversal has to carry the rate that was actually charged — if the
        /// rate changed in between, re-resolving would credit an amount that was never collected.
        /// </summary>
        Task<(GeneratedDocument? Document, string FailureMessage)> Issue(CreditNoteRequest request);

        /// <summary>
        /// Documents whose credit-note window is closing and that cover at least one transaction
        /// which did not succeed — a reversal somebody still has to decide about.
        ///
        /// The window closes on 30 November of the year after the supply's financial year (or the
        /// annual return, whichever is earlier). After it, reversing no longer recovers the tax,
        /// so a dispute settled late costs the full amount. These need chasing while that is still
        /// avoidable.
        /// </summary>
        Task<List<CreditNoteWindowAlert>> GetWindowAlerts(DateTime asOf, int daysAhead);
    }
}
