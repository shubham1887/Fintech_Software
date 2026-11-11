using System.Text.RegularExpressions;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BankConfig;
using FintechApp.Requests.Phase5;
using FintechApp.Responses.BankConfig;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IBankStatementConfigService" />
    ///
    /// Three pieces of configuration that were missing, and the reason each one matters:
    ///
    ///   WHICH BANKS ARE OURS. They were already maintained — under Payment Modes, as the
    ///   sub-modes a retailer deposits into, with the name, account number and IFSC on them. The
    ///   reconciliation side kept its own second list, so an operator had to type the same account
    ///   twice and the two could disagree about a digit. Now one is pulled from the other.
    ///
    ///   HOW EACH BANK WRITES A STATEMENT. Described at every upload before, which on a
    ///   white-label install is a per-client manual step, i.e. a defect: the person uploading the
    ///   statement is not the person who knows what a column map is. Now it is set once per bank.
    ///
    ///   WHAT A LINE MEANS. Nowhere at all before. Matching proves a line happened; only the
    ///   business knows that this debit was stock and that credit was a retailer's deposit.
    public class BankStatementConfigService : IBankStatementConfigService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBankStatementImportService _import;
        private readonly IBankLineClassifier _classifier;
        private readonly Logger _logger = new();

        public BankStatementConfigService(
            ApplicationDbContext context,
            IBankStatementImportService import,
            IBankLineClassifier classifier)
        {
            _context = context;
            _import = import;
            _classifier = classifier;
        }

        // ── Which banks are ours ────────────────────────────────────────────────────────────

        public async Task<BankConfigView> GetBanks()
        {
            var view = new BankConfigView();

            var banks = await _context.BankAccounts.AsNoTracking()
                .Where(b => !b.IsDeleted)
                .ToListAsync();

            var subModes = await _context.SubModes.AsNoTracking()
                .Where(s => !s.IsDelete)
                .ToListAsync();
            var modes = await _context.Modes.AsNoTracking()
                .Where(m => !m.IsDelete)
                .ToDictionaryAsync(m => m.ModeID, m => m.ModeName ?? string.Empty);

            var formatCounts = await _context.BankStatementFormats.AsNoTracking()
                .Where(f => !f.IsDeleted)
                .GroupBy(f => f.BankAccountID)
                .Select(g => new { BankAccountID = g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.BankAccountID, x => x.N);

            var imports = await _context.BankStatementImports.AsNoTracking()
                .Where(i => !i.IsDeleted)
                .GroupBy(i => i.BankAccountID)
                .Select(g => new { BankAccountID = g.Key, N = g.Count(), Last = g.Max(x => x.DateTime) })
                .ToListAsync();

            foreach (var b in banks.OrderBy(b => b.BankName).ThenBy(b => b.AccountName))
            {
                var sub = b.SubModeID > 0 ? subModes.FirstOrDefault(s => s.SubModeID == b.SubModeID) : null;
                var imp = imports.FirstOrDefault(i => i.BankAccountID == b.BankAccountID);

                var row = new BankConfigRow
                {
                    BankAccountID = b.BankAccountID,
                    AccountName = b.AccountName,
                    BankName = b.BankName,
                    AccountNumber = b.AccountNumber,
                    IFSC = b.IFSC,
                    LedgerAccountCode = b.LedgerAccountCode,
                    IsActive = b.IsActive,
                    SubModeID = b.SubModeID,
                    ModeID = b.ModeID,
                    ModeName = b.ModeID > 0 && modes.TryGetValue(b.ModeID, out var mn) ? mn : string.Empty,
                    SubModeName = sub?.SubModeName ?? string.Empty,
                    SubModeMissing = b.SubModeID > 0 && sub == null,
                    FormatCount = formatCounts.TryGetValue(b.BankAccountID, out var fc) ? fc : 0,
                    ImportCount = imp?.N ?? 0,
                    LastImportOn = imp?.Last,
                };
                row.Gap = DescribeGap(row);
                view.Banks.Add(row);
            }

            // Sub-modes that could be banks and are not linked yet. Only ones that look like a
            // bank account: a sub-mode called "Self" under Cash Deposit is not something anyone
            // reconciles a statement against, and offering it as one is noise.
            var linked = banks.Where(b => b.SubModeID > 0).Select(b => b.SubModeID).ToHashSet();
            foreach (var s in subModes.Where(s => !linked.Contains(s.SubModeID)))
            {
                string modeName = modes.TryGetValue(s.ModeID, out var mn) ? mn : string.Empty;
                if (!LooksLikeBank(modeName, s)) continue;

                view.UnlinkedSubModes.Add(new UnlinkedSubMode
                {
                    SubModeID = s.SubModeID,
                    ModeID = s.ModeID,
                    ModeName = modeName,
                    SubModeName = s.SubModeName ?? string.Empty,
                    BankName = string.IsNullOrWhiteSpace(s.BankName) ? s.SubModeName ?? string.Empty : s.BankName,
                    AccountNumber = s.AccountNumber ?? string.Empty,
                    IFSC = s.IFSC ?? string.Empty,
                    Gap = string.IsNullOrWhiteSpace(s.AccountNumber)
                        ? "No account number on the sub-mode. It can still be linked, but a statement cannot be matched to the account."
                        : string.Empty,
                });
            }

            view.Note = view.Banks.Count == 0
                ? "No bank is set up for reconciliation yet. Press Sync to bring in the accounts already "
                + "maintained under Payment Modes."
                : $"{view.Banks.Count} bank(s) set up"
                  + (view.UnlinkedSubModes.Count > 0
                      ? $", and {view.UnlinkedSubModes.Count} payment sub-mode(s) not yet brought in."
                      : ". Every bank sub-mode is linked.");

            return view;
        }

        public async Task<BankSyncResult> SyncBanksFromModes(int createdByUserID)
        {
            var result = new BankSyncResult();

            var modes = await _context.Modes.AsNoTracking()
                .Where(m => !m.IsDelete)
                .ToDictionaryAsync(m => m.ModeID, m => m.ModeName ?? string.Empty);

            var subModes = await _context.SubModes.AsNoTracking()
                .Where(s => !s.IsDelete && s.IsActive)
                .ToListAsync();

            var banks = await _context.BankAccounts
                .Where(b => !b.IsDeleted)
                .ToListAsync();

            foreach (var s in subModes)
            {
                string modeName = modes.TryGetValue(s.ModeID, out var mn) ? mn : string.Empty;
                if (!LooksLikeBank(modeName, s))
                {
                    result.Skipped++;
                    continue;
                }

                string bankName = string.IsNullOrWhiteSpace(s.BankName) ? (s.SubModeName ?? string.Empty) : s.BankName;
                var existing = banks.FirstOrDefault(b => b.SubModeID == s.SubModeID);

                if (existing == null)
                {
                    // An account already set up by hand with the same number is the SAME account.
                    // Linking it is right; creating a second one would split its statements in two.
                    existing = banks.FirstOrDefault(b => b.SubModeID == 0
                        && !string.IsNullOrWhiteSpace(s.AccountNumber)
                        && b.AccountNumber.Trim() == s.AccountNumber.Trim());

                    if (existing != null)
                    {
                        existing.SubModeID = s.SubModeID;
                        existing.ModeID = s.ModeID;
                        existing.UpdatedateTime = DateTime.UtcNow;
                        result.Updated++;
                        result.Notes.Add($"Linked the existing account {existing.AccountNumber} to the "
                                       + $"{modeName} → {s.SubModeName} sub-mode.");
                        continue;
                    }

                    _context.BankAccounts.Add(new BankAccount
                    {
                        AccountName = Trim(string.IsNullOrWhiteSpace(s.BeneficiaryName) ? s.SubModeName : s.BeneficiaryName, 120),
                        BankName = Trim(bankName, 120),
                        AccountNumber = Trim(s.AccountNumber ?? string.Empty, 40),
                        IFSC = Trim(s.IFSC ?? string.Empty, 15),
                        LedgerAccountCode = "1000",
                        SubModeID = s.SubModeID,
                        ModeID = s.ModeID,
                        IsActive = true,
                        DateTime = DateTime.UtcNow,
                        UpdatedateTime = DateTime.UtcNow,
                        IsDeleted = false,
                    });
                    result.Created++;
                    result.Notes.Add($"Added {bankName}"
                                   + (string.IsNullOrWhiteSpace(s.AccountNumber) ? " (no account number on the sub-mode yet)" : $" {s.AccountNumber}")
                                   + $" from {modeName} → {s.SubModeName}.");
                    continue;
                }

                // Linked already: keep the details in step with the sub-mode, which is the master.
                bool changed = false;
                if (!string.IsNullOrWhiteSpace(bankName) && existing.BankName != bankName) { existing.BankName = Trim(bankName, 120); changed = true; }
                if (!string.IsNullOrWhiteSpace(s.AccountNumber) && existing.AccountNumber != s.AccountNumber.Trim()) { existing.AccountNumber = Trim(s.AccountNumber, 40); changed = true; }
                if (!string.IsNullOrWhiteSpace(s.IFSC) && existing.IFSC != s.IFSC.Trim()) { existing.IFSC = Trim(s.IFSC, 15); changed = true; }
                if (existing.ModeID != s.ModeID) { existing.ModeID = s.ModeID; changed = true; }

                if (changed)
                {
                    existing.UpdatedateTime = DateTime.UtcNow;
                    result.Updated++;
                    result.Notes.Add($"Refreshed {existing.BankName} from its sub-mode.");
                }
            }

            if (result.Created > 0 || result.Updated > 0) await _context.SaveChangesAsync();

            result.Summary = result.Created == 0 && result.Updated == 0
                ? "Nothing to bring in — every bank sub-mode is already set up."
                : $"{result.Created} added, {result.Updated} refreshed from Payment Modes. "
                  + "Nothing is ever removed: a bank with statements against it stays, whatever happens to the sub-mode.";
            return result;
        }

        /// <summary>
        /// Is this sub-mode a bank account somebody could reconcile a statement against?
        ///
        /// A bank account is one that HAS an account number, or sits under a mode that is about
        /// banks. Cash and wallet sub-modes are deliberately left out: they are real payment modes
        /// and they have no statement to import.
        /// </summary>
        private static bool LooksLikeBank(string modeName, SubMode sub)
        {
            if (!string.IsNullOrWhiteSpace(sub.AccountNumber)) return true;
            if (!string.IsNullOrWhiteSpace(sub.IFSC)) return true;

            string m = (modeName ?? string.Empty).ToLowerInvariant();
            string s = (sub.SubModeName ?? string.Empty).ToLowerInvariant();
            string b = (sub.BankName ?? string.Empty).ToLowerInvariant();

            if (m.Contains("cash") || m.Contains("wallet") || m.Contains("credit") || m.Contains("office")) return false;
            if (m.Contains("bank") || m.Contains("neft") || m.Contains("imps") || m.Contains("rtgs")) return true;

            return b.Contains("bank") || s.Contains("bank");
        }

        private static string DescribeGap(BankConfigRow row)
        {
            if (row.SubModeMissing)
                return "The payment sub-mode it came from has been removed. The bank stays, but it is no longer kept in step.";
            if (string.IsNullOrWhiteSpace(row.AccountNumber))
                return "No account number. Fill it in on the payment sub-mode so a statement can be tied to the account.";
            if (row.FormatCount == 0)
                return "No statement format yet. CSV and Excel usually import without one; a PDF needs one.";
            return string.Empty;
        }

        // ── How each bank writes a statement ────────────────────────────────────────────────

        public async Task<List<BankStatementFormatRow>> GetFormats(int bankAccountId)
        {
            var formats = await _context.BankStatementFormats.AsNoTracking()
                .Where(f => !f.IsDeleted && (bankAccountId <= 0 || f.BankAccountID == bankAccountId))
                .ToListAsync();

            var bankNames = await _context.BankAccounts.AsNoTracking()
                .ToDictionaryAsync(b => b.BankAccountID, b => $"{b.BankName} {b.AccountNumber}".Trim());

            return formats
                .OrderBy(f => f.BankAccountID).ThenBy(f => f.Source).ThenBy(f => f.Name)
                .Select(f => new BankStatementFormatRow
                {
                    BankStatementFormatID = f.BankStatementFormatID,
                    BankAccountID = f.BankAccountID,
                    BankName = bankNames.TryGetValue(f.BankAccountID, out var bn) ? bn : $"bank {f.BankAccountID}",
                    Name = f.Name,
                    Source = BankStatementSource.Normalize(f.Source),
                    HeaderRow = f.HeaderRow,
                    SkipRows = f.SkipRows,
                    DateFormat = f.DateFormat,
                    TxnDateColumn = f.TxnDateColumn,
                    ValueDateColumn = f.ValueDateColumn,
                    DescriptionColumn = f.DescriptionColumn,
                    ReferenceColumn = f.ReferenceColumn,
                    DebitColumn = f.DebitColumn,
                    CreditColumn = f.CreditColumn,
                    AmountColumn = f.AmountColumn,
                    BalanceColumn = f.BalanceColumn,
                    PdfLinePattern = f.PdfLinePattern,
                    PdfSkipPattern = f.PdfSkipPattern,
                    PdfJoinWrappedLines = f.PdfJoinWrappedLines,
                    IsActive = f.IsActive,
                    Remark = f.Remark,
                    LastUsedOn = f.LastUsedOn,
                    Summary = SummariseFormat(f),
                })
                .ToList();
        }

        public async Task<(int FormatID, string Failure)> SaveFormat(SaveBankStatementFormatRequest request)
        {
            if (request.BankAccountID <= 0) return (0, "Pick the bank this format belongs to.");

            bool bankExists = await _context.BankAccounts
                .AnyAsync(b => b.BankAccountID == request.BankAccountID && !b.IsDeleted);
            if (!bankExists) return (0, "That bank does not exist.");

            string source = BankStatementSource.Normalize(request.Source);

            if (!string.IsNullOrWhiteSpace(request.PdfLinePattern))
            {
                string bad = CheckRegex(request.PdfLinePattern);
                if (bad.Length > 0) return (0, $"The PDF line pattern is not a valid expression: {bad}");
            }
            if (!string.IsNullOrWhiteSpace(request.PdfSkipPattern))
            {
                string bad = CheckRegex(request.PdfSkipPattern);
                if (bad.Length > 0) return (0, $"The PDF skip pattern is not a valid expression: {bad}");
            }
            if (!string.IsNullOrWhiteSpace(request.DateFormat))
            {
                try { _ = DateTime.Now.ToString(request.DateFormat); }
                catch (FormatException) { return (0, $"'{request.DateFormat}' is not a date format. Use something like dd/MM/yyyy."); }
            }

            // One ACTIVE format per bank per file type, because the importer picks by those two and
            // a second one would make which is used depend on the id.
            var clash = await _context.BankStatementFormats
                .FirstOrDefaultAsync(f => !f.IsDeleted && f.IsActive
                                       && f.BankAccountID == request.BankAccountID
                                       && f.Source == source
                                       && f.BankStatementFormatID != request.BankStatementFormatID);
            if (clash != null && request.IsActive)
                return (0, $"'{clash.Name}' is already the active {source} format for this bank. "
                         + "Deactivate it first, or edit it instead of adding another.");

            var entity = request.BankStatementFormatID > 0
                ? await _context.BankStatementFormats
                    .FirstOrDefaultAsync(f => f.BankStatementFormatID == request.BankStatementFormatID && !f.IsDeleted)
                : new BankStatementFormat { DateTime = DateTime.UtcNow, CreatedByUserID = request.CreatedByUserID };

            if (entity == null) return (0, "That format no longer exists.");

            entity.BankAccountID = request.BankAccountID;
            entity.Name = Trim(string.IsNullOrWhiteSpace(request.Name) ? $"{source} statement" : request.Name, 120);
            entity.Source = source;
            entity.HeaderRow = request.HeaderRow;
            entity.SkipRows = Math.Max(0, request.SkipRows);
            entity.DateFormat = Trim(request.DateFormat, 40);
            entity.TxnDateColumn = Trim(request.TxnDateColumn, 80);
            entity.ValueDateColumn = Trim(request.ValueDateColumn, 80);
            entity.DescriptionColumn = Trim(request.DescriptionColumn, 80);
            entity.ReferenceColumn = Trim(request.ReferenceColumn, 80);
            entity.DebitColumn = Trim(request.DebitColumn, 80);
            entity.CreditColumn = Trim(request.CreditColumn, 80);
            entity.AmountColumn = Trim(request.AmountColumn, 80);
            entity.BalanceColumn = Trim(request.BalanceColumn, 80);
            entity.PdfLinePattern = Trim(request.PdfLinePattern, 600);
            entity.PdfSkipPattern = Trim(request.PdfSkipPattern, 400);
            entity.PdfJoinWrappedLines = request.PdfJoinWrappedLines;
            entity.IsActive = request.IsActive;
            entity.Remark = Trim(request.Remark, 300);
            entity.UpdatedateTime = DateTime.UtcNow;

            if (request.BankStatementFormatID <= 0) _context.BankStatementFormats.Add(entity);
            await _context.SaveChangesAsync();
            return (entity.BankStatementFormatID, string.Empty);
        }

        public async Task<string> DeleteFormat(int formatId)
        {
            var entity = await _context.BankStatementFormats
                .FirstOrDefaultAsync(f => f.BankStatementFormatID == formatId && !f.IsDeleted);
            if (entity == null) return "That format no longer exists.";

            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<(BankFormatPreview? Preview, string Failure)> PreviewFormat(PreviewBankFormatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return (null, "Choose a statement file to try the format against.");

            BankStatementFormat? format = null;
            if (request.BankStatementFormatID > 0)
            {
                format = await _context.BankStatementFormats.AsNoTracking()
                    .FirstOrDefaultAsync(f => f.BankStatementFormatID == request.BankStatementFormatID && !f.IsDeleted);
                if (format == null) return (null, "That format no longer exists.");
            }
            else if (request.Format != null)
            {
                format = new BankStatementFormat
                {
                    BankAccountID = request.Format.BankAccountID,
                    Source = BankStatementSource.Normalize(request.Format.Source),
                    HeaderRow = request.Format.HeaderRow,
                    SkipRows = request.Format.SkipRows,
                    DateFormat = request.Format.DateFormat,
                    TxnDateColumn = request.Format.TxnDateColumn,
                    ValueDateColumn = request.Format.ValueDateColumn,
                    DescriptionColumn = request.Format.DescriptionColumn,
                    ReferenceColumn = request.Format.ReferenceColumn,
                    DebitColumn = request.Format.DebitColumn,
                    CreditColumn = request.Format.CreditColumn,
                    AmountColumn = request.Format.AmountColumn,
                    BalanceColumn = request.Format.BalanceColumn,
                    PdfLinePattern = request.Format.PdfLinePattern,
                    PdfSkipPattern = request.Format.PdfSkipPattern,
                    PdfJoinWrappedLines = request.Format.PdfJoinWrappedLines,
                };
            }

            // The preview runs the REAL importer in dry-run, so what it shows is what an import
            // would store. A preview with its own parser would agree with the importer right up
            // until the day it did not.
            var (dry, failure) = await _import.DryRun(new BankStatementImportRequest
            {
                BankAccountID = format?.BankAccountID ?? 0,
                BankStatementFormatID = request.BankStatementFormatID,
                Content = request.Content,
                FileName = request.FileName,
                Source = format?.Source ?? string.Empty,
            }, format, Math.Clamp(request.MaxRows <= 0 ? 15 : request.MaxRows, 1, 200));

            return failure.Length > 0 ? (null, failure) : (dry, string.Empty);
        }

        // ── What a line means ───────────────────────────────────────────────────────────────

        public async Task<List<BankLineCategoryRuleRow>> GetCategoryRules(int bankAccountId)
        {
            var rules = await _context.BankLineCategoryRules.AsNoTracking()
                .Where(r => !r.IsDeleted && (bankAccountId <= 0 || r.BankAccountID == 0 || r.BankAccountID == bankAccountId))
                .ToListAsync();

            var bankNames = await _context.BankAccounts.AsNoTracking()
                .ToDictionaryAsync(b => b.BankAccountID, b => $"{b.BankName} {b.AccountNumber}".Trim());

            var used = await _context.BankStatementLines.AsNoTracking()
                .Where(l => !l.IsDeleted && l.CategoryRuleID > 0)
                .GroupBy(l => l.CategoryRuleID)
                .Select(g => new { RuleID = g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.RuleID, x => x.N);

            return rules
                .OrderByDescending(r => r.Priority > 0 ? r.Priority : (r.BankAccountID > 0 ? 200 : 100))
                .ThenByDescending(r => (r.Pattern ?? string.Empty).Length)
                .Select(r => new BankLineCategoryRuleRow
                {
                    BankLineCategoryRuleID = r.BankLineCategoryRuleID,
                    BankAccountID = r.BankAccountID,
                    BankName = r.BankAccountID == 0
                        ? "Every bank"
                        : bankNames.TryGetValue(r.BankAccountID, out var bn) ? bn : $"bank {r.BankAccountID}",
                    Direction = BankLineDirection.Normalize(r.Direction),
                    MatchType = BankLineMatchType.Normalize(r.MatchType),
                    Pattern = r.Pattern,
                    MinAmount = r.MinAmount,
                    MaxAmount = r.MaxAmount,
                    Category = BankLineCategory.Normalize(r.Category),
                    LedgerAccountCode = r.LedgerAccountCode,
                    Priority = r.Priority,
                    IsActive = r.IsActive,
                    Remark = r.Remark,
                    MatchedLines = used.TryGetValue(r.BankLineCategoryRuleID, out var n) ? n : 0,
                    Summary = SummariseRule(r),
                })
                .ToList();
        }

        public async Task<(int RuleID, string Failure)> SaveCategoryRule(SaveBankLineCategoryRuleRequest request)
        {
            string pattern = (request.Pattern ?? string.Empty).Trim();
            if (pattern.Length == 0)
                return (0, "Enter the text to look for in the bank's narration — that is the whole rule.");

            string matchType = BankLineMatchType.Normalize(request.MatchType);
            if (matchType == BankLineMatchType.Regex)
            {
                string bad = CheckRegex(pattern);
                if (bad.Length > 0) return (0, $"That is not a valid expression: {bad}");
            }

            string category = BankLineCategory.Normalize(request.Category);
            if (category == BankLineCategory.Uncategorised)
                return (0, "Pick what this line is — purchase, a user's deposit, an expense, and so on.");

            string direction = BankLineDirection.Normalize(request.Direction);

            // A rule that would file a credit as an expense is a mis-booking waiting to happen,
            // and the natural side of each category is knowable, so it is refused here.
            string natural = BankLineCategory.NaturalSide(category);
            if (natural != BankLineDirection.Any && direction != BankLineDirection.Any && direction != natural)
                return (0, $"{category.Replace('_', ' ').ToLowerInvariant()} is money {(natural == BankLineDirection.Debit ? "out" : "in")}, "
                         + $"so a rule for it cannot fire on a {direction.ToLowerInvariant()} line.");

            if (request.MinAmount < 0m || request.MaxAmount < 0m)
                return (0, "An amount bound cannot be negative.");
            if (request.MaxAmount > 0m && request.MinAmount > request.MaxAmount)
                return (0, "The smallest amount is larger than the largest.");

            var entity = request.BankLineCategoryRuleID > 0
                ? await _context.BankLineCategoryRules
                    .FirstOrDefaultAsync(r => r.BankLineCategoryRuleID == request.BankLineCategoryRuleID && !r.IsDeleted)
                : new BankLineCategoryRule { DateTime = DateTime.UtcNow, CreatedByUserID = request.CreatedByUserID };

            if (entity == null) return (0, "That rule no longer exists.");

            entity.BankAccountID = Math.Max(0, request.BankAccountID);
            entity.Direction = direction == BankLineDirection.Any && natural != BankLineDirection.Any ? natural : direction;
            entity.MatchType = matchType;
            entity.Pattern = Trim(pattern, 300);
            entity.MinAmount = request.MinAmount;
            entity.MaxAmount = request.MaxAmount;
            entity.Category = category;
            entity.LedgerAccountCode = Trim(request.LedgerAccountCode, 20);
            entity.Priority = request.Priority;
            entity.IsActive = request.IsActive;
            entity.Remark = Trim(request.Remark, 300);
            entity.UpdatedateTime = DateTime.UtcNow;

            if (request.BankLineCategoryRuleID <= 0) _context.BankLineCategoryRules.Add(entity);
            await _context.SaveChangesAsync();
            return (entity.BankLineCategoryRuleID, string.Empty);
        }

        public async Task<string> DeleteCategoryRule(int ruleId)
        {
            var entity = await _context.BankLineCategoryRules
                .FirstOrDefaultAsync(r => r.BankLineCategoryRuleID == ruleId && !r.IsDeleted);
            if (entity == null) return "That rule no longer exists.";

            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<BankClassifyResult> Reclassify(int bankAccountId, int importId, int changedByUserID)
        {
            var result = new BankClassifyResult();

            var q = _context.BankStatementLines.Where(l => !l.IsDeleted);
            if (bankAccountId > 0) q = q.Where(l => l.BankAccountID == bankAccountId);
            if (importId > 0) q = q.Where(l => l.BankStatementImportID == importId);

            var lines = await q.ToListAsync();
            result.LinesSeen = lines.Count;
            if (lines.Count == 0)
            {
                result.Summary = "There are no statement lines to classify.";
                return result;
            }

            int bank = bankAccountId > 0 ? bankAccountId : lines[0].BankAccountID;

            result.Categorised = await _classifier.Classify(lines, bank);
            result.DuplicatesFlagged = await _classifier.FlagDuplicates(lines, bank);
            result.LeftAlone = lines.Count(l => l.CategorySetByUser);
            await _context.SaveChangesAsync();

            result.ByCategory = lines
                .GroupBy(l => BankLineCategory.Normalize(l.Category))
                .Select(g => new CategoryTally
                {
                    Category = g.Key,
                    Lines = g.Count(),
                    Debit = Math.Round(g.Sum(l => l.Debit), 2),
                    Credit = Math.Round(g.Sum(l => l.Credit), 2),
                })
                .OrderByDescending(t => t.Lines)
                .ToList();

            int uncategorised = result.ByCategory
                .FirstOrDefault(t => t.Category == BankLineCategory.Uncategorised)?.Lines ?? 0;

            result.Summary = $"{result.Categorised} line(s) changed, {uncategorised} still uncategorised"
                           + (result.LeftAlone > 0 ? $", {result.LeftAlone} left alone because a person set them" : string.Empty)
                           + (result.DuplicatesFlagged > 0 ? $", {result.DuplicatesFlagged} flagged as possible duplicates" : string.Empty)
                           + ".";
            return result;
        }

        public async Task<string> SetLineCategory(SetBankLineCategoryRequest request)
        {
            var line = await _context.BankStatementLines
                .FirstOrDefaultAsync(l => l.BankStatementLineID == request.BankStatementLineID && !l.IsDeleted);
            if (line == null) return "That statement line no longer exists.";

            string category = BankLineCategory.Normalize(request.Category);
            string side = line.Debit > 0m ? BankLineDirection.Debit : BankLineDirection.Credit;
            string natural = BankLineCategory.NaturalSide(category);
            if (natural != BankLineDirection.Any && natural != side)
                return $"This line is money {(side == BankLineDirection.Debit ? "out" : "in")}, "
                     + $"so it cannot be {category.Replace('_', ' ').ToLowerInvariant()}.";

            line.Category = category;
            line.CategorySetByUser = true;
            line.CategoryRuleID = 0;

            if (request.CounterpartyUserID > 0)
            {
                var user = await _context.UserMasters.AsNoTracking()
                    .Where(u => u.UserMasterID == request.CounterpartyUserID)
                    .Select(u => new { u.PersonName, u.FirmName, u.AliasName })
                    .FirstOrDefaultAsync();
                if (user == null) return "That user does not exist.";

                string name = !string.IsNullOrWhiteSpace(user.FirmName) ? user.FirmName!
                            : !string.IsNullOrWhiteSpace(user.PersonName) ? user.PersonName!
                            : $"user {request.CounterpartyUserID}";
                line.CounterpartyUserID = request.CounterpartyUserID;
                line.CounterpartyName = Trim(string.IsNullOrWhiteSpace(user.AliasName) ? name : $"{name} ({user.AliasName})", 150);
            }

            if (request.ProviderID > 0)
            {
                var provider = await _context.ProviderMasters.AsNoTracking()
                    .Where(p => p.ProviderID == request.ProviderID)
                    .Select(p => p.ProviderName)
                    .FirstOrDefaultAsync();
                if (provider == null) return "That provider does not exist.";
                line.CounterpartyUserID = 0;
                line.CounterpartyName = Trim(provider, 150);
            }
            else if (request.CounterpartyUserID <= 0 && category != BankLineCategory.Purchase && category != BankLineCategory.UserFund)
            {
                // An expense or a bank charge is about nobody in particular; a stale name from an
                // earlier guess would only mislead the export.
                line.CounterpartyUserID = 0;
                line.CounterpartyName = string.Empty;
            }

            if (request.ClearDuplicateFlag)
            {
                line.DuplicateOfLineID = 0;
                line.DuplicateReason = string.Empty;
            }

            line.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        // ── The same deposit claimed twice ──────────────────────────────────────────────────

        public async Task<DuplicateFundReport> GetDuplicateFundRequests(DateTime? from, DateTime? to)
        {
            DateTime start = (from ?? DateTime.Today.AddDays(1 - DateTime.Today.Day)).Date;
            DateTime end = (to ?? DateTime.Today).Date.AddDays(1);
            if (end <= start) end = start.AddDays(1);

            var report = new DuplicateFundReport { FromDate = start, ToDate = end.AddDays(-1) };

            var rows = await _context.FundTransactions.AsNoTracking()
                .Where(f => !f.IsDeleted && f.DateTime >= start && f.DateTime < end
                         && f.UTRNumber != null && f.UTRNumber != string.Empty)
                .Select(f => new
                {
                    f.FundTransactionID, f.DateTime, f.SenderID, f.Amount, f.UTRNumber,
                    f.Status, f.PayMode, f.BankName, f.Remark,
                })
                .ToListAsync();

            if (rows.Count == 0)
            {
                report.Summary = $"No fund request with a reference between {start:dd MMM yyyy} and {report.ToDate:dd MMM yyyy}.";
                return report;
            }

            var userIds = rows.Select(r => r.SenderID).Distinct().ToList();
            var users = await _context.UserMasters.AsNoTracking()
                .Where(u => userIds.Contains(u.UserMasterID))
                .Select(u => new { u.UserMasterID, u.PersonName, u.FirmName, u.AliasName })
                .ToListAsync();
            var userById = users.ToDictionary(u => u.UserMasterID, u => u);

            // What the bank itself says, where a statement covering these dates was imported.
            var bankCredits = await _context.BankStatementLines.AsNoTracking()
                .Where(l => !l.IsDeleted && l.Credit > 0m && l.ReferenceKey != string.Empty
                         && l.TxnDate >= start.AddDays(-7) && l.TxnDate < end.AddDays(7))
                .Select(l => new { l.ReferenceKey, l.Credit })
                .ToListAsync();
            var creditedByRef = bankCredits
                .GroupBy(l => l.ReferenceKey)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Credit), StringComparer.OrdinalIgnoreCase);

            foreach (var group in rows
                .GroupBy(r => NormaliseRef(r.UTRNumber ?? string.Empty))
                .Where(g => g.Key.Length > 0 && g.Count() > 1))
            {
                var claims = group
                    .OrderBy(r => r.DateTime)
                    .Select(r =>
                    {
                        userById.TryGetValue(r.SenderID, out var u);
                        string name = u == null ? $"user {r.SenderID}"
                            : !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName!
                            : !string.IsNullOrWhiteSpace(u.PersonName) ? u.PersonName!
                            : $"user {r.SenderID}";

                        return new DuplicateFundClaim
                        {
                            FundTransactionID = r.FundTransactionID,
                            DateTime = r.DateTime,
                            UserID = r.SenderID,
                            UserName = name,
                            UserCode = u?.AliasName ?? string.Empty,
                            Amount = r.Amount,
                            Status = r.Status ?? string.Empty,
                            PayMode = r.PayMode ?? string.Empty,
                            BankName = r.BankName ?? string.Empty,
                            Remark = Trim(r.Remark, 200),
                            Approved = IsApprovedStatus(r.Status),
                        };
                    })
                    .ToList();

                var approved = claims.Where(c => c.Approved).ToList();

                // Two requests for one deposit is only a problem when more than one was APPROVED.
                // A retailer re-raising a request that was rejected is ordinary behaviour.
                if (approved.Count <= 1) continue;

                decimal single = approved[0].Amount;
                decimal total = approved.Sum(c => c.Amount);
                creditedByRef.TryGetValue(group.Key, out decimal credited);

                var g = new DuplicateFundGroup
                {
                    Reference = group.First().UTRNumber ?? group.Key,
                    ClaimCount = claims.Count,
                    ApprovedCount = approved.Count,
                    ApprovedAmount = Math.Round(total, 2),
                    SingleAmount = Math.Round(single, 2),
                    Exposure = Math.Round(total - single, 2),
                    SeenOnStatement = credited > 0m,
                    BankCredited = Math.Round(credited, 2),
                    Claims = claims,
                };

                g.Verdict = credited > 0m
                    ? Math.Abs(credited - total) <= 1m
                        ? "The bank credited as much as was approved — the same reference was used for separate deposits."
                        : $"The bank credited {Money(credited)} against this reference while {Money(total)} was approved. "
                        + $"{Money(total - credited)} was funded on money the bank never received."
                    : "No bank statement line carries this reference yet. Import the statement for these dates to see "
                    + "what the bank actually credited.";

                report.Groups.Add(g);
            }

            report.Groups = report.Groups.OrderByDescending(x => x.Exposure).ToList();
            report.GroupCount = report.Groups.Count;
            report.TotalExposure = Math.Round(report.Groups.Sum(x => x.Exposure), 2);

            report.Summary = report.GroupCount == 0
                ? $"No deposit was claimed twice between {start:dd MMM yyyy} and {report.ToDate:dd MMM yyyy}."
                : $"{report.GroupCount} reference(s) claimed by more than one approved fund request, "
                  + $"{Money(report.TotalExposure)} funded beyond a single deposit.";

            return report;
        }

        private static bool IsApprovedStatus(string? status)
        {
            string s = (status ?? string.Empty).Trim().ToUpperInvariant();
            return s is "SUCCESS" or "APPROVED" or "ACCEPT" or "ACCEPTED" or "COMPLETE" or "COMPLETED";
        }

        private static string NormaliseRef(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return string.Empty;
            var chars = reference.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToUpperInvariant();
        }

        private static string Money(decimal v) => $"Rs {v:0.00}";

        // ── Plumbing ────────────────────────────────────────────────────────────────────────

        private static string CheckRegex(string pattern)
        {
            try
            {
                _ = new Regex(pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
                return string.Empty;
            }
            catch (ArgumentException ex) { return ex.Message; }
        }

        private static string SummariseFormat(BankStatementFormat f)
        {
            string src = BankStatementSource.Normalize(f.Source);
            var parts = new List<string> { $"Reads a {src} statement" };

            if (src == BankStatementSource.Pdf)
            {
                parts.Add(string.IsNullOrWhiteSpace(f.PdfLinePattern)
                    ? "by shape (date, narration, amounts at the end)"
                    : "with its own line pattern");
                if (!string.IsNullOrWhiteSpace(f.PdfSkipPattern)) parts.Add("skipping page furniture");
                if (f.PdfJoinWrappedLines) parts.Add("joining wrapped narrations");
            }
            else
            {
                bool mapped = new[] { f.TxnDateColumn, f.DescriptionColumn, f.DebitColumn, f.CreditColumn, f.AmountColumn }
                    .Any(v => !string.IsNullOrWhiteSpace(v));
                parts.Add(mapped ? "with the columns named here" : "detecting its columns");
                if (f.SkipRows > 0) parts.Add($"after {f.SkipRows} row(s) of preamble");
            }

            if (!string.IsNullOrWhiteSpace(f.DateFormat)) parts.Add($"dates as {f.DateFormat}");
            if (!f.IsActive) parts.Add("currently inactive");
            return string.Join(", ", parts) + ".";
        }

        private static string SummariseRule(BankLineCategoryRule r)
        {
            string dir = BankLineDirection.Normalize(r.Direction) switch
            {
                BankLineDirection.Debit => "a debit",
                BankLineDirection.Credit => "a credit",
                _ => "a line",
            };
            string how = BankLineMatchType.Normalize(r.MatchType) switch
            {
                BankLineMatchType.StartsWith => "starting with",
                BankLineMatchType.Regex => "matching",
                _ => "containing",
            };
            var parts = new List<string>
            {
                $"{dir} {how} \"{r.Pattern}\" is {BankLineCategory.Normalize(r.Category).Replace('_', ' ').ToLowerInvariant()}",
            };
            if (r.MinAmount > 0m || r.MaxAmount > 0m)
            {
                parts.Add(r.MaxAmount > 0m && r.MinAmount > 0m ? $"between Rs {r.MinAmount:0.00} and Rs {r.MaxAmount:0.00}"
                        : r.MinAmount > 0m ? $"above Rs {r.MinAmount:0.00}" : $"up to Rs {r.MaxAmount:0.00}");
            }
            if (!string.IsNullOrWhiteSpace(r.LedgerAccountCode)) parts.Add($"account {r.LedgerAccountCode}");
            if (!r.IsActive) parts.Add("currently inactive");
            return string.Join(", ", parts) + ".";
        }

        private static string Trim(string? s, int max)
            => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s.Trim() : s.Trim().Substring(0, max));
    }
}
