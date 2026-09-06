using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.ProviderOperatorMaster;
using FintechApp.Responses.ProviderOperatorMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderOperatorMasterService : IProviderOperatorMasterService
    {
        private readonly IProviderOperatorMasterRepository _repository;

        public ProviderOperatorMasterService(IProviderOperatorMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProviderOperatorMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<ProviderOperatorMasterResponse>> GetByProviderID(int providerId)
        {
            var list = await _repository.GetByProviderID(providerId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ProviderOperatorMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateProviderOperatorMasterRequest request)
        {
            var entity = new ProviderOperatorMaster
            {
                ProviderID = request.ProviderID,
                OperatorID = request.OperatorID,
                ServiceID = request.ServiceID,
                Operatorcode = request.Operatorcode,
                MarginType = request.MarginType,
                CommissionAdmin = request.CommissionAdmin,
                PendingCount = request.PendingCount,
                FailureCount = request.FailureCount,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderOperatorMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderOperatorID);
            if (entity == null) return ProviderOperatorMasterMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.OperatorID = request.OperatorID;
            entity.ServiceID = request.ServiceID;
            entity.Operatorcode = request.Operatorcode;
            entity.MarginType = request.MarginType;
            entity.CommissionAdmin = request.CommissionAdmin;
            entity.PendingCount = request.PendingCount;
            entity.FailureCount = request.FailureCount;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : ProviderOperatorMasterMessages.NotFound;
        }

        private static ProviderOperatorMasterResponse MapToResponse(ProviderOperatorMaster e) => new()
        {
            ProviderOperatorID = e.ProviderOperatorID,
            ProviderID = e.ProviderID,
            OperatorID = e.OperatorID,
            ServiceID = e.ServiceID,
            Operatorcode = e.Operatorcode,
            MarginType = e.MarginType,
            CommissionAdmin = e.CommissionAdmin,
            PendingCount = e.PendingCount,
            FailureCount = e.FailureCount,
            IsActive = e.IsActive,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
