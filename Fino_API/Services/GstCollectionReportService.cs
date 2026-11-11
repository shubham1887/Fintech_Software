using FintechApp.Data;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Gst;
using FintechApp.Responses.Gst;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IGstCollectionReportService" />
    ///
    /// Two stamps carry GST on a transaction and they answer different questions, which is why
    /// this report shows both instead of picking one:
    ///
    ///   TransactionTaxDetails — the OUTPUT TAX on the sale, resolved by the tax engine from the
    ///     service's billing basis. This is what GSTR-1 is built from.
    ///   TransactionMasters    — the CHARGE breakdown: the GST inside each charge and commission,
    ///     with the inclusive / exclusive decision that produced it. This is the question the desk
    ///     asks and the one no report could answer before.
    ///
    /// They should agree, and where they do not the report says so with the figure and the reason.
    /// Netting them, or showing only the larger, is how a return gets filed short.
    public class GstCollectionReportService : IGstCollectionReportService
    {
        private readonly ApplicationDbContext _context;

        public GstCollectionReportService(ApplicationDbContext context) => _context = context;

        public async Task<GstCollectionReport> Build(GstCollectionReportRequest request)
        {
            DateTime from = (request.FromDate ?? DateTime.Today.AddDays(1 - DateTime.Today.Day)).Date;
            DateTime to = (request.ToDate ?? DateTime.Today).Date;
            if (to < from) (from, to) = (to, from);
            DateTime toExclusive = to.AddDays(1);

            var report = new GstCollectionReport
            {
                FromDate = from,
                ToDate = to,
                PeriodLabel = from.Month == to.Month && from.Year == to.Year && from.Day == 1
                    ? from.ToString("MMMM yyyy")
                    : $"{from:dd MMM yyyy} to {to:dd MMM yyyy}",
            };

            // Only successful transactions carry GST. A failed one was refunded in full, charges
            // and all, so it owes nothing and must not appear in a tax total.
            var txns = await _context.TransactionMasters.AsNoTracking()
                .Where(t => !t.IsDeleted
                         && t.Status == StatusType.Success
                         && t.DateTime >= from && t.DateTime < toExclusive
                         && (request.ServiceID <= 0 || t.ServiceID == request.ServiceID))
                .Select(t => new TxnSlice
                {
                    ID = t.TransactionMasterID,
                    ServiceID = t.ServiceID,
                    Amount = t.Amount,
                    Stamped = t.ChargeGSTStampedOn != null,
                    RetailerGSTType = t.RetailerGSTType,
                    ProviderGSTType = t.ProviderGSTType,
                    RetailerGST = t.RetailerGST,
                    RetailerBase = t.RetailerBaseCharge,
                    DistributerGST = t.DistributerGST,
                    DistributerBase = t.DistributerBaseCharge,
                    SuperDistributerGST = t.SuperDistributerGST,
                    SuperDistributerBase = t.SuperDistributerBaseCharge,
                    ProviderGST = t.ProviderGSTAmount,
                    ProviderBase = t.ProviderBaseCharge,
                })
                .ToListAsync();

            report.SuccessCount = txns.Count;
            report.ChargeStampedCount = txns.Count(t => t.Stamped);
            report.ChargeUnstampedCount = txns.Count - report.ChargeStampedCount;

            if (txns.Count == 0)
            {
                report.Summary = $"No successful transaction in {report.PeriodLabel}.";
                return report;
            }

            var ids = txns.Select(t => t.ID).ToList();

            var taxes = await _context.TransactionTaxDetails.AsNoTracking()
                .Where(d => !d.IsDeleted && ids.Contains(d.TransactionMasterID)
                         && d.Direction == TaxDirection.Sale)
                .Select(d => new TaxSlice
                {
                    TxnID = d.TransactionMasterID,
                    BillingBasis = d.BillingBasis,
                    SupplyNature = d.SupplyNature,
                    SACCode = d.SACCode,
                    GrossValue = d.GrossValue,
                    TaxableValue = d.TaxableValue,
                    CGST = d.CGSTAmount,
                    SGST = d.SGSTAmount,
                    IGST = d.IGSTAmount,
                    Cess = d.CessAmount,
                })
                .ToListAsync();

            report.TaxStampedCount = taxes.Select(t => t.TxnID).Distinct().Count();
            report.TaxUnstampedCount = txns.Count - report.TaxStampedCount;

            var serviceNames = await _context.ServiceMasters.AsNoTracking()
                .ToDictionaryAsync(s => s.ServiceID, s => s.Name ?? string.Empty);

            // ── 1. The question asked: inclusive or exclusive, how much? ────────────────────
            //
            // The SIGN decides the side. A negative charge is money the user paid us (a surcharge),
            // so the GST inside it is ours to hand over. A positive one is money we paid out (a
            // commission), so the GST inside it is ours to claim.
            var buckets = new Dictionary<(string Treatment, string Kind, string Source), GstTreatmentRow>();

            void Add(string treatment, string kind, string source, decimal baseValue, decimal gst, int count = 1)
            {
                if (gst == 0m && baseValue == 0m) return;
                var key = (Treatment: Norm(treatment), Kind: kind, Source: source);
                if (!buckets.TryGetValue(key, out var row))
                {
                    row = new GstTreatmentRow { Treatment = key.Treatment, Kind = kind, Source = source };
                    buckets[key] = row;
                }
                row.BaseValue += Math.Abs(baseValue);
                row.GSTAmount += Math.Abs(gst);
                row.TransactionCount += count;
            }

            foreach (var t in txns)
            {
                string retl = t.Stamped ? t.RetailerGSTType : NotStamped;
                string prov = t.Stamped ? t.ProviderGSTType : NotStamped;

                if (t.RetailerGST < 0m)
                    Add(retl, Output, "Charges collected from users", t.RetailerBase, t.RetailerGST);
                else if (t.RetailerGST > 0m)
                    Add(retl, Input, "Commission paid to retailers", t.RetailerBase, t.RetailerGST);

                decimal uplineGst = t.DistributerGST + t.SuperDistributerGST;
                decimal uplineBase = t.DistributerBase + t.SuperDistributerBase;
                if (uplineGst > 0m)
                    Add(retl, Input, "Commission paid to the upline", uplineBase, uplineGst);
                else if (uplineGst < 0m)
                    Add(retl, Output, "Charges collected from the upline", uplineBase, uplineGst);

                if (t.ProviderGST < 0m)
                    Add(prov, Input, "Charges paid to providers", t.ProviderBase, t.ProviderGST);
                else if (t.ProviderGST > 0m)
                    Add(prov, Output, "Commission earned from providers", t.ProviderBase, t.ProviderGST);
            }

            report.ByTreatment = buckets.Values
                .OrderBy(r => r.Kind == Output ? 0 : 1)
                .ThenByDescending(r => r.GSTAmount)
                .ToList();

            report.GSTCollectedOnCharges = R2(report.ByTreatment
                .Where(r => r.Kind == Output && r.Source.StartsWith("Charges collected", StringComparison.Ordinal))
                .Sum(r => r.GSTAmount));
            report.GSTOnProviderCommission = R2(report.ByTreatment
                .Where(r => r.Kind == Output && r.Source.StartsWith("Commission earned", StringComparison.Ordinal))
                .Sum(r => r.GSTAmount));
            report.InputCreditOnProviderCharges = R2(report.ByTreatment
                .Where(r => r.Kind == Input && r.Source.StartsWith("Charges paid", StringComparison.Ordinal))
                .Sum(r => r.GSTAmount));
            report.InputCreditOnDownlineCommission = R2(report.ByTreatment
                .Where(r => r.Kind == Input && r.Source.StartsWith("Commission paid", StringComparison.Ordinal))
                .Sum(r => r.GSTAmount));

            // ── 2. Output tax as the returns see it ─────────────────────────────────────────
            report.ByBasis = taxes
                .GroupBy(t => (Basis: Up(t.BillingBasis), Nature: Up(t.SupplyNature)))
                .Select(g => new GstBasisRow
                {
                    BillingBasis = g.Key.Basis.Length == 0 ? "(unresolved)" : g.Key.Basis,
                    SupplyNature = g.Key.Nature.Length == 0 ? "(unresolved)" : g.Key.Nature,
                    GrossValue = R2(g.Sum(x => x.GrossValue)),
                    TaxableValue = R2(g.Sum(x => x.TaxableValue)),
                    CGSTAmount = R2(g.Sum(x => x.CGST)),
                    SGSTAmount = R2(g.Sum(x => x.SGST)),
                    IGSTAmount = R2(g.Sum(x => x.IGST)),
                    CessAmount = R2(g.Sum(x => x.Cess)),
                    TotalTax = R2(g.Sum(x => x.CGST + x.SGST + x.IGST + x.Cess)),
                    TransactionCount = g.Select(x => x.TxnID).Distinct().Count(),
                })
                .OrderByDescending(r => r.TotalTax)
                .ThenBy(r => r.BillingBasis)
                .ToList();

            report.OutputTaxStamped = R2(report.ByBasis.Sum(r => r.TotalTax));

            // ── 3. Per service ──────────────────────────────────────────────────────────────
            var taxByTxn = taxes.GroupBy(t => t.TxnID)
                .ToDictionary(g => g.Key, g => g.ToList());

            report.ByService = txns.GroupBy(t => t.ServiceID).Select(g =>
            {
                var rows = g.SelectMany(t => taxByTxn.TryGetValue(t.ID, out var list) ? list : new List<TaxSlice>()).ToList();
                return new GstServiceRow
                {
                    ServiceID = g.Key,
                    ServiceName = serviceNames.TryGetValue(g.Key, out var n) && n.Length > 0 ? n : $"Service {g.Key}",
                    BillingBasis = rows.Select(r => Up(r.BillingBasis)).Where(b => b.Length > 0).Distinct().FirstOrDefault() ?? "(not stamped)",
                    SACCode = rows.Select(r => r.SACCode).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? "(not set)",
                    TransactionCount = g.Count(),
                    Turnover = R2(g.Sum(t => t.Amount)),
                    OutputTaxStamped = R2(rows.Sum(r => r.CGST + r.SGST + r.IGST + r.Cess)),
                    GSTOnChargesCollected = R2(g.Sum(t => (t.RetailerGST < 0m ? Math.Abs(t.RetailerGST) : 0m)
                                                        + (t.ProviderGST > 0m ? t.ProviderGST : 0m))),
                    GSTOnAmountsPaid = R2(g.Sum(t => (t.RetailerGST > 0m ? t.RetailerGST : 0m)
                                                   + (t.ProviderGST < 0m ? Math.Abs(t.ProviderGST) : 0m)
                                                   + Math.Max(0m, t.DistributerGST + t.SuperDistributerGST))),
                    Treatments = g.Where(t => t.Stamped)
                                  .Select(t => Norm(t.RetailerGSTType))
                                  .Where(s => s.Length > 0).Distinct().OrderBy(s => s).ToList(),
                };
            }).OrderByDescending(r => r.OutputTaxStamped + r.GSTOnChargesCollected).ToList();

            // ── 4. Totals, and every difference named ───────────────────────────────────────
            report.TotalOutput = R2(report.OutputTaxStamped + report.GSTOnProviderCommission);
            report.TotalInputCredit = R2(report.InputCreditOnProviderCharges + report.InputCreditOnDownlineCommission);
            report.NetPayable = R2(report.TotalOutput - report.TotalInputCredit);

            // The one that matters: GST we took from users on charges, against the output tax
            // actually stamped for those same transactions. A charge taxed on the card but ignored
            // by the service's billing basis is revenue collected and not declared.
            foreach (var svc in report.ByService.Where(s => s.GSTOnChargesCollected > 0.01m))
            {
                decimal gap = R2(svc.GSTOnChargesCollected - svc.OutputTaxStamped);
                if (gap <= 0.05m) continue;

                // A charge that was taxed on the card but carries a ZERO-tax stamp is a different
                // problem from one the basis never taxed, and it has a different answer: the stamp
                // is write-once (a unique index on the transaction), so it cannot be re-resolved.
                // Telling the desk to "check the tax rule" when the rule is already right sends
                // them to look at the one thing that is not wrong.
                var zeroStamped = txns
                    .Where(t => t.ServiceID == svc.ServiceID
                             && Math.Abs(t.RetailerGST) > 0m
                             && taxByTxn.TryGetValue(t.ID, out var rows)
                             && rows.Sum(r => r.CGST + r.SGST + r.IGST + r.Cess) == 0m)
                    .ToList();

                decimal zeroStampedGst = R2(zeroStamped.Sum(t => Math.Abs(t.RetailerGST)));

                string detail;
                if (svc.BillingBasis == BillingBasis.Commission)
                {
                    detail = $"The charges on {svc.ServiceName} carry {Money(svc.GSTOnChargesCollected)} of GST, but its tax rule bills on "
                           + "COMMISSION and the engine taxes only a commission under that basis — a charge is ignored. On this install "
                           + "the user pays a charge here rather than earning a commission, so the rule's billing basis should be CHARGE. "
                           + "Change it on Tax Rules (it is effective-dated, so earlier periods keep what they were stamped with). "
                           + "Until then this amount is collected and not declared.";
                }
                else if (zeroStamped.Count > 0 && zeroStampedGst >= gap - 0.05m)
                {
                    detail = $"{zeroStamped.Count} transaction(s) carrying {Money(zeroStampedGst)} of GST on their charges were stamped "
                           + "with ZERO tax, under a billing basis that has since been corrected. A tax stamp is written once and cannot "
                           + "be replaced — the database enforces it, so that a settled transaction never changes what it says about its "
                           + "own tax. These therefore keep a zero stamp: the amount is collected, it is not in the output tax, and it "
                           + "needs either a correction entry or a disclosure. Everything settling now is stamped correctly.";
                }
                else
                {
                    detail = $"The charges carry {Money(svc.GSTOnChargesCollected)} of GST while the sale stamp shows "
                           + $"{Money(svc.OutputTaxStamped)}. Check the service's tax rule and its place of supply.";
                }

                report.Notes.Add(new GstReconNote
                {
                    Level = "WARNING",
                    Title = $"{svc.ServiceName}: {Money(gap)} of GST was collected on charges but is not in the output tax",
                    Amount = gap,
                    Detail = detail,
                });
            }

            if (report.ChargeUnstampedCount > 0)
            {
                report.Notes.Add(new GstReconNote
                {
                    Level = "INFO",
                    Title = $"{report.ChargeUnstampedCount} transaction(s) carry no charge breakdown",
                    Amount = 0m,
                    Detail = "They settled before the per-charge GST breakdown existed, so their inclusive / exclusive split is "
                           + "unknown and they are shown as NOT STAMPED above. Their output tax, where it was stamped, is still "
                           + "counted. Run the charge-GST backfill to fill them in.",
                });
            }

            if (report.TaxUnstampedCount > 0)
            {
                report.Notes.Add(new GstReconNote
                {
                    Level = "WARNING",
                    Title = $"{report.TaxUnstampedCount} successful transaction(s) have no output-tax stamp at all",
                    Amount = 0m,
                    Detail = "Nothing was resolved for them, so they contribute nothing to the figures above and would be "
                           + "missing from a return built on this period. They are usually transactions settled before the tax "
                           + "engine shipped.",
                });
            }

            report.Summary =
                $"{report.PeriodLabel}: output {Money(report.TotalOutput)} "
              + $"(sale tax {Money(report.OutputTaxStamped)} + provider commission {Money(report.GSTOnProviderCommission)}), "
              + $"input credit {Money(report.TotalInputCredit)}, net {Money(report.NetPayable)}. "
              + $"{report.ChargeStampedCount} of {report.SuccessCount} transactions carry a charge breakdown.";

            return report;
        }

        // ── Plumbing ────────────────────────────────────────────────────────────────────────

        private const string Output = "OUTPUT";
        private const string Input = "INPUT";
        private const string NotStamped = "NOT STAMPED";

        private sealed class TxnSlice
        {
            public int ID { get; set; }
            public int ServiceID { get; set; }
            public decimal Amount { get; set; }
            public bool Stamped { get; set; }
            public string RetailerGSTType { get; set; } = string.Empty;
            public string ProviderGSTType { get; set; } = string.Empty;
            public decimal RetailerGST { get; set; }
            public decimal RetailerBase { get; set; }
            public decimal DistributerGST { get; set; }
            public decimal DistributerBase { get; set; }
            public decimal SuperDistributerGST { get; set; }
            public decimal SuperDistributerBase { get; set; }
            public decimal ProviderGST { get; set; }
            public decimal ProviderBase { get; set; }
        }

        private sealed class TaxSlice
        {
            public int TxnID { get; set; }
            public string BillingBasis { get; set; } = string.Empty;
            public string SupplyNature { get; set; } = string.Empty;
            public string SACCode { get; set; } = string.Empty;
            public decimal GrossValue { get; set; }
            public decimal TaxableValue { get; set; }
            public decimal CGST { get; set; }
            public decimal SGST { get; set; }
            public decimal IGST { get; set; }
            public decimal Cess { get; set; }
        }

        /// <summary>A blank treatment on a stamped row means the resolver could not decide, which
        /// is a different thing from NONE and is shown as such.</summary>
        private static string Norm(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant();
            return v.Length == 0 ? "(unresolved)" : v;
        }

        private static string Up(string? v) => (v ?? string.Empty).Trim().ToUpperInvariant();
        private static string Money(decimal v) => $"Rs {v:0.00}";
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
