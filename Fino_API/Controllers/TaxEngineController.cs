using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TaxEngine;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The rule simulator.
    ///
    /// One endpoint, and it is the most important one in the subsystem for anybody who is not a
    /// developer. Put a service, a party, an amount and a date in; get back what the engine
    /// decided, which rule won, and every rule that matched with the reason it ranks where it
    /// does. Without it, configuring a tax rule is an act of faith — you change a row, you cannot
    /// see what it did, and you find out at the end of the month.
    ///
    /// Read-only. It resolves and reports; it writes nothing and stamps nothing.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TaxEngineController : TaxControllerBase
    {
        private readonly ITaxEngineService _engine;

        public TaxEngineController(ITaxEngineService engine, IConfiguration configuration)
            : base(configuration) => _engine = engine;

        /// <summary>Resolve a hypothetical transaction and explain the answer.</summary>
        [HttpPost("Simulate")]
        public Task<APIResponse> Simulate([FromForm] APIRequest apiRequest) =>
            Handle<TaxResolutionContext>(apiRequest, async (request, _) =>
            {
                if (string.IsNullOrWhiteSpace(request.Direction))
                    return Refuse("Direction is required: SALE or PURCHASE.");
                if (request.TransactionDate == default)
                    return Refuse("A date is required - rules and rates are resolved as of the transaction's own date.");

                var result = await _engine.Resolve(request);
                return Ok(result);
            });
    }
}
