using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase3;
using FintechApp.Requests.TaxEngine;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Purchases — the supplier bills we received, and the credit they carry. A purchase is a
    /// TaxInvoiceHeader with Direction = PURCHASE; the number on it is the supplier's, and the tax
    /// is theirs, recorded as billed rather than re-resolved.
    ///
    /// Capture, Update, Delete and the item list are writes. They are Admin + Support
    /// (operational) rather than Admin-only: recording an inward bill allocates no number of ours
    /// and puts nothing into GSTR-1. Capture is pinned in ApiAccessPolicy; Update / Delete / Save /
    /// Import fall to the same set through the write-verb rule. A bill whose credit has already
    /// been claimed in a return is refused by the service itself, whoever asks.
    ///
    /// Every write refreshes the input-credit register for the month it touched. That used to be
    /// a separate "Build" press on another screen, and a bill nobody built showed no credit.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PurchaseController : TaxControllerBase
    {
        private readonly IPurchaseCaptureService _purchases;
        private readonly IItcRegisterService _itc;

        public PurchaseController(IPurchaseCaptureService purchases, IItcRegisterService itc, IConfiguration configuration)
            : base(configuration)
        {
            _purchases = purchases;
            _itc = itc;
        }

        /// <summary>Purchase documents in a period, with where each one's credit stands.</summary>
        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle<PeriodRequest>(apiRequest, async (request, _) =>
            {
                if (request.PeriodFrom == default || request.PeriodTo == default)
                    return Refuse("A period is required.");
                return Ok(await _purchases.GetPurchaseRows(request.PeriodFrom, request.PeriodTo));
            });

        /// <summary>Providers, wallets, billable items and known suppliers for the entry form.</summary>
        [HttpPost("Lookups")]
        public Task<APIResponse> Lookups([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _purchases.GetLookups()));

        /// <summary>One bill with its lines.</summary>
        [HttpPost("GetById")]
        public Task<APIResponse> GetById([FromForm] APIRequest apiRequest) =>
            Handle<PurchaseIdRequest>(apiRequest, async (request, _) =>
            {
                var doc = await _purchases.GetPurchase(request.TaxInvoiceHeaderID);
                return doc == null ? Refuse("That purchase invoice was not found.") : Ok(doc);
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
                await RefreshCredit(Phase3Dates.PeriodKey(result.DocumentDate), keyData.UserID);
                return Ok(result);
            });

        /// <summary>Correct a captured bill. Refused once its credit has been claimed.</summary>
        [HttpPost("Update")]
        public Task<APIResponse> Update([FromForm] APIRequest apiRequest) =>
            Handle<PurchaseCaptureRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _purchases.UpdatePurchase(request);
                if (result == null) return Refuse(failure);
                await RefreshCredit(Phase3Dates.PeriodKey(result.DocumentDate), keyData.UserID);
                return Ok(result);
            });

        /// <summary>Remove a captured bill. Refused once its credit has been claimed.</summary>
        [HttpPost("Delete")]
        public Task<APIResponse> Delete([FromForm] APIRequest apiRequest) =>
            Handle<PurchaseIdRequest>(apiRequest, async (request, _) =>
            {
                string fail = await _purchases.DeletePurchase(request.TaxInvoiceHeaderID);
                return fail == StatusType.Ok ? Ok(new { Ok = true }) : Refuse(fail);
            });

        /// <summary>Record a supplier invoice from its e-invoice JSON (NIC/IRP schema).</summary>
        [HttpPost("ImportEInvoice")]
        public Task<APIResponse> ImportEInvoice([FromForm] APIRequest apiRequest) =>
            Handle<EInvoiceImportRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _purchases.ImportEInvoice(request);
                if (result == null) return Refuse(failure);
                await RefreshCredit(Phase3Dates.PeriodKey(result.DocumentDate), keyData.UserID);
                return Ok(result);
            });

        /// <summary>Add or change an item on the Description list.</summary>
        [HttpPost("SaveItem")]
        public Task<APIResponse> SaveItem([FromForm] APIRequest apiRequest) =>
            Handle<PurchaseItemSaveRequest>(apiRequest, async (request, _) =>
            {
                var (item, failure) = await _purchases.SaveItem(request);
                return item == null ? Refuse(failure) : Ok(item);
            });

        /// <summary>Take an item off the Description list. Bills already captured keep their own copy.</summary>
        [HttpPost("DeleteItem")]
        public Task<APIResponse> DeleteItem([FromForm] APIRequest apiRequest) =>
            Handle<PurchaseItemIdRequest>(apiRequest, async (request, _) =>
            {
                string fail = await _purchases.DeleteItem(request.PurchaseItemMasterID);
                return fail == StatusType.Ok ? Ok(new { Ok = true }) : Refuse(fail);
            });

        /// <summary>
        /// Bring the input-credit register up to date for one month. Never fails the write that
        /// called it: the bill is already saved, and the register is rebuilt again by the returns
        /// screen, so a hiccup here costs nothing but a moment of staleness.
        /// </summary>
        private async Task RefreshCredit(string periodKey, int userId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(periodKey))
                    await _itc.BuildRegister(new ItcBuildRequest { PeriodKey = periodKey, CreatedByUserID = userId });
            }
            catch (Exception ex) { Log.Write(ex); }
        }
    }
}
