using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <summary>
    /// The half of purchase capture that came with the Purchase Invoice screen: opening a bill,
    /// correcting it, removing it, and the lists the form is filled from (providers, wallets,
    /// the items a supplier bills, and the suppliers already seen).
    /// </summary>
    public partial class PurchaseCaptureService
    {
        private async Task<string> CompanyGstin()
        {
            var profile = await _context.InvoiceSettings.AsNoTracking()
                .OrderBy(x => x.InvoiceSettingID).FirstOrDefaultAsync();
            string gstin = (profile?.GSTIN ?? string.Empty).Trim().ToUpperInvariant();
            return gstin.Length == 15 ? gstin : string.Empty;
        }

        /// <summary>
        /// Why a captured bill may no longer be changed, or empty when it may.
        ///
        /// Once the credit on a bill has been CLAIMED it is inside a return; changing the bill
        /// underneath would leave the return saying one thing and the books another. The same
        /// goes for a credit that was reversed. Everything short of that is still only a record
        /// of what the supplier printed, and a typing mistake in it should be fixable.
        /// </summary>
        private async Task<string> LockReason(int headerId)
        {
            var entry = await _context.ItcRegisterEntries.AsNoTracking()
                .FirstOrDefaultAsync(e => e.TaxInvoiceHeaderID == headerId && !e.IsDeleted);
            if (entry == null) return string.Empty;
            if (entry.Status == ItcStatus.Claimed)
                return $"The credit on this bill was already claimed in the {entry.ClaimedInPeriod} return, so it can no longer be changed here.";
            if (entry.Status == ItcStatus.Reversed)
                return "The credit on this bill was claimed and then reversed, so it can no longer be changed here.";
            return string.Empty;
        }

        private static TaxInvoiceLine BuildLine(int headerId, int lineNo, PurchaseLineInput l)
        {
            decimal total = Math.Round(l.TaxableValue + l.CGSTAmount + l.SGSTAmount + l.IGSTAmount + l.CessAmount, 2);
            bool hasRate = l.UnitQuantity != 0m || l.UnitRate != 0m;
            return new TaxInvoiceLine
            {
                TaxInvoiceHeaderID = headerId,
                LineNo = lineNo,
                ServiceID = l.ServiceID,
                Description = (l.Description ?? string.Empty).Trim(),
                SACCode = (l.SACCode ?? string.Empty).Trim(),
                Quantity = l.Quantity <= 0 ? 1 : l.Quantity,
                BillingBasis = string.Empty,
                GrossValue = total,
                TaxableValue = l.TaxableValue,
                CGSTRate = l.CGSTRate, CGSTAmount = l.CGSTAmount,
                SGSTRate = l.SGSTRate, SGSTAmount = l.SGSTAmount,
                IGSTRate = l.IGSTRate, IGSTAmount = l.IGSTAmount,
                CessAmount = l.CessAmount,
                LineTotal = total,
                TaxRuleMasterID = 0,
                ITCEligible = l.ITCEligible,
                ReverseCharge = l.ReverseCharge,
                UnitQuantity = l.UnitQuantity,
                UnitRate = l.UnitRate,
                RateType = hasRate ? PurchaseRateType.Normalise(l.RateType) : string.Empty,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };
        }

        public async Task<object?> GetPurchase(int taxInvoiceHeaderID)
        {
            var header = await _context.TaxInvoiceHeaders.AsNoTracking()
                .FirstOrDefaultAsync(h => h.TaxInvoiceHeaderID == taxInvoiceHeaderID && !h.IsDeleted
                                       && h.Direction == TaxDirection.Purchase);
            if (header == null) return null;

            var itc = await _context.ItcRegisterEntries.AsNoTracking()
                .FirstOrDefaultAsync(e => e.TaxInvoiceHeaderID == taxInvoiceHeaderID && !e.IsDeleted);

            string providerName = string.Empty, walletName = string.Empty;
            if (header.ProviderID.HasValue)
                providerName = await _context.ProviderMasters.AsNoTracking()
                    .Where(p => p.ProviderID == header.ProviderID.Value)
                    .Select(p => p.ProviderName).FirstOrDefaultAsync() ?? string.Empty;
            if (header.WalletID.HasValue)
                walletName = await _context.WalletMaster.AsNoTracking()
                    .Where(w => w.WalletID == header.WalletID.Value)
                    .Select(w => w.Name).FirstOrDefaultAsync() ?? string.Empty;

            return new
            {
                Header = header,
                Lines = await _context.TaxInvoiceLines.AsNoTracking()
                    .Where(l => l.TaxInvoiceHeaderID == taxInvoiceHeaderID && !l.IsDeleted)
                    .OrderBy(l => l.LineNo).ToListAsync(),
                ProviderName = providerName,
                WalletName = walletName,
                ItcStatus = itc?.Status ?? string.Empty,
                ItcBucket = itc?.ReconBucket ?? string.Empty,
                EligibleITC = itc?.EligibleITC ?? 0m,
                LockReason = await LockReason(taxInvoiceHeaderID),
            };
        }

        public async Task<(PurchaseCaptureResult? Result, string Failure)> UpdatePurchase(PurchaseCaptureRequest request)
        {
            var header = await _context.TaxInvoiceHeaders
                .FirstOrDefaultAsync(h => h.TaxInvoiceHeaderID == request.TaxInvoiceHeaderID && !h.IsDeleted
                                       && h.Direction == TaxDirection.Purchase);
            if (header == null) return (null, "That purchase invoice was not found.");

            string locked = await LockReason(header.TaxInvoiceHeaderID);
            if (locked.Length > 0) return (null, locked);

            if (string.IsNullOrWhiteSpace(request.DocumentNo)) return (null, "The supplier's invoice number is required.");
            if (request.DocumentDate == default) return (null, "The invoice date is required.");
            var lines = request.Lines.Where(l => l.TaxableValue != 0m).ToList();
            if (lines.Count == 0) return (null, "A purchase needs at least one line with a value.");

            string gstin = (request.SupplierGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            string documentNo = request.DocumentNo.Trim();

            // The number may be corrected, but not onto another bill from the same supplier.
            bool clash = await _context.TaxInvoiceHeaders.AsNoTracking()
                .AnyAsync(h => h.Direction == TaxDirection.Purchase && !h.IsDeleted
                            && h.TaxInvoiceHeaderID != header.TaxInvoiceHeaderID
                            && h.PartyUserID == header.PartyUserID && h.DocumentNo == documentNo);
            if (clash) return (null, $"Another purchase invoice already carries the number {documentNo}.");

            string state = (request.SupplierStateCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(state) && gstin.Length >= 2) state = gstin.Substring(0, 2);

            decimal taxable = lines.Sum(l => l.TaxableValue);
            decimal cgst = lines.Sum(l => l.CGSTAmount);
            decimal sgst = lines.Sum(l => l.SGSTAmount);
            decimal igst = lines.Sum(l => l.IGSTAmount);
            decimal cess = lines.Sum(l => l.CessAmount);
            decimal roundOff = Math.Round(request.RoundOff, 2);
            decimal grand = Math.Round(taxable + cgst + sgst + igst + cess + roundOff, 2);

            string oldPeriod = header.PeriodKey;

            header.DocumentNo = documentNo;
            header.DocumentDate = request.DocumentDate.Date;
            header.WalletCreditDate = request.WalletCreditDate?.Date;
            header.PartyName = (request.SupplierName ?? string.Empty).Trim();
            header.PartyGSTIN = gstin;
            header.PartyRegistrationStatus = gstin.Length == 15
                ? PartyRegistrationStatus.Registered : PartyRegistrationStatus.Unregistered;
            header.SupplierStateCode = state;
            if (string.IsNullOrWhiteSpace(header.CompanyGSTIN)) header.CompanyGSTIN = await CompanyGstin();
            if (string.IsNullOrWhiteSpace(header.PlaceOfSupplyStateCode) && header.CompanyGSTIN.Length >= 2)
                header.PlaceOfSupplyStateCode = header.CompanyGSTIN.Substring(0, 2);
            header.SupplyNature = igst > 0 ? SupplyNature.Interstate : SupplyNature.Intrastate;
            header.GrossTotal = grand;
            header.TaxableTotal = taxable;
            header.CGSTTotal = cgst;
            header.SGSTTotal = sgst;
            header.IGSTTotal = igst;
            header.CessTotal = cess;
            header.RoundOff = roundOff;
            header.GrandTotal = grand;
            header.PeriodKey = Phase3Dates.PeriodKey(request.DocumentDate);
            header.IsConsolidated = lines.Count > 1;
            header.ProviderID = request.ProviderID > 0 ? request.ProviderID : null;
            header.WalletID = request.WalletID > 0 ? request.WalletID : null;
            header.Remark = (request.Remark ?? string.Empty).Trim();
            header.UpdatedateTime = DateTime.UtcNow;

            // The old lines are kept, marked deleted, rather than overwritten: what the bill said
            // before somebody corrected it is the first thing asked when a figure moves.
            var old = await _context.TaxInvoiceLines
                .Where(l => l.TaxInvoiceHeaderID == header.TaxInvoiceHeaderID && !l.IsDeleted).ToListAsync();
            foreach (var l in old) { l.IsDeleted = true; l.UpdatedateTime = DateTime.UtcNow; }

            int lineNo = 1;
            foreach (var l in lines)
                _context.TaxInvoiceLines.Add(BuildLine(header.TaxInvoiceHeaderID, lineNo++, l));

            // A corrected bill is no longer the bill that was matched against 2B, so the match is
            // dropped and has to be made again against the new figures.
            var entry = await _context.ItcRegisterEntries
                .FirstOrDefaultAsync(e => e.TaxInvoiceHeaderID == header.TaxInvoiceHeaderID && !e.IsDeleted);
            if (entry != null)
            {
                entry.ReconBucket = string.Empty;
                entry.Matched2BLineID = null;
                // The register is rebuilt per period; if the date moved the bill to another
                // month, the entry has to move with it or the rebuild of neither month finds it.
                entry.PeriodKey = header.PeriodKey;
                entry.UpdatedateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var result = await Describe(header, alreadyExisted: false);
            if (oldPeriod != header.PeriodKey)
                result.Warning = $"The invoice date moved this bill from {oldPeriod} to {header.PeriodKey}.";
            return (result, string.Empty);
        }

        public async Task<string> DeletePurchase(int taxInvoiceHeaderID)
        {
            var header = await _context.TaxInvoiceHeaders
                .FirstOrDefaultAsync(h => h.TaxInvoiceHeaderID == taxInvoiceHeaderID && !h.IsDeleted
                                       && h.Direction == TaxDirection.Purchase);
            if (header == null) return "That purchase invoice was not found.";

            string locked = await LockReason(taxInvoiceHeaderID);
            if (locked.Length > 0) return locked;

            header.IsDeleted = true;
            header.UpdatedateTime = DateTime.UtcNow;

            var lines = await _context.TaxInvoiceLines
                .Where(l => l.TaxInvoiceHeaderID == taxInvoiceHeaderID && !l.IsDeleted).ToListAsync();
            foreach (var l in lines) { l.IsDeleted = true; l.UpdatedateTime = DateTime.UtcNow; }

            var entries = await _context.ItcRegisterEntries
                .Where(e => e.TaxInvoiceHeaderID == taxInvoiceHeaderID && !e.IsDeleted).ToListAsync();
            foreach (var e in entries) { e.IsDeleted = true; e.UpdatedateTime = DateTime.UtcNow; }

            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<object> GetLookups()
        {
            var providers = await _context.ProviderMasters.AsNoTracking()
                .Where(p => !p.IsDeleted && !p.IsDelete)
                .OrderBy(p => p.ProviderName)
                .Select(p => new { p.ProviderID, p.ProviderName, p.IsActive }).ToListAsync();

            var wallets = await _context.WalletMaster.AsNoTracking()
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.UISequence).ThenBy(w => w.Name)
                .Select(w => new { w.WalletID, w.Name, w.ShortName, w.IsActive }).ToListAsync();

            var items = await _context.PurchaseItemMasters.AsNoTracking()
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.UISequence).ThenBy(i => i.Name).ToListAsync();

            // Every supplier that has ever billed us, newest bill first — the source of "pick the
            // provider and the supplier fills itself in". Nothing is configured for this: the
            // first bill from a supplier teaches it, and every bill after that is two clicks.
            var history = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Purchase)
                .OrderByDescending(h => h.DocumentDate).ThenByDescending(h => h.TaxInvoiceHeaderID)
                .Select(h => new { h.PartyName, h.PartyGSTIN, h.SupplierStateCode, h.ProviderID, h.WalletID })
                .ToListAsync();

            var suppliers = history
                .Where(h => !string.IsNullOrWhiteSpace(h.PartyName) || !string.IsNullOrWhiteSpace(h.PartyGSTIN))
                .GroupBy(h => string.IsNullOrWhiteSpace(h.PartyGSTIN)
                    ? (h.PartyName ?? string.Empty).Trim().ToUpperInvariant() : h.PartyGSTIN)
                .Select(g => new SupplierSeen(
                    g.First().PartyName ?? string.Empty, g.First().PartyGSTIN ?? string.Empty,
                    g.First().SupplierStateCode ?? string.Empty, g.Count()))
                .ToList();

            // Provider → the supplier on its most recent bill. Where a provider has never been
            // named on a bill, fall back to a supplier whose name starts with the provider's own
            // first word ("JaiJui DMT" → "JAIJUI FINTECH PRIVATE LIMITED"). That is a suggestion
            // the form shows and the user can overwrite — it is flagged, never a silent decision.
            var byProvider = new List<object>();
            foreach (var p in providers)
            {
                var last = history.FirstOrDefault(h => h.ProviderID == p.ProviderID);
                string name = last?.PartyName ?? string.Empty, gst = last?.PartyGSTIN ?? string.Empty,
                       st = last?.SupplierStateCode ?? string.Empty;
                int wallet = last?.WalletID ?? 0;
                bool guessed = false;
                if (last == null)
                {
                    string first = (p.ProviderName ?? string.Empty).Trim().Split(' ')[0];
                    if (first.Length >= 4)
                    {
                        var hit = suppliers.FirstOrDefault(s =>
                            s.SupplierName.StartsWith(first, StringComparison.OrdinalIgnoreCase));
                        if (hit != null)
                        {
                            name = hit.SupplierName; gst = hit.SupplierGSTIN; st = hit.SupplierStateCode;
                            guessed = true;
                        }
                    }
                }
                byProvider.Add(new
                {
                    p.ProviderID, p.ProviderName, p.IsActive,
                    SupplierName = name, SupplierGSTIN = gst, SupplierStateCode = st,
                    WalletID = wallet, Guessed = guessed,
                });
            }

            string companyGstin = await CompanyGstin();
            return new
            {
                Providers = byProvider,
                Wallets = wallets,
                Items = items,
                Suppliers = suppliers,
                CompanyGSTIN = companyGstin,
                CompanyStateCode = companyGstin.Length >= 2 ? companyGstin.Substring(0, 2) : string.Empty,
            };
        }

        public record SupplierSeen(string SupplierName, string SupplierGSTIN, string SupplierStateCode, int Bills);

        public async Task<(PurchaseItemMaster? Item, string Failure)> SaveItem(PurchaseItemSaveRequest request)
        {
            string name = (request.Name ?? string.Empty).Trim();
            if (name.Length == 0) return (null, "The item needs a name.");
            if (name.Length > 150) return (null, "The item name is too long (150 characters at most).");
            string sac = (request.SACCode ?? string.Empty).Trim();
            if (sac.Length > 10) return (null, "A SAC / HSN code is at most 10 characters.");
            if (request.GSTRatePercent < 0m || request.GSTRatePercent > 100m)
                return (null, "The GST rate must be between 0 and 100.");
            if (request.DefaultRate < 0m) return (null, "The rate cannot be negative.");

            bool duplicate = await _context.PurchaseItemMasters.AsNoTracking()
                .AnyAsync(i => !i.IsDeleted && i.Name == name && i.PurchaseItemMasterID != request.PurchaseItemMasterID);
            if (duplicate) return (null, $"An item called \"{name}\" already exists.");

            PurchaseItemMaster item;
            if (request.PurchaseItemMasterID > 0)
            {
                var found = await _context.PurchaseItemMasters
                    .FirstOrDefaultAsync(i => i.PurchaseItemMasterID == request.PurchaseItemMasterID && !i.IsDeleted);
                if (found == null) return (null, "That item was not found.");
                item = found;
            }
            else
            {
                int next = (await _context.PurchaseItemMasters.AsNoTracking().MaxAsync(i => (int?)i.UISequence) ?? 0) + 1;
                item = new PurchaseItemMaster { DateTime = DateTime.UtcNow, UISequence = next };
                _context.PurchaseItemMasters.Add(item);
            }

            item.Name = name;
            item.SACCode = sac;
            item.GSTRatePercent = request.GSTRatePercent;
            item.DefaultRate = request.DefaultRate;
            item.RateType = PurchaseRateType.Normalise(request.RateType);
            item.ITCEligible = request.ITCEligible;
            item.IsActive = request.IsActive;
            item.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (item, string.Empty);
        }

        public async Task<string> DeleteItem(int purchaseItemMasterID)
        {
            var item = await _context.PurchaseItemMasters
                .FirstOrDefaultAsync(i => i.PurchaseItemMasterID == purchaseItemMasterID && !i.IsDeleted);
            if (item == null) return "That item was not found.";
            // Bills keep their own copy of the description, SAC and rate, so removing an item
            // from the list changes nothing already captured.
            item.IsDeleted = true;
            item.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<List<object>> GetPurchaseRows(DateTime periodFrom, DateTime periodTo)
        {
            var headers = await GetPurchases(periodFrom, periodTo);
            var ids = headers.Select(h => h.TaxInvoiceHeaderID).ToList();

            var itc = (await _context.ItcRegisterEntries.AsNoTracking()
                    .Where(e => ids.Contains(e.TaxInvoiceHeaderID) && !e.IsDeleted).ToListAsync())
                .GroupBy(e => e.TaxInvoiceHeaderID).ToDictionary(g => g.Key, g => g.First());

            var providers = await _context.ProviderMasters.AsNoTracking()
                .ToDictionaryAsync(p => p.ProviderID, p => p.ProviderName);

            var items = (await _context.TaxInvoiceLines.AsNoTracking()
                    .Where(l => ids.Contains(l.TaxInvoiceHeaderID) && !l.IsDeleted)
                    .Select(l => new { l.TaxInvoiceHeaderID, l.Description }).ToListAsync())
                .GroupBy(l => l.TaxInvoiceHeaderID)
                .ToDictionary(g => g.Key, g => string.Join(", ",
                    g.Select(x => x.Description).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct()));

            return headers.Select(h =>
            {
                itc.TryGetValue(h.TaxInvoiceHeaderID, out var e);
                return (object)new
                {
                    h.TaxInvoiceHeaderID, h.DocumentNo, h.DocumentDate, h.WalletCreditDate,
                    h.PartyName, h.PartyGSTIN, h.SupplierStateCode, h.SupplyNature,
                    h.TaxableTotal, h.CGSTTotal, h.SGSTTotal, h.IGSTTotal, h.CessTotal, h.RoundOff, h.GrandTotal,
                    h.Status, h.PeriodKey, h.ProviderID, h.WalletID, h.Remark,
                    ProviderName = h.ProviderID.HasValue && providers.TryGetValue(h.ProviderID.Value, out var pn) ? pn : string.Empty,
                    Items = items.TryGetValue(h.TaxInvoiceHeaderID, out var d) ? d : string.Empty,
                    EligibleITC = e?.EligibleITC ?? 0m,
                    ItcStatus = e?.Status ?? string.Empty,
                    ItcBucket = e?.ReconBucket ?? string.Empty,
                    Locked = e != null && (e.Status == ItcStatus.Claimed || e.Status == ItcStatus.Reversed),
                };
            }).ToList();
        }
    }
}
