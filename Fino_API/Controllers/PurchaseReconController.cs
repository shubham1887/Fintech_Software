using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase3;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Purchase ↔ GSTR-2B reconciliation, in four buckets, with the three-way against the provider
    /// wallet. Preview changes nothing and is open to the whole accounting desk. Commit writes the
    /// match back onto the 2B lines and the ITC register, so it is Admin + Support.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PurchaseReconController : TaxControllerBase
    {
        private readonly IPurchaseReconService _recon;

        public PurchaseReconController(IPurchaseReconService recon, IConfiguration configuration)
            : base(configuration) => _recon = recon;

        /// <summary>Run the reconciliation and return the four buckets. Writes nothing.</summary>
        [HttpPost("Preview")]
        public Task<APIResponse> Preview([FromForm] APIRequest apiRequest) =>
            Handle<ReconRequest>(apiRequest, async (request, _) =>
            {
                if (string.IsNullOrWhiteSpace(request.PeriodKey)) return Refuse("A period (yyyyMM) is required.");
                return Ok(await _recon.Reconcile(request, commit: false));
            });

        /// <summary>Run it and persist the match onto the 2B lines and the ITC register.</summary>
        [HttpPost("Commit")]
        public Task<APIResponse> Commit([FromForm] APIRequest apiRequest) =>
            Handle<ReconRequest>(apiRequest, async (request, keyData) =>
            {
                if (string.IsNullOrWhiteSpace(request.PeriodKey)) return Refuse("A period (yyyyMM) is required.");
                request.CreatedByUserID = keyData.UserID;
                return Ok(await _recon.Reconcile(request, commit: true));
            });
    }
}
