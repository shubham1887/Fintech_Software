using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase3;
using FintechApp.Requests.TaxEngine;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Purchases — the supplier bills we received, and the credit they carry. A purchase is a
    /// TaxInvoiceHeader with Direction = PURCHASE; the number on it is the supplier's, and the tax
    /// is theirs, recorded as billed rather than re-resolved.
    ///
    /// Capture and Import are writes. They are Admin + Support (operational, idempotent on the
    /// supplier's number) rather than Admin-only: recording an inward bill allocates no number of
    /// ours and puts nothing into GSTR-1. Claiming the credit it carries is the guarded act, and
    /// that lives on the ITC controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PurchaseController : TaxControllerBase
    {
        private readonly IPurchaseCaptureService _purchases;

        public PurchaseController(IPurchaseCaptureService purchases, IConfiguration configuration)
            : base(configuration) => _purchases = purchases;

        /// <summary>Purchase documents in a period — the purchase register.</summary>
        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle<PeriodRequest>(apiRequest, async (request, _) =>
            {
                if (request.PeriodFrom == default || request.PeriodTo == default)
                    return Refuse("A period is required.");
                return Ok(await _purchases.GetPurchases(request.PeriodFrom, request.PeriodTo));
            });

        /// <summary>Record a supplier invoice from entered figures. Idempotent on (supplier, number).</summary>
        [HttpPost("Capture")]
        public Task<APIResponse> Capture([FromForm] APIRequest apiRequest) =>
            Handle<PurchaseCaptureRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var result = await _purchases.CapturePurchase(request);
                // A pure validation failure comes back with no id and a warning; surface it as a refusal.
                if (result.TaxInvoiceHeaderID == 0 && !result.AlreadyExisted)
                    return Refuse(string.IsNullOrWhiteSpace(result.Warning) ? "The purchase could not be captured." : result.Warning);
                return Ok(result);
            });

        /// <summary>Record a supplier invoice from its e-invoice JSON (NIC/IRP schema).</summary>
        [HttpPost("ImportEInvoice")]
        public Task<APIResponse> ImportEInvoice([FromForm] APIRequest apiRequest) =>
            Handle<EInvoiceImportRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _purchases.ImportEInvoice(request);
                return result == null ? Refuse(failure) : Ok(result);
            });
    }
}
