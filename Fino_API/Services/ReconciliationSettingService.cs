using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.ReconciliationSetting;
using FintechApp.Responses.ReconciliationSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ReconciliationSettingService : IReconciliationSettingService
    {
        private readonly IReconciliationSettingRepository _repository;

        public ReconciliationSettingService(IReconciliationSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ReconciliationSettingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<ReconciliationSettingResponse>> GetByProviderID(int providerId)
        {
            var list = await _repository.GetByProviderID(providerId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ReconciliationSettingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateReconciliationSettingRequest request)
        {
            var entity = new ReconciliationSetting
            {
                ProviderID = request.ProviderID,
                ConsumerNumber = request.ConsumerNumber,
                Amount = request.Amount,
                Status = request.Status,
                WorksheetName = request.WorksheetName,
                APIUserRequestID = request.APIUserRequestID,
                SuccessStatusAs = request.SuccessStatusAs,
                FailureStatusAs = request.FailureStatusAs
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateReconciliationSettingRequest request)
        {
            var entity = await _repository.GetByID(request.ReconciliationSettingID);
            if (entity == null) return ReconciliationSettingMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.ConsumerNumber = request.ConsumerNumber;
            entity.Amount = request.Amount;
            entity.Status = request.Status;
            entity.WorksheetName = request.WorksheetName;
            entity.APIUserRequestID = request.APIUserRequestID;
            entity.SuccessStatusAs = request.SuccessStatusAs;
            entity.FailureStatusAs = request.FailureStatusAs;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : ReconciliationSettingMessages.NotFound;
        }

        private static ReconciliationSettingResponse MapToResponse(ReconciliationSetting e) => new()
        {
            ReconciliationSettingID = e.ReconciliationSettingID,
            ProviderID = e.ProviderID,
            ConsumerNumber = e.ConsumerNumber,
            Amount = e.Amount,
            Status = e.Status,
            WorksheetName = e.WorksheetName,
            APIUserRequestID = e.APIUserRequestID,
            SuccessStatusAs = e.SuccessStatusAs,
            FailureStatusAs = e.FailureStatusAs
        };
    }
}
