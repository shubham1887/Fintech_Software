using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.POSSettingMaster;
using FintechApp.Responses.POSSettingMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class POSSettingMasterService : IPOSSettingMasterService
    {
        private readonly IPOSSettingMasterRepository _repository;
        public POSSettingMasterService(IPOSSettingMasterRepository repository) { _repository = repository; }
        private static POSSettingMasterResponse MapToResponse(POSSettingMaster e) => new() { POSSettingMasterID = e.POSSettingMasterID, Name = e.Name, UserName = e.UserName, Password = e.Password, MerchantID = e.MerchantID, Token = e.Token, Slat = e.Slat, PublicKey = e.PublicKey, IsActiveWeb = e.IsActiveWeb, IsActive = e.IsActive, MinimumAmount = e.MinimumAmount, MaximumAmount = e.MaximumAmount, };
        public async Task<List<POSSettingMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<POSSettingMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreatePOSSettingMasterRequest request) { var entity = new POSSettingMaster { Name = request.Name, UserName = request.UserName, Password = request.Password, MerchantID = request.MerchantID, Token = request.Token, Slat = request.Slat, PublicKey = request.PublicKey, IsActiveWeb = request.IsActiveWeb, IsActive = request.IsActive, MinimumAmount = request.MinimumAmount, MaximumAmount = request.MaximumAmount, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdatePOSSettingMasterRequest request) { var entity = await _repository.GetByID(request.POSSettingMasterID); if (entity == null) return Messages.POSSettingMasterMessages.NotFound; entity.Name = request.Name; entity.UserName = request.UserName; entity.Password = request.Password; entity.MerchantID = request.MerchantID; entity.Token = request.Token; entity.Slat = request.Slat; entity.PublicKey = request.PublicKey; entity.IsActiveWeb = request.IsActiveWeb; entity.IsActive = request.IsActive; entity.MinimumAmount = request.MinimumAmount; entity.MaximumAmount = request.MaximumAmount; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.POSSettingMasterMessages.NotFound; }
    }
}
