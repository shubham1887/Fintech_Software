using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Gst;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// GST collection: what was collected, what may be claimed, and what is left to pay — with the
    /// inclusive / exclusive split the accounting desk asks for.
    ///
    /// Read-only, so the desk reads it and nobody can change anything through it. It is also the
    /// first screen that puts the two GST stamps side by side: the output tax the returns are built
    /// from, and the per-charge breakdown that says how each figure was arrived at. Where they
    /// disagree the report names the difference and what to do about it, because a GST report whose
    /// totals quietly absorb a gap is worse than no report.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class GstCollectionController : TaxControllerBase
    {
        private readonly IGstCollectionReportService _report;

        public GstCollectionController(IGstCollectionReportService report, IConfiguration configuration)
            : base(configuration) => _report = report;

        /// <summary>The report for a period, optionally for one service.</summary>
        [HttpPost("Report")]
        public Task<APIResponse> Report([FromForm] APIRequest apiRequest) =>
            Handle<GstCollectionReportRequest>(apiRequest, async (request, _) =>
                Ok(await _report.Build(request)));
    }
}
