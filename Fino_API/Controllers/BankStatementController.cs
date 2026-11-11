using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase5;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Bank statements: upload one, list what has been uploaded, read its lines.
    ///
    /// Reads are open to the accounting desk. Import is a write — it supersedes the previous
    /// statement for the same range — and the WriteVerb regex does match "Import", so it is
    /// Admin+Support without needing a pin.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class BankStatementController : TaxControllerBase
    {
        private readonly IBankStatementImportService _statements;

        public BankStatementController(IBankStatementImportService statements, IConfiguration configuration)
            : base(configuration) => _statements = statements;

        /// <summary>Upload a statement (CSV text, or .xlsx as base64).</summary>
        [HttpPost("Import")]
        public Task<APIResponse> Import([FromForm] APIRequest apiRequest) =>
            Handle<BankStatementImportRequest>(apiRequest, async (request, keyData) =>
            {
                request.ImportedByUserID = keyData.UserID;
                var (result, failure) = await _statements.Import(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(result) : Refuse(failure);
            });

        /// <summary>The uploads for an account, newest first.</summary>
        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle<BankStatementQueryRequest>(apiRequest, async (request, _) =>
                Ok(await _statements.GetImports(request.BankAccountID)));

        /// <summary>The lines of one upload, or of an account over a date range.</summary>
        [HttpPost("Lines")]
        public Task<APIResponse> Lines([FromForm] APIRequest apiRequest) =>
            Handle<BankStatementQueryRequest>(apiRequest, async (request, _) =>
            {
                if (request.BankStatementImportID <= 0 && request.BankAccountID <= 0)
                    return Refuse("A bank account or an import is required.");
                return Ok(await _statements.GetLines(request));
            });
    }

    /// <summary>The bank accounts we reconcile, and the ledger account each maps to.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class BankAccountController : TaxControllerBase
    {
        private readonly IBankAccountService _accounts;

        public BankAccountController(IBankAccountService accounts, IConfiguration configuration)
            : base(configuration) => _accounts = accounts;

        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _accounts.GetAll()));

        /// <summary>Create or update one. "Save" is matched by the WriteVerb regex, so Admin+Support.</summary>
        [HttpPost("Save")]
        public Task<APIResponse> Save([FromForm] APIRequest apiRequest) =>
            Handle<SaveBankAccountRequest>(apiRequest, async (request, _) =>
            {
                var (id, failure) = await _accounts.Save(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(new { BankAccountID = id }) : Refuse(failure);
            });
    }
}
