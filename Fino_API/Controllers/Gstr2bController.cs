using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase3;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// GSTR-2B: the government's statement of what our suppliers filed about us. Importing it is
    /// operational (Admin + Support) — it decides nothing and a re-import simply supersedes the last
    /// snapshot. The reconciliation against our books lives on the PurchaseRecon controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class Gstr2bController : TaxControllerBase
    {
        private readonly IGstr2bImportService _service;

        public Gstr2bController(IGstr2bImportService service, IConfiguration configuration)
            : base(configuration) => _service = service;

        /// <summary>Import a 2B snapshot for a period from the portal JSON.</summary>
        [HttpPost("Import")]
        public Task<APIResponse> Import([FromForm] APIRequest apiRequest) =>
            Handle<Gstr2bImportRequest>(apiRequest, async (request, keyData) =>
            {
                request.ImportedByUserID = keyData.UserID;
                var (result, failure) = await _service.Import(request);
                return result == null ? Refuse(failure) : Ok(result);
            });

        /// <summary>The import snapshots for a registration, newest first.</summary>
        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle<Gstr2bQueryRequest>(apiRequest, async (request, _) =>
            {
                if (string.IsNullOrWhiteSpace(request.CompanyGSTIN)) return Refuse("The company GSTIN is required.");
                return Ok(await _service.GetImports(request.CompanyGSTIN));
            });

        /// <summary>The lines of the active snapshot for a period.</summary>
        [HttpPost("Lines")]
        public Task<APIResponse> Lines([FromForm] APIRequest apiRequest) =>
            Handle<Gstr2bQueryRequest>(apiRequest, async (request, _) =>
            {
                if (string.IsNullOrWhiteSpace(request.CompanyGSTIN) || string.IsNullOrWhiteSpace(request.PeriodKey))
                    return Refuse("The company GSTIN and period are required.");
                return Ok(await _service.GetActiveLines(request.CompanyGSTIN, request.PeriodKey));
            });
    }

    /// <summary>Read filter for the 2B screens.</summary>
    public class Gstr2bQueryRequest
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
    }
}
