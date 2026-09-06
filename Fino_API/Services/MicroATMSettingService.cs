using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.MicroATMSetting;
using FintechApp.Responses.MicroATMSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class MicroATMSettingService : IMicroATMSettingService
    {
        private readonly IMicroATMSettingRepository _repository;
        public MicroATMSettingService(IMicroATMSettingRepository repository) { _repository = repository; }
        private static MicroATMSettingResponse MapToResponse(MicroATMSetting e) => new() { MicroATMSettingID = e.MicroATMSettingID, Name = e.Name, UserName = e.UserName, Password = e.Password, MerchantID = e.MerchantID, Token = e.Token, Slat = e.Slat, PublicKey = e.PublicKey, MinimumAmount = e.MinimumAmount, MaximumAmount = e.MaximumAmount, DisplayName = e.DisplayName, Sequence = e.Sequence, IsActive = e.IsActive, IsActiveWeb = e.IsActiveWeb, };
        public async Task<List<MicroATMSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<MicroATMSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateMicroATMSettingRequest request) { var entity = new MicroATMSetting { Name = request.Name, UserName = request.UserName, Password = request.Password, MerchantID = request.MerchantID, Token = request.Token, Slat = request.Slat, PublicKey = request.PublicKey, MinimumAmount = request.MinimumAmount, MaximumAmount = request.MaximumAmount, DisplayName = request.DisplayName, Sequence = request.Sequence, IsActive = request.IsActive, IsActiveWeb = request.IsActiveWeb, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateMicroATMSettingRequest request) { var entity = await _repository.GetByID(request.MicroATMSettingID); if (entity == null) return Messages.MicroATMSettingMessages.NotFound; entity.Name = request.Name; entity.UserName = request.UserName; entity.Password = request.Password; entity.MerchantID = request.MerchantID; entity.Token = request.Token; entity.Slat = request.Slat; entity.PublicKey = request.PublicKey; entity.MinimumAmount = request.MinimumAmount; entity.MaximumAmount = request.MaximumAmount; entity.DisplayName = request.DisplayName; entity.Sequence = request.Sequence; entity.IsActive = request.IsActive; entity.IsActiveWeb = request.IsActiveWeb; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.MicroATMSettingMessages.NotFound; }
    }
}
