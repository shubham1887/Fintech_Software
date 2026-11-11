using FintechApp.Entities;
using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Imports GSTR-2B — the government's static statement of what our suppliers filed about us —
    /// as a snapshot per registration per period. A re-import supersedes the previous snapshot
    /// rather than merging, because a later 2B can drop a line a supplier amended.
    /// </summary>
    public interface IGstr2bImportService
    {
        Task<(Gstr2bImportResult? Result, string Failure)> Import(Gstr2bImportRequest request);

        /// <summary>The import snapshots for a registration, newest first.</summary>
        Task<List<Gstr2bImport>> GetImports(string companyGstin);

        /// <summary>The lines of the active snapshot for a period.</summary>
        Task<List<Gstr2bLine>> GetActiveLines(string companyGstin, string periodKey);
    }
}
