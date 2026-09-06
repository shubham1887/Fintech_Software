using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.eKYCMaster;
using FintechApp.Responses.eKYCMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class eKYCMasterService : IeKYCMasterService
    {
        private readonly IeKYCMasterRepository _repository;
        public eKYCMasterService(IeKYCMasterRepository repository) { _repository = repository; }
        private static eKYCMasterResponse MapToResponse(eKYCMaster e) => new() { eKYCMasterID = e.eKYCMasterID, ProviderID = e.ProviderID, UserID = e.UserID, OrderID = e.OrderID, OTPGenerateHash = e.OTPGenerateHash, ChargeAmount = e.ChargeAmount, OTPGenerateRequest = e.OTPGenerateRequest, OTPGenerateResponse = e.OTPGenerateResponse, VerifyRequest = e.VerifyRequest, VerifyResponse = e.VerifyResponse, OTPToken = e.OTPToken, KYCToken = e.KYCToken, wadh = e.wadh, };
        public async Task<List<eKYCMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<eKYCMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<eKYCMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateeKYCMasterRequest request) { var entity = new eKYCMaster { ProviderID = request.ProviderID, UserID = request.UserID, OrderID = request.OrderID, OTPGenerateHash = request.OTPGenerateHash, ChargeAmount = request.ChargeAmount, OTPGenerateRequest = request.OTPGenerateRequest, OTPGenerateResponse = request.OTPGenerateResponse, VerifyRequest = request.VerifyRequest, VerifyResponse = request.VerifyResponse, OTPToken = request.OTPToken, KYCToken = request.KYCToken, wadh = request.wadh, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateeKYCMasterRequest request) { var entity = await _repository.GetByID(request.eKYCMasterID); if (entity == null) return Messages.eKYCMasterMessages.NotFound; entity.ProviderID = request.ProviderID; entity.UserID = request.UserID; entity.OrderID = request.OrderID; entity.OTPGenerateHash = request.OTPGenerateHash; entity.ChargeAmount = request.ChargeAmount; entity.OTPGenerateRequest = request.OTPGenerateRequest; entity.OTPGenerateResponse = request.OTPGenerateResponse; entity.VerifyRequest = request.VerifyRequest; entity.VerifyResponse = request.VerifyResponse; entity.OTPToken = request.OTPToken; entity.KYCToken = request.KYCToken; entity.wadh = request.wadh; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.eKYCMasterMessages.NotFound; }
    }
}
