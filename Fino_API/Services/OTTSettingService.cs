using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OTTSetting;
using FintechApp.Responses.OTTSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class OTTSettingService : IOTTSettingService
    {
        private readonly IOTTSettingRepository _repository;
        public OTTSettingService(IOTTSettingRepository repository) { _repository = repository; }
        private static OTTSettingResponse MapToResponse(OTTSetting e) => new() { OTTSettingID = e.OTTSettingID, Name = e.Name, MerchantID = e.MerchantID, Token = e.Token, Slat = e.Slat, ActionURL = e.ActionURL, StatusCheckURL = e.StatusCheckURL, IsActive = e.IsActive, MinimumAmount = e.MinimumAmount, MaximumAmount = e.MaximumAmount, Mode = e.Mode, DisplayName = e.DisplayName, };
        public async Task<List<OTTSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OTTSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateOTTSettingRequest request) { var entity = new OTTSetting { Name = request.Name, MerchantID = request.MerchantID, Token = request.Token, Slat = request.Slat, ActionURL = request.ActionURL, StatusCheckURL = request.StatusCheckURL, IsActive = request.IsActive, MinimumAmount = request.MinimumAmount, MaximumAmount = request.MaximumAmount, Mode = request.Mode, DisplayName = request.DisplayName, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateOTTSettingRequest request) { var entity = await _repository.GetByID(request.OTTSettingID); if (entity == null) return Messages.OTTSettingMessages.NotFound; entity.Name = request.Name; entity.MerchantID = request.MerchantID; entity.Token = request.Token; entity.Slat = request.Slat; entity.ActionURL = request.ActionURL; entity.StatusCheckURL = request.StatusCheckURL; entity.IsActive = request.IsActive; entity.MinimumAmount = request.MinimumAmount; entity.MaximumAmount = request.MaximumAmount; entity.Mode = request.Mode; entity.DisplayName = request.DisplayName; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.OTTSettingMessages.NotFound; }
    }
}
