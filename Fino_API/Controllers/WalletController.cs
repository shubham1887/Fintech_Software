using FintechApp.Authorization;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Wallet;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class WalletController : ControllerBase
    {
        private readonly IWalletMasterService _walletMasterService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public WalletController(IWalletMasterService walletMasterService, IConfiguration configuration)
        {
            _walletMasterService = walletMasterService;
            _logger = new Logger();

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }
        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = await _walletMasterService.Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailResponse(JsonConvert.SerializeObject(ex)
                    );
            }
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wallets));
        }
        // All wallets incl. disabled — admin management list (rename / enable-disable).
        [HttpPost("SelectAll")]
        public async Task<APIResponse> SelectAll([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = await _walletMasterService.SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailResponse(JsonConvert.SerializeObject(ex));
            }
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wallets));
        }

        [HttpPost("SelectByUserID")]
        public async Task<APIResponse> SelectByUserID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel? keyDataModel;
            WalletMaster wallets;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(aPIRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                wallets = await _walletMasterService.SelectByID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailResponse(JsonConvert.SerializeObject(ex)
                    );
            }
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wallets));
        }

        // Admin renames a wallet or enables/disables it (field-level). No create/delete allowed.
        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateWalletRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(aPIRequest.KeyData));
                request = JsonConvert.DeserializeObject<UpdateWalletRequest>(_encryptDecrypt.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            fail = await _walletMasterService.UpdateField(request.WalletID, request.FieldName, request.NewValue);
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            return ResponseProcess.GetSuccessResponse(Messages.WalletMessages.UpdatedSuccessfully);
        }
    }
}
