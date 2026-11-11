using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Tds;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// How much TDS comes off a commission, and whose.
    ///
    /// TDS used to be one switch on the commission tax profile: on or off per service, provider or
    /// slab, at the section's statutory rate. The business does not work that way — "0.01% on this
    /// service for every retailer, except this distributor who is on a flat ₹2" is an ordinary
    /// instruction, and there was nowhere to write it down.
    ///
    /// Six scopes, matched most specific first: one user on one service, that user everywhere, a
    /// role on one service, that role everywhere, a service for everyone, the house default.
    /// Percentage or flat rupees, on the commission net of GST or as quoted, with a floor and a cap.
    ///
    /// Both the admin and the accountant set these — the brief says so, and it is the accounting
    /// desk that knows what the rate should be. Nothing here deducts anything on its own: the
    /// Key Setting master switch has to be on, and it is off until somebody turns it on.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TdsSettingController : TaxControllerBase
    {
        private readonly ITdsAdminService _tds;

        public TdsSettingController(ITdsAdminService tds, IConfiguration configuration)
            : base(configuration) => _tds = tds;

        /// <summary>Every rule, most specific first, with the master switch's state.</summary>
        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _tds.GetRules()));

        /// <summary>Create or update one rule.</summary>
        [HttpPost("Save")]
        public Task<APIResponse> Save([FromForm] APIRequest apiRequest) =>
            Handle<SaveTdsRuleRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (id, failure) = await _tds.SaveRule(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(new { TdsRuleID = id }) : Refuse(failure);
            });

        /// <summary>Withdraw a rule. Deductions already taken under it keep pointing at it.</summary>
        [HttpPost("Delete")]
        public Task<APIResponse> Delete([FromForm] APIRequest apiRequest) =>
            Handle<TdsRuleIdRequest>(apiRequest, async (request, _) =>
            {
                string r = await _tds.DeleteRule(request.TdsRuleID);
                return r == StaticModels.StatusType.Ok ? Ok(new { Deleted = true }) : Refuse(r);
            });

        /// <summary>
        /// Try a commission through whichever rule wins, and see every step. Changes nothing — and
        /// it is the only way to catch a rate typed as 0.01 that was meant as 0.01% before a month
        /// of deductions is booked on it.
        /// </summary>
        [HttpPost("Preview")]
        public Task<APIResponse> Preview([FromForm] APIRequest apiRequest) =>
            Handle<TdsPreviewRequest>(apiRequest, async (request, _) =>
            {
                var (preview, failure) = await _tds.Preview(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(preview) : Refuse(failure);
            });
    }
}
