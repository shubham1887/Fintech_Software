using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IPurchaseReconService" />
    public class PurchaseReconService : IPurchaseReconService
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = new();

        public PurchaseReconService(ApplicationDbContext context) => _context = context;

        public async Task<ReconResult> Reconcile(ReconRequest request, bool commit)
        {
            decimal tol = request.Tolerance > 0 ? request.Tolerance : ReconTolerance.TaxRupees;
            string gstin = request.CompanyGSTIN.Trim().ToUpperInvariant();
            string periodKey = request.PeriodKey.Trim();

            var result = new ReconResult
            {
                CompanyGSTIN = gstin,
                PeriodKey = periodKey,
                Tolerance = tol,
                Committed = commit,
            };
            if (periodKey.Length != 6) { result.No2BImported = true; return result; }

            // Our books: purchases whose period is this one. (PeriodKey is stamped from the
            // document date at capture, so no date arithmetic is needed here.)
            var purchases = await _context.TaxInvoiceHeaders
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Purchase && h.PeriodKey == periodKey
                         && (gstin.Length == 0 || h.CompanyGSTIN == gstin))
                .ToListAsync();

            // The government's view: the active 2B snapshot for this period.
            int activeImport = await _context.Gstr2bImports.AsNoTracking()
                .Where(i => i.CompanyGSTIN == gstin && i.PeriodKey == periodKey
                         && i.Status == Gstr2bImportStatus.Active && !i.IsDeleted)
                .Select(i => i.Gstr2bImportID).FirstOrDefaultAsync();

            var twoB = activeImport == 0
                ? new List<Gstr2bLine>()
                : await _context.Gstr2bLines
                    .Where(l => l.Gstr2bImportID == activeImport && !l.IsDeleted).ToListAsync();

            result.No2BImported = activeImport == 0;

            // Index 2B by (supplier GSTIN, normalised number). A normalised number ignores the
            // punctuation two systems disagree on: "INV/26-27/412" and "INV-26-27-412" are one bill.
            var twoBByKey = new Dictionary<string, Gstr2bLine>(StringComparer.OrdinalIgnoreCase);
            foreach (var l in twoB)
            {
                string k = MatchKey(l.SupplierGSTIN, l.DocumentNo);
                twoBByKey.TryAdd(k, l);
            }

            var rows = new List<ReconRow>();
            var matched2B = new HashSet<int>();
            var toPersist = new List<(Gstr2bLine Line, string Status, int? HeaderId, decimal Var)>();

            foreach (var p in purchases)
            {
                decimal booksTax = p.CGSTTotal + p.SGSTTotal + p.IGSTTotal + p.CessTotal;
                string key = MatchKey(p.PartyGSTIN, p.DocumentNo);

                if (twoBByKey.TryGetValue(key, out var line))
                {
                    matched2B.Add(line.Gstr2bLineID);
                    decimal portalTax = line.IGST + line.CGST + line.SGST + line.Cess;
                    decimal variance = Math.Round(booksTax - portalTax, 2);
                    bool within = Math.Abs(variance) <= tol;
                    string bucket = within ? ReconBucket.Matched : ReconBucket.Mismatch;

                    rows.Add(MakeRow(bucket, p, line, variance));
                    toPersist.Add((line, within ? "Matched" : "Mismatch", p.TaxInvoiceHeaderID, variance));
                }
                else
                {
                    // In our books, the supplier has not filed it: credit not yet available.
                    rows.Add(MakeRow(ReconBucket.MissingIn2B, p, null, booksTax));
                }
            }

            // 2B lines no purchase claimed: credit the supplier declared that we never recorded.
            foreach (var l in twoB.Where(l => !matched2B.Contains(l.Gstr2bLineID)))
            {
                decimal portalTax = l.IGST + l.CGST + l.SGST + l.Cess;
                rows.Add(new ReconRow
                {
                    Bucket = ReconBucket.MissingInBooks,
                    SupplierGSTIN = l.SupplierGSTIN,
                    SupplierName = l.SupplierName,
                    DocumentNo = l.DocumentNo,
                    DocumentDate = l.DocumentDate,
                    Gstr2bLineID = l.Gstr2bLineID,
                    Portal2BTaxable = l.TaxableValue,
                    Portal2BTax = portalTax,
                    TaxVariance = -portalTax,
                    ItcAvailableIn2B = l.ItcAvailable,
                });
                toPersist.Add((l, "Unmatched", null, 0m));
            }

            result.Rows = rows.OrderBy(r => r.Bucket).ThenBy(r => r.SupplierName).ToList();
            result.Summary = rows
                .GroupBy(r => r.Bucket)
                .Select(g => new ReconBucketSummary
                {
                    Bucket = g.Key,
                    Count = g.Count(),
                    BooksTax = g.Sum(x => x.BooksTax),
                    Portal2BTax = g.Sum(x => x.Portal2BTax),
                })
                .OrderBy(s => s.Bucket).ToList();

            result.ThreeWay = await BuildThreeWay(periodKey, gstin, purchases, rows);

            if (commit)
            {
                foreach (var (line, status, headerId, variance) in toPersist)
                {
                    var tracked = await _context.Gstr2bLines.FirstOrDefaultAsync(x => x.Gstr2bLineID == line.Gstr2bLineID);
                    if (tracked == null) continue;
                    tracked.MatchStatus = status;
                    tracked.MatchedHeaderID = headerId;
                    tracked.TaxVariance = variance;
                    tracked.UpdatedateTime = DateTime.UtcNow;
                }

                // Stamp the bucket onto any ITC entry that already exists for a matched purchase, so
                // the register can refuse to let a credit be claimed from an unmatched document.
                foreach (var r in rows.Where(r => r.TaxInvoiceHeaderID.HasValue))
                {
                    var entry = await _context.ItcRegisterEntries
                        .FirstOrDefaultAsync(e => e.TaxInvoiceHeaderID == r.TaxInvoiceHeaderID!.Value && !e.IsDeleted);
                    if (entry == null) continue;
                    entry.ReconBucket = r.Bucket;
                    entry.Matched2BLineID = r.Gstr2bLineID;
                    entry.UpdatedateTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return result;
        }

        /// <summary>Provider wallet top-ups vs captured purchases vs matched 2B, per provider.</summary>
        private async Task<List<ThreeWayRow>> BuildThreeWay(
            string periodKey, string gstin, List<TaxInvoiceHeader> purchases, List<ReconRow> rows)
        {
            // The purchases that carry a provider link are the only ones a wallet top-up can line up
            // against. Group them by provider.
            var byProvider = purchases
                .Where(p => p.ProviderID.HasValue && p.ProviderID.Value > 0)
                .GroupBy(p => p.ProviderID!.Value)
                .ToList();
            if (byProvider.Count == 0) return new List<ThreeWayRow>();

            // The period's day range, for the wallet top-ups (ProviderPurchases is dated, not keyed).
            if (!TryPeriod(periodKey, out var from, out var to)) return new List<ThreeWayRow>();

            var providerIds = byProvider.Select(g => g.Key).ToList();

            var topUps = await _context.ProviderPurchases.AsNoTracking()
                .Where(pp => !pp.IsDelete && providerIds.Contains(pp.ProviderID)
                          && pp.PurchaseDate >= from && pp.PurchaseDate < to)
                .GroupBy(pp => pp.ProviderID)
                .Select(g => new { ProviderID = g.Key, Amount = g.Sum(x => x.Amount) })
                .ToListAsync();
            var topUpByProvider = topUps.ToDictionary(x => x.ProviderID, x => x.Amount);

            var names = await _context.ProviderMasters.AsNoTracking()
                .Where(p => providerIds.Contains(p.ProviderID))
                .Select(p => new { p.ProviderID, p.ProviderName }).ToListAsync();
            var nameByProvider = names.ToDictionary(x => x.ProviderID, x => x.ProviderName ?? string.Empty);

            // Matched-2B tax per purchase header, from the rows already computed.
            var matchedTaxByHeader = rows
                .Where(r => r.Bucket == ReconBucket.Matched && r.TaxInvoiceHeaderID.HasValue)
                .GroupBy(r => r.TaxInvoiceHeaderID!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Portal2BTax));

            var outRows = new List<ThreeWayRow>();
            foreach (var g in byProvider)
            {
                decimal captured = g.Sum(p => p.GrandTotal);
                decimal capturedTax = g.Sum(p => p.CGSTTotal + p.SGSTTotal + p.IGSTTotal + p.CessTotal);
                decimal matched2BTax = g.Sum(p => matchedTaxByHeader.TryGetValue(p.TaxInvoiceHeaderID, out var t) ? t : 0m);
                decimal topUp = topUpByProvider.TryGetValue(g.Key, out var amt) ? amt : 0m;

                outRows.Add(new ThreeWayRow
                {
                    ProviderID = g.Key,
                    ProviderName = nameByProvider.TryGetValue(g.Key, out var nm) ? nm : $"#{g.Key}",
                    WalletTopUps = topUp,
                    CapturedPurchaseGross = captured,
                    CapturedPurchaseTax = capturedTax,
                    Matched2BTax = matched2BTax,
                    WalletVsBooks = Math.Round(topUp - captured, 2),
                });
            }
            return outRows.OrderBy(r => r.ProviderName).ToList();
        }

        private static ReconRow MakeRow(string bucket, TaxInvoiceHeader p, Gstr2bLine? line, decimal variance) => new()
        {
            Bucket = bucket,
            SupplierGSTIN = p.PartyGSTIN,
            SupplierName = p.PartyName,
            DocumentNo = p.DocumentNo,
            DocumentDate = p.DocumentDate,
            TaxInvoiceHeaderID = p.TaxInvoiceHeaderID,
            BooksTaxable = p.TaxableTotal,
            BooksTax = p.CGSTTotal + p.SGSTTotal + p.IGSTTotal + p.CessTotal,
            Gstr2bLineID = line?.Gstr2bLineID,
            Portal2BTaxable = line?.TaxableValue ?? 0m,
            Portal2BTax = line == null ? 0m : line.IGST + line.CGST + line.SGST + line.Cess,
            TaxVariance = variance,
            ItcAvailableIn2B = line?.ItcAvailable ?? false,
        };

        /// <summary>Supplier GSTIN + the document number stripped to letters and digits, upper-cased —
        /// so punctuation the two sides disagree on never splits one bill into two.</summary>
        private static string MatchKey(string gstin, string documentNo)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in documentNo ?? string.Empty)
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToUpperInvariant(c));
            return $"{(gstin ?? string.Empty).Trim().ToUpperInvariant()}|{sb}";
        }

        private static bool TryPeriod(string periodKey, out DateTime from, out DateTime to)
        {
            from = default; to = default;
            if (periodKey.Length != 6) return false;
            if (!int.TryParse(periodKey.Substring(0, 4), out int y)) return false;
            if (!int.TryParse(periodKey.Substring(4, 2), out int m) || m < 1 || m > 12) return false;
            from = new DateTime(y, m, 1);
            to = from.AddMonths(1);
            return true;
        }
    }
}
