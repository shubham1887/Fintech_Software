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
    /// <inheritdoc cref="IGstr2bImportService" />
    public class Gstr2bImportService : IGstr2bImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = new();

        public Gstr2bImportService(ApplicationDbContext context) => _context = context;

        public async Task<(Gstr2bImportResult? Result, string Failure)> Import(Gstr2bImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Json))
                return (null, "The GSTR-2B JSON is empty.");
            if (string.IsNullOrWhiteSpace(request.CompanyGSTIN))
                return (null, "The company GSTIN is required - a 2B belongs to one registration.");

            JObject root;
            try { root = JObject.Parse(request.Json); }
            catch (Exception ex) { _logger.Write(ex); return (null, "The GSTR-2B JSON could not be parsed."); }

            // The portal wraps everything under "data"; some exports omit it. Accept either.
            var data = (root["data"] as JObject) ?? root;
            var docdata = (data["docdata"] as JObject) ?? data;

            string periodKey = request.PeriodKey.Trim();
            if (string.IsNullOrWhiteSpace(periodKey))
            {
                // rtnprd is MMYYYY; our key is yyyyMM.
                string rtn = (string?)data["rtnprd"] ?? string.Empty;
                if (rtn.Length == 6) periodKey = rtn.Substring(2, 4) + rtn.Substring(0, 2);
            }
            if (periodKey.Length != 6)
                return (null, "A return period (yyyyMM) is required and could not be read from the JSON (rtnprd).");

            DateTime? gendt = TryDate((string?)data["gendt"]);
            var warnings = new List<string>();
            var lines = new List<Gstr2bLine>();

            // B2B invoices.
            ParseSection(docdata["b2b"] as JArray, "inv", TaxDocumentType.TaxInvoice, lines, warnings);
            // Credit / debit notes. Their document type comes from the note's own "ntty" (C/D).
            ParseSection(docdata["cdnr"] as JArray, "nt", null, lines, warnings);

            if (lines.Count == 0)
                return (null, "No B2B invoices or notes were found in the JSON. Check it is a GSTR-2B download.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                string gstin = request.CompanyGSTIN.Trim().ToUpperInvariant();

                // Supersede any existing active snapshot for this registration + period. The unique
                // filtered index allows only one Active at a time, so this has to happen first.
                var prior = await _context.Gstr2bImports
                    .Where(i => i.CompanyGSTIN == gstin && i.PeriodKey == periodKey
                             && i.Status == Gstr2bImportStatus.Active && !i.IsDeleted)
                    .ToListAsync();
                bool superseded = prior.Count > 0;
                foreach (var p in prior) { p.Status = Gstr2bImportStatus.Superseded; p.UpdatedateTime = DateTime.UtcNow; }
                if (superseded) await _context.SaveChangesAsync();

                var import = new Gstr2bImport
                {
                    CompanyGSTIN = gstin,
                    PeriodKey = periodKey,
                    GeneratedDate = gendt,
                    Source = "JSON",
                    LineCount = lines.Count,
                    TotalTaxable = lines.Sum(l => l.TaxableValue),
                    TotalTax = lines.Sum(l => l.IGST + l.CGST + l.SGST + l.Cess),
                    Status = Gstr2bImportStatus.Active,
                    ImportedByUserID = request.ImportedByUserID,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                };
                _context.Gstr2bImports.Add(import);
                await _context.SaveChangesAsync();

                // De-duplicate within the file on (supplier, number, type) so a doubled download
                // line does not trip the unique index and abort the whole import.
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var l in lines)
                {
                    string key = $"{l.SupplierGSTIN}|{l.DocumentNo}|{l.DocumentType}";
                    if (!seen.Add(key)) { warnings.Add($"Duplicate in file skipped: {l.SupplierGSTIN} {l.DocumentNo}"); continue; }
                    l.Gstr2bImportID = import.Gstr2bImportID;
                    l.MatchStatus = "Unmatched";
                    l.DateTime = DateTime.UtcNow;
                    l.UpdatedateTime = DateTime.UtcNow;
                    _context.Gstr2bLines.Add(l);
                }
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return (new Gstr2bImportResult
                {
                    Gstr2bImportID = import.Gstr2bImportID,
                    PeriodKey = periodKey,
                    LinesImported = seen.Count,
                    TotalTaxable = import.TotalTaxable,
                    TotalTax = import.TotalTax,
                    SupersededPrevious = superseded,
                    Warnings = warnings,
                }, string.Empty);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.Write(ex);
                return (null, "The 2B import failed and was rolled back; nothing was saved.");
            }
        }

        public async Task<List<Gstr2bImport>> GetImports(string companyGstin)
        {
            string g = companyGstin.Trim().ToUpperInvariant();
            return await _context.Gstr2bImports.AsNoTracking()
                .Where(i => i.CompanyGSTIN == g && !i.IsDeleted)
                .OrderByDescending(i => i.Gstr2bImportID).ToListAsync();
        }

        public async Task<List<Gstr2bLine>> GetActiveLines(string companyGstin, string periodKey)
        {
            string g = companyGstin.Trim().ToUpperInvariant();
            var active = await _context.Gstr2bImports.AsNoTracking()
                .Where(i => i.CompanyGSTIN == g && i.PeriodKey == periodKey
                         && i.Status == Gstr2bImportStatus.Active && !i.IsDeleted)
                .Select(i => i.Gstr2bImportID).FirstOrDefaultAsync();
            if (active == 0) return new List<Gstr2bLine>();
            return await _context.Gstr2bLines.AsNoTracking()
                .Where(l => l.Gstr2bImportID == active && !l.IsDeleted)
                .OrderBy(l => l.SupplierGSTIN).ThenBy(l => l.DocumentNo).ToListAsync();
        }

        /// <summary>Reads one portal section (b2b or cdnr) into our line rows. Each entry groups a
        /// supplier (ctin / trdnm) with an array of documents under <paramref name="docArrayKey"/>.</summary>
        private static void ParseSection(JArray? section, string docArrayKey, string? fixedDocType,
                                         List<Gstr2bLine> into, List<string> warnings)
        {
            if (section == null) return;
            foreach (var party in section)
            {
                string ctin = ((string?)party["ctin"] ?? string.Empty).Trim().ToUpperInvariant();
                string trdnm = (string?)party["trdnm"] ?? string.Empty;
                if (party[docArrayKey] is not JArray docs) continue;

                foreach (var d in docs)
                {
                    // A credit/debit note carries its own kind in "ntty" (C = credit, D = debit).
                    string docType = fixedDocType ?? ((string?)d["ntty"] == "D" ? TaxDocumentType.DebitNote : TaxDocumentType.CreditNote);
                    string number = (string?)d["inum"] ?? (string?)d["nt_num"] ?? (string?)d["num"] ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(number)) { warnings.Add($"A {ctin} document had no number and was skipped."); continue; }

                    into.Add(new Gstr2bLine
                    {
                        SupplierGSTIN = ctin,
                        SupplierName = trdnm,
                        DocumentType = docType,
                        DocumentNo = number.Trim(),
                        DocumentDate = TryDate((string?)d["dt"] ?? (string?)d["nt_dt"]) ?? DateTime.Today,
                        PlaceOfSupplyStateCode = ((string?)d["pos"] ?? string.Empty).Trim(),
                        ReverseCharge = string.Equals((string?)d["rev"], "Y", StringComparison.OrdinalIgnoreCase),
                        TaxableValue = Amount(d, "txval"),
                        IGST = Amount(d, "igst", "iamt"),
                        CGST = Amount(d, "cgst", "camt"),
                        SGST = Amount(d, "sgst", "samt"),
                        Cess = Amount(d, "cess", "csamt"),
                        ItcAvailable = !string.Equals((string?)d["itcavl"], "N", StringComparison.OrdinalIgnoreCase),
                        ItcUnavailableReason = (string?)d["rsn"] ?? string.Empty,
                    });
                }
            }
        }

        /// <summary>
        /// One money figure of a 2B document, wherever the file keeps it.
        ///
        /// The portal has written this three ways. A GSTR-2B download names the heads
        /// <c>igst / cgst / sgst / cess</c> and puts them either on the document or, when the
        /// download carries item details, on each row of <c>items</c>. Files produced by older
        /// tooling use the GSTR-2A names — <c>iamt / camt / samt / csamt</c> — nested under
        /// <c>itms[].itm_det</c>. Reading only one of those gives every document a tax of zero,
        /// which then "mismatches" every bill in the books and no credit can ever be claimed —
        /// silently, because zero is a valid number. So: the document's own figure under either
        /// name if it has one, otherwise the sum over its item rows.
        /// </summary>
        private static decimal Amount(JToken doc, params string[] names)
        {
            foreach (string n in names)
                if (doc[n] != null && doc[n]!.Type != JTokenType.Null) return (decimal?)doc[n] ?? 0m;

            decimal sum = 0m;
            var rows = (doc["items"] as JArray) ?? (doc["itms"] as JArray);
            if (rows == null) return 0m;
            foreach (var row in rows)
            {
                var src = (row["itm_det"] as JObject) ?? row;
                foreach (string n in names)
                    if (src[n] != null && src[n]!.Type != JTokenType.Null) { sum += (decimal?)src[n] ?? 0m; break; }
            }
            return sum;
        }

        private static DateTime? TryDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            foreach (var fmt in new[] { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd" })
                if (DateTime.TryParseExact(s, fmt, null, System.Globalization.DateTimeStyles.None, out var d)) return d;
            return DateTime.TryParse(s, out var g) ? g : null;
        }
    }
}
