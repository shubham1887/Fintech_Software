using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase5;
using FintechApp.Responses.Phase5;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IBankStatementImportService" />
    public class BankStatementImportService : IBankStatementImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBankLineClassifier _classifier;
        private readonly Logger _logger = new();

        public BankStatementImportService(ApplicationDbContext context, IBankLineClassifier classifier)
        {
            _context = context;
            _classifier = classifier;
        }

        // ── Header synonyms ───────────────────────────────────────────────────────────────
        // Every bank names these differently and none of them is going to change for us. Matching
        // on a contains-any list is what lets HDFC, ICICI, SBI, Axis and a hand-made Excel sheet
        // all import without anyone configuring a thing.
        private static readonly string[] TxnDateNames =
            { "transaction date", "txn date", "tran date", "date", "posting date", "post date", "entry date" };
        private static readonly string[] ValueDateNames = { "value date", "value dt", "val date" };
        private static readonly string[] DescriptionNames =
            { "narration", "description", "particulars", "transaction remarks", "remarks", "details", "transaction particulars" };
        private static readonly string[] ReferenceNames =
            { "chq / ref no", "chq/ref no", "ref no", "reference no", "reference", "cheque no", "chq no", "utr", "utr no", "transaction id", "txn id", "ref" };
        private static readonly string[] DebitNames =
            { "withdrawal amt", "withdrawal amount", "withdrawal", "debit amount", "debit", "dr", "paid out", "amount debited" };
        private static readonly string[] CreditNames =
            { "deposit amt", "deposit amount", "deposit", "credit amount", "credit", "cr", "paid in", "amount credited" };
        private static readonly string[] AmountNames = { "amount", "transaction amount", "txn amount" };
        private static readonly string[] BalanceNames =
            { "closing balance", "balance", "running balance", "available balance", "balance amt" };

        private static readonly string[] DateFormats =
        {
            "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy", "d/M/yyyy", "d-M-yyyy",
            "yyyy-MM-dd", "yyyy/MM/dd",
            "dd-MMM-yyyy", "dd MMM yyyy", "dd-MMM-yy", "dd/MM/yy", "dd-MM-yy",
            "MM/dd/yyyy", "dd-MMM-yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss",
        };

        /// <summary>A UTR / RRN / cheque reference inside a narration — 10 or more digits, or the
        /// bank-code-plus-digits shape ("SBIN0123456789"). Used only when there is no reference
        /// column, which on an Excel export pasted together by hand is most of the time.</summary>
        private static readonly Regex RefInText =
            new(@"\b([A-Z]{3,6}\d{6,}|\d{10,})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async Task<(BankStatementImportResult? Result, string Failure)> Import(BankStatementImportRequest request)
        {
            if (request.BankAccountID <= 0) return (null, "A bank account is required.");
            if (string.IsNullOrWhiteSpace(request.Content)) return (null, "The statement file is empty.");

            var account = await _context.BankAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.BankAccountID == request.BankAccountID && !a.IsDeleted);
            if (account == null) return (null, "That bank account does not exist.");

            // What shape is this file? The caller may say; otherwise the file name does, and
            // failing that the bank's own saved format. An operator uploading their statement
            // should not have to answer a question their file already answers.
            string source = BankStatementSource.Normalize(request.Source);
            if ((request.Source ?? string.Empty).Trim().Length == 0)
            {
                string ext = Path.GetExtension(request.FileName ?? string.Empty).Trim('.').ToUpperInvariant();
                if (ext is "XLSX" or "XLS") source = BankStatementSource.Xlsx;
                else if (ext == "PDF") source = BankStatementSource.Pdf;
            }

            // The bank's saved format: set up once, used at every upload. An explicit column map
            // on the request still wins, because a one-off file sometimes differs from the norm.
            var format = await ResolveFormat(request.BankStatementFormatID, account.BankAccountID, source);
            if (format != null && (request.Source ?? string.Empty).Trim().Length == 0)
                source = BankStatementSource.Normalize(format.Source);

            var warnings = new List<string>();

            List<string[]> grid;
            try
            {
                grid = source switch
                {
                    BankStatementSource.Xlsx => ReadXlsx(request.Content),
                    BankStatementSource.Pdf => ReadPdf(request.Content, format, warnings),
                    _ => ReadCsv(request.Content),
                };
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, $"The file could not be read as {source}. {ex.Message}");
            }

            if (grid.Count == 0)
                return (null, warnings.Count > 0 ? warnings[0] : "The statement has no rows.");

            // Skip a preamble the format knows about (a bank that prints its address above the
            // table). Done before detection so the header is found where it actually is.
            if (format != null && format.SkipRows > 0 && format.SkipRows < grid.Count)
                grid = grid.Skip(format.SkipRows).ToList();

            // headerRow is the index of the header, or -1 when the file has none and the caller
            // mapped the columns by index — data then starts at row 0, which headerRow + 1 gives.
            var (headerRow, map) = DetectColumns(grid, request.Columns ?? ToColumnMap(format));
            if (map.TxnDate < 0)
                return (null, "No transaction-date column could be found. The statement needs a row naming "
                            + "its columns, or an explicit column mapping.");
            if (map.Debit < 0 && map.Credit < 0 && map.Amount < 0)
                return (null, "No amount column could be found. Map the columns and try again.");

            var result = new BankStatementImportResult
            {
                DetectedColumns = Describe(headerRow >= 0 ? grid[headerRow] : Array.Empty<string>(), map),
            };
            result.Warnings.AddRange(warnings);
            var lines = new List<BankStatementLine>();

            for (int r = headerRow + 1; r < grid.Count; r++)
            {
                var row = grid[r];
                if (row.All(string.IsNullOrWhiteSpace)) continue;

                string rawDate = Cell(row, map.TxnDate);
                if (!TryDate(rawDate, out var txnDate))
                {
                    // Footers ("Closing balance", "*** End of statement ***") land here. Only say
                    // something when the row looks like it was meant to be data.
                    if (HasAnyAmount(row, map))
                        result.Warnings.Add($"Row {r + 1}: '{rawDate}' is not a date I can read; the row was skipped.");
                    continue;
                }

                decimal debit = 0m, credit = 0m;
                if (map.Amount >= 0 && map.Debit < 0 && map.Credit < 0)
                {
                    // One signed column. Negative (or a Dr suffix) is money out.
                    decimal signed = ParseAmount(Cell(row, map.Amount), out bool drSuffix);
                    if (drSuffix) signed = -Math.Abs(signed);
                    if (signed < 0) debit = Math.Abs(signed); else credit = signed;
                }
                else
                {
                    debit = Math.Abs(ParseAmount(Cell(row, map.Debit), out _));
                    credit = Math.Abs(ParseAmount(Cell(row, map.Credit), out _));
                }

                if (debit == 0m && credit == 0m) continue; // an opening-balance row carries neither

                string description = Cell(row, map.Description);
                string reference = Cell(row, map.ReferenceNo);
                if (reference.Length == 0)
                {
                    var m = RefInText.Match(description);
                    if (m.Success) reference = m.Value;
                }

                decimal? balance = null;
                if (map.RunningBalance >= 0)
                {
                    string b = Cell(row, map.RunningBalance);
                    if (b.Length > 0) balance = ParseAmount(b, out _);
                }

                lines.Add(new BankStatementLine
                {
                    BankAccountID = account.BankAccountID,
                    TxnDate = txnDate.Date,
                    ValueDate = map.ValueDate >= 0 && TryDate(Cell(row, map.ValueDate), out var vd) ? vd.Date : null,
                    Description = Trim(description, 500),
                    ReferenceNo = Trim(reference, 100),
                    ReferenceKey = NormaliseRef(reference),
                    Debit = Math.Round(debit, 2),
                    Credit = Math.Round(credit, 2),
                    RunningBalance = balance,
                    RowNo = r + 1,
                    MatchStatus = "Unmatched",
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                    IsDeleted = false,
                });
            }

            if (lines.Count == 0)
                return (null, "No transaction rows could be read from the statement. " +
                              (result.Warnings.Count > 0 ? result.Warnings[0] : "Check the file and the column mapping."));

            DateTime from = lines.Min(l => l.TxnDate);
            DateTime to = lines.Max(l => l.TxnDate);

            // A re-upload REPLACES any active statement whose range OVERLAPS this one — not merely
            // one with the identical range. Overlap, because the realistic mistake is uploading
            // 1–15 October and then 1–31 October: with an exact-match rule both stay active, every
            // line in the first fortnight exists twice, and the reconciliation then reports each of
            // them as a separate unmatched movement. A bank can also restate a line after it
            // clears, so merging is wrong in either case.
            var previous = await _context.BankStatementImports
                .Where(i => i.BankAccountID == account.BankAccountID
                         && i.Status == BankStatementStatus.Active && !i.IsDeleted
                         && i.PeriodFrom <= to && i.PeriodTo >= from)
                .ToListAsync();
            foreach (var p in previous)
            {
                p.Status = BankStatementStatus.Superseded;
                p.UpdatedateTime = DateTime.UtcNow;
                result.SupersededPrevious = true;

                // The old lines go with it, and so do their pairings — a match that points at a
                // superseded line would otherwise hold its ledger line hostage for ever.
                var oldLineIds = await _context.BankStatementLines
                    .Where(l => l.BankStatementImportID == p.BankStatementImportID && !l.IsDeleted)
                    .Select(l => l.BankStatementLineID).ToListAsync();
                if (oldLineIds.Count > 0)
                {
                    await _context.BankStatementLines
                        .Where(l => oldLineIds.Contains(l.BankStatementLineID))
                        .ExecuteUpdateAsync(s => s.SetProperty(l => l.IsDeleted, true)
                                                  .SetProperty(l => l.UpdatedateTime, DateTime.UtcNow));
                    await _context.BankReconMatches
                        .Where(m => oldLineIds.Contains(m.BankStatementLineID) && !m.IsDeleted)
                        .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsDeleted, true)
                                                  .SetProperty(m => m.UpdatedateTime, DateTime.UtcNow));
                }
            }

            var import = new BankStatementImport
            {
                BankAccountID = account.BankAccountID,
                PeriodFrom = from,
                PeriodTo = to,
                FileName = Trim(request.FileName, 260),
                Source = source,
                LineCount = lines.Count,
                TotalDebit = lines.Sum(l => l.Debit),
                TotalCredit = lines.Sum(l => l.Credit),
                ClosingBalance = lines.OrderBy(l => l.TxnDate).ThenBy(l => l.RowNo).LastOrDefault()?.RunningBalance,
                Status = BankStatementStatus.Active,
                ImportedByUserID = request.ImportedByUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false,
            };
            _context.BankStatementImports.Add(import);
            await _context.SaveChangesAsync();

            foreach (var l in lines) l.BankStatementImportID = import.BankStatementImportID;
            _context.BankStatementLines.AddRange(lines);
            await _context.SaveChangesAsync();

            // What each line IS, and whether any of it is the same money twice. Done here, once,
            // while the statement is in hand: a category derived on every screen open would change
            // under the reader the first time somebody edits a rule, and the duplicate check needs
            // the whole statement at once to see a reference repeated.
            try
            {
                result.Categorised = await _classifier.Classify(lines, account.BankAccountID);
                result.DuplicatesFlagged = await _classifier.FlagDuplicates(lines, account.BankAccountID);
                await _context.SaveChangesAsync();

                if (result.DuplicatesFlagged > 0)
                    result.Warnings.Add($"{result.DuplicatesFlagged} line(s) look like the same money twice — "
                                      + "either repeated on the statement, or claimed by more than one fund "
                                      + "request. They are flagged on the register; nothing was rejected.");
            }
            catch (Exception ex)
            {
                // The statement is imported either way. Classification is an aid, not the record.
                _logger.Write(ex);
                result.Warnings.Add("The statement imported, but the lines could not be categorised. "
                                  + "Use Re-classify on the Bank Setup screen once that is looked at.");
            }

            if (format != null)
            {
                format.LastUsedOn = DateTime.UtcNow;
                format.UpdatedateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            result.BankStatementImportID = import.BankStatementImportID;
            result.PeriodFrom = from;
            result.PeriodTo = to;
            result.LinesImported = lines.Count;
            result.TotalDebit = import.TotalDebit;
            result.TotalCredit = import.TotalCredit;
            result.ClosingBalance = import.ClosingBalance;
            return (result, string.Empty);
        }

        public async Task<List<BankStatementImportRow>> GetImports(int bankAccountId)
        {
            var imports = await _context.BankStatementImports.AsNoTracking()
                .Where(i => !i.IsDeleted && (bankAccountId <= 0 || i.BankAccountID == bankAccountId))
                .OrderByDescending(i => i.BankStatementImportID)
                .Take(100)
                .ToListAsync();
            if (imports.Count == 0) return new List<BankStatementImportRow>();

            var ids = imports.Select(i => i.BankStatementImportID).ToList();
            var unmatched = await _context.BankStatementLines.AsNoTracking()
                .Where(l => ids.Contains(l.BankStatementImportID) && !l.IsDeleted && l.MatchStatus != "Matched")
                .GroupBy(l => l.BankStatementImportID)
                .Select(g => new { Id = g.Key, N = g.Count() })
                .ToListAsync();
            var unmatchedById = unmatched.ToDictionary(x => x.Id, x => x.N);

            return imports.Select(i => new BankStatementImportRow
            {
                BankStatementImportID = i.BankStatementImportID,
                BankAccountID = i.BankAccountID,
                PeriodFrom = i.PeriodFrom,
                PeriodTo = i.PeriodTo,
                FileName = i.FileName,
                Source = i.Source,
                LineCount = i.LineCount,
                TotalDebit = i.TotalDebit,
                TotalCredit = i.TotalCredit,
                ClosingBalance = i.ClosingBalance,
                Status = i.Status,
                ImportedOn = i.DateTime,
                UnmatchedLines = unmatchedById.TryGetValue(i.BankStatementImportID, out var n) ? n : 0,
            }).ToList();
        }

        public async Task<List<BankStatementLine>> GetLines(BankStatementQueryRequest query)
        {
            var q = _context.BankStatementLines.AsNoTracking().Where(l => !l.IsDeleted);

            if (query.BankStatementImportID > 0)
                q = q.Where(l => l.BankStatementImportID == query.BankStatementImportID);
            else if (query.BankAccountID > 0)
                q = q.Where(l => l.BankAccountID == query.BankAccountID);

            if (query.PeriodFrom.HasValue) q = q.Where(l => l.TxnDate >= query.PeriodFrom.Value.Date);
            if (query.PeriodTo.HasValue) q = q.Where(l => l.TxnDate <= query.PeriodTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(query.MatchStatus))
                q = q.Where(l => l.MatchStatus == query.MatchStatus);

            return await q.OrderBy(l => l.TxnDate).ThenBy(l => l.RowNo).Take(5000).ToListAsync();
        }

        public async Task<(Responses.BankConfig.BankFormatPreview? Preview, string Failure)> DryRun(
            BankStatementImportRequest request, BankStatementFormat? format, int maxRows)
        {
            if (string.IsNullOrWhiteSpace(request.Content)) return (null, "The file is empty.");

            string source = BankStatementSource.Normalize(
                (request.Source ?? string.Empty).Trim().Length > 0 ? request.Source : format?.Source);
            if ((request.Source ?? string.Empty).Trim().Length == 0 && format == null)
            {
                string ext = Path.GetExtension(request.FileName ?? string.Empty).Trim('.').ToUpperInvariant();
                if (ext is "XLSX" or "XLS") source = BankStatementSource.Xlsx;
                else if (ext == "PDF") source = BankStatementSource.Pdf;
            }

            // A saved format was asked for by id rather than passed in.
            if (format == null && request.BankStatementFormatID > 0)
                format = await ResolveFormat(request.BankStatementFormatID, request.BankAccountID, source);

            var preview = new Responses.BankConfig.BankFormatPreview { Source = source };

            List<string[]> grid;
            try
            {
                grid = source switch
                {
                    BankStatementSource.Xlsx => ReadXlsx(request.Content),
                    BankStatementSource.Pdf => ReadPdf(request.Content, format, preview.Warnings),
                    _ => ReadCsv(request.Content),
                };
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, $"The file could not be read as {source}. {ex.Message}");
            }

            if (format != null && format.SkipRows > 0 && format.SkipRows < grid.Count)
                grid = grid.Skip(format.SkipRows).ToList();

            preview.TotalRowsSeen = grid.Count;
            if (grid.Count == 0)
            {
                preview.Verdict = preview.Warnings.Count > 0
                    ? preview.Warnings[0]
                    : "Nothing could be read out of the file.";
                return (preview, string.Empty);
            }

            var (headerRow, map) = DetectColumns(grid, request.Columns ?? ToColumnMap(format));
            preview.DetectedColumns = Describe(headerRow >= 0 ? grid[headerRow] : Array.Empty<string>(), map);

            if (map.TxnDate < 0)
            {
                preview.Verdict = "No transaction-date column was found. For a CSV or Excel file, name the date "
                                + "column on the format; for a PDF, the line pattern decides it.";
                // Still show the raw rows: seeing what came out is how the pattern gets fixed.
                for (int r = 0; r < Math.Min(grid.Count, maxRows); r++)
                {
                    preview.Rows.Add(new Responses.BankConfig.BankFormatPreviewRow
                    {
                        RowNo = r + 1,
                        Raw = string.Join(" | ", grid[r]),
                        Read = false,
                        Problem = "No date column, so nothing could be read from this row.",
                    });
                }
                return (preview, string.Empty);
            }

            for (int r = headerRow + 1; r < grid.Count && preview.Rows.Count < maxRows; r++)
            {
                var row = grid[r];
                if (row.All(string.IsNullOrWhiteSpace)) continue;

                string raw = string.Join(" | ", row.Where(c => !string.IsNullOrWhiteSpace(c)));
                string rawDate = Cell(row, map.TxnDate);

                if (!TryDate(rawDate, out var txnDate))
                {
                    preview.RowsSkipped++;
                    preview.Rows.Add(new Responses.BankConfig.BankFormatPreviewRow
                    {
                        RowNo = r + 1,
                        Raw = raw,
                        Read = false,
                        Problem = rawDate.Length == 0
                            ? "No date on this row — a heading or a footer, probably."
                            : $"'{rawDate}' is not a date I can read.",
                    });
                    continue;
                }

                decimal debit = 0m, credit = 0m;
                if (map.Amount >= 0 && map.Debit < 0 && map.Credit < 0)
                {
                    decimal signed = ParseAmount(Cell(row, map.Amount), out bool drSuffix);
                    if (drSuffix) signed = -Math.Abs(signed);
                    if (signed < 0) debit = Math.Abs(signed); else credit = signed;
                }
                else
                {
                    debit = Math.Abs(ParseAmount(Cell(row, map.Debit), out _));
                    credit = Math.Abs(ParseAmount(Cell(row, map.Credit), out _));
                }

                if (debit == 0m && credit == 0m)
                {
                    preview.RowsSkipped++;
                    preview.Rows.Add(new Responses.BankConfig.BankFormatPreviewRow
                    {
                        RowNo = r + 1, Raw = raw, TxnDate = txnDate.ToString("dd MMM yyyy"), Read = false,
                        Problem = "No amount on this row — an opening-balance line, probably.",
                    });
                    continue;
                }

                string description = Cell(row, map.Description);
                string reference = Cell(row, map.ReferenceNo);
                if (reference.Length == 0)
                {
                    var m = RefInText.Match(description);
                    if (m.Success) reference = m.Value;
                }

                preview.RowsRead++;
                preview.Rows.Add(new Responses.BankConfig.BankFormatPreviewRow
                {
                    RowNo = r + 1,
                    TxnDate = txnDate.ToString("dd MMM yyyy"),
                    Description = Trim(description, 200),
                    Reference = Trim(reference, 60),
                    Debit = Math.Round(debit, 2),
                    Credit = Math.Round(credit, 2),
                    Balance = map.RunningBalance >= 0 ? Cell(row, map.RunningBalance) : string.Empty,
                    Raw = raw,
                    Read = true,
                });
            }

            preview.Verdict = preview.RowsRead == 0
                ? "No row read as a transaction. For a PDF, adjust the line pattern against the raw text shown "
                + "below; for a CSV or Excel file, check the column names on the format."
                : $"{preview.RowsRead} of the first {preview.Rows.Count} rows read cleanly. "
                + "Check the dates and the amounts against the statement before saving.";

            return (preview, string.Empty);
        }

        // ── The bank's own format ─────────────────────────────────────────────────────────

        /// <summary>
        /// The format to read this file with: the one asked for, else the bank's active format for
        /// this kind of file, else none (and the importer detects as it always did).
        /// </summary>
        private async Task<BankStatementFormat?> ResolveFormat(int formatId, int bankAccountId, string source)
        {
            if (formatId > 0)
            {
                return await _context.BankStatementFormats
                    .FirstOrDefaultAsync(f => f.BankStatementFormatID == formatId && !f.IsDeleted);
            }

            var candidates = await _context.BankStatementFormats
                .Where(f => !f.IsDeleted && f.IsActive && f.BankAccountID == bankAccountId)
                .ToListAsync();

            return candidates.FirstOrDefault(f => BankStatementSource.Normalize(f.Source) == source);
        }

        /// <summary>The saved format as the column map the detector already understands.</summary>
        private static BankColumnMap? ToColumnMap(BankStatementFormat? format)
        {
            if (format == null) return null;

            var map = new BankColumnMap
            {
                TxnDate = format.TxnDateColumn ?? string.Empty,
                ValueDate = format.ValueDateColumn ?? string.Empty,
                Description = format.DescriptionColumn ?? string.Empty,
                ReferenceNo = format.ReferenceColumn ?? string.Empty,
                Debit = format.DebitColumn ?? string.Empty,
                Credit = format.CreditColumn ?? string.Empty,
                Amount = format.AmountColumn ?? string.Empty,
                RunningBalance = format.BalanceColumn ?? string.Empty,
            };

            // All blank means "the format exists for its PDF pattern or its skip rows, and the
            // columns are still detected" — which is the normal case for CSV and Excel.
            bool anything = new[] { map.TxnDate, map.ValueDate, map.Description, map.ReferenceNo,
                                    map.Debit, map.Credit, map.Amount, map.RunningBalance }
                .Any(v => !string.IsNullOrWhiteSpace(v));
            return anything ? map : null;
        }

        // ── Parsing ───────────────────────────────────────────────────────────────────────

        private sealed class ColumnMapIdx
        {
            public int TxnDate = -1, ValueDate = -1, Description = -1, ReferenceNo = -1;
            public int Debit = -1, Credit = -1, Amount = -1, RunningBalance = -1;
        }

        /// <summary>
        /// Find the header row and work out which column is which.
        ///
        /// The header is rarely row 1 — banks put the account holder, the address and the period
        /// above it — so the first row that names a date AND an amount is taken as the header. An
        /// explicit mapping from the caller overrides the detection, for the bank nobody expected.
        /// </summary>
        private static (int HeaderRow, ColumnMapIdx Map) DetectColumns(List<string[]> grid, BankColumnMap? explicitMap)
        {
            int limit = Math.Min(grid.Count, 30);
            for (int r = 0; r < limit; r++)
            {
                var header = grid[r].Select(h => (h ?? string.Empty).Trim().ToLowerInvariant()).ToArray();
                var map = new ColumnMapIdx
                {
                    TxnDate = Find(header, TxnDateNames),
                    ValueDate = Find(header, ValueDateNames),
                    Description = Find(header, DescriptionNames),
                    ReferenceNo = Find(header, ReferenceNames),
                    Debit = Find(header, DebitNames),
                    Credit = Find(header, CreditNames),
                    RunningBalance = Find(header, BalanceNames),
                };
                // Only look for a single signed "amount" when debit/credit were not both found —
                // otherwise a statement with all three columns reads the wrong one.
                if (map.Debit < 0 || map.Credit < 0) map.Amount = Find(header, AmountNames);

                // "Value date" also contains "date", so a greedy TxnDate can grab it. If both
                // landed on the same column, let the more specific one keep it.
                if (map.TxnDate >= 0 && map.TxnDate == map.ValueDate) map.ValueDate = -1;

                bool hasDate = map.TxnDate >= 0;
                bool hasAmount = map.Debit >= 0 || map.Credit >= 0 || map.Amount >= 0;
                if (!hasDate || !hasAmount) continue;

                ApplyExplicit(header, map, explicitMap);
                return (r, map);
            }

            // No header row the parser recognises. If the caller mapped the columns by INDEX we can
            // still read the file: -1 means "there is no header", and the caller's headerRow + 1
            // then starts at row 0. A mapping by NAME cannot help here — there are no names to
            // match — so TxnDate stays -1 and the caller reports that.
            if (explicitMap != null)
            {
                var map = new ColumnMapIdx();
                ApplyExplicit(Array.Empty<string>(), map, explicitMap);
                return (-1, map);
            }
            return (-1, new ColumnMapIdx());
        }

        private static void ApplyExplicit(string[] header, ColumnMapIdx map, BankColumnMap? m)
        {
            if (m == null) return;
            Set(ref map.TxnDate, m.TxnDate);
            Set(ref map.ValueDate, m.ValueDate);
            Set(ref map.Description, m.Description);
            Set(ref map.ReferenceNo, m.ReferenceNo);
            Set(ref map.Debit, m.Debit);
            Set(ref map.Credit, m.Credit);
            Set(ref map.Amount, m.Amount);
            Set(ref map.RunningBalance, m.RunningBalance);

            void Set(ref int target, string spec)
            {
                if (string.IsNullOrWhiteSpace(spec)) return;
                if (int.TryParse(spec.Trim(), out int idx) && idx >= 0) { target = idx; return; }
                int byName = Array.FindIndex(header, h => h == spec.Trim().ToLowerInvariant());
                if (byName >= 0) target = byName;
            }
        }

        /// <summary>First column whose header contains one of the names. Longest name first, so
        /// "withdrawal amt" beats a bare "amt" when both would match.</summary>
        private static int Find(string[] header, string[] names)
        {
            foreach (string name in names.OrderByDescending(n => n.Length))
                for (int i = 0; i < header.Length; i++)
                    if (header[i].Length > 0 && header[i].Contains(name, StringComparison.Ordinal))
                        return i;
            return -1;
        }

        private static string Describe(string[] header, ColumnMapIdx map)
        {
            var parts = new List<string>();
            void Add(string label, int idx)
            {
                if (idx < 0) return;
                string name = idx < header.Length ? (header[idx] ?? string.Empty).Trim() : string.Empty;
                parts.Add($"{label} = {(name.Length > 0 ? name : $"column {idx + 1}")}");
            }
            Add("date", map.TxnDate);
            Add("description", map.Description);
            Add("reference", map.ReferenceNo);
            Add("debit", map.Debit);
            Add("credit", map.Credit);
            Add("amount", map.Amount);
            Add("balance", map.RunningBalance);
            return string.Join(", ", parts);
        }

        private static bool HasAnyAmount(string[] row, ColumnMapIdx map)
        {
            foreach (int idx in new[] { map.Debit, map.Credit, map.Amount })
            {
                if (idx < 0) continue;
                if (ParseAmount(Cell(row, idx), out _) != 0m) return true;
            }
            return false;
        }

        private static string Cell(string[] row, int idx)
            => idx >= 0 && idx < row.Length ? (row[idx] ?? string.Empty).Trim() : string.Empty;

        private static bool TryDate(string s, out DateTime value)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();

            // An .xlsx date cell comes through as a serial number when the column is not formatted
            // as a date. 20000 ≈ 1954 and 60000 ≈ 2064, which brackets anything a statement holds.
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double serial)
                && serial > 20000 && serial < 60000 && !s.Contains('.'))
            {
                try { value = DateTime.FromOADate(serial); return true; } catch { /* fall through */ }
            }

            if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.AllowWhiteSpaces, out value)) return true;

            return DateTime.TryParse(s, CultureInfo.GetCultureInfo("en-IN"),
                                     DateTimeStyles.AllowWhiteSpaces, out value);
        }

        /// <summary>
        /// Amounts as banks print them: "1,23,456.78", "(500.00)", "500.00 Dr", "-500". Returns the
        /// signed value; <paramref name="drSuffix"/> reports a Dr/CR marker, which is the only way
        /// a single-column statement says which way the money went.
        /// </summary>
        private static decimal ParseAmount(string s, out bool drSuffix)
        {
            drSuffix = false;
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            s = s.Trim();

            if (s.EndsWith("dr", StringComparison.OrdinalIgnoreCase)) { drSuffix = true; s = s[..^2]; }
            else if (s.EndsWith("cr", StringComparison.OrdinalIgnoreCase)) { s = s[..^2]; }

            bool bracketed = s.StartsWith('(') && s.EndsWith(')');
            if (bracketed) s = s[1..^1];

            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
                if (char.IsDigit(c) || c == '.' || c == '-') sb.Append(c);

            if (!decimal.TryParse(sb.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
                return 0m;
            return bracketed ? -Math.Abs(v) : v;
        }

        /// <summary>
        /// The reference-like tokens inside a free-text narration.
        ///
        /// A voucher narration is prose — "UPI vendor payment UTR 778899001122" — while a bank's
        /// reference column is the bare identifier. Normalising the WHOLE narration and comparing
        /// it for equality can therefore never match, so the narration is broken into its
        /// alphanumeric runs instead and only the ones that could be an identifier are kept: six
        /// characters or more AND containing a digit, which keeps "778899001122" and "SBIN0012345"
        /// while dropping "VENDOR" and "PAYMENT".
        /// </summary>
        internal static IEnumerable<string> ReferenceTokens(string? narration)
        {
            if (string.IsNullOrWhiteSpace(narration)) yield break;

            var sb = new StringBuilder();
            bool hasDigit = false;

            foreach (char c in narration + " ")
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToUpperInvariant(c));
                    if (char.IsDigit(c)) hasDigit = true;
                }
                else
                {
                    if (sb.Length >= 6 && hasDigit) yield return sb.ToString();
                    sb.Clear();
                    hasDigit = false;
                }
            }
        }

        /// <summary>A reference stripped to letters and digits, upper-cased, so punctuation the two
        /// sides disagree on never splits one payment into two.</summary>
        internal static string NormaliseRef(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return string.Empty;
            var sb = new StringBuilder(reference.Length);
            foreach (char c in reference)
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToUpperInvariant(c));
            return sb.Length > 100 ? sb.ToString(0, 100) : sb.ToString();
        }

        private static string Trim(string? s, int max)
            => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s[..max]);

        // ── File readers ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// CSV, with the delimiter sniffed rather than assumed. Indian statements are exported as
        /// comma, tab, semicolon and pipe depending on the portal; guessing wrong puts the whole
        /// row in column one, which then fails as "no date column" rather than as anything useful.
        /// </summary>
        private static List<string[]> ReadCsv(string content)
        {
            var rawLines = content.Replace("\r\n", "\n").Replace('\r', '\n')
                                  .Split('\n', StringSplitOptions.None);
            char delimiter = SniffDelimiter(rawLines);

            var grid = new List<string[]>();
            foreach (string line in rawLines)
            {
                if (line.Length == 0) { grid.Add(Array.Empty<string>()); continue; }
                grid.Add(SplitCsvLine(line, delimiter));
            }
            return grid;
        }

        private static char SniffDelimiter(string[] lines)
        {
            char[] candidates = { ',', '\t', ';', '|' };
            var best = candidates[0];
            int bestScore = -1;
            foreach (char c in candidates)
            {
                // The delimiter that most consistently splits the first 20 non-empty lines into
                // the same number of columns, favouring more columns.
                var counts = lines.Where(l => l.Trim().Length > 0).Take(20)
                                  .Select(l => SplitCsvLine(l, c).Length).ToList();
                if (counts.Count == 0) continue;
                int modal = counts.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Key;
                int agree = counts.Count(x => x == modal);
                int score = modal <= 1 ? -1 : agree * 10 + modal;
                if (score > bestScore) { bestScore = score; best = c; }
            }
            return best;
        }

        /// <summary>One CSV line, honouring double quotes (a narration regularly contains the
        /// delimiter, and "" is an escaped quote inside a quoted field).</summary>
        private static string[] SplitCsvLine(string line, char delimiter)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else sb.Append(c);
                }
                else if (c == '"') inQuotes = true;
                else if (c == delimiter) { fields.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
            fields.Add(sb.ToString());
            return fields.ToArray();
        }

        // ── PDF ───────────────────────────────────────────────────────────────────────────
        //
        // A PDF has no columns. It has glyphs at coordinates, and every bank arranges them
        // differently, so there is no equivalent of "find the header row" that works twice. What
        // does work is: take the text line by line, throw away what is obviously not a transaction,
        // and split what is left with a pattern that belongs to that bank.
        //
        // The default pattern below reads the shape nearly every Indian statement shares — a date,
        // then narration, then one or two amounts and a balance at the end — which gets most banks
        // in without configuration. When it does not, the bank's format carries its own pattern and
        // the preview screen is there to get it right against a real file.

        /// <summary>A date at the start of a line: 01/04/2026, 01-04-2026, 01 Apr 2026.</summary>
        private static readonly Regex PdfDateAtStart = new(
            @"^\s*(?<date>\d{1,2}[-/\. ](?:\d{1,2}|[A-Za-z]{3,9})[-/\. ]\d{2,4})\b",
            RegexOptions.Compiled);

        /// <summary>
        /// Trailing money on a statement line: up to three figures at the end (debit or credit,
        /// then the running balance), each optionally with a Dr/Cr suffix.
        /// </summary>
        private static readonly Regex PdfTrailingAmounts = new(
            @"(?<amt>-?[\d,]+\.\d{2}\s*(?:Dr|Cr)?)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Text out of a PDF, as a grid the rest of the importer can read.
        ///
        /// Produces a fixed seven-column shape — date, value date, description, reference, debit,
        /// credit, balance — and a header row naming them, so column detection downstream finds
        /// exactly what it expects and a PDF needs no special case anywhere else.
        /// </summary>
        private List<string[]> ReadPdf(string base64, BankStatementFormat? format, List<string> warnings)
        {
            byte[] bytes = Convert.FromBase64String(base64.Trim());

            var textLines = new List<string>();
            using (var ms = new MemoryStream(bytes))
            using (var doc = UglyToad.PdfPig.PdfDocument.Open(ms))
            {
                foreach (var page in doc.GetPages())
                {
                    // Words carry their position, which is the only way back to lines: a PDF has
                    // no line breaks, only glyphs placed on a page. Words sharing a baseline are
                    // one line, left to right. The baseline is rounded to about two points so a
                    // font that sits a hair higher does not split a row in two.
                    foreach (var group in page.GetWords()
                                 .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                                 .GroupBy(w => Math.Round(w.BoundingBox.Bottom / 2.0, MidpointRounding.AwayFromZero))
                                 .OrderByDescending(g => g.Key))
                    {
                        string line = string.Join(' ', group
                            .OrderBy(w => w.BoundingBox.Left)
                            .Select(w => w.Text))
                            .Replace(' ', ' ')
                            .Trim();

                        if (line.Length > 0) textLines.Add(line);
                    }
                }
            }

            if (textLines.Count == 0)
            {
                warnings.Add("No text could be extracted from the PDF. If it is a scan of a printed page "
                           + "there is no text in it to read — ask the bank for the Excel or CSV export.");
                return new List<string[]>();
            }

            Regex? skip = Compile(format?.PdfSkipPattern, warnings, "skip");
            Regex? linePattern = Compile(format?.PdfLinePattern, warnings, "line");
            bool joinWrapped = format?.PdfJoinWrappedLines ?? true;

            // A narration longer than the column wraps onto the next line with no date of its own.
            // Without joining it back the tail of the narration is thrown away silently, which is
            // exactly where the counterparty's name usually is.
            var merged = new List<string>();
            foreach (var line in textLines)
            {
                if (skip != null && skip.IsMatch(line)) continue;

                bool startsRow = linePattern != null ? linePattern.IsMatch(line) : PdfDateAtStart.IsMatch(line);
                if (!startsRow && joinWrapped && merged.Count > 0 && !PdfDateAtStart.IsMatch(line))
                {
                    merged[^1] = merged[^1] + " " + line.Trim();
                    continue;
                }
                merged.Add(line);
            }

            var grid = new List<string[]>
            {
                new[] { "Date", "Value Date", "Narration", "Ref No", "Debit", "Credit", "Balance" },
            };

            foreach (var line in merged)
            {
                var row = linePattern != null ? FromPattern(linePattern, line) : FromShape(line);
                if (row != null) grid.Add(row);
            }

            if (grid.Count == 1)
            {
                warnings.Add(linePattern != null
                    ? "The PDF line pattern on this bank's format matched no line. Open the format, press "
                    + "Preview against this file, and adjust the pattern against what it shows."
                    : "No line in the PDF looked like a transaction (a date, then narration, then amounts). "
                    + "Set a line pattern on this bank's format and preview it against this file.");
            }

            return grid;
        }

        /// <summary>One line split by the bank's own pattern, through its named groups.</summary>
        private static string[]? FromPattern(Regex pattern, string line)
        {
            var m = pattern.Match(line);
            if (!m.Success) return null;

            string G(string name) => m.Groups[name].Success ? m.Groups[name].Value.Trim() : string.Empty;

            string debit = G("debit"), credit = G("credit"), amount = G("amount");
            if (debit.Length == 0 && credit.Length == 0 && amount.Length > 0)
            {
                // One signed figure, or one with a Dr/Cr suffix.
                decimal signed = ParseAmount(amount, out bool dr);
                if (dr || signed < 0) debit = Math.Abs(signed).ToString(CultureInfo.InvariantCulture);
                else credit = signed.ToString(CultureInfo.InvariantCulture);
            }

            return new[] { G("date"), G("valuedate"), G("desc"), G("ref"), debit, credit, G("balance") };
        }

        /// <summary>
        /// One line read by shape, for a bank with no configured pattern: a date at the front,
        /// amounts at the back, narration in the middle.
        ///
        /// Two trailing figures are read as "amount then balance" — which is the common layout and
        /// also the one that cannot be resolved without knowing the bank: whether that amount is a
        /// debit or a credit is decided by the running balance going down or up, and that is only
        /// knowable row to row. So the amount is left in the debit column with a Dr/Cr suffix
        /// honoured where the bank prints one, and the format's pattern is the way to be exact.
        /// </summary>
        private static string[]? FromShape(string line)
        {
            var dm = PdfDateAtStart.Match(line);
            if (!dm.Success) return null;

            string date = dm.Groups["date"].Value.Trim();
            string rest = line.Substring(dm.Length);

            var amounts = new List<string>();
            while (amounts.Count < 3)
            {
                var am = PdfTrailingAmounts.Match(rest);
                if (!am.Success) break;
                amounts.Insert(0, am.Groups["amt"].Value.Trim());
                rest = rest.Substring(0, am.Index).TrimEnd();
            }

            if (amounts.Count == 0) return null;

            string balance = amounts.Count >= 2 ? amounts[^1] : string.Empty;
            string value = amounts.Count >= 2 ? amounts[^2] : amounts[0];

            string debit = string.Empty, credit = string.Empty;
            decimal signed = ParseAmount(value, out bool drSuffix);
            bool crSuffix = value.EndsWith("cr", StringComparison.OrdinalIgnoreCase);
            if (crSuffix) credit = Math.Abs(signed).ToString(CultureInfo.InvariantCulture);
            else if (drSuffix || signed < 0) debit = Math.Abs(signed).ToString(CultureInfo.InvariantCulture);
            else debit = Math.Abs(signed).ToString(CultureInfo.InvariantCulture);

            // Three figures means the bank printed debit, credit and balance: the blank one is the
            // side this row is not.
            if (amounts.Count >= 3)
            {
                decimal d = Math.Abs(ParseAmount(amounts[0], out _));
                decimal c = Math.Abs(ParseAmount(amounts[1], out _));
                debit = d > 0m ? d.ToString(CultureInfo.InvariantCulture) : string.Empty;
                credit = c > 0m ? c.ToString(CultureInfo.InvariantCulture) : string.Empty;
            }

            string reference = string.Empty;
            var rm = RefInText.Match(rest);
            if (rm.Success) reference = rm.Value;

            return new[] { date, string.Empty, rest.Trim(), reference, debit, credit, balance };
        }

        /// <summary>A pattern somebody typed. An invalid one is reported and ignored, never thrown.</summary>
        private static Regex? Compile(string? pattern, List<string> warnings, string what)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return null;
            try
            {
                return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                    TimeSpan.FromMilliseconds(500));
            }
            catch (ArgumentException ex)
            {
                warnings.Add($"The PDF {what} pattern on this bank's format is not a valid expression and was "
                           + $"ignored: {ex.Message}");
                return null;
            }
        }

        /// <summary>The first worksheet of an .xlsx, arriving as base64 because the AES envelope
        /// carries text. Cell values are read as formatted strings so a date cell comes through as
        /// a date rather than as its serial number.</summary>
        private static List<string[]> ReadXlsx(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64.Trim());
            using var ms = new MemoryStream(bytes);
            using var book = new XLWorkbook(ms);
            var sheet = book.Worksheets.First();

            var used = sheet.RangeUsed();
            if (used == null) return new List<string[]>();

            int lastCol = used.LastColumn().ColumnNumber();
            var grid = new List<string[]>();
            foreach (var row in used.Rows())
            {
                var cells = new string[lastCol];
                for (int c = 1; c <= lastCol; c++)
                {
                    var cell = row.Cell(c);
                    cells[c - 1] = cell.DataType == XLDataType.DateTime && cell.TryGetValue<DateTime>(out var dt)
                        ? dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        : cell.GetFormattedString();
                }
                grid.Add(cells);
            }
            return grid;
        }
    }
}
