using FintechApp.Responses.TaxInvoice;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The e-invoice (IRN) queue.
    ///
    /// A queue, not an inline call, for two reasons. The IRP goes down, and an invoice must not
    /// fail to exist because a government portal was unavailable — the document is issued locally
    /// and registered afterwards. And the deadline is worth watching: above Rs 10 crore AATO a
    /// document has to reach the IRP within 30 days of its date or the portal will not issue an
    /// IRN at all, after which the only route left is a credit note and a fresh invoice. A
    /// document still Pending as that date approaches needs an alert while something can still be
    /// done about it.
    /// </summary>
    public interface IEInvoiceService
    {
        /// <summary>
        /// Queue a document for registration. Idempotent: a document already queued is left alone,
        /// so the issuing path can call this without checking first.
        /// </summary>
        Task<(bool Queued, string Message)> Enqueue(int taxInvoiceHeaderID);

        /// <summary>Queue every issued sale document in a period that has no log row yet.</summary>
        Task<int> EnqueuePeriod(DateTime periodFrom, DateTime periodTo);

        /// <summary>
        /// Attempt the pending registrations, oldest deadline first. Returns what happened.
        /// Safe to run on a schedule and safe to run twice.
        /// </summary>
        Task<EInvoiceProcessResult> ProcessQueue(int maxDocuments);

        /// <summary>
        /// Documents still Pending whose 30-day reporting deadline falls within
        /// <paramref name="daysAhead"/>, or has already passed. Soonest first.
        /// </summary>
        Task<List<EInvoiceDeadlineAlert>> GetDeadlineAlerts(DateTime asOf, int daysAhead);
    }
}
