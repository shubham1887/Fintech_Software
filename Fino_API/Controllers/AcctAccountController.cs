using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>The chart of accounts. Read-only over the API — the chart is seeded and changed in
    /// SQL, not from a screen, so there is no write action here to pin.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class AcctAccountController : TaxControllerBase
    {
        private readonly IChartOfAccountsService _chart;

        public AcctAccountController(IChartOfAccountsService chart, IConfiguration configuration)
            : base(configuration) => _chart = chart;

        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _chart.GetAccounts()));
    }
}
