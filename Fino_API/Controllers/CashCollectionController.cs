using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.CashCollection;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class CashCollectionController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ICashCollectionMasterService _cashCollectionService;
        private readonly IOutstandingService _outstanding;

        public CashCollectionController(IConfiguration configuration, ICashCollectionMasterService cashCollectionService,
            IOutstandingService outstanding)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _cashCollectionService = cashCollectionService;
            _outstanding = outstanding;
        }

        [HttpPost("Add")]
        public async Task<APIResponse> Add([FromForm] APIRequest apiRequest)
        {
            AddCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<AddCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, result) = await _cashCollectionService.AddCashCollection(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
        }

        [HttpPost("Reverse")]
        public async Task<APIResponse> Reverse([FromForm] APIRequest apiRequest)
        {
            ReverseCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<ReverseCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, result) = await _cashCollectionService.ReverseCashCollection(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
        }

        [HttpPost("SendCollectionMessage")]
        public async Task<APIResponse> SendCollectionMessage([FromForm] APIRequest apiRequest)
        {
            SelectCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            string failureMessage = await _cashCollectionService.SendCollectionMessage(mainRequest.UserID, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject("Outstanding message send successfully"));
        }

        [HttpPost("SelectByUserID")]
        public async Task<APIResponse> SelectByUserID([FromForm] APIRequest apiRequest)
        {
            SelectCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            // No user named = the caller's own standing (the header tile and the mobile home
            // send none). Passing the 0 through created a standing row for "user 0" and failed
            // on the foreign key, every time, for every user opening their own balance.
            int targetUserID = mainRequest.UserID > 0 ? mainRequest.UserID : keyDataModel.UserID;
            var (failureMessage, data) = await _cashCollectionService.SelectUserOutstanding(targetUserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("OutstandingReport")]
        public async Task<APIResponse> OutstandingReport([FromForm] APIRequest apiRequest)
        {
            SelectCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, wrapper) = await _cashCollectionService.OutstandingReport(
                mainRequest, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wrapper));
        }

        /// <summary>
        /// One user's outstanding statement: date, counterparty, opening, credit, debit, closing,
        /// remark. The page the View button on the outstanding list opens.
        ///
        /// Who may read it is decided in the service from the SIGNED token identity, never from
        /// the body — a statement is a record of money owed between two businesses.
        /// </summary>
        [HttpPost("Statement")]
        public async Task<APIResponse> Statement([FromForm] APIRequest apiRequest)
        {
            OutstandingStatementRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<OutstandingStatementRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            // No user named = their own statement. The retailer opening "my outstanding" should
            // not have to send an id, and should not be able to send someone else's either.
            if (mainRequest.UserID <= 0) mainRequest.UserID = keyDataModel.UserID;

            var (failureMessage, wrapper) = await _cashCollectionService.Statement(
                mainRequest, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wrapper));
        }

        /// <summary>
        /// Sets a user's credit limit — how much they may owe at once.
        ///
        /// Admin-only, enforced here AND pinned in ApiAccessPolicy. The limit is the whole control
        /// on this feature: it decides whether the Credit payment mode is offered at all, and it
        /// is the ceiling the posting refuses to go past. Letting an upline raise their own
        /// downline's limit would make it a formality.
        /// </summary>
        [HttpPost("SetLimit")]
        [JwtAuthorization(UserTypeCode.Admin)]
        public async Task<APIResponse> SetLimit([FromForm] APIRequest apiRequest)
        {
            SetCreditLimitRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SetCreditLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            string failureMessage = await _outstanding.SetCreditLimit(mainRequest.UserID, mainRequest.MaxOutstanding);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject("Credit limit saved."));
        }

        /// <summary>
        /// Drops an account's own credit limit so the network default (Key Setting
        /// <c>DefaultCreditLimit</c>) applies to it again.
        ///
        /// The counterpart of <see cref="SetLimit"/>, and admin-only for the same reason. Without
        /// it, setting a limit once would be irreversible: an account could only be returned to the
        /// default by guessing what the default currently is and typing that number in — which
        /// stops being true the moment the default changes.
        /// </summary>
        [HttpPost("ClearLimit")]
        [JwtAuthorization(UserTypeCode.Admin)]
        public async Task<APIResponse> ClearLimit([FromForm] APIRequest apiRequest)
        {
            SetCreditLimitRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SetCreditLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            string failureMessage = await _outstanding.ClearCreditLimit(mainRequest.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject("This account now uses the network default credit limit."));
        }
    }
}
