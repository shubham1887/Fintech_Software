using FintechApp.Authorization;
using System.Linq;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Sender;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class SenderController : ControllerBase
    {
        private readonly ISenderService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        public SenderController(ISenderService service, IConfiguration configuration) { _service = service; _logger = new Logger(); _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty); }

        // A DMT sender is a walk-in customer (name, mobile, address) onboarded by one retailer.
        // Admin/support see the network; everyone else is scoped to their own so no merchant can
        // list, read, edit or delete another's customers. (See BeneficiaryController.)
        private static bool SeesAll(KeyDataModel k) =>
            k.UserTypeID == StaticModels.UserTypeCode.Admin || k.UserTypeID == StaticModels.UserTypeCode.SupportUser;
        [HttpPost("GetAll")] public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetAll(); if (data != null && !SeesAll(keyDataModel)) data = data.Where(x => x.UserID == keyDataModel.UserID).ToList(); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
        [HttpPost("GetById")] public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; GetSenderRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetSenderRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetByID(request.SenderID); if (data == null || (!SeesAll(keyDataModel) && data.UserID != keyDataModel.UserID)) return ResponseProcess.GetFailResponse(Messages.SenderMessages.NotFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }

        [HttpPost("GetByUserID")] public async Task<APIResponse> GetByUserID([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; GetSenderRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetSenderRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            int scopeUserId = SeesAll(keyDataModel) ? request.UserID : keyDataModel.UserID;
            try { var data = await _service.GetByUserID(scopeUserId); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
        [HttpPost("Create")] public async Task<APIResponse> Create([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; CreateSenderRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<CreateSenderRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { if (!SeesAll(keyDataModel)) request.UserID = keyDataModel.UserID; fail = await _service.Create(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.SenderMessages.CreatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
        [HttpPost("Update")] public async Task<APIResponse> Update([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; UpdateSenderRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<UpdateSenderRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var owner = await _service.GetByID(request.SenderID); if (owner == null || (!SeesAll(keyDataModel) && owner.UserID != keyDataModel.UserID)) return ResponseProcess.GetFailResponse(Messages.SenderMessages.NotFound); if (!SeesAll(keyDataModel)) request.UserID = keyDataModel.UserID; fail = await _service.Update(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.SenderMessages.UpdatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
        [HttpPost("Delete")] public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; DeleteSenderRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<DeleteSenderRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var owner = await _service.GetByID(request.SenderID); if (owner == null || (!SeesAll(keyDataModel) && owner.UserID != keyDataModel.UserID)) return ResponseProcess.GetFailResponse(Messages.SenderMessages.NotFound); fail = await _service.Delete(request.SenderID); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.SenderMessages.DeletedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
    }
}
