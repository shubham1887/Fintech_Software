using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LeadGenerationSetting;
using FintechApp.Responses.LeadGenerationSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class LeadGenerationSettingService : ILeadGenerationSettingService
    {
        private readonly ILeadGenerationSettingRepository _repository;
        public LeadGenerationSettingService(ILeadGenerationSettingRepository repository) { _repository = repository; }

        private static LeadGenerationSettingResponse MapToResponse(LeadGenerationSetting e) => new()
        {
            LeadGenerationSettingID = e.LeadGenerationSettingID,
            Name = e.Name,
            MerchantID = e.MerchantID,
            Token = e.Token,
            Slat = e.Slat,
            PublicKey = e.PublicKey,
            IsActiveWeb = e.IsActiveWeb,
            IsActive = e.IsActive,
            Mode = e.Mode,
            DisplayName = e.DisplayName,
            Sequence = e.Sequence,
            IsActiveForWebsite = e.IsActiveForWebsite,
        };

        public async Task<List<LeadGenerationSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LeadGenerationSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateLeadGenerationSettingRequest request)
        {
            var entity = new LeadGenerationSetting
            {
                Name = request.Name,
                MerchantID = request.MerchantID,
                Token = request.Token,
                Slat = request.Slat,
                PublicKey = request.PublicKey,
                IsActiveWeb = request.IsActiveWeb,
                IsActive = request.IsActive,
                Mode = request.Mode,
                DisplayName = request.DisplayName,
                Sequence = request.Sequence,
                IsActiveForWebsite = request.IsActiveForWebsite,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateLeadGenerationSettingRequest request)
        {
            var entity = await _repository.GetByID(request.LeadGenerationSettingID);
            if (entity == null) return Messages.LeadGenerationSettingMessages.NotFound;
            entity.Name = request.Name;
            entity.MerchantID = request.MerchantID;
            entity.Token = request.Token;
            entity.Slat = request.Slat;
            entity.PublicKey = request.PublicKey;
            entity.IsActiveWeb = request.IsActiveWeb;
            entity.IsActive = request.IsActive;
            entity.Mode = request.Mode;
            entity.DisplayName = request.DisplayName;
            entity.Sequence = request.Sequence;
            entity.IsActiveForWebsite = request.IsActiveForWebsite;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.LeadGenerationSettingMessages.NotFound;
        }
    }
}
