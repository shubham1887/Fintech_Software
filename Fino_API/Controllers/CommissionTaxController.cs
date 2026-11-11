using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.CommissionTax;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// How a commission is taxed: inclusive or exclusive of GST, charged on the commission or on
    /// the whole amount, and whether TDS is deducted from it.
    ///
    /// These were literals in the code until now — every invoice path passed "tax inclusive =
    /// true", and TDS was computed and stamped but never deducted. A provider paying commission
    /// exclusive of GST and one paying it inclusive produced the identical figure, and the net
    /// profit was out by the tax on every rupee.
    ///
    /// Reading the profiles and trying one out change nothing, so the accounting desk does both.
    /// Save and Delete change how every later commission is split and whether real money is
    /// withheld from real people, so both are Admin — and neither verb would be caught by the
    /// WriteVerb regex on its own.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class CommissionTaxController : TaxControllerBase
    {
        private readonly ICommissionTaxProfileService _profiles;

        public CommissionTaxController(ICommissionTaxProfileService profiles, IConfiguration configuration)
            : base(configuration) => _profiles = profiles;

        /// <summary>The configured profiles, newest-effective first within each scope.</summary>
        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle<CommissionTaxProfileQuery>(apiRequest, async (request, _) =>
                Ok(await _profiles.GetAll(request.Scope)));

        /// <summary>
        /// Work a commission through the resolved treatment and show every step — which profile
        /// won, how the figure split, and what TDS would come off. The same reason the tax
        /// simulator exists: an admin has to see WHY before a month is booked on it.
        /// </summary>
        [HttpPost("Preview")]
        public Task<APIResponse> Preview([FromForm] APIRequest apiRequest) =>
            Handle<CommissionTaxPreviewRequest>(apiRequest, async (request, _) =>
            {
                var (preview, failure) = await _profiles.Preview(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(preview) : Refuse(failure);
            });

        /// <summary>Create or update one profile.</summary>
        [HttpPost("Save")]
        public Task<APIResponse> Save([FromForm] APIRequest apiRequest) =>
            Handle<SaveCommissionTaxProfileRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (id, failure) = await _profiles.Save(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(new { CommissionTaxProfileID = id }) : Refuse(failure);
            });

        /// <summary>Withdraw a profile. The house default cannot be removed.</summary>
        [HttpPost("Delete")]
        public Task<APIResponse> Delete([FromForm] APIRequest apiRequest) =>
            Handle<CommissionTaxProfileIdRequest>(apiRequest, async (request, _) =>
            {
                string r = await _profiles.Delete(request.CommissionTaxProfileID);
                return r.EqualsIgnoreCase(StatusType.Ok) ? Ok(new { Deleted = true }) : Refuse(r);
            });
    }
}
