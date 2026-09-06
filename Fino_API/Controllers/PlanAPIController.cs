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
    /// Plan-data endpoints for API/app consumers, mirroring <see cref="ROfferController"/>.
    /// Each lookup comes in two shapes: a GET variant (plain query parameters, provider JSON returned
    /// as-is) for server-to-server callers, and a POST variant (AES-encrypted request + response)
    /// for the app clients.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PlanAPIController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IPlanApiService _planApi;
        private readonly IPlanOperatorCodeService _planOperators;

        public PlanAPIController(IConfiguration configuration, IPlanApiService planApi, IPlanOperatorCodeService planOperators)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _planApi = planApi;
            _planOperators = planOperators;
        }

        // Decrypts keyData + request and validates the session. Returns the parsed request, or an error response.
        private (PlanFetchAppRequest? req, APIResponse? error) Parse(APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            PlanFetchAppRequest? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<PlanFetchAppRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return (null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured)); }
            if (keyDataModel == null || req == null) return (null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (null, ResponseProcess.GetFailResponse(fail));
            return (req, null);
        }

        // POST shape — provider JSON encrypted into Data, like every other app endpoint.
        private APIResponse Encrypted(PlanApiResponse result)
        {
            if (!result.Success) return ResponseProcess.GetFailResponse(result.Message);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(result.Data));
        }

        // GET shape — provider JSON verbatim for server-to-server callers.
        private static APIResponse Plain(PlanApiResponse result)
        {
            if (!result.Success) return ResponseProcess.GetFailResponse(result.Message);
            return ResponseProcess.GetSuccessResponse(result.Data);
        }

        private static string Number(PlanFetchAppRequest req)
            => string.IsNullOrWhiteSpace(req.DthNumber) ? req.MobileNo : req.DthNumber;

        /// <summary>
        /// Same resolution as <see cref="ROfferController"/>: the caller's OperatorID wins, then any
        /// field already carrying a provider code (all digits). Routing mnemonics resolve to empty
        /// rather than being forwarded — the provider answers "Wrong Operator Code" for those.
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

        // --- Prepaid mobile plans -------------------------------------------------------------

        // GET variant — fetches prepaid mobile plans live from planapi.in for an operator + circle.
        [HttpGet("GetSelectPlan")]
        public async Task<APIResponse> GetSelectPlan([FromQuery] string keyData, [FromQuery] string operatorCode, [FromQuery] string circle)
        {
            if (string.IsNullOrWhiteSpace(operatorCode))
                return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Plain(await _planApi.MobilePlans(operatorCode, circle ?? string.Empty));
        }

        // POST variant — fetches prepaid mobile plans live from planapi.in (operator + circle in encrypted request).
        [HttpPost("SelectPlan")]
        public async Task<APIResponse> SelectPlan([FromForm] APIRequest apiRequest)
        {
            var (req, error) = Parse(apiRequest);
            if (error != null) return error;
            string opCode = await ProviderOpCode(req!);
            if (string.IsNullOrWhiteSpace(opCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Encrypted(await _planApi.MobilePlans(opCode, req!.Circle));
        }

        // --- R-Offer --------------------------------------------------------------------------

        [HttpGet("GetSelectOffer")]
        public async Task<APIResponse> GetSelectOffer([FromQuery] string keyData, [FromQuery] string operatorCode, [FromQuery] string mobileNo)
        {
            if (string.IsNullOrWhiteSpace(mobileNo)) return ResponseProcess.GetFailResponse(Plan.Message.MobileNumberRequired);
            if (string.IsNullOrWhiteSpace(operatorCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Plain(await _planApi.RofferCheck(operatorCode, mobileNo));
        }

        [HttpPost("SelectOffer")]
        public async Task<APIResponse> SelectOffer([FromForm] APIRequest apiRequest)
        {
            var (req, error) = Parse(apiRequest);
            if (error != null) return error;
            if (string.IsNullOrWhiteSpace(req!.MobileNo)) return ResponseProcess.GetFailResponse(Plan.Message.MobileNumberRequired);
            string opCode = await ProviderOpCode(req);
            if (string.IsNullOrWhiteSpace(opCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Encrypted(await _planApi.RofferCheck(opCode, req.MobileNo));
        }

        // --- Operator detection ---------------------------------------------------------------

        [HttpGet("GetOperatorByMobile")]
        public async Task<APIResponse> GetOperatorByMobile([FromQuery] string keyData, [FromQuery] string mobileNo)
        {
            if (string.IsNullOrWhiteSpace(mobileNo)) return ResponseProcess.GetFailResponse(Plan.Message.MobileNumberRequired);
            return Plain(await _planApi.OperatorFetch(mobileNo));
        }

        [HttpGet("GetOperatorByDTHNumber")]
        public async Task<APIResponse> GetOperatorByDTHNumber([FromQuery] string keyData, [FromQuery] string dthNumber)
        {
            if (string.IsNullOrWhiteSpace(dthNumber)) return ResponseProcess.GetFailResponse(Plan.Message.DthNumberRequired);
            return Plain(await _planApi.DthOperatorFetch(dthNumber));
        }

        [HttpPost("GetOperatorByDTHNumber")]
        public async Task<APIResponse> SelectOperatorByDTHNumber([FromForm] APIRequest apiRequest)
        {
            var (req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.DthNumberRequired);
            return Encrypted(await _planApi.DthOperatorFetch(number));
        }

        // --- DTH plans ------------------------------------------------------------------------

        [HttpGet("GetSelectDTHPlan")]
        public async Task<APIResponse> GetSelectDTHPlan([FromQuery] string keyData, [FromQuery] string operatorCode)
        {
            if (string.IsNullOrWhiteSpace(operatorCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Plain(await _planApi.DthPlans(operatorCode));
        }

        [HttpPost("SelectDTHPlan")]
        public async Task<APIResponse> SelectDTHPlan([FromForm] APIRequest apiRequest)
        {
            var (req, error) = Parse(apiRequest);
            if (error != null) return error;
            string opCode = await ProviderOpCode(req!);
            if (string.IsNullOrWhiteSpace(opCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);
            return Encrypted(await _planApi.DthPlans(opCode));
        }

        // --- DTH customer info ----------------------------------------------------------------

        // Full info (provider bills this as 3 hits).
        [HttpGet("GetSelectDTHCustInfo")]
        public async Task<APIResponse> GetSelectDTHCustInfo([FromQuery] string keyData, [FromQuery] string opCode, [FromQuery] string dthNumber)
        {
            if (string.IsNullOrWhiteSpace(dthNumber)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Plain(await _planApi.DthInfoCheck(opCode ?? string.Empty, dthNumber));
        }

        [HttpPost("SelectDTHCustInfo")]
        public async Task<APIResponse> SelectDTHCustInfo([FromForm] APIRequest apiRequest)
        {
            var (req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Encrypted(await _planApi.DthInfoCheck(await ProviderOpCode(req), number));
        }

        // Basic details only — VC, name, RMN, address.
        [HttpGet("GetSelectDTHBasicInfo")]
        public async Task<APIResponse> GetSelectDTHBasicInfo([FromQuery] string keyData, [FromQuery] string opCode, [FromQuery] string dthNumber)
        {
            if (string.IsNullOrWhiteSpace(dthNumber)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Plain(await _planApi.DthBasicDetails(opCode ?? string.Empty, dthNumber));
        }

        [HttpPost("SelectDTHBasicInfo")]
        public async Task<APIResponse> SelectDTHBasicInfo([FromForm] APIRequest apiRequest)
        {
            var (req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Encrypted(await _planApi.DthBasicDetails(await ProviderOpCode(req), number));
        }

        // Info including the last recharge date.
        [HttpGet("GetSelectDTHInfoWithLastRecharge")]
        public async Task<APIResponse> GetSelectDTHInfoWithLastRecharge([FromQuery] string keyData, [FromQuery] string opCode, [FromQuery] string dthNumber)
        {
            if (string.IsNullOrWhiteSpace(dthNumber)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Plain(await _planApi.DthInfoWithLastRecharge(opCode ?? string.Empty, dthNumber));
        }

        [HttpPost("SelectDTHInfoWithLastRecharge")]
        public async Task<APIResponse> SelectDTHInfoWithLastRecharge([FromForm] APIRequest apiRequest)
        {
            var (req, error) = Parse(apiRequest);
            if (error != null) return error;
            string number = Number(req!);
            if (string.IsNullOrWhiteSpace(number)) return ResponseProcess.GetFailResponse(Plan.Message.NumberRequired);
            return Encrypted(await _planApi.DthInfoWithLastRecharge(await ProviderOpCode(req), number));
        }
    }
}
