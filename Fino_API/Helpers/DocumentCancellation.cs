using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Cancelling a sale document, done in one place because doing it in two is how the bug
    /// below survived.
    ///
    /// A cancellation is two acts, not one:
    ///
    ///   1. The HEADER is marked Cancelled. Its number is kept — GSTR-1 table 13 declares the
    ///      numbers a series issued, and a number that vanishes makes the sequence impossible to
    ///      demonstrate to an officer.
    ///
    ///   2. Its ANNEXURE LINES are released, by soft-deleting them. Each line claims a source —
    ///      a fund transaction or a transaction master — and both unique indexes on that table
    ///      are filtered <c>WHERE … AND IsDeleted = 0</c>. So while the lines stand, their
    ///      sources are spent, and no replacement document can ever be raised for them.
    ///
    /// Step 2 was missing. The effect was silent and total: the shipped recovery path told the
    /// operator to "cancel the documents with the wrong place of supply, fix the parties, then
    /// generate again", and the generate found every source already invoiced and produced
    /// nothing. It had never been hit on this install because the feature shipped when there
    /// were no documents to cancel.
    ///
    /// The released rows are not destroyed — soft-deleted rows stay readable, so what the
    /// withdrawn number covered is still answerable long after the replacement is issued.
    /// </summary>
    public static class DocumentCancellation
    {
        /// <summary>
        /// Marks the documents cancelled and frees their sources. Does NOT save — the caller owns
        /// the transaction, so the header change and the release cannot be committed separately.
        /// </summary>
        public static async Task Cancel(
            ApplicationDbContext context, IReadOnlyCollection<TaxInvoiceHeader> documents, string reason)
        {
            if (documents.Count == 0) return;

            foreach (var h in documents)
            {
                h.Status = TaxDocumentStatus.Cancelled;
                h.Remark = string.IsNullOrWhiteSpace(h.Remark) ? reason : $"{h.Remark} | {reason}";
                h.UpdatedateTime = DateTime.UtcNow;
            }

            await ReleaseAnnexureLines(context, documents.Select(h => h.TaxInvoiceHeaderID).ToList());
        }

        /// <summary>
        /// Hands every source these documents were holding back to the generators, so a
        /// replacement can be raised for them.
        /// </summary>
        public static async Task ReleaseAnnexureLines(ApplicationDbContext context, List<int> headerIds)
        {
            if (headerIds.Count == 0) return;

            var lines = await context.TaxInvoiceAnnexureLines
                .Where(a => !a.IsDeleted && headerIds.Contains(a.TaxInvoiceHeaderID))
                .ToListAsync();

            foreach (var a in lines)
            {
                a.IsDeleted = true;
                a.UpdatedateTime = DateTime.UtcNow;
            }
        }
    }
}
