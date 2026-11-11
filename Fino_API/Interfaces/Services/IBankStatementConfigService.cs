using FintechApp.Entities;
using FintechApp.Requests.BankConfig;
using FintechApp.Responses.BankConfig;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The configuration behind bank reconciliation: which banks are ours, how each one lays its
    /// statement out, and what a line on it means.
    ///
    /// All three used to be either hard-coded, passed at every upload, or nowhere at all.
    /// </summary>
    public interface IBankStatementConfigService
    {
        /// <summary>Every bank we reconcile, with its sub-mode link and how many formats it has.</summary>
        Task<BankConfigView> GetBanks();

        /// <summary>
        /// Pull the company's bank accounts from Payment Modes → sub-modes, which is where the
        /// operator already maintains them (the accounts a retailer deposits into). Creates what is
        /// missing, refreshes name / account number / IFSC on what is linked, and never deletes:
        /// a bank with statements imported against it stays, whatever happens to the sub-mode.
        /// </summary>
        Task<BankSyncResult> SyncBanksFromModes(int createdByUserID);

        /// <summary>The formats configured for one bank, or all of them when 0.</summary>
        Task<List<BankStatementFormatRow>> GetFormats(int bankAccountId);

        Task<(int FormatID, string Failure)> SaveFormat(SaveBankStatementFormatRequest request);
        Task<string> DeleteFormat(int formatId);

        /// <summary>
        /// Read a sample of a file through a format WITHOUT importing it, and show what each column
        /// was read as. The only honest way to set up a PDF pattern: try it, look at the rows, fix
        /// the pattern, try again.
        /// </summary>
        Task<(BankFormatPreview? Preview, string Failure)> PreviewFormat(PreviewBankFormatRequest request);

        /// <summary>The category rules, most specific first.</summary>
        Task<List<BankLineCategoryRuleRow>> GetCategoryRules(int bankAccountId);

        Task<(int RuleID, string Failure)> SaveCategoryRule(SaveBankLineCategoryRuleRequest request);
        Task<string> DeleteCategoryRule(int ruleId);

        /// <summary>
        /// Re-run the rules over statement lines that nobody has categorised by hand. Used after
        /// adding a rule, so an operator sees it take effect on what is already imported instead of
        /// re-uploading the statement.
        /// </summary>
        Task<BankClassifyResult> Reclassify(int bankAccountId, int importId, int changedByUserID);

        /// <summary>Set one line's category by hand. A human decision is never overwritten by a rule.</summary>
        Task<string> SetLineCategory(SetBankLineCategoryRequest request);

        /// <summary>
        /// Fund requests that claim the SAME deposit — the same UTR raised twice and approved
        /// twice. The bank credited once; the wallet was funded more than once.
        ///
        /// It does not need a statement to run: the duplicate is visible in the requests
        /// themselves. Where a statement HAS been imported, what the bank actually credited is
        /// shown beside the claims, which is what settles the argument.
        /// </summary>
        Task<DuplicateFundReport> GetDuplicateFundRequests(DateTime? from, DateTime? to);
    }

    /// <summary>
    /// What a statement line is, and who it is about — decided by the rules, from the bank's own
    /// narration.
    /// </summary>
    public interface IBankLineClassifier
    {
        /// <summary>
        /// Classify lines in place: category, the rule that decided, and the counterparty where the
        /// narration names somebody we can identify. Returns how many changed.
        /// </summary>
        Task<int> Classify(List<BankStatementLine> lines, int bankAccountId);

        /// <summary>
        /// Flag lines that look like the same money twice — the same reference and amount on two
        /// lines, or a credit that answers a fund request already settled by another line.
        /// </summary>
        Task<int> FlagDuplicates(List<BankStatementLine> lines, int bankAccountId);
    }
}
