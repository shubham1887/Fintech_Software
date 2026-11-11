using FintechApp.Authorization;
using System.Linq;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Beneficiary;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class BeneficiaryController : ControllerBase
    {
        private readonly IBeneficiaryService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        public BeneficiaryController(IBeneficiaryService service, IConfiguration configuration) { _service = service; _logger = new Logger(); _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty); }

        // A DMT beneficiary is a saved bank account (name, account number, IFSC) belonging to one
        // retailer's walk-in sender. Every endpoint here was [JwtAuthorization] (any signed-in
        // user) and keyed on a client-supplied id with no ownership check — so any retailer could
        // list the whole platform's beneficiaries, read another merchant's by UserID, or edit and
        // delete a beneficiary by BeneficiaryID that was never theirs. Admin and support run the
        // network and legitimately see everything; everyone else is scoped to their own rows.
        private static bool SeesAll(KeyDataModel k) =>
            k.UserTypeID == StaticModels.UserTypeCode.Admin || k.UserTypeID == StaticModels.UserTypeCode.SupportUser;
        [HttpPost("GetAll")] public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetAll(); if (data != null && !SeesAll(keyDataModel)) data = data.Where(x => x.UserID == keyDataModel.UserID).ToList(); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
        [HttpPost("GetById")] public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; GetBeneficiaryRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetBeneficiaryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetByID(request.BeneficiaryID); if (data == null || (!SeesAll(keyDataModel) && data.UserID != keyDataModel.UserID)) return ResponseProcess.GetFailResponse(Messages.BeneficiaryMessages.NotFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }

        [HttpPost("GetByUserID")] public async Task<APIResponse> GetByUserID([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; GetBeneficiaryRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetBeneficiaryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            // A non-admin may only ask for their own beneficiaries, whatever UserID they send.
            int scopeUserId = SeesAll(keyDataModel) ? request.UserID : keyDataModel.UserID;
            try { var data = await _service.GetByUserID(scopeUserId); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
        [HttpPost("Create")] public async Task<APIResponse> Create([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; CreateBeneficiaryRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<CreateBeneficiaryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { if (!SeesAll(keyDataModel)) request.UserID = keyDataModel.UserID; /* the owner is the signed-in retailer, not whoever the client names */ fail = await _service.Create(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.BeneficiaryMessages.CreatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
        [HttpPost("Update")] public async Task<APIResponse> Update([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; UpdateBeneficiaryRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<UpdateBeneficiaryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var owner = await _service.GetByID(request.BeneficiaryID); if (owner == null || (!SeesAll(keyDataModel) && owner.UserID != keyDataModel.UserID)) return ResponseProcess.GetFailResponse(Messages.BeneficiaryMessages.NotFound); if (!SeesAll(keyDataModel)) request.UserID = keyDataModel.UserID; /* an edit can't reassign ownership */ fail = await _service.Update(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.BeneficiaryMessages.UpdatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
        [HttpPost("Delete")] public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; DeleteBeneficiaryRequest? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<DeleteBeneficiaryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var owner = await _service.GetByID(request.BeneficiaryID); if (owner == null || (!SeesAll(keyDataModel) && owner.UserID != keyDataModel.UserID)) return ResponseProcess.GetFailResponse(Messages.BeneficiaryMessages.NotFound); fail = await _service.Delete(request.BeneficiaryID); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.BeneficiaryMessages.DeletedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); } }
    }
}
