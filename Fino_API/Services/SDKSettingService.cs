using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SDKSetting;
using FintechApp.Responses.SDKSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SDKSettingService : ISDKSettingService
    {
        private readonly ISDKSettingRepository _repository;
        public SDKSettingService(ISDKSettingRepository repository) { _repository = repository; }
        private static SDKSettingResponse MapToResponse(SDKSetting e) => new() { SDKSettingID = e.SDKSettingID, Name = e.Name, MerchantID = e.MerchantID, Token = e.Token, URL = e.URL, PublicKey = e.PublicKey, IsActive = e.IsActive, };
        public async Task<List<SDKSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SDKSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateSDKSettingRequest request) { var entity = new SDKSetting { Name = request.Name, MerchantID = request.MerchantID, Token = request.Token, URL = request.URL, PublicKey = request.PublicKey, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSDKSettingRequest request) { var entity = await _repository.GetByID(request.SDKSettingID); if (entity == null) return Messages.SDKSettingMessages.NotFound; entity.Name = request.Name; entity.MerchantID = request.MerchantID; entity.Token = request.Token; entity.URL = request.URL; entity.PublicKey = request.PublicKey; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SDKSettingMessages.NotFound; }
    }
}
