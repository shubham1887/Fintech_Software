using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DematSetting;
using FintechApp.Responses.DematSetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class DematSettingService : IDematSettingService
    {
        private readonly IDematSettingRepository _repository;
        public DematSettingService(IDematSettingRepository repository) { _repository = repository; }
        private static DematSettingResponse MapToResponse(DematSetting e) => new() { DematSettingID = e.DematSettingID, Name = e.Name, DisplayName = e.DisplayName, UserName = e.UserName, Token = e.Token, UTMSource = e.UTMSource, EmailID = e.EmailID, Amount = e.Amount, Sequence = e.Sequence, IsActiveWeb = e.IsActiveWeb, IsActive = e.IsActive, };
        public async Task<List<DematSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<DematSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateDematSettingRequest request) { var entity = new DematSetting { Name = request.Name, DisplayName = request.DisplayName, UserName = request.UserName, Token = request.Token, UTMSource = request.UTMSource, EmailID = request.EmailID, Amount = request.Amount, Sequence = request.Sequence, IsActiveWeb = request.IsActiveWeb, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateDematSettingRequest request) { var entity = await _repository.GetByID(request.DematSettingID); if (entity == null) return Messages.DematSettingMessages.NotFound; entity.Name = request.Name; entity.DisplayName = request.DisplayName; entity.UserName = request.UserName; entity.Token = request.Token; entity.UTMSource = request.UTMSource; entity.EmailID = request.EmailID; entity.Amount = request.Amount; entity.Sequence = request.Sequence; entity.IsActiveWeb = request.IsActiveWeb; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.DematSettingMessages.NotFound; }
    }
}
