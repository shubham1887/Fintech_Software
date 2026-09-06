using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SenderOTP;
using FintechApp.Responses.SenderOTP;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SenderOTPService : ISenderOTPService
    {
        private readonly ISenderOTPRepository _repository;
        public SenderOTPService(ISenderOTPRepository repository) { _repository = repository; }
        private static SenderOTPResponse MapToResponse(SenderOTP e) => new() { SenderOTPID = e.SenderOTPID, Mobile = e.Mobile, OTP = e.OTP, SenderID = e.SenderID, UserID = e.UserID, IsOTPVerified = e.IsOTPVerified, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<SenderOTPResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SenderOTPResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<SenderOTPResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateSenderOTPRequest request) { var entity = new SenderOTP { Mobile = request.Mobile, OTP = request.OTP, SenderID = request.SenderID, UserID = request.UserID, IsOTPVerified = request.IsOTPVerified, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSenderOTPRequest request) { var entity = await _repository.GetByID(request.SenderOTPID); if (entity == null) return Messages.SenderOTPMessages.NotFound; entity.Mobile = request.Mobile; entity.OTP = request.OTP; entity.SenderID = request.SenderID; entity.UserID = request.UserID; entity.IsOTPVerified = request.IsOTPVerified; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SenderOTPMessages.NotFound; }
    }
}
