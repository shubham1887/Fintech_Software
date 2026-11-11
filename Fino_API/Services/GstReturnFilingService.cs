using System.Text;
using ClosedXML.Excel;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase6;
using FintechApp.Responses.Phase6;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IGstReturnFilingService" />
    public class GstReturnFilingService : IGstReturnFilingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGstr1Service _gstr1;
        private readonly IGstr3bService _gstr3b;
        private readonly IGstr9Service _gstr9;
        private readonly Logger _logger = new();

        public GstReturnFilingService(
            ApplicationDbContext context, IGstr1Service gstr1, IGstr3bService gstr3b, IGstr9Service gstr9)
        {
            _context = context;
            _gstr1 = gstr1;
            _gstr3b = gstr3b;
            _gstr9 = gstr9;
        }

        // ── Save / file ───────────────────────────────────────────────────────────────────

        public async Task<(GstReturnFilingRow? Row, string Failure)> Save(SaveGstReturnRequest request)
        {
            string type = (request.ReturnType ?? string.Empty).Trim().ToUpperInvariant();
            if (!GstReturnType.All.Contains(type)) return (null, "Unknown return type.");

            string gstin = (request.CompanyGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            if (gstin.Length != 15) return (null, "The company GSTIN is required.");

            var (payload, totals, periodKey, failure) = await BuildPayload(type, gstin, request.PeriodKey,
                                                                          request.FinancialYearStart, request.B2CLThreshold);
            if (payload == null) return (null, failure);

            // A regeneration SUPERSEDES rather than overwrites: the previous snapshot is evidence
            // of what was declared, and a rate or rule that changed since would otherwise rewrite
            // history silently.
            var live = await _context.GstReturnFilings
                .Where(f => !f.IsDeleted && f.ReturnType == type && f.PeriodKey == periodKey
                         && f.CompanyGSTIN == gstin && f.Status != GstFilingStatus.Superseded)
                .ToListAsync();
            foreach (var old in live)
            {
                old.Status = GstFilingStatus.Superseded;
                old.UpdatedateTime = DateTime.UtcNow;
            }

            var entity = new GstReturnFiling
            {
                ReturnType = type,
                PeriodKey = periodKey,
                CompanyGSTIN = gstin,
                TotalTaxableValue = totals.Taxable,
                TotalIGST = totals.Igst,
                TotalCGST = totals.Cgst,
                TotalSGST = totals.Sgst,
                TotalCess = totals.Cess,
                DocumentCount = totals.Documents,
                PayloadJson = payload.ToString(Formatting.None),
                Status = request.MarkFiled ? GstFilingStatus.Filed : GstFilingStatus.Draft,
                FiledOn = request.MarkFiled ? DateTime.UtcNow : null,
                AcknowledgementNo = Trim(request.AcknowledgementNo, 50),
                Note = Trim(request.Note, 300),
                CreatedByUserID = request.CreatedByUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false,
            };
            _context.GstReturnFilings.Add(entity);
            await _context.SaveChangesAsync();

            return (ToRow(entity), string.Empty);
        }

        public async Task<List<GstReturnFilingRow>> GetFilings(GstReturnFilingQuery query)
        {
            string gstin = (query.CompanyGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            var q = _context.GstReturnFilings.AsNoTracking().Where(f => !f.IsDeleted);

            if (gstin.Length == 15) q = q.Where(f => f.CompanyGSTIN == gstin);
            if (!string.IsNullOrWhiteSpace(query.ReturnType))
                q = q.Where(f => f.ReturnType == query.ReturnType.Trim().ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(query.PeriodKey))
                q = q.Where(f => f.PeriodKey == query.PeriodKey.Trim());

            var rows = await q.OrderByDescending(f => f.PeriodKey)
                              .ThenByDescending(f => f.GstReturnFilingID)
                              .Take(300).ToListAsync();
            return rows.Select(ToRow).ToList();
        }

        // ── Export ────────────────────────────────────────────────────────────────────────

        public async Task<(GstReturnExport? Export, string Failure)> Export(GstReturnExportRequest request)
        {
            string type = (request.ReturnType ?? string.Empty).Trim().ToUpperInvariant();
            if (!GstReturnType.All.Contains(type)) return (null, "Unknown return type.");

            string gstin = (request.CompanyGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            if (gstin.Length != 15) return (null, "The company GSTIN is required.");

            string format = (request.Format ?? "JSON").Trim().ToUpperInvariant();

            var (payload, _, periodKey, failure) = await BuildPayload(type, gstin, request.PeriodKey,
                                                                     request.FinancialYearStart, request.B2CLThreshold);
            if (payload == null) return (null, failure);

            if (format == "JSON")
            {
                byte[] bytes = Encoding.UTF8.GetBytes(payload.ToString(Formatting.Indented));
                return (new GstReturnExport
                {
                    FileName = $"{type}_{gstin}_{periodKey}.json",
                    ContentType = "application/json",
                    Base64 = Convert.ToBase64String(bytes),
                    ByteCount = bytes.Length,
                }, string.Empty);
            }

            if (format == "EXCEL")
            {
                byte[] bytes = BuildWorkbook(type, payload);
                return (new GstReturnExport
                {
                    FileName = $"{type}_{gstin}_{periodKey}.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Base64 = Convert.ToBase64String(bytes),
                    ByteCount = bytes.Length,
                }, string.Empty);
            }

            return (null, "Format must be JSON or EXCEL.");
        }

        // ── Payload construction ──────────────────────────────────────────────────────────

        private sealed record Totals(decimal Taxable, decimal Igst, decimal Cgst, decimal Sgst, decimal Cess, int Documents);

        private async Task<(JObject? Payload, Totals Totals, string PeriodKey, string Failure)> BuildPayload(
            string type, string gstin, string? periodKey, int fyStart, decimal b2clThreshold)
        {
            var empty = new Totals(0, 0, 0, 0, 0, 0);

            if (type == GstReturnType.Gstr1)
            {
                var (r, f) = await _gstr1.Build(new GstReturnRequest
                { CompanyGSTIN = gstin, PeriodKey = periodKey ?? string.Empty, B2CLThreshold = b2clThreshold });
                if (r == null) return (null, empty, string.Empty, f);
                return (Gstr1Json(r), new Totals(r.TotalTaxableValue, r.TotalIGST, r.TotalCGST, r.TotalSGST,
                                                 r.TotalCess, r.TotalDocuments), r.PeriodKey, string.Empty);
            }

            if (type == GstReturnType.Gstr3b)
            {
                var (r, f) = await _gstr3b.Build(new GstReturnRequest
                { CompanyGSTIN = gstin, PeriodKey = periodKey ?? string.Empty, B2CLThreshold = b2clThreshold });
                if (r == null) return (null, empty, string.Empty, f);
                return (Gstr3bJson(r), new Totals(
                    r.Outward.FirstOrDefault(o => o.Code == "a")?.TaxableValue ?? 0m,
                    r.OutputIGST, r.OutputCGST, r.OutputSGST, r.OutputCess, 0), r.PeriodKey, string.Empty);
            }

            var (g9, g9Fail) = await _gstr9.Build(new Gstr9Request { CompanyGSTIN = gstin, FinancialYearStart = fyStart });
            if (g9 == null) return (null, empty, string.Empty, g9Fail);
            // GSTR-9 has no month, so its period key is the FY start year followed by 0000 — one
            // column that still sorts and groups with the monthly returns.
            return (Gstr9Json(g9), new Totals(g9.TotalTurnover,
                                              g9.OutwardByRate.Sum(s => s.IGST), g9.OutwardByRate.Sum(s => s.CGST),
                                              g9.OutwardByRate.Sum(s => s.SGST), g9.OutwardByRate.Sum(s => s.Cess),
                                              g9.Months.Count),
                    $"{g9.FinancialYearStart}0000", string.Empty);
        }

        /// <summary>
        /// GSTR-1 in the portal's offline-utility shape: `gstin`, `fp` (MMyyyy, which is the
        /// portal's order, not ours), then one array per table with the portal's own field names.
        /// Those names are terse on purpose — they are what the utility reads, and renaming them
        /// to something legible would make the file useless for the only job it has.
        /// </summary>
        private static JObject Gstr1Json(Gstr1Result r)
        {
            var root = new JObject
            {
                ["gstin"] = r.CompanyGSTIN,
                ["fp"] = ToFp(r.PeriodKey),
                ["gt"] = r.TotalTaxableValue,
                ["cur_gt"] = r.TotalTaxableValue,
            };

            JArray Items(Gstr1Document d, bool note) =>
                new(d.Slabs.Select((s, i) => new JObject
                {
                    ["num"] = i + 1,
                    ["itm_det"] = new JObject
                    {
                        ["rt"] = s.Rate,
                        ["txval"] = s.TaxableValue,
                        ["iamt"] = s.IGST,
                        ["camt"] = s.CGST,
                        ["samt"] = s.SGST,
                        ["csamt"] = s.Cess,
                    },
                }));

            if (r.B2B.Count > 0)
            {
                root["b2b"] = new JArray(r.B2B.Select(p => new JObject
                {
                    ["ctin"] = p.GSTIN,
                    ["inv"] = new JArray(p.Documents.Select(d => new JObject
                    {
                        ["inum"] = d.DocumentNo,
                        ["idt"] = d.DocumentDate.ToString("dd-MM-yyyy"),
                        ["val"] = d.InvoiceValue,
                        ["pos"] = d.PlaceOfSupplyStateCode,
                        ["rchrg"] = d.ReverseCharge ? "Y" : "N",
                        ["inv_typ"] = "R",
                        ["itms"] = Items(d, false),
                    })),
                }));
            }

            if (r.B2CL.Count > 0)
            {
                root["b2cl"] = new JArray(r.B2CL.Select(p => new JObject
                {
                    ["pos"] = p.GSTIN, // for B2CL the group key IS the place of supply
                    ["inv"] = new JArray(p.Documents.Select(d => new JObject
                    {
                        ["inum"] = d.DocumentNo,
                        ["idt"] = d.DocumentDate.ToString("dd-MM-yyyy"),
                        ["val"] = d.InvoiceValue,
                        ["itms"] = Items(d, false),
                    })),
                }));
            }

            if (r.B2CS.Count > 0)
            {
                root["b2cs"] = new JArray(r.B2CS.Select(b => new JObject
                {
                    ["sply_ty"] = b.SupplyType,
                    ["pos"] = b.PlaceOfSupplyStateCode,
                    ["typ"] = "OE",
                    ["rt"] = b.Rate,
                    ["txval"] = b.TaxableValue,
                    ["iamt"] = b.IGST,
                    ["camt"] = b.CGST,
                    ["samt"] = b.SGST,
                    ["csamt"] = b.Cess,
                }));
            }

            if (r.CDNR.Count > 0)
            {
                root["cdnr"] = new JArray(r.CDNR.Select(p => new JObject
                {
                    ["ctin"] = p.GSTIN,
                    ["nt"] = new JArray(p.Documents.Select(d => new JObject
                    {
                        ["ntty"] = d.DocumentType == TaxDocumentType.CreditNote ? "C" : "D",
                        ["nt_num"] = d.DocumentNo,
                        ["nt_dt"] = d.DocumentDate.ToString("dd-MM-yyyy"),
                        ["val"] = d.InvoiceValue,
                        ["pos"] = d.PlaceOfSupplyStateCode,
                        ["rchrg"] = d.ReverseCharge ? "Y" : "N",
                        ["inum"] = d.OriginalDocumentNo,
                        ["idt"] = d.OriginalDocumentDate?.ToString("dd-MM-yyyy") ?? string.Empty,
                        ["itms"] = Items(d, true),
                    })),
                }));
            }

            if (r.CDNUR.Count > 0)
            {
                root["cdnur"] = new JArray(r.CDNUR.SelectMany(p => p.Documents).Select(d => new JObject
                {
                    ["typ"] = "B2CL",
                    ["ntty"] = d.DocumentType == TaxDocumentType.CreditNote ? "C" : "D",
                    ["nt_num"] = d.DocumentNo,
                    ["nt_dt"] = d.DocumentDate.ToString("dd-MM-yyyy"),
                    ["val"] = d.InvoiceValue,
                    ["pos"] = d.PlaceOfSupplyStateCode,
                    ["inum"] = d.OriginalDocumentNo,
                    ["idt"] = d.OriginalDocumentDate?.ToString("dd-MM-yyyy") ?? string.Empty,
                    ["itms"] = Items(d, true),
                }));
            }

            if (r.Exempt.Count > 0)
            {
                root["nil"] = new JObject
                {
                    ["inv"] = new JArray(r.Exempt.Select(e => new JObject
                    {
                        ["sply_ty"] = "OTHOUT",
                        ["nil_amt"] = e.NilRated,
                        ["expt_amt"] = e.Exempted,
                        ["ngsup_amt"] = e.NonGST,
                    })),
                };
            }

            if (r.HSN.Count > 0)
            {
                root["hsn"] = new JObject
                {
                    ["data"] = new JArray(r.HSN.Select((h, i) => new JObject
                    {
                        ["num"] = i + 1,
                        ["hsn_sc"] = h.SACCode,
                        ["desc"] = h.Description,
                        ["uqc"] = "OTH",
                        ["qty"] = h.Quantity,
                        ["rt"] = h.Rate,
                        ["txval"] = h.TaxableValue,
                        ["iamt"] = h.IGST,
                        ["camt"] = h.CGST,
                        ["samt"] = h.SGST,
                        ["csamt"] = h.Cess,
                    })),
                };
            }

            if (r.Documents.Count > 0)
            {
                root["doc_issue"] = new JObject
                {
                    ["doc_det"] = new JArray(r.Documents.Select((d, i) => new JObject
                    {
                        ["doc_num"] = i + 1,
                        ["doc_typ"] = d.DocumentType,
                        ["docs"] = new JArray(new JObject
                        {
                            ["num"] = 1,
                            ["from"] = d.FromNumber,
                            ["to"] = d.ToNumber,
                            ["totnum"] = d.TotalIssued,
                            ["cancel"] = d.Cancelled,
                            ["net_issue"] = d.NetIssued,
                        }),
                    })),
                };
            }

            return root;
        }

        private static JObject Gstr3bJson(Gstr3bResult r) => new()
        {
            ["gstin"] = r.CompanyGSTIN,
            ["ret_period"] = ToFp(r.PeriodKey),
            ["sup_details"] = new JObject
            {
                ["osup_det"] = Sup(r, "a"),
                ["osup_zero"] = Sup(r, "b"),
                ["osup_nil_exmp"] = Sup(r, "c"),
                ["isup_rev"] = Sup(r, "d"),
                ["osup_nongst"] = Sup(r, "e"),
            },
            ["inter_sup"] = new JObject
            {
                ["unreg_details"] = new JArray(r.InterStateSupplies.Select(i => new JObject
                {
                    ["pos"] = i.PlaceOfSupplyStateCode,
                    ["txval"] = i.TaxableValue,
                    ["iamt"] = i.IGST,
                })),
            },
            ["itc_elg"] = new JObject
            {
                ["itc_avl"] = new JArray(r.Itc.Where(i => i.Code.StartsWith('A')).Select(i => new JObject
                {
                    ["ty"] = i.Code,
                    ["iamt"] = i.IGST, ["camt"] = i.CGST, ["samt"] = i.SGST, ["csamt"] = i.Cess,
                })),
                ["itc_rev"] = new JArray(r.Itc.Where(i => i.Code.StartsWith('B')).Select(i => new JObject
                {
                    ["ty"] = i.Code,
                    ["iamt"] = i.IGST, ["camt"] = i.CGST, ["samt"] = i.SGST, ["csamt"] = i.Cess,
                })),
                ["itc_net"] = new JObject
                {
                    ["iamt"] = r.NetItcIGST, ["camt"] = r.NetItcCGST,
                    ["samt"] = r.NetItcSGST, ["csamt"] = r.NetItcCess,
                },
            },
            ["tax_payable"] = new JObject
            {
                ["iamt"] = r.PayableIGST, ["camt"] = r.PayableCGST,
                ["samt"] = r.PayableSGST, ["csamt"] = r.PayableCess,
                ["total"] = r.TotalPayable,
            },
            // Not part of the portal's schema: carried so the file itself records whether the
            // summary agreed with GSTR-1 when it was generated.
            ["_gstr1_check"] = new JObject
            {
                ["gstr1_txval"] = r.Gstr1TaxableValue,
                ["variance_txval"] = r.VarianceTaxableValue,
                ["variance_tax"] = r.VarianceTax,
                ["agrees"] = r.AgreesWithGstr1,
            },
        };

        private static JObject Sup(Gstr3bResult r, string code)
        {
            var row = r.Outward.FirstOrDefault(o => o.Code == code);
            return new JObject
            {
                ["txval"] = row?.TaxableValue ?? 0m,
                ["iamt"] = row?.IGST ?? 0m,
                ["camt"] = row?.CGST ?? 0m,
                ["samt"] = row?.SGST ?? 0m,
                ["csamt"] = row?.Cess ?? 0m,
            };
        }

        private static JObject Gstr9Json(Gstr9Result r) => new()
        {
            ["gstin"] = r.CompanyGSTIN,
            ["fy"] = r.FinancialYearLabel,
            ["turnover"] = r.TotalTurnover,
            ["tax_payable"] = r.TotalTaxPayable,
            ["outward_by_rate"] = new JArray(r.OutwardByRate.Select(s => new JObject
            {
                ["rt"] = s.Rate, ["txval"] = s.TaxableValue,
                ["iamt"] = s.IGST, ["camt"] = s.CGST, ["samt"] = s.SGST, ["csamt"] = s.Cess,
            })),
            ["itc"] = new JObject
            {
                ["iamt"] = r.ItcAvailedIGST, ["camt"] = r.ItcAvailedCGST,
                ["samt"] = r.ItcAvailedSGST, ["csamt"] = r.ItcAvailedCess,
                ["reversed"] = r.ItcReversed,
            },
            ["months"] = new JArray(r.Months.Select(m => new JObject
            {
                ["fp"] = ToFp(m.PeriodKey),
                ["books_txval"] = m.BooksTaxableValue,
                ["books_tax"] = m.BooksTax,
                ["filed_txval"] = m.FiledTaxableValue,
                ["filed_tax"] = m.FiledTax,
                ["was_filed"] = m.WasFiled,
                ["variance_txval"] = m.VarianceTaxableValue,
            })),
        };

        /// <summary>yyyyMM → MMyyyy, which is the order the portal writes a return period in.</summary>
        private static string ToFp(string periodKey) =>
            periodKey.Length == 6 ? periodKey.Substring(4, 2) + periodKey.Substring(0, 4) : periodKey;

        // ── Excel ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// One worksheet per top-level array in the payload, flattened. Driven off the JSON rather
        /// than written per return type on purpose: the workbook then cannot drift from the file
        /// that is actually filed, which is the only thing it is for.
        /// </summary>
        private static byte[] BuildWorkbook(string type, JObject payload)
        {
            using var book = new XLWorkbook();
            var summary = book.Worksheets.Add("Summary");
            int row = 1;
            summary.Cell(row, 1).Value = type;
            summary.Cell(row++, 2).Value = "generated " + DateTime.Now.ToString("dd-MM-yyyy HH:mm");

            foreach (var prop in payload.Properties())
            {
                if (prop.Value is JValue v)
                {
                    summary.Cell(row, 1).Value = prop.Name;
                    summary.Cell(row++, 2).Value = v.ToString();
                }
            }
            summary.Columns().AdjustToContents();

            foreach (var prop in payload.Properties())
            {
                var rows = Flatten(prop.Value);
                if (rows.Count == 0) continue;

                // Excel sheet names are capped at 31 characters and cannot carry : \ / ? * [ ].
                string name = SheetName(prop.Name);
                if (book.Worksheets.Contains(name)) continue;
                var sheet = book.Worksheets.Add(name);

                var columns = rows.SelectMany(d => d.Keys).Distinct().ToList();
                for (int c = 0; c < columns.Count; c++)
                {
                    sheet.Cell(1, c + 1).Value = columns[c];
                    sheet.Cell(1, c + 1).Style.Font.Bold = true;
                }

                for (int rr = 0; rr < rows.Count; rr++)
                    for (int c = 0; c < columns.Count; c++)
                        if (rows[rr].TryGetValue(columns[c], out var cell))
                            sheet.Cell(rr + 2, c + 1).Value = cell;

                sheet.SheetView.FreezeRows(1);
                sheet.Columns().AdjustToContents();
            }

            using var ms = new MemoryStream();
            book.SaveAs(ms);
            return ms.ToArray();
        }

        /// <summary>Turn a nested JSON node into flat rows, one per leaf object, with dotted names
        /// for the path. A GSTR-1 invoice three levels down still has to be one spreadsheet row.</summary>
        private static List<Dictionary<string, string>> Flatten(JToken node)
        {
            var output = new List<Dictionary<string, string>>();
            Walk(node, new Dictionary<string, string>(), string.Empty, output);
            return output;

            static void Walk(JToken token, Dictionary<string, string> inherited, string prefix,
                             List<Dictionary<string, string>> output)
            {
                switch (token)
                {
                    case JArray arr:
                        foreach (var item in arr) Walk(item, inherited, prefix, output);
                        break;

                    case JObject obj:
                    {
                        var scalars = new Dictionary<string, string>(inherited);
                        var children = new List<(string Name, JToken Value)>();

                        foreach (var p in obj.Properties())
                        {
                            string name = prefix.Length == 0 ? p.Name : $"{prefix}.{p.Name}";
                            if (p.Value is JValue val) scalars[name] = val.ToString();
                            else children.Add((name, p.Value));
                        }

                        if (children.Count == 0) { output.Add(scalars); break; }
                        foreach (var (name, value) in children) Walk(value, scalars, name, output);
                        break;
                    }

                    case JValue leaf:
                        var single = new Dictionary<string, string>(inherited)
                        { [prefix.Length == 0 ? "value" : prefix] = leaf.ToString() };
                        output.Add(single);
                        break;
                }
            }
        }

        private static string SheetName(string raw)
        {
            var sb = new StringBuilder();
            foreach (char c in raw)
                sb.Append(c is ':' or '\\' or '/' or '?' or '*' or '[' or ']' ? '_' : c);
            string s = sb.ToString().Trim('_');
            if (s.Length == 0) s = "Sheet";
            return s.Length <= 31 ? s : s[..31];
        }

        // ── Mapping ───────────────────────────────────────────────────────────────────────

        private static GstReturnFilingRow ToRow(GstReturnFiling f) => new()
        {
            GstReturnFilingID = f.GstReturnFilingID,
            ReturnType = f.ReturnType,
            PeriodKey = f.PeriodKey,
            PeriodLabel = f.PeriodKey.EndsWith("0000")
                ? $"FY {f.PeriodKey[..4]}-{(int.Parse(f.PeriodKey[..4]) + 1) % 100:00}"
                : Gstr1Service.PeriodLabel(f.PeriodKey),
            TotalTaxableValue = f.TotalTaxableValue,
            TotalIGST = f.TotalIGST,
            TotalCGST = f.TotalCGST,
            TotalSGST = f.TotalSGST,
            TotalCess = f.TotalCess,
            DocumentCount = f.DocumentCount,
            Status = f.Status,
            FiledOn = f.FiledOn,
            AcknowledgementNo = f.AcknowledgementNo,
            Note = f.Note,
            GeneratedOn = f.DateTime,
        };

        private static string Trim(string? s, int max)
            => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s[..max]);
    }
}
