using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.VerificationSetting;
using FintechApp.Responses.VerificationSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class VerificationSettingService : IVerificationSettingService
    {
        private readonly IVerificationSettingRepository _repository;
        public VerificationSettingService(IVerificationSettingRepository repository) { _repository = repository; }
        private static VerificationSettingResponse MapToResponse(VerificationSetting e) => new() { VerificationSettingID = e.VerificationSettingID, Name = e.Name, Token = e.Token, CallBackURL = e.CallBackURL, UserName = e.UserName, Sequence = e.Sequence, AuthCode = e.AuthCode, Mode = e.Mode, IsActive = e.IsActive, };
        public async Task<List<VerificationSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<VerificationSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateVerificationSettingRequest request) { var entity = new VerificationSetting { Name = request.Name, Token = request.Token, CallBackURL = request.CallBackURL, UserName = request.UserName, Sequence = request.Sequence, AuthCode = request.AuthCode, Mode = request.Mode, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateVerificationSettingRequest request) { var entity = await _repository.GetByID(request.VerificationSettingID); if (entity == null) return Messages.VerificationSettingMessages.NotFound; entity.Name = request.Name; entity.Token = request.Token; entity.CallBackURL = request.CallBackURL; entity.UserName = request.UserName; entity.Sequence = request.Sequence; entity.AuthCode = request.AuthCode; entity.Mode = request.Mode; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.VerificationSettingMessages.NotFound; }
    }
}
