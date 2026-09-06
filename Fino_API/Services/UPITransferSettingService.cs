using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UPITransferSetting;
using FintechApp.Responses.UPITransferSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UPITransferSettingService : IUPITransferSettingService
    {
        private readonly IUPITransferSettingRepository _repository;
        public UPITransferSettingService(IUPITransferSettingRepository repository) { _repository = repository; }
        private static UPITransferSettingResponse MapToResponse(UPITransferSetting e) => new() { UPITransferSettingID = e.UPITransferSettingID, Name = e.Name, DisplayName = e.DisplayName, Token = e.Token, CallBackURL = e.CallBackURL, UserName = e.UserName, SenderLimit = e.SenderLimit, MaximumAmount = e.MaximumAmount, MinAmount = e.MinAmount, Sequence = e.Sequence, AuthCode = e.AuthCode, IsActive = e.IsActive, };
        public async Task<List<UPITransferSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UPITransferSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateUPITransferSettingRequest request) { var entity = new UPITransferSetting { Name = request.Name, DisplayName = request.DisplayName, Token = request.Token, CallBackURL = request.CallBackURL, UserName = request.UserName, SenderLimit = request.SenderLimit, MaximumAmount = request.MaximumAmount, MinAmount = request.MinAmount, Sequence = request.Sequence, AuthCode = request.AuthCode, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUPITransferSettingRequest request) { var entity = await _repository.GetByID(request.UPITransferSettingID); if (entity == null) return Messages.UPITransferSettingMessages.NotFound; entity.Name = request.Name; entity.DisplayName = request.DisplayName; entity.Token = request.Token; entity.CallBackURL = request.CallBackURL; entity.UserName = request.UserName; entity.SenderLimit = request.SenderLimit; entity.MaximumAmount = request.MaximumAmount; entity.MinAmount = request.MinAmount; entity.Sequence = request.Sequence; entity.AuthCode = request.AuthCode; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UPITransferSettingMessages.NotFound; }
    }
}
