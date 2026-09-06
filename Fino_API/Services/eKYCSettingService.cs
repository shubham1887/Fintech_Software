using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.eKYCSetting;
using FintechApp.Responses.eKYCSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class eKYCSettingService : IeKYCSettingService
    {
        private readonly IeKYCSettingRepository _repository;
        public eKYCSettingService(IeKYCSettingRepository repository) { _repository = repository; }
        private static eKYCSettingResponse MapToResponse(eKYCSetting e) => new() { eKYCSettingID = e.eKYCSettingID, Name = e.Name, Slat = e.Slat, Token = e.Token, AuthKey = e.AuthKey, ChargeAmount = e.ChargeAmount, EncryptionKey = e.EncryptionKey, MerchantID = e.MerchantID, Mode = e.Mode, IsActive = e.IsActive, IsActiveWeb = e.IsActiveWeb, };
        public async Task<List<eKYCSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<eKYCSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateeKYCSettingRequest request) { var entity = new eKYCSetting { Name = request.Name, Slat = request.Slat, Token = request.Token, AuthKey = request.AuthKey, ChargeAmount = request.ChargeAmount, EncryptionKey = request.EncryptionKey, MerchantID = request.MerchantID, Mode = request.Mode, IsActive = request.IsActive, IsActiveWeb = request.IsActiveWeb, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateeKYCSettingRequest request) { var entity = await _repository.GetByID(request.eKYCSettingID); if (entity == null) return Messages.eKYCSettingMessages.NotFound; entity.Name = request.Name; entity.Slat = request.Slat; entity.Token = request.Token; entity.AuthKey = request.AuthKey; entity.ChargeAmount = request.ChargeAmount; entity.EncryptionKey = request.EncryptionKey; entity.MerchantID = request.MerchantID; entity.Mode = request.Mode; entity.IsActive = request.IsActive; entity.IsActiveWeb = request.IsActiveWeb; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.eKYCSettingMessages.NotFound; }
    }
}
