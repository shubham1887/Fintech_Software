using FintechApp.Authorization;
using FintechApp.Interfaces.Repositories;
using FintechApp.Requests;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The effective-dated masters the rules point at: rates, SAC codes and TDS sections.
    ///
    /// Read-only here. These rows are never edited in place — a change closes the current row and
    /// opens a new one, which is what lets a two-year-old transaction still answer with the rate
    /// it was actually charged. Writing them is a separate, deliberate act and does not belong on
    /// the endpoint every screen calls to fill a dropdown.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TaxMasterController : TaxControllerBase
    {
        private readonly ITaxConfigurationRepository _config;

        public TaxMasterController(ITaxConfigurationRepository config, IConfiguration configuration)
            : base(configuration) => _config = config;

        [HttpPost("Rates")]
        public Task<APIResponse> Rates([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _config.GetRates()));

        [HttpPost("HSNSAC")]
        public Task<APIResponse> HSNSAC([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _config.GetHSNSAC()));

        [HttpPost("TDSSections")]
        public Task<APIResponse> TDSSections([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _config.GetTDSSections()));

        /// <summary>Everything a rule editor needs to populate its dropdowns, in one call.</summary>
        [HttpPost("All")]
        public Task<APIResponse> All([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(new
            {
                Rates = await _config.GetRates(),
                HSNSAC = await _config.GetHSNSAC(),
                TDSSections = await _config.GetTDSSections(),
            }));
    }
}
