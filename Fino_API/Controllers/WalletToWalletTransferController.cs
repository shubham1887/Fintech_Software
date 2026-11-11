using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.WalletToWalletTransfer;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class WalletToWalletTransferController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IWalletToWalletTransferService _transferService;

        public WalletToWalletTransferController(IConfiguration configuration, IWalletToWalletTransferService transferService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _transferService = transferService;
        }

        // Transfers funds from the user's AEPS wallet to another of their wallets.
        // Debits AEPS wallet, credits target wallet, records two FundTransaction legs.
        [HttpPost("AEPSToOther")]
        public async Task<APIResponse> AEPSToOther([FromForm] APIRequest apiRequest)
        {
            AEPSToOtherRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<AEPSToOtherRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _transferService.AEPSToOther(req, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }
        // The general transfer: any of the caller's own wallets to any other of them. The wallet
        // ids are the caller's to choose, but never the OWNER — the user id comes from the signed
        // token, so a transfer can only ever move the caller's own money.
        [HttpPost("Transfer")]
        public async Task<APIResponse> Transfer([FromForm] APIRequest apiRequest)
        {
            WalletTransferRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<WalletTransferRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _transferService.Transfer(req, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // The caller's own wallets with balances — fills the From / To pickers on the transfer screen.
        [HttpPost("MyWallets")]
        public async Task<APIResponse> MyWallets([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var data = await _transferService.MyWallets(keyDataModel.UserID);
            if (data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Where an AEPS transfer may land: the caller's wallets except AEPS itself. The "AEPS to
        // Other Wallet" screen fills its one picker from here, so the list never offers a
        // destination the transfer would refuse.
        [HttpPost("AEPSDestinations")]
        public async Task<APIResponse> AEPSDestinations([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var data = await _transferService.AEPSDestinations(keyDataModel.UserID);
            if (data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

    }
}