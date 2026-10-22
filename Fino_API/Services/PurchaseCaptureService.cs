using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IPurchaseCaptureService" />
    public class PurchaseCaptureService : IPurchaseCaptureService
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = new();

        public PurchaseCaptureService(ApplicationDbContext context) => _context = context;

        public async Task<PurchaseCaptureResult> CapturePurchase(PurchaseCaptureRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DocumentNo))
                return Fail("The supplier's document number is required.");
            if (request.DocumentDate == default)
                return Fail("The document date is required.");
            if (request.Lines.Count == 0)
                return Fail("A purchase needs at least one line.");

            // Resolve the supplier. A party set up with a PartyTaxProfile is preferred (the engine
            // keys purchase rules on the user id); otherwise the GSTIN / name / state on the
            // request identify them on the document, with PartyUserID 0.
            int supplierUserId = request.SupplierUserID;
            string gstin = request.SupplierGSTIN.Trim().ToUpperInvariant();
            string name = request.SupplierName.Trim();
            string state = request.SupplierStateCode.Trim();

            if (supplierUserId <= 0 && gstin.Length == 15)
            {
                var byGstin = await _context.UserMasters.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.GSTNo == gstin && !u.IsDeleted);
                if (byGstin != null)
                {
                    supplierUserId = byGstin.UserMasterID;
                    if (string.IsNullOrWhiteSpace(name)) name = byGstin.FirmName ?? byGstin.PersonName ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(state)) state = byGstin.BusinessGSTStateCode ?? string.Empty;
                }
            }

            string documentNo = request.DocumentNo.Trim();

            // Idempotent: the same supplier bill is never captured twice. The DB enforces this with
            // a unique index on (PartyUserID, DocumentNo) filtered to purchases; we check first so a
            // re-send returns the existing document instead of surfacing a constraint violation.
            var existing = await _context.TaxInvoiceHeaders.AsNoTracking()
                .FirstOrDefaultAsync(h => h.Direction == TaxDirection.Purchase
                                       && h.PartyUserID == supplierUserId
                                       && h.DocumentNo == documentNo
                                       && !h.IsDeleted);
            if (existing != null)
                return await Describe(existing, alreadyExisted: true);

            if (string.IsNullOrWhiteSpace(state) && gstin.Length >= 2)
                state = gstin.Substring(0, 2);

            string registration = gstin.Length == 15 ? PartyRegistrationStatus.Registered : PartyRegistrationStatus.Unregistered;

            decimal taxable = request.Lines.Sum(l => l.TaxableValue);
            decimal cgst = request.Lines.Sum(l => l.CGSTAmount);
            decimal sgst = request.Lines.Sum(l => l.SGSTAmount);
            decimal igst = request.Lines.Sum(l => l.IGSTAmount);
            decimal cess = request.Lines.Sum(l => l.CessAmount);
            decimal tax = cgst + sgst + igst + cess;
            decimal grand = Math.Round(taxable + tax, 2);
            // Inter-state when any IGST was charged. The supplier already decided the split; we
            // record it rather than recomputing it, so our books match their document to the paisa.
            string supplyNature = igst > 0 ? SupplyNature.Interstate : SupplyNature.Intrastate;

            var header = new TaxInvoiceHeader
            {
                Direction = TaxDirection.Purchase,
                DocumentType = TaxDocumentType.TaxInvoice,
                TaxDocumentSeriesID = null,
                DocumentNo = documentNo,
                DocumentDate = request.DocumentDate.Date,
                WalletCreditDate = request.WalletCreditDate?.Date,
                PartyUserID = supplierUserId,
                PartyName = name,
                PartyGSTIN = gstin,
                PartyRegistrationStatus = registration,
                CompanyGSTIN = request.CompanyGSTIN.Trim().ToUpperInvariant(),
                SupplierStateCode = state,
                PlaceOfSupplyStateCode = string.IsNullOrWhiteSpace(request.PlaceOfSupplyStateCode)
                    ? (request.CompanyGSTIN.Trim().Length >= 2 ? request.CompanyGSTIN.Trim().Substring(0, 2) : string.Empty)
                    : request.PlaceOfSupplyStateCode.Trim(),
                SupplyNature = supplyNature,
                GrossTotal = grand,          // what the purchase cost us in full
                TaxableTotal = taxable,
                CGSTTotal = cgst,
                SGSTTotal = sgst,
                IGSTTotal = igst,
                CessTotal = cess,
                RoundOff = 0m,
                GrandTotal = grand,
                Status = TaxDocumentStatus.Issued,
                PeriodKey = Phase3Dates.PeriodKey(request.DocumentDate),
                IsConsolidated = request.Lines.Count > 1,
                ProviderID = request.ProviderID > 0 ? request.ProviderID : null,
                WalletID = request.WalletID > 0 ? request.WalletID : null,
                Remark = request.Remark.Trim(),
                CreatedByUserID = request.CreatedByUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };
            _context.TaxInvoiceHeaders.Add(header);
            await _context.SaveChangesAsync();

            int lineNo = 1;
            foreach (var l in request.Lines)
            {
                _context.TaxInvoiceLines.Add(new TaxInvoiceLine
                {
                    TaxInvoiceHeaderID = header.TaxInvoiceHeaderID,
                    LineNo = lineNo++,
                    ServiceID = l.ServiceID,
                    Description = l.Description.Trim(),
                    SACCode = l.SACCode.Trim(),
                    Quantity = l.Quantity <= 0 ? 1 : l.Quantity,
                    BillingBasis = string.Empty,
                    GrossValue = Math.Round(l.TaxableValue + l.CGSTAmount + l.SGSTAmount + l.IGSTAmount + l.CessAmount, 2),
                    TaxableValue = l.TaxableValue,
                    CGSTRate = l.CGSTRate, CGSTAmount = l.CGSTAmount,
                    SGSTRate = l.SGSTRate, SGSTAmount = l.SGSTAmount,
                    IGSTRate = l.IGSTRate, IGSTAmount = l.IGSTAmount,
                    CessAmount = l.CessAmount,
                    LineTotal = Math.Round(l.TaxableValue + l.CGSTAmount + l.SGSTAmount + l.IGSTAmount + l.CessAmount, 2),
                    TaxRuleMasterID = 0,
                    ITCEligible = l.ITCEligible,
                    ReverseCharge = l.ReverseCharge,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                });
            }
            await _context.SaveChangesAsync();

            return await Describe(header, alreadyExisted: false);
        }

        public async Task<(PurchaseCaptureResult? Result, string Failure)> ImportEInvoice(EInvoiceImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Json))
                return (null, "The e-invoice JSON is empty.");

            JObject root;
            try { root = JObject.Parse(request.Json); }
            catch (Exception ex) { _logger.Write(ex); return (null, "The e-invoice JSON could not be parsed."); }

            // NIC schema: SellerDtls is the supplier, DocDtls carries the number/date/type, ItemList
            // the lines, ValDtls the totals. Reading is case-insensitive on the top-level sections
            // because real downloads vary in capitalisation.
            JToken? Sec(string name) => root.Properties()
                .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))?.Value;

            var seller = Sec("SellerDtls");
            var doc = Sec("DocDtls");
            var items = Sec("ItemList") as JArray;
            if (seller == null || doc == null || items == null || items.Count == 0)
                return (null, "The JSON is not a recognisable e-invoice (SellerDtls / DocDtls / ItemList missing).");

            string DocType(string t) => t?.ToUpperInvariant() switch
            {
                "CRN" => TaxDocumentType.CreditNote,
                "DBN" => TaxDocumentType.DebitNote,
                _ => TaxDocumentType.TaxInvoice,
            };

            DateTime ParseDate(string? s)
            {
                // NIC dates are dd/MM/yyyy; fall back to whatever parses.
                if (DateTime.TryParseExact(s, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d)) return d;
                return DateTime.TryParse(s, out var g) ? g : DateTime.Today;
            }

            var lines = new List<PurchaseLineInput>();
            foreach (var it in items)
            {
                lines.Add(new PurchaseLineInput
                {
                    ServiceID = request.DefaultServiceID,
                    Description = (string?)it["PrdDesc"] ?? string.Empty,
                    SACCode = (string?)it["HsnCd"] ?? string.Empty,
                    Quantity = (int?)it["Qty"] ?? 1,
                    TaxableValue = (decimal?)it["AssAmt"] ?? 0m,
                    CGSTAmount = (decimal?)it["CgstAmt"] ?? 0m,
                    SGSTAmount = (decimal?)it["SgstAmt"] ?? 0m,
                    IGSTAmount = (decimal?)it["IgstAmt"] ?? 0m,
                    CessAmount = (decimal?)it["CesAmt"] ?? 0m,
                    ITCEligible = true,
                });
            }

            var capture = new PurchaseCaptureRequest
            {
                SupplierGSTIN = (string?)seller["Gstin"] ?? string.Empty,
                SupplierName = (string?)seller["LglNm"] ?? (string?)seller["TrdNm"] ?? string.Empty,
                SupplierStateCode = (string?)seller["Stcd"] ?? string.Empty,
                CompanyGSTIN = request.CompanyGSTIN,
                DocumentNo = (string?)doc["No"] ?? string.Empty,
                DocumentDate = ParseDate((string?)doc["Dt"]),
                ProviderID = request.ProviderID,
                WalletID = request.WalletID,
                WalletCreditDate = request.WalletCreditDate,
                Remark = "Imported from e-invoice JSON",
                Lines = lines,
                CreatedByUserID = request.CreatedByUserID,
            };

            var result = await CapturePurchase(capture);
            if (result.TaxInvoiceHeaderID == 0 && !result.AlreadyExisted)
                return (null, string.IsNullOrWhiteSpace(result.Warning) ? "The purchase could not be captured." : result.Warning);
            return (result, string.Empty);
        }

        public async Task<List<TaxInvoiceHeader>> GetPurchases(DateTime periodFrom, DateTime periodTo)
        {
            DateTime start = periodFrom.Date, end = periodTo.Date.AddDays(1);
            return await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Purchase
                         && h.DocumentDate >= start && h.DocumentDate < end)
                .OrderByDescending(h => h.DocumentDate).ThenByDescending(h => h.TaxInvoiceHeaderID)
                .ToListAsync();
        }

        private async Task<PurchaseCaptureResult> Describe(TaxInvoiceHeader h, bool alreadyExisted)
        {
            decimal eligible = await _context.TaxInvoiceLines.AsNoTracking()
                .Where(l => l.TaxInvoiceHeaderID == h.TaxInvoiceHeaderID && !l.IsDeleted && l.ITCEligible)
                .SumAsync(l => (decimal?)(l.CGSTAmount + l.SGSTAmount + l.IGSTAmount + l.CessAmount)) ?? 0m;

            return new PurchaseCaptureResult
            {
                TaxInvoiceHeaderID = h.TaxInvoiceHeaderID,
                DocumentNo = h.DocumentNo,
                DocumentDate = h.DocumentDate,
                SupplierName = h.PartyName,
                SupplierGSTIN = h.PartyGSTIN,
                TaxableTotal = h.TaxableTotal,
                TaxTotal = h.CGSTTotal + h.SGSTTotal + h.IGSTTotal + h.CessTotal,
                GrandTotal = h.GrandTotal,
                EligibleITC = eligible,
                AlreadyExisted = alreadyExisted,
            };
        }

        private static PurchaseCaptureResult Fail(string message) => new() { Warning = message };
    }
}
