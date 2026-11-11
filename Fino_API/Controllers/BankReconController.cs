using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase5;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Bank statement against the ledger's Bank account, in four buckets.
    ///
    /// Preview changes nothing and is open to the whole accounting desk. Commit, Match and Unmatch
    /// write the pairing — which is what makes a ledger line count as reconciled for every later
    /// statement — so all three are pinned to Admin+Support in ApiAccessPolicy. None of those three
    /// verbs is matched by the WriteVerb regex, so without the pin they would inherit the
    /// controller grant and the accountant would be reconciling the books.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class BankReconController : TaxControllerBase
    {
        private readonly IBankReconService _recon;

        public BankReconController(IBankReconService recon, IConfiguration configuration)
            : base(configuration) => _recon = recon;

        /// <summary>Run the reconciliation and return the four buckets. Writes nothing.</summary>
        [HttpPost("Preview")]
        public Task<APIResponse> Preview([FromForm] APIRequest apiRequest) =>
            Handle<BankReconRequest>(apiRequest, async (request, _) =>
            {
                var (result, failure) = await _recon.Reconcile(request, commit: false);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>Run it and persist the pairings.</summary>
        [HttpPost("Commit")]
        public Task<APIResponse> Commit([FromForm] APIRequest apiRequest) =>
            Handle<BankReconRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _recon.Reconcile(request, commit: true);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>Pair a statement line to a ledger line by hand.</summary>
        [HttpPost("Match")]
        public Task<APIResponse> Match([FromForm] APIRequest apiRequest) =>
            Handle<BankManualMatchRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                string r = await _recon.ManualMatch(request);
                return r.EqualsIgnoreCase(StatusType.Ok) ? Ok(new { Matched = true }) : Refuse(r);
            });

        /// <summary>Break a pairing that was made wrongly.</summary>
        [HttpPost("Unmatch")]
        public Task<APIResponse> Unmatch([FromForm] APIRequest apiRequest) =>
            Handle<BankUnmatchRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                string r = await _recon.Unmatch(request);
                return r.EqualsIgnoreCase(StatusType.Ok) ? Ok(new { Unmatched = true }) : Refuse(r);
            });
    }
}
