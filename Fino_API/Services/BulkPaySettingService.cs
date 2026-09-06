using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BulkPaySetting;
using FintechApp.Responses.BulkPaySetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class BulkPaySettingService : IBulkPaySettingService
    {
        private readonly IBulkPaySettingRepository _repository;
        public BulkPaySettingService(IBulkPaySettingRepository repository) { _repository = repository; }
        private static BulkPaySettingResponse MapToResponse(BulkPaySetting e) => new() { BulkPaySettingID = e.BulkPaySettingID, Name = e.Name, MerchantID = e.MerchantID, Token = e.Token, Slat = e.Slat, ActionURL = e.ActionURL, StatusCheckURL = e.StatusCheckURL, IsActiveWeb = e.IsActiveWeb, IsActive = e.IsActive, MinimumAmount = e.MinimumAmount, MaximumAmount = e.MaximumAmount, Mode = e.Mode, };
        public async Task<List<BulkPaySettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<BulkPaySettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateBulkPaySettingRequest request) { var entity = new BulkPaySetting { Name = request.Name, MerchantID = request.MerchantID, Token = request.Token, Slat = request.Slat, ActionURL = request.ActionURL, StatusCheckURL = request.StatusCheckURL, IsActiveWeb = request.IsActiveWeb, IsActive = request.IsActive, MinimumAmount = request.MinimumAmount, MaximumAmount = request.MaximumAmount, Mode = request.Mode, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateBulkPaySettingRequest request) { var entity = await _repository.GetByID(request.BulkPaySettingID); if (entity == null) return Messages.BulkPaySettingMessages.NotFound; entity.Name = request.Name; entity.MerchantID = request.MerchantID; entity.Token = request.Token; entity.Slat = request.Slat; entity.ActionURL = request.ActionURL; entity.StatusCheckURL = request.StatusCheckURL; entity.IsActiveWeb = request.IsActiveWeb; entity.IsActive = request.IsActive; entity.MinimumAmount = request.MinimumAmount; entity.MaximumAmount = request.MaximumAmount; entity.Mode = request.Mode; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.BulkPaySettingMessages.NotFound; }
    }
}
