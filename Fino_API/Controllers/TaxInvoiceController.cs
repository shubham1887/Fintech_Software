using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Jobs;
using FintechApp.Requests;
using FintechApp.Requests.TaxEngine;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Sale documents: listing them, and generating them from what already happened.
    ///
    /// Both generators accept DryRun, and the first run over a real period should use it. That run
    /// is the moment somebody finds out how much was never invoiced, and the number should be
    /// readable before a few thousand documents are created from it.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TaxInvoiceController : TaxControllerBase
    {
        private readonly ITaxInvoiceService _invoices;
        private readonly MonthlyInvoiceJob _monthly;

        public TaxInvoiceController(ITaxInvoiceService invoices, MonthlyInvoiceJob monthly, IConfiguration configuration)
            : base(configuration)
        {
            _invoices = invoices;
            _monthly = monthly;
        }

        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle<InvoiceListRequest>(apiRequest, async (request, _) =>
                Ok(await _invoices.GetDocuments(request.PeriodFrom, request.PeriodTo,
                                                request.Direction, request.DocumentType, request.PartyUserID)));

        /// <summary>One document with its lines, annexure, IRN status and any credit notes.</summary>
        [HttpPost("GetById")]
        public Task<APIResponse> GetById([FromForm] APIRequest apiRequest) =>
            Handle<DocumentIdRequest>(apiRequest, async (request, _) =>
            {
                var doc = await _invoices.GetDocument(request.TaxInvoiceHeaderID);
                return doc == null ? Refuse($"Document {request.TaxInvoiceHeaderID} not found.") : Ok(doc);
            });

        /// <summary>Wallet top-ups on gross-basis wallets become invoices for the full amount.</summary>
        [HttpPost("GenerateTopUps")]
        public Task<APIResponse> GenerateTopUps([FromForm] APIRequest apiRequest) =>
            Handle<InvoiceGenerationRequest>(apiRequest, async (request, keyData) =>
            {
                string invalid = ValidatePeriod(request);
                if (invalid.Length > 0) return Refuse(invalid);
                request.CreatedByUserID = keyData.UserID;
                return Ok(await _invoices.GenerateWalletTopUpInvoices(request));
            });

        /// <summary>Commission and charge become one consolidated document per party per service.</summary>
        [HttpPost("GenerateCommission")]
        public Task<APIResponse> GenerateCommission([FromForm] APIRequest apiRequest) =>
            Handle<InvoiceGenerationRequest>(apiRequest, async (request, keyData) =>
            {
                string invalid = ValidatePeriod(request);
                if (invalid.Length > 0) return Refuse(invalid);
                request.CreatedByUserID = keyData.UserID;
                return Ok(await _invoices.GenerateCommissionInvoices(request));
            });

        /// <summary>
        /// Issue (or preview) a whole month's sale documents in one act — commission and charge
        /// plus the gross-basis wallet top-ups.
        ///
        /// The same method the month-end schedule runs, deliberately: the refusal when a party has
        /// no place of supply has to behave identically whether a person or a cron started it, and
        /// two copies of that rule would eventually disagree. Pass DryRun to see the figures and
        /// the blockers without writing anything.
        /// </summary>
        [HttpPost("GenerateMonth")]
        public Task<APIResponse> GenerateMonth([FromForm] APIRequest apiRequest) =>
            Handle<MonthlyInvoiceRequest>(apiRequest, async (request, keyData) =>
                Ok(await _monthly.Generate(request.PeriodKey, request.DryRun, keyData.UserID,
                                           request.Consolidation, request.SaveAsDefault)));

        /// <summary>
        /// Re-issue a month under a different grouping: cancel its sale documents, generate again.
        ///
        /// The sharpest action on this controller — it withdraws numbers a customer may already
        /// hold — so it refuses a filed period outright and supports DryRun, which reports how
        /// many documents would be cancelled before any of them are.
        /// </summary>
        [HttpPost("RegroupMonth")]
        public Task<APIResponse> RegroupMonth([FromForm] APIRequest apiRequest) =>
            Handle<MonthlyInvoiceRequest>(apiRequest, async (request, keyData) =>
                Ok(await _monthly.Regroup(request.PeriodKey, request.Consolidation, request.DryRun,
                                          keyData.UserID, request.SaveAsDefault)));

        /// <summary>Reverse charge: we owe the tax on somebody else's supply, so we document it.</summary>
        /// <summary>Withdraw an issued sale document (no IRN, no credit note). Admin only — see ApiAccessPolicy.</summary>
        [HttpPost("Cancel")]
        public Task<APIResponse> Cancel([FromForm] APIRequest apiRequest) =>
            Handle<CancelDocumentRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (ok, message) = await _invoices.CancelDocument(request);
                return ok ? Ok(new { Cancelled = true, Message = message }) : Refuse(message);
            });

        [HttpPost("IssueSelfInvoice")]
        public Task<APIResponse> IssueSelfInvoice([FromForm] APIRequest apiRequest) =>
            Handle<SelfInvoiceRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (doc, failure) = await _invoices.IssueSelfInvoice(request);
                return doc == null ? Refuse(failure) : Ok(new { Document = doc, Warning = failure });
            });

        private static string ValidatePeriod(InvoiceGenerationRequest r)
        {
            if (r.PeriodFrom == default || r.PeriodTo == default) return "A period is required.";
            if (r.PeriodTo.Date < r.PeriodFrom.Date) return "PeriodTo is before PeriodFrom.";
            if (string.IsNullOrWhiteSpace(r.CompanyGSTIN))
                return "The company GSTIN is required - a document number is only unique within the registration "
                     + "that raised it.";
            if (string.IsNullOrWhiteSpace(r.SupplierStateCode))
                return "The company state code is required, or every supply would be treated as intra-state and "
                     + "the tax would go to the wrong head.";
            return string.Empty;
        }
    }
}
