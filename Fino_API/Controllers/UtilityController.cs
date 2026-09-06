using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Utility;
using FintechApp.Responses;
using FintechApp.Responses.ProviderGateway;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class UtilityController : ControllerBase
    {
        private readonly IProviderGatewayService _gateway;
        private readonly IOperatorRepository _operatorRepository;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public UtilityController(IProviderGatewayService gateway, IOperatorRepository operatorRepository, IConfiguration configuration)
        {
            _gateway = gateway;
            _operatorRepository = operatorRepository;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        // Live bill-details enquiry via the provider gateway: routes through the operator's
        // configured provider (ProviderConfiguration), calls its BILLFETCH endpoint template and
        // reads the reply through the BILLFETCH response rules.
        [HttpPost("BillFetch")]
        public async Task<APIResponse> BillFetch([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            BillFetchRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<BillFetchRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            if (request.OperatorID <= 0 || string.IsNullOrWhiteSpace(request.ConsumerNumber))
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                var operatorMaster = await _operatorRepository.GetOperatorById(request.OperatorID);
                if (operatorMaster == null || operatorMaster.IsDelete == true || !operatorMaster.IsActive)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidOperator);

                var (failureMessage, data) = await _gateway.FetchBill(new BillFetchContext
                {
                    OperatorID = request.OperatorID,
                    ServiceID = request.ServiceID > 0 ? request.ServiceID : operatorMaster.ServiceID,
                    ConsumerNumber = request.ConsumerNumber.Trim(),
                    Param1 = request.Param1 ?? string.Empty,
                    Param2 = request.Param2 ?? string.Empty,
                    Param3 = request.Param3 ?? string.Empty,
                    UserID = keyDataModel.UserID,
                    IPAddress = keyDataModel.IPAddress ?? string.Empty,
                });

                if (data == null) return ResponseProcess.GetFailResponse(failureMessage);

                // The raw provider body stays server-side (it can carry provider internals).
                data.RawResponse = string.Empty;
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}
