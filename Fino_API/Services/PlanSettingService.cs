using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PlanSetting;
using FintechApp.Responses.PlanSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PlanSettingService : IPlanSettingService
    {
        private readonly IPlanSettingRepository _repository;
        public PlanSettingService(IPlanSettingRepository repository) { _repository = repository; }

        private static PlanSettingResponse MapToResponse(PlanSetting e) => new()
        {
            PlanSettingID = e.PlanSettingID,
            Name = e.Name,
            UserID = e.UserID,
            Token = e.Token,
            IsActiveWeb = e.IsActiveWeb,
            IsActive = e.IsActive,
            Circle = e.Circle,
            Password = e.Password,
        };

        public async Task<List<PlanSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PlanSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreatePlanSettingRequest request)
        {
            var entity = new PlanSetting
            {
                Name = request.Name,
                UserID = request.UserID,
                Token = request.Token,
                IsActiveWeb = request.IsActiveWeb,
                IsActive = request.IsActive,
                Circle = request.Circle,
                Password = request.Password,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePlanSettingRequest request)
        {
            var entity = await _repository.GetByID(request.PlanSettingID);
            if (entity == null) return Messages.PlanSettingMessages.NotFound;
            entity.Name = request.Name;
            entity.UserID = request.UserID;
            entity.Token = request.Token;
            entity.IsActiveWeb = request.IsActiveWeb;
            entity.IsActive = request.IsActive;
            entity.Circle = request.Circle;
            entity.Password = request.Password;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.PlanSettingMessages.NotFound;
        }
    }
}
