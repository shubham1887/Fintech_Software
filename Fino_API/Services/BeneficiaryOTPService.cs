using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BeneficiaryOTP;
using FintechApp.Responses.BeneficiaryOTP;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class BeneficiaryOTPService : IBeneficiaryOTPService
    {
        private readonly IBeneficiaryOTPRepository _repository;
        public BeneficiaryOTPService(IBeneficiaryOTPRepository repository) { _repository = repository; }
        private static BeneficiaryOTPResponse MapToResponse(BeneficiaryOTP e) => new() { BeneficiaryOTPID = e.BeneficiaryOTPID, SenderMobile = e.SenderMobile, OTP = e.OTP, SenderID = e.SenderID, BeneficiaryID = e.BeneficiaryID, UserID = e.UserID, IsOTPVerified = e.IsOTPVerified, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<BeneficiaryOTPResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<BeneficiaryOTPResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<BeneficiaryOTPResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateBeneficiaryOTPRequest request) { var entity = new BeneficiaryOTP { SenderMobile = request.SenderMobile, OTP = request.OTP, SenderID = request.SenderID, BeneficiaryID = request.BeneficiaryID, UserID = request.UserID, IsOTPVerified = request.IsOTPVerified, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateBeneficiaryOTPRequest request) { var entity = await _repository.GetByID(request.BeneficiaryOTPID); if (entity == null) return Messages.BeneficiaryOTPMessages.NotFound; entity.SenderMobile = request.SenderMobile; entity.OTP = request.OTP; entity.SenderID = request.SenderID; entity.BeneficiaryID = request.BeneficiaryID; entity.UserID = request.UserID; entity.IsOTPVerified = request.IsOTPVerified; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.BeneficiaryOTPMessages.NotFound; }
    }
}
