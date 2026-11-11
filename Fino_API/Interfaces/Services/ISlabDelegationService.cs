using FintechApp.Requests.SlabDelegation;
using FintechApp.Responses.SlabDelegation;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Hierarchy-based commission slab delegation: a super-distributor or distributor cutting
    /// child rate cards out of the one assigned to them, inside the margin it allows.
    ///
    /// Every method takes the caller's id AND role and re-derives what they may touch from the
    /// database — never from anything the request says. The request only ever names a slab or a
    /// user; whether that slab is the caller's and whether that user is in their network is
    /// answered here.
    /// </summary>
    public interface ISlabDelegationService
    {
        /// <summary>The card assigned to the caller — their ceiling, read-only.</summary>
        Task<(string failureMessage, DelegatedSlabView? data)> MyParentSlab(int callerUserID, int callerTypeID);

        /// <summary>The cards the caller owns.</summary>
        Task<(string failureMessage, List<DelegatedSlabSummary> data)> MySlabs(int callerUserID, int callerTypeID);

        /// <summary>One of the caller's own cards, with every band and its parent ceiling.</summary>
        Task<(string failureMessage, DelegatedSlabView? data)> SlabView(int slabMasterID, int callerUserID, int callerTypeID);

        /// <summary>
        /// Cut a new child card: clones every band of the caller's own card, seeded at the parent's
        /// own figures, so a card created and never edited pays exactly what the parent pays and
        /// changes nothing about the economics until the owner decides something.
        /// </summary>
        Task<(string failureMessage, int slabMasterID)> CreateChildSlab(
            CreateChildSlabRequest request, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>Move the commission figures, or the band itself, on one of the caller's cards.</summary>
        Task<string> UpdateRow(UpdateDelegatedRowRequest request, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>
        /// Add a band to one of the caller's own cards — splitting a parent band into finer ranges
        /// is the reseller's own pricing decision. The new band still has to sit inside a single
        /// parent band and inside what that band pays out.
        /// </summary>
        Task<string> CreateRow(CreateDelegatedRowRequest request, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>
        /// Remove a band from one of the caller's own cards. The amounts it covered fall back to
        /// the parent's card through the engine's existing walk, which pays the parent's own rate —
        /// so removing a band gives margin away rather than taking any.
        /// </summary>
        Task<string> DeleteRow(int slabDetailID, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>
        /// Remove one of the caller's own cards altogether. Refused while anyone is assigned to it,
        /// or while another card has been cut from it — either would move real users onto different
        /// rates without anybody having chosen those rates.
        /// </summary>
        Task<string> DeleteSlab(int slabMasterID, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>Hand one of the caller's cards to one of their downline users.</summary>
        Task<string> AssignSlab(AssignDelegatedSlabRequest request, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>Who in the caller's network holds which of the caller's cards.</summary>
        Task<(string failureMessage, List<DelegatedAssignment> data)> Assignments(int callerUserID, int callerTypeID);

        /// <summary>Take the assignment back. The user falls through to their upline's card again.</summary>
        Task<string> UnassignSlab(int userSlabMappingID, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>
        /// Pull every out-of-limit band on one card back to exactly what the parent now allows, as
        /// one act. The owner's own tier absorbs the change, which is the same place their margin
        /// came from, so the row total lands on the parent's total and the card pays again.
        /// </summary>
        Task<(string failureMessage, int repairedBands)> FixToParent(
            int slabMasterID, int callerUserID, int callerTypeID, string ipAddress);

        /// <summary>
        /// Re-check every card cut from <paramref name="parentSlabMasterID"/> after that card was
        /// edited, marking the bands that no longer fit. Called from the admin slab editor and from
        /// <see cref="UpdateRow"/>, because a distributor's change cascades to its own children the
        /// same way an admin's does. Returns how many bands changed state.
        /// </summary>
        Task<int> RevalidateChildren(int parentSlabMasterID, int byUserID, string ipAddress);

        /// <summary>Rate-change history, scoped to the caller's own branch unless they are admin.</summary>
        Task<(string failureMessage, SlabAuditWrapper data)> Audit(
            SlabAuditRequest request, int callerUserID, int callerTypeID);
    }
}
