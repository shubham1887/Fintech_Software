using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase6;
using FintechApp.Responses.Phase6;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IGstr9Service" />
    public class Gstr9Service : IGstr9Service
    {
        private readonly ApplicationDbContext _context;
        private readonly IGstr1Service _gstr1;
        private readonly Logger _logger = new();

        public Gstr9Service(ApplicationDbContext context, IGstr1Service gstr1)
        {
            _context = context;
            _gstr1 = gstr1;
        }

        public async Task<(Gstr9Result? Result, string Failure)> Build(Gstr9Request request)
        {
            string gstin = (request.CompanyGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            if (gstin.Length != 15) return (null, "The company GSTIN is required. Set it on Invoice Settings.");

            int fy = request.FinancialYearStart;
            if (fy < 2000 || fy > 2100) return (null, "A financial year is required, e.g. 2026 for 2026-27.");

            var result = new Gstr9Result
            {
                CompanyGSTIN = gstin,
                FinancialYearStart = fy,
                FinancialYearLabel = $"{fy}-{(fy + 1) % 100:00}",
            };

            // India's financial year runs 1 April to 31 March, so the twelve periods are April of
            // the start year through March of the next. Iterating the months rather than taking a
            // date range keeps every month present in the output even when it has no documents —
            // a missing month is a finding, not a row to omit.
            var periods = Enumerable.Range(0, 12)
                .Select(i => new DateTime(fy, 4, 1).AddMonths(i))
                .Select(d => d.ToString("yyyyMM"))
                .ToList();

            // What was actually filed, from the snapshots. Never recomputed: recomputing would
            // answer "as per books" twice and hide the very difference Part II exists to show.
            var filings = await _context.GstReturnFilings.AsNoTracking()
                .Where(f => !f.IsDeleted && f.CompanyGSTIN == gstin
                         && f.ReturnType == GstReturnType.Gstr1
                         && f.Status == GstFilingStatus.Filed
                         && periods.Contains(f.PeriodKey))
                .ToListAsync();
            var filedByPeriod = filings.ToDictionary(f => f.PeriodKey, f => f);

            var rateAcc = new Dictionary<decimal, RateSlab>();

            foreach (string p in periods)
            {
                var (g1, failure) = await _gstr1.Build(new GstReturnRequest
                {
                    CompanyGSTIN = gstin,
                    PeriodKey = p,
                    // The annual return must be built on the limit that applied in each month, and
                    // nothing here knows that. The current limit is used and said so in a warning,
                    // rather than quietly assuming a cut-over date.
                    B2CLThreshold = 0m,
                });
                if (g1 == null)
                {
                    result.Warnings.Add($"{Gstr1Service.PeriodLabel(p)} could not be built: {failure}");
                    continue;
                }

                decimal booksTax = g1.TotalIGST + g1.TotalCGST + g1.TotalSGST + g1.TotalCess;
                var row = new Gstr9MonthRow
                {
                    PeriodKey = p,
                    PeriodLabel = Gstr1Service.PeriodLabel(p),
                    BooksTaxableValue = g1.TotalTaxableValue,
                    BooksTax = Round(booksTax),
                };

                if (filedByPeriod.TryGetValue(p, out var filed))
                {
                    row.WasFiled = true;
                    row.FiledTaxableValue = filed.TotalTaxableValue;
                    row.FiledTax = Round(filed.TotalIGST + filed.TotalCGST + filed.TotalSGST + filed.TotalCess);
                }

                row.VarianceTaxableValue = Round(row.BooksTaxableValue - row.FiledTaxableValue);
                row.VarianceTax = Round(row.BooksTax - row.FiledTax);
                result.Months.Add(row);

                // Part II wants the year's outward supplies by rate, so each month's slabs are
                // folded in as they are produced rather than re-queried at the end.
                foreach (var party in g1.B2B.Concat(g1.B2CL).Concat(g1.CDNR).Concat(g1.CDNUR))
                    foreach (var d in party.Documents)
                        foreach (var s in d.Slabs)
                            Accumulate(rateAcc, s.Rate, s.TaxableValue, s.IGST, s.CGST, s.SGST, s.Cess);

                foreach (var b in g1.B2CS)
                    Accumulate(rateAcc, b.Rate, b.TaxableValue, b.IGST, b.CGST, b.SGST, b.Cess);
            }

            foreach (var s in rateAcc.Values)
            {
                s.TaxableValue = Round(s.TaxableValue); s.IGST = Round(s.IGST);
                s.CGST = Round(s.CGST); s.SGST = Round(s.SGST); s.Cess = Round(s.Cess);
            }
            result.OutwardByRate = rateAcc.Values.OrderBy(s => s.Rate).ToList();

            // Part III: the credit availed across the year, from the ITC register.
            var claimed = await _context.ItcRegisterEntries.AsNoTracking()
                .Where(e => !e.IsDeleted && e.Status == ItcStatus.Claimed && periods.Contains(e.ClaimedInPeriod))
                .Select(e => new { e.IGST, e.CGST, e.SGST, e.Cess })
                .ToListAsync();

            result.ItcAvailedIGST = Round(claimed.Sum(x => x.IGST));
            result.ItcAvailedCGST = Round(claimed.Sum(x => x.CGST));
            result.ItcAvailedSGST = Round(claimed.Sum(x => x.SGST));
            result.ItcAvailedCess = Round(claimed.Sum(x => x.Cess));

            DateTime fyFrom = new(fy, 4, 1), fyTo = new(fy + 1, 4, 1);
            var reversed = await _context.ItcRegisterEntries.AsNoTracking()
                .Where(e => !e.IsDeleted && e.Status == ItcStatus.Reversed
                         && e.ReversedOn.HasValue && e.ReversedOn.Value >= fyFrom && e.ReversedOn.Value < fyTo)
                .Select(e => new { e.IGST, e.CGST, e.SGST, e.Cess })
                .ToListAsync();
            result.ItcReversed = Round(reversed.Sum(x => x.IGST + x.CGST + x.SGST + x.Cess));

            result.TotalTurnover = Round(result.Months.Sum(m => m.BooksTaxableValue));
            result.TotalTaxPayable = Round(result.Months.Sum(m => m.BooksTax));
            result.TotalFiledTaxableValue = Round(result.Months.Sum(m => m.FiledTaxableValue));
            result.TotalVarianceTaxableValue = Round(result.TotalTurnover - result.TotalFiledTaxableValue);
            result.MonthsFiled = result.Months.Count(m => m.WasFiled);
            result.MonthsMissing = result.Months.Count(m => !m.WasFiled);

            if (result.MonthsMissing > 0)
            {
                result.Warnings.Add(
                    $"{result.MonthsMissing} of 12 months have no filed GSTR-1 snapshot, so the " +
                    "\"as filed\" column is blank for them and the annual variance is overstated by " +
                    "exactly their turnover. Save and mark each month's GSTR-1 as filed to close this.");
            }
            result.Warnings.Add(
                "Every month was rebuilt on the CURRENT B2CL limit. A month filed under the older " +
                $"Rs {GstReturnLimits.B2CLInvoiceValueLegacy:N0} limit may split differently between B2CL and " +
                "B2CS than the return actually filed — the totals are unaffected.");

            return (result, string.Empty);
        }

        private static void Accumulate(
            Dictionary<decimal, RateSlab> acc, decimal rate,
            decimal taxable, decimal igst, decimal cgst, decimal sgst, decimal cess)
        {
            if (!acc.TryGetValue(rate, out var s)) acc[rate] = s = new RateSlab { Rate = rate };
            s.TaxableValue += taxable;
            s.IGST += igst; s.CGST += cgst; s.SGST += sgst; s.Cess += cess;
        }

        private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
