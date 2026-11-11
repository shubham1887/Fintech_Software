using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.BankConfig;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The configuration behind bank reconciliation: which banks are ours, how each one writes its
    /// statement, and what a line on it means.
    ///
    /// Three things were either in two places, asked at every upload, or nowhere:
    ///
    ///   * the company's bank accounts already lived under Payment Modes as the sub-modes a
    ///     retailer deposits into. Reconciliation kept a second list beside it, so the same account
    ///     was typed twice and the two could disagree. Sync pulls one from the other.
    ///   * the shape of each bank's statement was described at every upload. On a white-label
    ///     install that is a per-client manual step, which is a defect: the person uploading the
    ///     file is not the person who knows what a column map is. Now it is set once, PDF included.
    ///   * what a line IS — stock bought, a retailer's deposit, an expense — had nowhere to live.
    ///     Matching proves a line happened; only the business knows what it was for.
    ///
    /// Reading is the accounting desk's. Changing configuration, and anything that re-writes what a
    /// line is taken to be, is Admin's — it decides how money is read for every statement after it.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class BankSetupController : TaxControllerBase
    {
        private readonly IBankStatementConfigService _config;

        public BankSetupController(IBankStatementConfigService config, IConfiguration configuration)
            : base(configuration) => _config = config;

        /// <summary>Every bank we reconcile, plus the payment sub-modes not yet brought in.</summary>
        [HttpPost("Banks")]
        public Task<APIResponse> Banks([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _config.GetBanks()));

        /// <summary>Pull the company's bank accounts from Payment Modes. Adds and refreshes; never removes.</summary>
        [HttpPost("SyncBanks")]
        public Task<APIResponse> SyncBanks([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async keyData => Ok(await _config.SyncBanksFromModes(keyData.UserID)));

        /// <summary>The statement formats, for one bank or all of them.</summary>
        [HttpPost("Formats")]
        public Task<APIResponse> Formats([FromForm] APIRequest apiRequest) =>
            Handle<BankFormatIdRequest>(apiRequest, async (request, _) =>
                Ok(await _config.GetFormats(request.BankStatementFormatID)));

        /// <summary>Create or update one bank's statement format.</summary>
        [HttpPost("SaveFormat")]
        public Task<APIResponse> SaveFormat([FromForm] APIRequest apiRequest) =>
            Handle<SaveBankStatementFormatRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (id, failure) = await _config.SaveFormat(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(new { BankStatementFormatID = id }) : Refuse(failure);
            });

        [HttpPost("DeleteFormat")]
        public Task<APIResponse> DeleteFormat([FromForm] APIRequest apiRequest) =>
            Handle<BankFormatIdRequest>(apiRequest, async (request, _) =>
            {
                string r = await _config.DeleteFormat(request.BankStatementFormatID);
                return r == StaticModels.StatusType.Ok ? Ok(new { Deleted = true }) : Refuse(r);
            });

        /// <summary>
        /// Try a format against a real file and see the rows it produces, importing nothing. The
        /// only honest way to set up a PDF: try it, look at what came out, fix the pattern.
        /// </summary>
        [HttpPost("PreviewFormat")]
        public Task<APIResponse> PreviewFormat([FromForm] APIRequest apiRequest) =>
            Handle<PreviewBankFormatRequest>(apiRequest, async (request, _) =>
            {
                var (preview, failure) = await _config.PreviewFormat(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(preview) : Refuse(failure);
            });

        /// <summary>The rules that say what a statement line is.</summary>
        [HttpPost("Rules")]
        public Task<APIResponse> Rules([FromForm] APIRequest apiRequest) =>
            Handle<BankRuleIdRequest>(apiRequest, async (request, _) =>
                Ok(await _config.GetCategoryRules(request.BankLineCategoryRuleID)));

        [HttpPost("SaveRule")]
        public Task<APIResponse> SaveRule([FromForm] APIRequest apiRequest) =>
            Handle<SaveBankLineCategoryRuleRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (id, failure) = await _config.SaveCategoryRule(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(new { BankLineCategoryRuleID = id }) : Refuse(failure);
            });

        [HttpPost("DeleteRule")]
        public Task<APIResponse> DeleteRule([FromForm] APIRequest apiRequest) =>
            Handle<BankRuleIdRequest>(apiRequest, async (request, _) =>
            {
                string r = await _config.DeleteCategoryRule(request.BankLineCategoryRuleID);
                return r == StaticModels.StatusType.Ok ? Ok(new { Deleted = true }) : Refuse(r);
            });

        /// <summary>Re-run the rules over statements already imported, after adding a rule.</summary>
        [HttpPost("Reclassify")]
        public Task<APIResponse> Reclassify([FromForm] APIRequest apiRequest) =>
            Handle<ReclassifyRequest>(apiRequest, async (request, keyData) =>
                Ok(await _config.Reclassify(request.BankAccountID, request.BankStatementImportID, keyData.UserID)));

        /// <summary>A person deciding what one line is. No rule overwrites it afterwards.</summary>
        [HttpPost("SetLineCategory")]
        public Task<APIResponse> SetLineCategory([FromForm] APIRequest apiRequest) =>
            Handle<SetBankLineCategoryRequest>(apiRequest, async (request, keyData) =>
            {
                request.ChangedByUserID = keyData.UserID;
                string r = await _config.SetLineCategory(request);
                return r == StaticModels.StatusType.Ok ? Ok(new { Saved = true }) : Refuse(r);
            });

        /// <summary>
        /// Fund requests claiming the same deposit — the same UTR approved more than once. Read-only
        /// and it needs no statement, though it says more when one has been imported.
        /// </summary>
        [HttpPost("DuplicateFunds")]
        public Task<APIResponse> DuplicateFunds([FromForm] APIRequest apiRequest) =>
            Handle<Requests.Gst.GstCollectionReportRequest>(apiRequest, async (request, _) =>
                Ok(await _config.GetDuplicateFundRequests(request.FromDate, request.ToDate)));
    }
}
