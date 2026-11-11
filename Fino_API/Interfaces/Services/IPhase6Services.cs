using FintechApp.Requests.Phase6;
using FintechApp.Responses.Phase6;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// GSTR-1: outward supplies, document by document, sorted into the portal's own tables.
    ///
    /// Everything is derived from documents already issued — Phase 6 enters nothing twice. The one
    /// decision this service makes that is not arithmetic is which TABLE a document belongs in,
    /// and that is driven by the party's registration status, the supply's nature and the invoice
    /// value, never by anything typed.
    /// </summary>
    public interface IGstr1Service
    {
        Task<(Gstr1Result? Result, string Failure)> Build(GstReturnRequest request);
    }

    /// <summary>
    /// GSTR-3B: the monthly summary and the tax actually payable, with the GSTR-1 comparison built
    /// in — a 1-vs-3B mismatch is the most common notice a taxpayer gets, so the difference is
    /// computed rather than left to be noticed.
    /// </summary>
    public interface IGstr3bService
    {
        Task<(Gstr3bResult? Result, string Failure)> Build(GstReturnRequest request);
    }

    /// <summary>
    /// GSTR-9: the year consolidated, with the books beside what was actually filed. The second
    /// column comes from the saved snapshots, never from recomputing — recomputing would answer
    /// the first question twice and hide the difference the annual return exists to surface.
    /// </summary>
    public interface IGstr9Service
    {
        Task<(Gstr9Result? Result, string Failure)> Build(Gstr9Request request);
    }

    /// <summary>Keeping a return: the snapshot, the filed flag, and the downloadable file.</summary>
    public interface IGstReturnFilingService
    {
        Task<(GstReturnFilingRow? Row, string Failure)> Save(SaveGstReturnRequest request);
        Task<List<GstReturnFilingRow>> GetFilings(GstReturnFilingQuery query);
        Task<(GstReturnExport? Export, string Failure)> Export(GstReturnExportRequest request);
    }
}
