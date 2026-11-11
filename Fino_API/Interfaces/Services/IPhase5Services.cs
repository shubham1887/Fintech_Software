using FintechApp.Entities;
using FintechApp.Requests.Phase5;
using FintechApp.Responses.Phase5;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Reads a bank statement into rows. CSV and XLSX, with the columns detected rather than
    /// declared — every bank lays a statement out differently, and an operator who has to describe
    /// their file before they can upload it will not upload it.
    /// </summary>
    public interface IBankStatementImportService
    {
        Task<(BankStatementImportResult? Result, string Failure)> Import(BankStatementImportRequest request);

        /// <summary>
        /// Parse a file exactly as <see cref="Import"/> would and return the first rows, WITHOUT
        /// storing anything. The format screen's "try it" — and it runs the real parser on purpose,
        /// because a preview with its own would agree with the importer right up until it did not.
        /// </summary>
        Task<(Responses.BankConfig.BankFormatPreview? Preview, string Failure)> DryRun(
            BankStatementImportRequest request, Entities.BankStatementFormat? format, int maxRows);

        /// <summary>The uploads for an account, newest first.</summary>
        Task<List<BankStatementImportRow>> GetImports(int bankAccountId);

        /// <summary>The lines of one import, or of the active import covering a range.</summary>
        Task<List<BankStatementLine>> GetLines(BankStatementQueryRequest query);
    }

    /// <summary>
    /// Bank statement against the ledger's Bank account, in four buckets.
    ///
    /// Preview changes nothing. Commit writes the pairings, which is what makes a ledger line
    /// known-reconciled for every later statement.
    /// </summary>
    public interface IBankReconService
    {
        Task<(BankReconResult? Result, string Failure)> Reconcile(BankReconRequest request, bool commit);

        /// <summary>Pair a statement line to a ledger line by hand.</summary>
        Task<string> ManualMatch(BankManualMatchRequest request);

        /// <summary>Break a pairing.</summary>
        Task<string> Unmatch(BankUnmatchRequest request);
    }

    /// <summary>The bank accounts we reconcile, and the ledger account each maps to.</summary>
    public interface IBankAccountService
    {
        Task<List<BankAccount>> GetAll();
        Task<(int BankAccountID, string Failure)> Save(SaveBankAccountRequest request);
    }
}
