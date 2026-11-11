using FintechApp.Entities;
using FintechApp.StaticModels;

namespace FintechApp.Interfaces.Repositories
{
    public interface ICashCollectionMasterRepository
    {
        Task<List<CashCollectionMaster>> GetAll();
        Task<CashCollectionMaster?> GetByID(int id);
        Task<List<CashCollectionMaster>> GetByUserID(int userID);
        Task<int> Insert(CashCollectionMaster entity);
        Task<int> Update(CashCollectionMaster entity);
        Task<int> Delete(int id);

        /// <summary>
        /// The caller's own outstanding row, read PAST the list cache.
        ///
        /// <see cref="GetAll"/> and the two helpers built on it serve the whole table from an
        /// in-process cache, which is right for the report screens and wrong for anything about
        /// to make a decision on a balance: the cached copy can be minutes old, and a credit limit
        /// checked against a stale figure is not checked at all.
        /// </summary>
        Task<CashCollectionMaster?> GetLiveByUserID(int userID);

        /// <summary>
        /// Changes a user's outstanding by <paramref name="delta"/> in a single atomic statement,
        /// returning the true opening and closing balance.
        ///
        /// A positive delta is credit extended (the debt grows); a negative delta is a payment
        /// collected (the debt shrinks). Two guards are applied inside the same UPDATE, so they
        /// hold under concurrency rather than merely most of the time:
        ///
        ///   • a credit may not take the balance past <c>MaxOutstanding</c> (when
        ///     <paramref name="enforceLimit"/> is set — the admin's own adjustments do not);
        ///   • a collection may not take the balance below zero.
        ///
        /// Returns <see cref="OutstandingDeltaResult.Applied"/> false with the reason set when a
        /// guard refused it, and never throws for a business refusal.
        /// </summary>
        /// <remarks>
        /// <c>effectiveLimit</c> is the ceiling to enforce for a credit. It is passed IN rather
        /// than read off the row, because the limit in force may be the Key Setting default rather
        /// than this account's own column, and only the service knows which applies. Ignored when
        /// <c>enforceLimit</c> is false.
        /// </remarks>
        Task<OutstandingDeltaResult> TryApplyDelta(int userID, decimal delta, bool enforceLimit, decimal effectiveLimit);

        /// <summary>
        /// The user's outstanding row, created at zero if they do not have one yet.
        ///
        /// Every user needs a row before a balance can be moved, and nothing in the product
        /// creates one at sign-up — so the first credit, the first collection and the first look
        /// at the report would each otherwise fail on a user who simply has no debt yet.
        /// <paramref name="defaultMaxOutstanding"/> is the credit limit a brand-new row starts
        /// with; zero means "no credit until an admin sets a limit", which is the safe default.
        /// </summary>
        Task<CashCollectionMaster?> EnsureRow(int userID, decimal defaultMaxOutstanding = 0m);

        /// <summary>
        /// Sets the credit limit, and ONLY the credit limit.
        ///
        /// Deliberately not done by loading the row and saving it back: that writes every column,
        /// so an admin saving a limit at the same moment a credit is approved would silently undo
        /// the balance change with the figure they had read a second earlier. A targeted UPDATE
        /// cannot — the balance belongs to <see cref="TryApplyDelta"/> and nothing else writes it.
        /// </summary>
        /// <remarks>
        /// <c>hasExplicitLimit</c> is true when a person set this figure, which is what makes a
        /// deliberate 0 ("no credit for this account") different from the create-time 0 ("nobody
        /// has configured it, so the default applies").
        /// </remarks>
        Task<int> SetMaxOutstanding(int userID, decimal maxOutstanding, bool hasExplicitLimit);
    }
}
