using FintechApp.Entities;
using FintechApp.Requests.PartyState;
using FintechApp.Responses.PartyState;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Works out a party's place of supply from whatever the system already knows, in a fixed
    /// order of trust: the tax profile, then the user's own GST state field, then the GSTIN's
    /// first two digits, then the state onboarding picked from the dropdown.
    ///
    /// It exists because the alternative — asking somebody to type a state for every party — is a
    /// per-client manual step, and on an install sold repeatedly that is a defect rather than a
    /// task. Most parties already have the answer recorded somewhere; it was simply never
    /// translated into the two-digit code GST uses.
    ///
    /// It returns EMPTY when nothing is known, and never the string "00". That distinction is the
    /// whole point: "00" looks like a state, is never equal to the supplier's, and therefore sends
    /// every supply to IGST and is rejected by the portal, while empty lets the tax engine say so
    /// and fall back to intra-state.
    /// </summary>
    public interface IPartyStateResolver
    {
        Task<string> Resolve(int userId, PartyTaxProfile? profile, string gstin, string? businessGstStateCode);

        /// <summary>StateMasters.StateID → GST code, joined on the state's name.</summary>
        Task<Dictionary<int, string>> StateIdMap();
    }

    /// <summary>The place-of-supply review: who has one, where it came from, and who still needs it.</summary>
    public interface IPartyStateService
    {
        Task<PartyStateReport> Review();

        /// <summary>
        /// Write a tax profile for every party whose place of supply can be DERIVED but is not yet
        /// recorded. Idempotent, and it never overwrites a value somebody set by hand.
        /// </summary>
        Task<(PartyStateBackfillResult? Result, string Failure)> Backfill(PartyStateBackfillRequest request);

        /// <summary>Set one party's place of supply by hand, for the ones nothing can derive.</summary>
        Task<string> SetPlaceOfSupply(SetPartyStateRequest request);

        /// <summary>
        /// Cancel the documents stamped with an invalid place of supply so they can be generated
        /// again, now that the parties are fixed.
        ///
        /// CANCEL, never delete: GSTR-1 table 13 declares the numbers a series issued, and a
        /// number that simply vanishes makes "consecutive" impossible to demonstrate for the whole
        /// series. A cancelled document keeps its number and is reported as cancelled.
        ///
        /// It refuses outright for a period whose GSTR-1 has been FILED, or one the ledger has
        /// sealed. A filed document is a declaration already made, and the remedy for that is a
        /// credit note and a fresh invoice — not quietly withdrawing what was declared.
        /// </summary>
        Task<(DocumentRegenerationResult? Result, string Failure)> CancelForRegeneration(
            DocumentRegenerationRequest request);
    }
}
