using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.BBPSSettings;
using FintechApp.Responses.BBPSSettings;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class BBPSSettingsService : IBBPSSettingsService
    {
        private readonly IBBPSSettingsRepository _repository;

        public BBPSSettingsService(IBBPSSettingsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BBPSSettingsResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<BBPSSettingsResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateBBPSSettingsRequest request)
        {
            var entity = new BBPSSettings
            {
                Name = request.Name,
                IsActiveForWebsite = request.IsActiveForWebsite,
                IsActive = request.IsActive,
                Token = request.Token,
                AgentID = request.AgentID,
                PartnerID = request.PartnerID,
                MerchantID = request.MerchantID
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateBBPSSettingsRequest request)
        {
            var entity = await _repository.GetByID(request.BBPSSettingID);
            if (entity == null) return BBPSSettingsMessages.NotFound;
            entity.Name = request.Name;
            entity.IsActiveForWebsite = request.IsActiveForWebsite;
            entity.IsActive = request.IsActive;
            entity.Token = request.Token;
            entity.AgentID = request.AgentID;
            entity.PartnerID = request.PartnerID;
            entity.MerchantID = request.MerchantID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : BBPSSettingsMessages.NotFound;
        }

        private static BBPSSettingsResponse MapToResponse(BBPSSettings e) => new()
        {
            BBPSSettingID = e.BBPSSettingID,
            Name = e.Name,
            IsActiveForWebsite = e.IsActiveForWebsite,
            IsActive = e.IsActive,
            Token = e.Token,
            AgentID = e.AgentID,
            PartnerID = e.PartnerID,
            MerchantID = e.MerchantID
        };
    }
}
