using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TaxEngine;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Reciprocal wallet swaps with exchange partners.
    ///
    /// The register is the screen that matters. A swap nets to zero against the partner, which is
    /// why it feels like an adjustment and why its turnover went missing — but under GST Section
    /// 7(1)(a) barter is supply and each leg is taxable, so the turnover is the SUM of the legs.
    /// The register shows both numbers side by side so the difference cannot be overlooked.
    ///
    /// InvoiceOutward raises our own side only. The partner's side cannot be generated — their
    /// invoice has to be obtained — so it stays in the awaiting-document queue, where it is
    /// visible rather than forgotten.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ExchangeRegisterController : TaxControllerBase
    {
        private readonly IExchangeLegService _exchange;

        public ExchangeRegisterController(IExchangeLegService exchange, IConfiguration configuration)
            : base(configuration) => _exchange = exchange;

        /// <summary>Partner by partner: outward, inward, net, turnover, and what lacks a document.</summary>
        [HttpPost("Register")]
        public Task<APIResponse> Register([FromForm] APIRequest apiRequest) =>
            Handle<PeriodRequest>(apiRequest, async (request, _) =>
            {
                if (request.PeriodFrom == default || request.PeriodTo == default)
                    return Refuse("A period is required.");
                return Ok(await _exchange.GetRegister(request.PeriodFrom, request.PeriodTo));
            });

        /// <summary>Record a leg for every swap movement in the period. Safe to re-run.</summary>
        [HttpPost("DetectLegs")]
        public Task<APIResponse> DetectLegs([FromForm] APIRequest apiRequest) =>
            Handle<InvoiceGenerationRequest>(apiRequest, async (request, keyData) =>
            {
                if (request.PeriodFrom == default || request.PeriodTo == default)
                    return Refuse("A period is required.");
                request.CreatedByUserID = keyData.UserID;
                return Ok(await _exchange.DetectLegs(request));
            });

        /// <summary>Raise our sale invoice for every outward leg still awaiting one.</summary>
        [HttpPost("InvoiceOutward")]
        public Task<APIResponse> InvoiceOutward([FromForm] APIRequest apiRequest) =>
            Handle<InvoiceGenerationRequest>(apiRequest, async (request, keyData) =>
            {
                if (request.PeriodFrom == default || request.PeriodTo == default)
                    return Refuse("A period is required.");
                if (string.IsNullOrWhiteSpace(request.CompanyGSTIN) || string.IsNullOrWhiteSpace(request.SupplierStateCode))
                    return Refuse("The company GSTIN and state code are required to raise a document.");
                request.CreatedByUserID = keyData.UserID;
                return Ok(await _exchange.InvoiceOutwardLegs(request));
            });
    }
}
