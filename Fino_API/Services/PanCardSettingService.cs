using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PanCardSetting;
using FintechApp.Responses.PanCardSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class PanCardSettingService : IPanCardSettingService
    {
        private readonly IPanCardSettingRepository _repository;
        public PanCardSettingService(IPanCardSettingRepository repository) { _repository = repository; }
        private static PanCardSettingResponse MapToResponse(PanCardSetting e) => new() { PanCardSettingID = e.PanCardSettingID, Name = e.Name, MerchantID = e.MerchantID, Token = e.Token, Slat = e.Slat, PublicKey = e.PublicKey, Mode = e.Mode, IsActive = e.IsActive, };
        public async Task<List<PanCardSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PanCardSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreatePanCardSettingRequest request) { var entity = new PanCardSetting { Name = request.Name, MerchantID = request.MerchantID, Token = request.Token, Slat = request.Slat, PublicKey = request.PublicKey, Mode = request.Mode, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdatePanCardSettingRequest request) { var entity = await _repository.GetByID(request.PanCardSettingID); if (entity == null) return Messages.PanCardSettingMessages.NotFound; entity.Name = request.Name; entity.MerchantID = request.MerchantID; entity.Token = request.Token; entity.Slat = request.Slat; entity.PublicKey = request.PublicKey; entity.Mode = request.Mode; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.PanCardSettingMessages.NotFound; }
    }
}
