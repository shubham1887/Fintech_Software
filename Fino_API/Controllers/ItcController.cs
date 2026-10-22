using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase3;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The input-tax-credit register and its two deadline alerts.
    ///
    /// Reads — the register and both alert queries — are open to the whole accounting desk; seeing
    /// what is about to expire is exactly their job. The acts that change what credit we take are
    /// split by weight: building the register and marking a supplier paid are Admin + Support
    /// (operational), while CLAIMING a credit and REVERSING one change declared input tax and are
    /// Admin only — the same line Phase 2 drew for issuing a document.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ItcController : TaxControllerBase
    {
        private readonly IItcRegisterService _itc;

        public ItcController(IItcRegisterService itc, IConfiguration configuration)
            : base(configuration) => _itc = itc;

        [HttpPost("Register")]
        public Task<APIResponse> Register([FromForm] APIRequest apiRequest) =>
            Handle<ItcQueryRequest>(apiRequest, async (request, _) =>
                Ok(await _itc.GetRegister(request.PeriodKey)));

        [HttpPost("Rule37Alerts")]
        public Task<APIResponse> Rule37Alerts([FromForm] APIRequest apiRequest) =>
            Handle<ItcQueryRequest>(apiRequest, async (request, _) =>
                Ok(await _itc.GetRule37Alerts(DateTime.Today, request.DaysAhead)));

        [HttpPost("Section164Alerts")]
        public Task<APIResponse> Section164Alerts([FromForm] APIRequest apiRequest) =>
            Handle<ItcQueryRequest>(apiRequest, async (request, _) =>
                Ok(await _itc.GetSection16_4Alerts(DateTime.Today, request.DaysAhead)));

        /// <summary>Build / rebuild the register for a period from the captured purchases.</summary>
        [HttpPost("Build")]
        public Task<APIResponse> Build([FromForm] APIRequest apiRequest) =>
            Handle<ItcBuildRequest>(apiRequest, async (request, keyData) =>
            {
                if (string.IsNullOrWhiteSpace(request.PeriodKey)) return Refuse("A period (yyyyMM) is required.");
                request.CreatedByUserID = keyData.UserID;
                return Ok(await _itc.BuildRegister(request));
            });

        /// <summary>Record that a supplier has been paid, which stops the Rule 37 clock.</summary>
        [HttpPost("MarkPaid")]
        public Task<APIResponse> MarkPaid([FromForm] APIRequest apiRequest) =>
            Handle<ItcMarkPaidRequest>(apiRequest, async (request, _) =>
            {
                string fail = await _itc.MarkSupplierPaid(request);
                return fail == StaticModels.StatusType.Ok ? Ok(new { Ok = true }) : Refuse(fail);
            });

        /// <summary>Claim eligible, matched credits into a return period. Admin only.</summary>
        [HttpPost("Claim")]
        public Task<APIResponse> Claim([FromForm] APIRequest apiRequest) =>
            Handle<ItcClaimRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (claimed, failure) = await _itc.Claim(request);
                // A partial refusal is not a hard failure: some claimed, some could not. Report both.
                return Ok(new { Claimed = claimed, Message = failure });
            });

        /// <summary>Reverse a claimed credit (Rule 37 / Section 16(4) / Manual). Admin only.</summary>
        [HttpPost("Reverse")]
        public Task<APIResponse> Reverse([FromForm] APIRequest apiRequest) =>
            Handle<ItcReverseRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                string fail = await _itc.Reverse(request);
                return fail == StaticModels.StatusType.Ok ? Ok(new { Ok = true }) : Refuse(fail);
            });
    }
}
