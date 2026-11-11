using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase6;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The GST returns: GSTR-1, GSTR-3B and GSTR-9, all derived from documents already issued.
    ///
    /// Building and exporting a return CHANGE NOTHING, so the whole accounting desk may do both —
    /// an accountant who cannot produce the return cannot do the job the role exists for. Saving a
    /// snapshot is pinned to Admin below, because marking a period filed is what GSTR-9 later
    /// reconciles against and what freezes the declared figures; neither "Save" nor "MarkFiled"
    /// would be caught by the WriteVerb regex on its own.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class GstReturnController : TaxControllerBase
    {
        private readonly IGstr1Service _gstr1;
        private readonly IGstr3bService _gstr3b;
        private readonly IGstr9Service _gstr9;
        private readonly IGstReturnFilingService _filings;
        private readonly FintechApp.Services.IGstMonthService _month;

        public GstReturnController(
            IGstr1Service gstr1, IGstr3bService gstr3b, IGstr9Service gstr9,
            IGstReturnFilingService filings, FintechApp.Services.IGstMonthService month, IConfiguration configuration)
            : base(configuration)
        {
            _month = month;
            _gstr1 = gstr1;
            _gstr3b = gstr3b;
            _gstr9 = gstr9;
            _filings = filings;
        }

        /// <summary>
        /// The month on one page: sales, purchases, confirmed credit, what is left to pay, and
        /// which step is still open. A read — it changes nothing.
        /// </summary>
        [HttpPost("MonthSummary")]
        public Task<APIResponse> MonthSummary([FromForm] APIRequest apiRequest) =>
            Handle<GstMonthRequest>(apiRequest, async (request, _) =>
            {
                var (result, failure) = await _month.Summary(request.PeriodKey);
                return result == null ? Refuse(failure) : Ok(result);
            });

        /// <summary>
        /// Upload the GSTR-2B file and let everything that follows from it happen: store it,
        /// match it to the purchase bills, claim the credit on the bills the supplier filed.
        /// Claiming credit moves the return, so this is pinned to Admin in ApiAccessPolicy.
        /// </summary>
        [HttpPost("Import2b")]
        public Task<APIResponse> Import2b([FromForm] APIRequest apiRequest) =>
            Handle<Gstr2bAutoRequest>(apiRequest, async (request, keyData) =>
            {
                var (result, failure) = await _month.Import2bAndClaim(request.Json, request.PeriodKey, keyData.UserID);
                return result == null ? Refuse(failure) : Ok(result);
            });

        /// <summary>Outward supplies, in the portal's own tables.</summary>
        [HttpPost("Gstr1")]
        public Task<APIResponse> Gstr1([FromForm] APIRequest apiRequest) =>
            Handle<GstReturnRequest>(apiRequest, async (request, _) =>
            {
                var (result, failure) = await _gstr1.Build(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>The monthly summary, the tax payable, and the GSTR-1 cross-check.</summary>
        [HttpPost("Gstr3b")]
        public Task<APIResponse> Gstr3b([FromForm] APIRequest apiRequest) =>
            Handle<GstReturnRequest>(apiRequest, async (request, _) =>
            {
                var (result, failure) = await _gstr3b.Build(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>The year consolidated, books beside what was filed.</summary>
        [HttpPost("Gstr9")]
        public Task<APIResponse> Gstr9([FromForm] APIRequest apiRequest) =>
            Handle<Gstr9Request>(apiRequest, async (request, _) =>
            {
                var (result, failure) = await _gstr9.Build(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>The snapshots held, newest period first.</summary>
        [HttpPost("Filings")]
        public Task<APIResponse> Filings([FromForm] APIRequest apiRequest) =>
            Handle<GstReturnFilingQuery>(apiRequest, async (request, _) =>
                Ok(await _filings.GetFilings(request)));

        /// <summary>A downloadable JSON (the offline-utility shape) or Excel workbook.</summary>
        [HttpPost("Export")]
        public Task<APIResponse> Export([FromForm] APIRequest apiRequest) =>
            Handle<GstReturnExportRequest>(apiRequest, async (request, _) =>
            {
                var (export, failure) = await _filings.Export(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(export) : Refuse(failure);
            });

        /// <summary>Keep this generation, optionally declaring it filed.</summary>
        [HttpPost("Save")]
        public Task<APIResponse> Save([FromForm] APIRequest apiRequest) =>
            Handle<SaveGstReturnRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (row, failure) = await _filings.Save(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(row) : Refuse(failure);
            });
    }
}
