using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase4;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The daily integrity checks — the trial balance, and the two tie-outs (wallet vs control,
    /// provider stock vs control). All read-only: a check reports, it never corrects. The whole
    /// desk may run them; acting on a discrepancy is a posting, which goes through the ledger's own
    /// Admin-gated acts.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class AcctIntegrityController : TaxControllerBase
    {
        private readonly ILedgerIntegrityService _integrity;

        public AcctIntegrityController(ILedgerIntegrityService integrity, IConfiguration configuration)
            : base(configuration) => _integrity = integrity;

        [HttpPost("TrialBalance")]
        public Task<APIResponse> TrialBalance([FromForm] APIRequest apiRequest) =>
            Handle<LedgerQueryRequest>(apiRequest, async (request, _) => Ok(await _integrity.TrialBalance(request.PeriodKey)));

        [HttpPost("WalletCheck")]
        public Task<APIResponse> WalletCheck([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _integrity.WalletVsControl()));

        [HttpPost("ProviderStockCheck")]
        public Task<APIResponse> ProviderStockCheck([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _integrity.ProviderStockVsControl()));

        [HttpPost("RunAll")]
        public Task<APIResponse> RunAll([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _integrity.RunAll()));
    }
}
