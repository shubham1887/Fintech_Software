using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Plan;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Fino.PlanApi.Interfaces;
using Fino.PlanApi.Messages;
using Fino.PlanApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Live plan-data lookups (planapi.in) for the recharge screen: operator detection, prepaid
    /// plans, R-Offer, DTH operator detection, DTH plans and DTH customer info.
    /// Every action returns the provider's own JSON (AES-encrypted in Data, like the rest of the API).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ROfferController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IPlanApiService _planApi;
        private readonly IPlanOperatorCodeService _planOperators;

        public ROfferController(IConfiguration configuration, IPlanApiService planApi, IPlanOperatorCodeService planOperators)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _planApi = planApi;
            _planOperators = planOperators;
        }

        // Decrypts keyData + request and validates the session. Returns the parsed request, or an error response.
        private (KeyDataModel? key, PlanFetchAppRequest? req, APIResponse? error) Parse(APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            PlanFetchAppRequest? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<PlanFetchAppRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured)); }
            if (keyDataModel == null || req == null) return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (null, null, ResponseProcess.GetFailResponse(fail));
            return (keyDataModel, req, null);
        }

        // The provider replies with JSON already; Data carries it encrypted so the client decrypts
        // it exactly like every other endpoint's payload.
        private APIResponse Result(PlanApiResponse result)
        {
            if (!result.Success) return ResponseProcess.GetFailResponse(result.Message);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(result.Data));
        }

        // The DTH/customer number may arrive as DthNumber or (for the shared recharge form) MobileNo.
        private static string Number(PlanFetchAppRequest req)
            => string.IsNullOrWhiteSpace(req.DthNumber) ? req.MobileNo : req.DthNumber;

        /// <summary>
        /// The operator code to send the provider, resolved in this order:
        ///
        ///   1. The caller's own OperatorID — authoritative, because it is the operator the user
        ///      actually picked on screen (see <see cref="PlanApiCatalog"/> for the code table).
        ///   2. OpCode / OperatorCode, when they already carry a provider code (all digits). Kept
        ///      numeric-but-unknown-friendly so a code the provider adds after this build still works.
        ///
        /// Anything else — notably the OperatorMaster routing mnemonics ("AIRT", "VODA", "TPLY")
        /// that the app's clients used to send — resolves to empty, and the caller reports that
        /// plans are unavailable. Forwarding those made the provider answer "Wrong Operator Code".
        /// </summary>
        private async Task<string> ProviderOpCode(PlanFetchAppRequest req)
        {
            if (req.OperatorID > 0)
            {
                string resolved = await _planOperators.ResolveOpCode(req.OperatorID);
                if (resolved.Length > 0) return resolved;
            }
            foreach (string candidate in new[] { req.OpCode, req.OperatorCode })
            {
                string value = (candidate ?? string.Empty).Trim();
                if (value.Length > 0 && value.All(char.IsDigit)) return value;
            }
            return string.Empty;
        }

        // Detect operator + circle/state for a mobile number (live via planapi.in OperatorFetchNew).
        // This is the logged-in (JWT + encrypted) counterpart of PublicAPI/GetOperatorByMobileNumber,
        // so the recharge screen can auto-fill the operator and store the circle the moment a 10-digit
        // number is typed. Returns the provider JSON: Operator / OpCode / Circle / CircleCode.
        [HttpPost("GetOperatorByMobile")]
        public async Task<APIResponse> GetOperatorByMobile([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            if (string.IsNullOrWhiteSpace(req!.MobileNo)) return ResponseProcess.GetFailResponse(Plan.Message.MobileNumberRequired);
            return Result(await _planApi.OperatorFetch(req.MobileNo));
        }

        // Prepaid mobile plans for an operator + circle (live).
        [HttpPost("SelectPlan")]
        public async Task<APIResponse> SelectPlan([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            string opCode = await ProviderOpCode(req!);
            if (string.IsNullOrWhiteSpace(opCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Result(await _planApi.MobilePlans(opCode, req!.Circle));
        }

        // Special/loyalty offers (R-Offer) for a mobile number (live).
        [HttpPost("SelectOffer")]
        public async Task<APIResponse> SelectOffer([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            if (string.IsNullOrWhiteSpace(req!.MobileNo)) return ResponseProcess.GetFailResponse(Plan.Message.MobileNumberRequired);
            string opCode = await ProviderOpCode(req);
            if (string.IsNullOrWhiteSpace(opCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Result(await _planApi.RofferCheck(opCode, req.MobileNo));
        }

        // Detect the DTH operator from a customer/VC number (live via planapi.in DthOperatorFetch).
        // Returns the provider JSON: DthNumber / DthName / DthOpCode.
        [HttpPost("GetOperatorByDTHNumber")]
        public async Task<APIResponse> GetOperatorByDTHNumber([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.DthNumberRequired);
            return Result(await _planApi.DthOperatorFetch(number));
        }

        // DTH recharge plans for an operator (live).
        [HttpPost("SelectDTHPlan")]
        public async Task<APIResponse> SelectDTHPlan([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            string opCode = await ProviderOpCode(req!);
            if (string.IsNullOrWhiteSpace(opCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Result(await _planApi.DthPlans(opCode));
        }

        // Full DTH customer subscription info — VC, name, RMN, balance, plan, next recharge date and
        // address. NOTE: the provider bills this call as 3 hits, so the UI keeps it behind an explicit
        // button rather than firing it while the number is typed.
        [HttpPost("SelectDTHCustInfo")]
        public async Task<APIResponse> SelectDTHCustInfo([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Result(await _planApi.DthInfoCheck(await ProviderOpCode(req), number));
        }

        // Basic DTH details only — VC, name, RMN, address. The cheap counterpart of SelectDTHCustInfo,
        // enough to confirm the customer before a recharge.
        [HttpPost("SelectDTHBasicInfo")]
        public async Task<APIResponse> SelectDTHBasicInfo([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Result(await _planApi.DthBasicDetails(await ProviderOpCode(req), number));
        }

        // DTH customer info including the last recharge date.
        [HttpPost("SelectDTHInfoWithLastRecharge")]
        public async Task<APIResponse> SelectDTHInfoWithLastRecharge([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Result(await _planApi.DthInfoWithLastRecharge(await ProviderOpCode(req), number));
        }

        // --- Provider reference data (PlanApiCatalog) ------------------------------------------

        // The plan provider's own operator codes, optionally filtered by Type ("DTH", "PREPAID",
        // "POSTPAID", …). Lets an admin screen show/choose the code an operator must be mapped to.
        [HttpPost("OperatorCodes")]
        public async Task<APIResponse> OperatorCodes([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            var list = string.IsNullOrWhiteSpace(req!.Type) ? PlanApiCatalog.Operators : PlanApiCatalog.OfType(req.Type);
            return await Task.FromResult(ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(list))));
        }

        // The plan provider's own circle/state codes. The mobile-plan lookup returns nothing without
        // the right CircleCode, so the recharge screen offers these when auto-detection comes up empty.
        [HttpPost("Circles")]
        public async Task<APIResponse> Circles([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            return await Task.FromResult(ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(PlanApiCatalog.Circles))));
        }

        // The provider OpCode an app operator resolves to — so an admin can confirm the mapping
        // (and see which operators have no counterpart in the provider's catalog).
        [HttpPost("ResolveOperatorCode")]
        public async Task<APIResponse> ResolveOperatorCode([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            string code = await ProviderOpCode(req!);
            var match = PlanApiCatalog.FindByCode(code);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new
            {
                req!.OperatorID,
                OpCode = code,
                Name = match?.Name ?? string.Empty,
                Type = match?.Type ?? string.Empty,
                Resolved = code.Length > 0,
            })));
        }

        // No server-side cache to refresh — plans are fetched live per request from the provider.
        [HttpPost("HeavyRefresh")]
        public async Task<APIResponse> HeavyRefresh([FromForm] APIRequest apiRequest)
        {
            var (key, req, error) = Parse(apiRequest);
            if (error != null) return error;
            return await Task.FromResult(ResponseProcess.GetSuccessResponse(Plan.Message.CacheNotRequired));
        }
    }
}
