using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.PartyState;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The place-of-supply review: which parties have one, where it came from, and which still
    /// need a human.
    ///
    /// This exists because a missing place of supply is not a cosmetic gap. It decides whether a
    /// supply is intra-state (CGST + SGST) or inter-state (IGST), and the portal rejects a return
    /// carrying an invalid one — so a party with no state silently puts the tax in the wrong heads
    /// and blocks the filing.
    ///
    /// Reading the review is the accounting desk's job. Backfill and Set WRITE the party's tax
    /// identity, which every later document is stamped from, so both are pinned to Admin below —
    /// neither verb is caught by the WriteVerb regex.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PartyStateController : TaxControllerBase
    {
        private readonly IPartyStateService _parties;

        public PartyStateController(IPartyStateService parties, IConfiguration configuration)
            : base(configuration) => _parties = parties;

        /// <summary>Every party, with its resolved place of supply and the source it came from.</summary>
        [HttpPost("Review")]
        public Task<APIResponse> Review([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _parties.Review()));

        /// <summary>Write the derived code onto every party that can have one. DryRun reports only.</summary>
        [HttpPost("Backfill")]
        public Task<APIResponse> Backfill([FromForm] APIRequest apiRequest) =>
            Handle<PartyStateBackfillRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _parties.Backfill(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>
        /// Cancel the documents stamped with an invalid place of supply so they can be made again.
        /// Refused outright once GSTR-1 for the period has been filed — that needs a credit note.
        /// </summary>
        [HttpPost("CancelForRegeneration")]
        public Task<APIResponse> CancelForRegeneration([FromForm] APIRequest apiRequest) =>
            Handle<DocumentRegenerationRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _parties.CancelForRegeneration(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>Set one party's place of supply by hand.</summary>
        [HttpPost("SetPlaceOfSupply")]
        public Task<APIResponse> SetPlaceOfSupply([FromForm] APIRequest apiRequest) =>
            Handle<SetPartyStateRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                string r = await _parties.SetPlaceOfSupply(request);
                return r.EqualsIgnoreCase(StatusType.Ok) ? Ok(new { Saved = true }) : Refuse(r);
            });
    }
}
