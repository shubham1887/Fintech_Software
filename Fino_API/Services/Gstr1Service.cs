using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase6;
using FintechApp.Responses.Phase6;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IGstr1Service" />
    public class Gstr1Service : IGstr1Service
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = new();

        public Gstr1Service(ApplicationDbContext context) => _context = context;

        public async Task<(Gstr1Result? Result, string Failure)> Build(GstReturnRequest request)
        {
            string gstin = (request.CompanyGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            string period = (request.PeriodKey ?? string.Empty).Trim();
            if (period.Length != 6) return (null, "A period (yyyyMM) is required.");
            if (gstin.Length != 15) return (null, "The company GSTIN is required. Set it on Invoice Settings.");

            decimal threshold = request.B2CLThreshold > 0
                ? request.B2CLThreshold
                : GstReturnLimits.B2CLInvoiceValue;

            var result = new Gstr1Result
            {
                CompanyGSTIN = gstin,
                PeriodKey = period,
                PeriodLabel = PeriodLabel(period),
                B2CLThreshold = threshold,
            };

            // Only ISSUED documents are turnover. A draft is not a supply and a cancelled one is
            // not either — but a cancelled NUMBER still has to be declared in table 13, which is
            // why the series summary below reads a wider set than this.
            var headers = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted
                         && h.Direction == TaxDirection.Sale
                         && h.CompanyGSTIN == gstin
                         && h.PeriodKey == period
                         && h.Status == TaxDocumentStatus.Issued)
                .ToListAsync();

            if (headers.Count == 0)
            {
                result.Documents = await BuildDocumentSeries(gstin, period);
                return (result, string.Empty);
            }

            var headerIds = headers.Select(h => h.TaxInvoiceHeaderID).ToList();
            var lines = await _context.TaxInvoiceLines.AsNoTracking()
                .Where(l => !l.IsDeleted && headerIds.Contains(l.TaxInvoiceHeaderID))
                .ToListAsync();
            var linesByHeader = lines.GroupBy(l => l.TaxInvoiceHeaderID)
                                     .ToDictionary(g => g.Key, g => g.ToList());

            // Credit and debit notes point back at the document they adjust; the return has to
            // carry that original's number and date, so they are loaded in one go rather than per
            // note.
            var originalIds = headers.Where(h => h.OriginalInvoiceHeaderID.HasValue)
                                     .Select(h => h.OriginalInvoiceHeaderID!.Value).Distinct().ToList();
            var originals = originalIds.Count == 0
                ? new Dictionary<int, (string No, DateTime Date)>()
                : (await _context.TaxInvoiceHeaders.AsNoTracking()
                        .Where(h => originalIds.Contains(h.TaxInvoiceHeaderID))
                        .Select(h => new { h.TaxInvoiceHeaderID, h.DocumentNo, h.DocumentDate })
                        .ToListAsync())
                   .ToDictionary(x => x.TaxInvoiceHeaderID, x => (x.DocumentNo, x.DocumentDate));

            var b2b = new List<Gstr1Document>();
            var b2cl = new List<Gstr1Document>();
            var b2cs = new List<(TaxInvoiceHeader H, List<TaxInvoiceLine> L)>();
            var cdnr = new List<Gstr1Document>();
            var cdnur = new List<Gstr1Document>();
            var exempt = new List<(TaxInvoiceHeader H, List<TaxInvoiceLine> L)>();
            var partyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var h in headers)
            {
                var hl = linesByHeader.TryGetValue(h.TaxInvoiceHeaderID, out var got) ? got : new List<TaxInvoiceLine>();
                bool registered = IsRegistered(h);
                if (registered && !string.IsNullOrWhiteSpace(h.PartyGSTIN))
                    partyNames[h.PartyGSTIN.Trim()] = h.PartyName;

                // A bill of supply carries no tax: it is table 8, whoever it went to.
                if (h.DocumentType == TaxDocumentType.BillOfSupply)
                {
                    exempt.Add((h, hl));
                    continue;
                }

                // A self-invoice is OUR liability under reverse charge on somebody else's supply.
                // It is an inward transaction reported in 3B table 3.1(d), never an outward supply
                // in GSTR-1 — including it would declare our own purchase as our turnover.
                if (h.DocumentType == TaxDocumentType.SelfInvoice) continue;

                bool isNote = h.DocumentType is TaxDocumentType.CreditNote or TaxDocumentType.DebitNote;
                var doc = ToDocument(h, hl, originals);

                if (isNote)
                {
                    if (registered) cdnr.Add(doc);
                    else if (IsInterState(h) && h.GrandTotal > threshold) cdnur.Add(doc);
                    else
                    {
                        // A note against a B2CS supply is not reported as a note at all: it nets
                        // off the consolidated figures, which is what the negative lines below do.
                        b2cs.Add((h, hl));
                    }
                    continue;
                }

                if (registered)
                {
                    if (string.IsNullOrWhiteSpace(h.PartyGSTIN))
                    {
                        result.Warnings.Add(
                            $"{h.DocumentNo}: the party is marked registered but has no GSTIN, so it could not go in B2B. " +
                            "Set the GSTIN on the party's tax profile and rebuild.");
                        continue;
                    }
                    b2b.Add(doc);
                }
                else if (IsInterState(h) && h.GrandTotal > threshold)
                {
                    b2cl.Add(doc);
                }
                else
                {
                    b2cs.Add((h, hl));
                }
            }

            result.B2B = GroupByParty(b2b, headers, partyNames);
            result.B2CL = GroupByPlaceOfSupply(b2cl);
            result.CDNR = GroupByParty(cdnr, headers, partyNames);
            result.CDNUR = GroupByPlaceOfSupply(cdnur);
            result.B2CS = BuildB2cs(b2cs);
            result.Exempt = BuildExempt(exempt);
            result.HSN = BuildHsn(headers, linesByHeader);
            result.Documents = await BuildDocumentSeries(gstin, period);

            // A place of supply the portal does not recognise ("00", or blank) is the one error
            // that looks like a number on screen and is rejected outright on upload. It comes from
            // a party with no state on record, so it is a data fix rather than a code one — but
            // the return has to say so before someone tries to file it.
            var badPos = headers
                .Where(h => h.DocumentType != TaxDocumentType.SelfInvoice
                         && !GstStates.IsKnown(h.PlaceOfSupplyStateCode))
                .ToList();
            if (badPos.Count > 0)
            {
                var parties = badPos.Select(h => h.PartyName).Where(n => !string.IsNullOrWhiteSpace(n))
                                    .Distinct().Take(3).ToList();
                result.Warnings.Add(
                    $"{badPos.Count} document(s) have no valid place of supply (showing as " +
                    $"'{(badPos[0].PlaceOfSupplyStateCode ?? string.Empty).Trim()}'), so the portal will reject " +
                    "the upload. Set the state on each party's tax profile and regenerate the documents" +
                    (parties.Count > 0 ? $" — e.g. {string.Join(", ", parties)}." : "."));
            }

            // Totals across every table that carries tax. B2CS is included because it is turnover
            // just as much as B2B is — it simply has no document numbers.
            var taxed = b2b.Concat(b2cl).Concat(cdnr).Concat(cdnur).ToList();
            result.TotalTaxableValue = Round(
                taxed.Sum(d => d.Slabs.Sum(s => s.TaxableValue)) + result.B2CS.Sum(r => r.TaxableValue));
            result.TotalIGST = Round(taxed.Sum(d => d.Slabs.Sum(s => s.IGST)) + result.B2CS.Sum(r => r.IGST));
            result.TotalCGST = Round(taxed.Sum(d => d.Slabs.Sum(s => s.CGST)) + result.B2CS.Sum(r => r.CGST));
            result.TotalSGST = Round(taxed.Sum(d => d.Slabs.Sum(s => s.SGST)) + result.B2CS.Sum(r => r.SGST));
            result.TotalCess = Round(taxed.Sum(d => d.Slabs.Sum(s => s.Cess)) + result.B2CS.Sum(r => r.Cess));
            result.TotalDocuments = headers.Count;

            return (result, string.Empty);
        }

        // ── Table builders ────────────────────────────────────────────────────────────────

        /// <summary>
        /// B2B and CDNR are reported grouped by the counterparty's GSTIN, because the portal keys
        /// them that way and because the recipient reconciles their 2B against exactly that group.
        /// </summary>
        private static List<Gstr1Party> GroupByParty(
            List<Gstr1Document> docs, List<TaxInvoiceHeader> headers, Dictionary<string, string> names)
        {
            var gstinByHeader = headers.ToDictionary(h => h.TaxInvoiceHeaderID, h => (h.PartyGSTIN ?? string.Empty).Trim());

            return docs
                .GroupBy(d => gstinByHeader.TryGetValue(d.TaxInvoiceHeaderID, out var g) ? g : string.Empty)
                .Select(g => new Gstr1Party
                {
                    GSTIN = g.Key,
                    PartyName = names.TryGetValue(g.Key, out var n) ? n : string.Empty,
                    DocumentCount = g.Count(),
                    TaxableValue = Round(g.Sum(d => d.Slabs.Sum(s => s.TaxableValue))),
                    TotalTax = Round(g.Sum(d => d.Slabs.Sum(s => s.IGST + s.CGST + s.SGST + s.Cess))),
                    Documents = g.OrderBy(d => d.DocumentDate).ThenBy(d => d.DocumentNo).ToList(),
                })
                .OrderBy(p => p.GSTIN)
                .ToList();
        }

        /// <summary>B2CL and CDNUR have no counterparty GSTIN, so they group by place of supply —
        /// which is the dimension the portal reports them on.</summary>
        private static List<Gstr1Party> GroupByPlaceOfSupply(List<Gstr1Document> docs) =>
            docs.GroupBy(d => d.PlaceOfSupplyStateCode)
                .Select(g => new Gstr1Party
                {
                    GSTIN = g.Key,
                    PartyName = GstStates.Name(g.Key),
                    DocumentCount = g.Count(),
                    TaxableValue = Round(g.Sum(d => d.Slabs.Sum(s => s.TaxableValue))),
                    TotalTax = Round(g.Sum(d => d.Slabs.Sum(s => s.IGST + s.CGST + s.SGST + s.Cess))),
                    Documents = g.OrderBy(d => d.DocumentDate).ThenBy(d => d.DocumentNo).ToList(),
                })
                .OrderBy(p => p.GSTIN)
                .ToList();

        /// <summary>
        /// Table 7, consolidated by place of supply and rate. A credit note against a B2CS supply
        /// arrives here too and SUBTRACTS — the table is a net figure, and reporting the note
        /// separately would declare the original turnover twice.
        /// </summary>
        private static List<Gstr1B2csRow> BuildB2cs(List<(TaxInvoiceHeader H, List<TaxInvoiceLine> L)> items)
        {
            var acc = new Dictionary<(string Pos, decimal Rate, string Type), Gstr1B2csRow>();

            foreach (var (h, lines) in items)
            {
                int sign = h.DocumentType == TaxDocumentType.CreditNote ? -1 : 1;
                string pos = (h.PlaceOfSupplyStateCode ?? string.Empty).Trim();
                string type = IsInterState(h) ? "INTER" : "INTRA";

                // The keys THIS document contributed to, so each row counts the document once even
                // when the document has several lines at the same rate.
                var touched = new HashSet<(string, decimal, string)>();

                foreach (var l in lines)
                {
                    decimal rate = CombinedRate(l);
                    var key = (pos, rate, type);
                    if (!acc.TryGetValue(key, out var row))
                    {
                        acc[key] = row = new Gstr1B2csRow
                        {
                            PlaceOfSupplyStateCode = pos,
                            PlaceOfSupplyName = GstStates.Name(pos),
                            SupplyType = type,
                            Rate = rate,
                        };
                    }
                    row.TaxableValue += sign * l.TaxableValue;
                    row.IGST += sign * l.IGSTAmount;
                    row.CGST += sign * l.CGSTAmount;
                    row.SGST += sign * l.SGSTAmount;
                    row.Cess += sign * l.CessAmount;
                    touched.Add(key);
                }

                foreach (var k in touched) acc[k].DocumentCount++;
            }

            foreach (var r in acc.Values)
            {
                r.TaxableValue = Round(r.TaxableValue);
                r.IGST = Round(r.IGST); r.CGST = Round(r.CGST);
                r.SGST = Round(r.SGST); r.Cess = Round(r.Cess);
            }

            return acc.Values
                .OrderBy(r => r.PlaceOfSupplyStateCode).ThenBy(r => r.Rate)
                .ToList();
        }

        /// <summary>
        /// Table 8. The split between nil-rated, exempt and non-GST is the billing basis the engine
        /// resolved: EXEMPT means an exempt supply, NONGST means one outside GST entirely. A bill
        /// of supply whose basis says neither is reported as exempt, which is the conservative
        /// reading — it keeps the turnover in the return.
        /// </summary>
        private static List<Gstr1ExemptRow> BuildExempt(List<(TaxInvoiceHeader H, List<TaxInvoiceLine> L)> items)
        {
            var row = new Gstr1ExemptRow { Category = "Outward supplies (Bill of Supply)" };

            foreach (var (h, lines) in items)
            {
                int sign = h.DocumentType == TaxDocumentType.CreditNote ? -1 : 1;
                foreach (var l in lines)
                {
                    decimal v = sign * l.TaxableValue;
                    string basis = (l.BillingBasis ?? string.Empty).Trim().ToUpperInvariant();
                    if (basis == BillingBasis.NonGst) row.NonGST += v;
                    else if (CombinedRate(l) == 0m && basis != BillingBasis.Exempt) row.NilRated += v;
                    else row.Exempted += v;
                }
            }

            row.NilRated = Round(row.NilRated);
            row.Exempted = Round(row.Exempted);
            row.NonGST = Round(row.NonGST);

            return row.NilRated == 0m && row.Exempted == 0m && row.NonGST == 0m
                ? new List<Gstr1ExemptRow>()
                : new List<Gstr1ExemptRow> { row };
        }

        /// <summary>Table 12, by SAC and rate across every document — including bills of supply,
        /// whose turnover is reported here even though it carries no tax.</summary>
        private static List<Gstr1HsnRow> BuildHsn(
            List<TaxInvoiceHeader> headers, Dictionary<int, List<TaxInvoiceLine>> linesByHeader)
        {
            var signByHeader = headers.ToDictionary(
                h => h.TaxInvoiceHeaderID,
                h => h.DocumentType == TaxDocumentType.CreditNote ? -1 : 1);

            var acc = new Dictionary<(string Sac, decimal Rate), Gstr1HsnRow>();

            foreach (var h in headers)
            {
                // A self-invoice is inward; it is not part of outward HSN.
                if (h.DocumentType == TaxDocumentType.SelfInvoice) continue;
                if (!linesByHeader.TryGetValue(h.TaxInvoiceHeaderID, out var lines)) continue;
                int sign = signByHeader[h.TaxInvoiceHeaderID];

                foreach (var l in lines)
                {
                    string sac = (l.SACCode ?? string.Empty).Trim();
                    decimal rate = CombinedRate(l);
                    var key = (sac, rate);
                    if (!acc.TryGetValue(key, out var row))
                    {
                        acc[key] = row = new Gstr1HsnRow
                        {
                            SACCode = sac.Length > 0 ? sac : "(not set)",
                            Description = l.Description ?? string.Empty,
                            Rate = rate,
                        };
                    }
                    row.Quantity += sign * Math.Max(l.Quantity, 1);
                    row.TaxableValue += sign * l.TaxableValue;
                    row.IGST += sign * l.IGSTAmount;
                    row.CGST += sign * l.CGSTAmount;
                    row.SGST += sign * l.SGSTAmount;
                    row.Cess += sign * l.CessAmount;
                    row.TotalValue += sign * l.LineTotal;
                }
            }

            foreach (var r in acc.Values)
            {
                r.TaxableValue = Round(r.TaxableValue); r.TotalValue = Round(r.TotalValue);
                r.IGST = Round(r.IGST); r.CGST = Round(r.CGST);
                r.SGST = Round(r.SGST); r.Cess = Round(r.Cess);
            }

            return acc.Values.OrderBy(r => r.SACCode).ThenBy(r => r.Rate).ToList();
        }

        /// <summary>
        /// Table 13. This one deliberately reads a WIDER set than the rest of the return: it
        /// declares the numbers a series issued, so a cancelled document still has to be counted.
        /// A gap in the sequence that nobody declared is what makes "consecutive" impossible to
        /// demonstrate, which is the whole reason the table exists.
        /// </summary>
        private async Task<List<Gstr1DocSeriesRow>> BuildDocumentSeries(string gstin, string period)
        {
            var all = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted
                         && h.Direction == TaxDirection.Sale
                         && h.CompanyGSTIN == gstin
                         && h.PeriodKey == period
                         && h.Status != TaxDocumentStatus.Draft)
                .Select(h => new { h.DocumentType, h.DocumentNo, h.Status })
                .ToListAsync();

            return all
                .GroupBy(x => x.DocumentType)
                .Select(g =>
                {
                    // A cancelled document is kept as "DEL-<number>" (TaxInvoiceService.CancelDocument);
                    // table 13 declares the number the series issued, so the prefix comes off here.
                    var numbers = g.Select(x => x.DocumentNo ?? string.Empty)
                                   .Select(n => n.StartsWith("DEL-", StringComparison.OrdinalIgnoreCase) ? n[4..] : n)
                                   .Where(n => n.Length > 0)
                                   .OrderBy(n => n, StringComparer.Ordinal).ToList();
                    int cancelled = g.Count(x => x.Status == TaxDocumentStatus.Cancelled);
                    return new Gstr1DocSeriesRow
                    {
                        SeriesCode = TaxDocumentSeriesCode.For(g.Key),
                        DocumentType = g.Key,
                        FromNumber = numbers.FirstOrDefault() ?? string.Empty,
                        ToNumber = numbers.LastOrDefault() ?? string.Empty,
                        TotalIssued = g.Count(),
                        Cancelled = cancelled,
                        NetIssued = g.Count() - cancelled,
                    };
                })
                .OrderBy(r => r.SeriesCode)
                .ToList();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────────────

        private static Gstr1Document ToDocument(
            TaxInvoiceHeader h, List<TaxInvoiceLine> lines, Dictionary<int, (string No, DateTime Date)> originals)
        {
            // A credit note's figures go into the return as positive numbers in a table that is
            // itself a reduction — the portal does the subtracting, so negating here would reverse
            // it twice.
            var slabs = lines
                .GroupBy(CombinedRate)
                .Select(g => new RateSlab
                {
                    Rate = g.Key,
                    TaxableValue = Round(g.Sum(l => l.TaxableValue)),
                    IGST = Round(g.Sum(l => l.IGSTAmount)),
                    CGST = Round(g.Sum(l => l.CGSTAmount)),
                    SGST = Round(g.Sum(l => l.SGSTAmount)),
                    Cess = Round(g.Sum(l => l.CessAmount)),
                })
                .OrderBy(s => s.Rate)
                .ToList();

            var doc = new Gstr1Document
            {
                TaxInvoiceHeaderID = h.TaxInvoiceHeaderID,
                DocumentNo = h.DocumentNo,
                DocumentDate = h.DocumentDate,
                DocumentType = h.DocumentType,
                InvoiceValue = h.GrandTotal,
                PlaceOfSupplyStateCode = (h.PlaceOfSupplyStateCode ?? string.Empty).Trim(),
                SupplyNature = h.SupplyNature,
                ReverseCharge = lines.Any(l => l.ReverseCharge),
                NoteReason = h.NoteReason ?? string.Empty,
                Slabs = slabs,
            };

            if (h.OriginalInvoiceHeaderID.HasValue
                && originals.TryGetValue(h.OriginalInvoiceHeaderID.Value, out var orig))
            {
                doc.OriginalDocumentNo = orig.No;
                doc.OriginalDocumentDate = orig.Date;
            }

            return doc;
        }

        /// <summary>The rate as the return reports it: IGST, or CGST + SGST together. 18, never
        /// 9 and 9 — a slab is a rate, not a pair of half-rates.</summary>
        private static decimal CombinedRate(TaxInvoiceLine l) =>
            l.IGSTRate > 0 ? l.IGSTRate : l.CGSTRate + l.SGSTRate;

        /// <summary>
        /// Registered for the purposes of B2B. SEZ counts: a supply to an SEZ unit is reported
        /// invoice-wise like any other registered supply, under its own table heading.
        /// </summary>
        private static bool IsRegistered(TaxInvoiceHeader h)
        {
            string s = (h.PartyRegistrationStatus ?? string.Empty).Trim().ToUpperInvariant();
            return s is PartyRegistrationStatus.Registered or PartyRegistrationStatus.SEZ
                     or PartyRegistrationStatus.Composition;
        }

        private static bool IsInterState(TaxInvoiceHeader h) =>
            (h.SupplyNature ?? string.Empty).Trim().ToUpperInvariant() == SupplyNature.Interstate;

        private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        internal static string PeriodLabel(string periodKey)
        {
            if (periodKey.Length != 6
                || !int.TryParse(periodKey.AsSpan(0, 4), out int y)
                || !int.TryParse(periodKey.AsSpan(4, 2), out int m)
                || m < 1 || m > 12) return periodKey;
            return new DateTime(y, m, 1).ToString("MMM yyyy");
        }
    }
}
