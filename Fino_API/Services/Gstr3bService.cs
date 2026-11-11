using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase6;
using FintechApp.Responses.Phase6;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IGstr3bService" />
    public class Gstr3bService : IGstr3bService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGstr1Service _gstr1;
        private readonly Logger _logger = new();

        public Gstr3bService(ApplicationDbContext context, IGstr1Service gstr1)
        {
            _context = context;
            _gstr1 = gstr1;
        }

        /// <summary>
        /// The month, with the credit earlier months left unused brought into it.
        ///
        /// A month is not an island: credit that exceeded one month's tax is still there the
        /// next. So the months before this one are worked first, in order, each handing its
        /// unused credit to the next — the same thing the portal's credit ledger does. The
        /// figures are this system's own (documents issued, credits claimed here); where they
        /// differ from the portal's ledger, the portal is right and the difference is a bill or
        /// a claim that was not recorded here.
        /// </summary>
        public async Task<(Gstr3bResult? Result, string Failure)> Build(GstReturnRequest request)
        {
            var (result, failure) = await BuildMonth(request);
            if (result == null) return (result, failure);

            var opening = GstSetOff.Heads.Zero;
            foreach (string prior in await PeriodsBefore(result.CompanyGSTIN, result.PeriodKey))
            {
                var (month, _) = await BuildMonth(new GstReturnRequest
                {
                    CompanyGSTIN = result.CompanyGSTIN, PeriodKey = prior, B2CLThreshold = request.B2CLThreshold,
                });
                if (month == null) continue;
                opening = Settle(month, opening).CreditLeft;
            }

            var settled = Settle(result, opening);
            result.OpeningCreditIGST = opening.IGST;
            result.OpeningCreditCGST = opening.CGST;
            result.OpeningCreditSGST = opening.SGST;
            result.OpeningCreditCess = opening.Cess;
            result.PayableIGST = settled.Payable.IGST;
            result.PayableCGST = settled.Payable.CGST;
            result.PayableSGST = settled.Payable.SGST;
            result.PayableCess = settled.Payable.Cess;
            result.TotalPayable = Round(settled.Payable.Total);
            result.ClosingCreditIGST = settled.CreditLeft.IGST;
            result.ClosingCreditCGST = settled.CreditLeft.CGST;
            result.ClosingCreditSGST = settled.CreditLeft.SGST;
            result.ClosingCreditCess = settled.CreditLeft.Cess;
            return (result, string.Empty);
        }

        /// <summary>One month's output tax against the credit available to it, in the statutory order.</summary>
        private static GstSetOff.Result Settle(Gstr3bResult month, GstSetOff.Heads opening) =>
            GstSetOff.Apply(
                new GstSetOff.Heads(month.OutputIGST, month.OutputCGST, month.OutputSGST, month.OutputCess),
                opening + new GstSetOff.Heads(month.NetItcIGST, month.NetItcCGST, month.NetItcSGST, month.NetItcCess));

        /// <summary>
        /// Every month from the first one this registration has a document in, up to (not
        /// including) <paramref name="period"/>. Capped at three years: credit older than that
        /// is not something to reconstruct from documents.
        /// </summary>
        private async Task<List<string>> PeriodsBefore(string gstin, string period)
        {
            var list = new List<string>();
            if (!TryPeriodRange(period, out var target, out _)) return list;

            string? first = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.PeriodKey != "" && (h.CompanyGSTIN == gstin || h.CompanyGSTIN == ""))
                .MinAsync(h => (string?)h.PeriodKey);
            if (string.IsNullOrWhiteSpace(first) || !TryPeriodRange(first, out var start, out _)) return list;

            DateTime floor = target.AddMonths(-36);
            if (start < floor) start = floor;
            for (var d = start; d < target; d = d.AddMonths(1)) list.Add($"{d:yyyyMM}");
            return list;
        }

        private async Task<(Gstr3bResult? Result, string Failure)> BuildMonth(GstReturnRequest request)
        {
            string gstin = (request.CompanyGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            string period = (request.PeriodKey ?? string.Empty).Trim();
            if (period.Length != 6) return (null, "A period (yyyyMM) is required.");
            if (gstin.Length != 15) return (null, "The company GSTIN is required. Set it on Invoice Settings.");

            // 3B is a summary of exactly the supplies GSTR-1 reports one by one, so it is built
            // FROM that build rather than from a second query. Two queries over the same documents
            // would be two chances to filter them differently, and the difference between the two
            // returns is the thing this whole screen exists to rule out.
            var (g1, failure) = await _gstr1.Build(request);
            if (g1 == null) return (null, failure);

            var result = new Gstr3bResult
            {
                CompanyGSTIN = gstin,
                PeriodKey = period,
                PeriodLabel = Gstr1Service.PeriodLabel(period),
                Warnings = new List<string>(g1.Warnings),
            };

            // ── Table 3.1 ────────────────────────────────────────────────────────────────
            // (a) Outward taxable supplies, other than zero-rated, nil-rated and exempted.
            decimal taxable = g1.TotalTaxableValue;
            decimal igst = g1.TotalIGST, cgst = g1.TotalCGST, sgst = g1.TotalSGST, cess = g1.TotalCess;

            result.Outward.Add(new Gstr3bOutwardRow
            {
                Code = "a",
                Nature = "Outward taxable supplies (other than zero rated, nil rated and exempted)",
                TaxableValue = taxable, IGST = igst, CGST = cgst, SGST = sgst, Cess = cess,
            });

            // (b) Zero-rated: exports and SEZ supplies, which carry no output tax here.
            var zeroRated = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Sale
                         && h.CompanyGSTIN == gstin && h.PeriodKey == period
                         && h.Status == TaxDocumentStatus.Issued
                         && (h.SupplyNature == SupplyNature.Export || h.SupplyNature == SupplyNature.SEZ))
                .SumAsync(h => (decimal?)h.TaxableTotal) ?? 0m;

            result.Outward.Add(new Gstr3bOutwardRow
            {
                Code = "b", Nature = "Outward taxable supplies (zero rated)", TaxableValue = Round(zeroRated),
            });

            // (c) Other outward supplies: nil rated and exempted — table 8 of GSTR-1.
            decimal nilExempt = g1.Exempt.Sum(e => e.NilRated + e.Exempted);
            result.Outward.Add(new Gstr3bOutwardRow
            {
                Code = "c", Nature = "Other outward supplies (nil rated, exempted)", TaxableValue = Round(nilExempt),
            });

            // (d) Inward supplies liable to reverse charge. These are OUR liability on somebody
            // else's supply — the self-invoices Phase 2 raises. They are deliberately absent from
            // GSTR-1, which is why this figure is read separately rather than taken from it.
            var rcmHeaders = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.CompanyGSTIN == gstin && h.PeriodKey == period
                         && h.Status == TaxDocumentStatus.Issued
                         && h.DocumentType == TaxDocumentType.SelfInvoice)
                .Select(h => new { h.TaxableTotal, h.IGSTTotal, h.CGSTTotal, h.SGSTTotal, h.CessTotal })
                .ToListAsync();

            result.Outward.Add(new Gstr3bOutwardRow
            {
                Code = "d",
                Nature = "Inward supplies liable to reverse charge",
                TaxableValue = Round(rcmHeaders.Sum(x => x.TaxableTotal)),
                IGST = Round(rcmHeaders.Sum(x => x.IGSTTotal)),
                CGST = Round(rcmHeaders.Sum(x => x.CGSTTotal)),
                SGST = Round(rcmHeaders.Sum(x => x.SGSTTotal)),
                Cess = Round(rcmHeaders.Sum(x => x.CessTotal)),
            });

            // (e) Non-GST outward supplies.
            result.Outward.Add(new Gstr3bOutwardRow
            {
                Code = "e", Nature = "Non-GST outward supplies", TaxableValue = Round(g1.Exempt.Sum(e => e.NonGST)),
            });

            // ── Table 3.2 ────────────────────────────────────────────────────────────────
            // Of the 3.1(a) supplies, the part made to unregistered persons in another state. That
            // is exactly the B2CL table plus the inter-state B2CS rows.
            var interState = new Dictionary<string, Gstr3bInterStateRow>();

            void AddInterState(string pos, decimal tv, decimal ig)
            {
                string key = (pos ?? string.Empty).Trim();
                if (!interState.TryGetValue(key, out var row))
                {
                    interState[key] = row = new Gstr3bInterStateRow
                    {
                        PlaceOfSupplyStateCode = key,
                        PlaceOfSupplyName = GstStates.Name(key),
                        Recipient = "Unregistered persons",
                    };
                }
                row.TaxableValue += tv;
                row.IGST += ig;
            }

            foreach (var p in g1.B2CL)
                foreach (var d in p.Documents)
                    AddInterState(d.PlaceOfSupplyStateCode, d.Slabs.Sum(s => s.TaxableValue), d.Slabs.Sum(s => s.IGST));

            foreach (var r in g1.B2CS.Where(r => r.SupplyType == "INTER"))
                AddInterState(r.PlaceOfSupplyStateCode, r.TaxableValue, r.IGST);

            foreach (var r in interState.Values) { r.TaxableValue = Round(r.TaxableValue); r.IGST = Round(r.IGST); }
            result.InterStateSupplies = interState.Values
                .Where(r => r.TaxableValue != 0m || r.IGST != 0m)
                .OrderBy(r => r.PlaceOfSupplyStateCode).ToList();

            // ── Table 4: input tax credit ────────────────────────────────────────────────
            // (A)(5) "All other ITC" comes from the ITC register Phase 3 builds — the credit that
            // was actually claimed in this period, not every credit that exists. A credit sitting
            // eligible but unclaimed is not in 3B, and saying otherwise would overstate the claim.
            var claimed = await _context.ItcRegisterEntries.AsNoTracking()
                .Where(e => !e.IsDeleted && e.Status == ItcStatus.Claimed && e.ClaimedInPeriod == period)
                .Select(e => new { e.IGST, e.CGST, e.SGST, e.Cess })
                .ToListAsync();

            // (A)(3) Inward supplies liable to reverse charge — the tax we pay on a self-invoice is
            // also credit we may take, which is what makes reverse charge cash-neutral when the
            // credit is eligible.
            result.Itc.Add(new Gstr3bItcRow
            {
                Code = "A(3)",
                Detail = "Inward supplies liable to reverse charge (other than 1 & 2 above)",
                IGST = Round(rcmHeaders.Sum(x => x.IGSTTotal)),
                CGST = Round(rcmHeaders.Sum(x => x.CGSTTotal)),
                SGST = Round(rcmHeaders.Sum(x => x.SGSTTotal)),
                Cess = Round(rcmHeaders.Sum(x => x.CessTotal)),
            });

            result.Itc.Add(new Gstr3bItcRow
            {
                Code = "A(5)",
                Detail = "All other ITC",
                IGST = Round(claimed.Sum(x => x.IGST)),
                CGST = Round(claimed.Sum(x => x.CGST)),
                SGST = Round(claimed.Sum(x => x.SGST)),
                Cess = Round(claimed.Sum(x => x.Cess)),
            });

            // (B)(2) Reversed IN this period — keyed on ReversedOn, not ClaimedInPeriod. A credit
            // claimed in May and given back in October is a May claim and an October reversal;
            // keying the reversal on the claim period would take it out of the month it was
            // actually given back in, and 3B would never match the cash.
            if (!TryPeriodRange(period, out var from, out var to))
                return (null, "That period is not a valid yyyyMM.");

            var reversed = await _context.ItcRegisterEntries.AsNoTracking()
                .Where(e => !e.IsDeleted && e.Status == ItcStatus.Reversed
                         && e.ReversedOn.HasValue && e.ReversedOn.Value >= from && e.ReversedOn.Value < to)
                .Select(e => new { e.IGST, e.CGST, e.SGST, e.Cess })
                .ToListAsync();

            result.Itc.Add(new Gstr3bItcRow
            {
                Code = "B(2)",
                Detail = "ITC reversed — others (Rule 37 / Section 16(4))",
                IGST = Round(reversed.Sum(x => x.IGST)),
                CGST = Round(reversed.Sum(x => x.CGST)),
                SGST = Round(reversed.Sum(x => x.SGST)),
                Cess = Round(reversed.Sum(x => x.Cess)),
            });

            decimal availIg = result.Itc.Where(i => i.Code.StartsWith('A')).Sum(i => i.IGST);
            decimal availCg = result.Itc.Where(i => i.Code.StartsWith('A')).Sum(i => i.CGST);
            decimal availSg = result.Itc.Where(i => i.Code.StartsWith('A')).Sum(i => i.SGST);
            decimal availCs = result.Itc.Where(i => i.Code.StartsWith('A')).Sum(i => i.Cess);
            decimal revIg = result.Itc.Where(i => i.Code.StartsWith('B')).Sum(i => i.IGST);
            decimal revCg = result.Itc.Where(i => i.Code.StartsWith('B')).Sum(i => i.CGST);
            decimal revSg = result.Itc.Where(i => i.Code.StartsWith('B')).Sum(i => i.SGST);
            decimal revCs = result.Itc.Where(i => i.Code.StartsWith('B')).Sum(i => i.Cess);

            result.Itc.Add(new Gstr3bItcRow
            {
                Code = "C",
                Detail = "Net ITC available (A − B)",
                IGST = Round(availIg - revIg), CGST = Round(availCg - revCg),
                SGST = Round(availSg - revSg), Cess = Round(availCs - revCs),
            });

            // ── Tax payable ──────────────────────────────────────────────────────────────
            // Output tax includes the reverse-charge liability, because 3.1(d) is a liability even
            // though the matching credit sits in table 4.
            result.OutputIGST = Round(igst + rcmHeaders.Sum(x => x.IGSTTotal));
            result.OutputCGST = Round(cgst + rcmHeaders.Sum(x => x.CGSTTotal));
            result.OutputSGST = Round(sgst + rcmHeaders.Sum(x => x.SGSTTotal));
            result.OutputCess = Round(cess + rcmHeaders.Sum(x => x.CessTotal));

            result.NetItcIGST = Round(availIg - revIg);
            result.NetItcCGST = Round(availCg - revCg);
            result.NetItcSGST = Round(availSg - revSg);
            result.NetItcCess = Round(availCs - revCs);

            // Payable is NOT worked out here. It depends on the credit earlier months left
            // unused, and on the order credit may be used across heads — both of which Build
            // applies once it has walked the months before this one. See GstSetOff.

            // ── The 1-vs-3B check ────────────────────────────────────────────────────────
            result.Gstr1TaxableValue = g1.TotalTaxableValue;
            result.Gstr1TotalTax = Round(g1.TotalIGST + g1.TotalCGST + g1.TotalSGST + g1.TotalCess);
            result.VarianceTaxableValue = Round(taxable - result.Gstr1TaxableValue);
            result.VarianceTax = Round((igst + cgst + sgst + cess) - result.Gstr1TotalTax);
            result.AgreesWithGstr1 = Math.Abs(result.VarianceTaxableValue) <= 1m
                                  && Math.Abs(result.VarianceTax) <= 1m;

            return (result, string.Empty);
        }

        private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>The day range a yyyyMM period covers, half-open so the last day is included
        /// whatever time of day a timestamp carries.</summary>
        private static bool TryPeriodRange(string periodKey, out DateTime from, out DateTime to)
        {
            from = default; to = default;
            if (periodKey.Length != 6) return false;
            if (!int.TryParse(periodKey.AsSpan(0, 4), out int y)) return false;
            if (!int.TryParse(periodKey.AsSpan(4, 2), out int m) || m < 1 || m > 12) return false;
            from = new DateTime(y, m, 1);
            to = from.AddMonths(1);
            return true;
        }
    }
}
