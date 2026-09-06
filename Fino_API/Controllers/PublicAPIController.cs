using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Operator;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Fino.PlanApi.Interfaces;
using Fino.PlanApi.Requests;
using Fino.PlanApi.Messages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicAPIController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IOperatorService _operatorService;
        private readonly IPlanApiService _planApi;

        public PublicAPIController(IConfiguration configuration, IOperatorService operatorService, IPlanApiService planApi)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _operatorService = operatorService;
            _planApi = planApi;
        }

        // Public — returns operators for a given ServiceID (used by B2B API consumers)
        [HttpPost("SelectOperator")]
        public async Task<APIResponse> SelectOperator([FromForm] APIRequest apiRequest)
        {
            GetOperatorRequest? req;
            try { req = JsonConvert.DeserializeObject<GetOperatorRequest>(apiRequest.Request); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            var list = await _operatorService.GetOperators(req);
            if (list == null || list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(list));
        }

        // Public — detects mobile operator & circle by mobile number (live via planapi.in OperatorFetchNew).
        [HttpPost("GetOperatorByMobileNumber")]
        public async Task<APIResponse> GetOperatorByMobileNumber([FromForm] APIRequest apiRequest)
        {
            PlanFetchRequest? req;
            try { req = JsonConvert.DeserializeObject<PlanFetchRequest>(apiRequest.Request); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null || string.IsNullOrWhiteSpace(req.MobileNo)) return ResponseProcess.GetFailResponse(Plan.Message.MobileNumberRequired);

            var result = await _planApi.OperatorFetch(req.MobileNo);
            if (!result.Success) return ResponseProcess.GetFailResponse(result.Message);
            return ResponseProcess.GetSuccessResponse(result.Data);
        }

        // Public — returns R-Offer (special offers) for a mobile number (live via planapi.in).
        [HttpPost("SelectROffer")]
        public async Task<APIResponse> SelectROffer([FromForm] APIRequest apiRequest)
        {
            PlanFetchRequest? req;
            try { req = JsonConvert.DeserializeObject<PlanFetchRequest>(apiRequest.Request); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null || string.IsNullOrWhiteSpace(req.MobileNo)) return ResponseProcess.GetFailResponse(Plan.Message.MobileNumberRequired);

            var result = await _planApi.RofferCheck(req.OperatorCode, req.MobileNo);
            if (!result.Success) return ResponseProcess.GetFailResponse(result.Message);
            return ResponseProcess.GetSuccessResponse(result.Data);
        }

        // Public — returns prepaid plans for an operator + circle (live via planapi.in).
        [HttpPost("SelectPlan")]
        public async Task<APIResponse> SelectPlan([FromForm] APIRequest apiRequest)
        {
            PlanFetchRequest? req;
            try { req = JsonConvert.DeserializeObject<PlanFetchRequest>(apiRequest.Request); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null || string.IsNullOrWhiteSpace(req.OperatorCode)) return ResponseProcess.GetFailResponse(Plan.Message.OperatorCodeRequired);

            var result = await _planApi.MobilePlans(req.OperatorCode, req.Circle);
            if (!result.Success) return ResponseProcess.GetFailResponse(result.Message);
            return ResponseProcess.GetSuccessResponse(result.Data);
        }
    }
}
