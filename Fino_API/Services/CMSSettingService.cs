using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CMSSetting;
using FintechApp.Responses.CMSSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class CMSSettingService : ICMSSettingService
    {
        private readonly ICMSSettingRepository _repository;
        public CMSSettingService(ICMSSettingRepository repository) { _repository = repository; }
        private static CMSSettingResponse MapToResponse(CMSSetting e) => new() { CMSSettingID = e.CMSSettingID, Name = e.Name, MerchantID = e.MerchantID, Token = e.Token, Slat = e.Slat, PublicKey = e.PublicKey, IsActiveWeb = e.IsActiveWeb, IsActive = e.IsActive, Mode = e.Mode, };
        public async Task<List<CMSSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CMSSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateCMSSettingRequest request) { var entity = new CMSSetting { Name = request.Name, MerchantID = request.MerchantID, Token = request.Token, Slat = request.Slat, PublicKey = request.PublicKey, IsActiveWeb = request.IsActiveWeb, IsActive = request.IsActive, Mode = request.Mode, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateCMSSettingRequest request) { var entity = await _repository.GetByID(request.CMSSettingID); if (entity == null) return Messages.CMSSettingMessages.NotFound; entity.Name = request.Name; entity.MerchantID = request.MerchantID; entity.Token = request.Token; entity.Slat = request.Slat; entity.PublicKey = request.PublicKey; entity.IsActiveWeb = request.IsActiveWeb; entity.IsActive = request.IsActive; entity.Mode = request.Mode; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.CMSSettingMessages.NotFound; }
    }
}
