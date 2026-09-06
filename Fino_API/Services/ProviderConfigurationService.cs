using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderConfiguration;
using FintechApp.Responses.ProviderConfiguration;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderConfigurationService : IProviderConfigurationService
    {
        private readonly IProviderConfigurationRepository _repository;

        public ProviderConfigurationService(IProviderConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProviderConfigurationResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<ProviderConfigurationResponse>> GetByProviderID(int providerId)
        {
            var list = await _repository.GetByProviderID(providerId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<ProviderConfigurationResponse>> GetByOperatorID(int operatorId)
        {
            var list = await _repository.GetByOperatorID(operatorId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ProviderConfigurationResponse?> GetByID(int configurationId)
        {
            var entity = await _repository.GetByID(configurationId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateProviderConfigurationRequest request)
        {
            var entity = new ProviderConfiguration
            {
                OperatorID = request.OperatorID,
                ProviderID = request.ProviderID,
                ServiceID = request.ServiceID,
                OpCode = request.OpCode,
                Type = request.Type,
                Lapu = request.Lapu,
                Quantity = request.Quantity,
                AdminComm = request.AdminComm,
                AdminCommType = request.AdminCommType,
                IsActive = request.IsActive,
                CommissionCategory = request.CommissionCategory,
                PendingCount = request.PendingCount,
                PendingInMinute = request.PendingInMinute,
                FailCount = request.FailCount,
                FailInMinute = request.FailInMinute,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateProviderConfigurationRequest request)
        {
            var entity = await _repository.GetByID(request.ConfigurationID);
            if (entity == null) return Messages.ProviderConfigurationMessages.NotFound;

            entity.OperatorID = request.OperatorID;
            entity.ProviderID = request.ProviderID;
            entity.ServiceID = request.ServiceID;
            entity.OpCode = request.OpCode;
            entity.Type = request.Type;
            entity.Lapu = request.Lapu;
            entity.Quantity = request.Quantity;
            entity.AdminComm = request.AdminComm;
            entity.AdminCommType = request.AdminCommType;
            entity.IsActive = request.IsActive;
            entity.CommissionCategory = request.CommissionCategory;
            entity.PendingCount = request.PendingCount;
            entity.PendingInMinute = request.PendingInMinute;
            entity.FailCount = request.FailCount;
            entity.FailInMinute = request.FailInMinute;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int configurationId)
        {
            var entity = await _repository.GetByID(configurationId);
            if (entity == null) return Messages.ProviderConfigurationMessages.NotFound;

            int result = await _repository.Delete(configurationId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static ProviderConfigurationResponse MapToResponse(ProviderConfiguration e) => new()
        {
            ConfigurationID = e.ConfigurationID,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName ?? string.Empty,
            ProviderID = e.ProviderID,
            ProviderName = e.ProviderMaster?.ProviderName ?? string.Empty,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            OpCode = e.OpCode,
            Type = e.Type,
            Lapu = e.Lapu,
            Quantity = e.Quantity,
            AdminComm = e.AdminComm,
            AdminCommType = e.AdminCommType,
            IsActive = e.IsActive,
            CommissionCategory = e.CommissionCategory,
            PendingCount = e.PendingCount,
            PendingInMinute = e.PendingInMinute,
            FailCount = e.FailCount,
            FailInMinute = e.FailInMinute,
            DateTime = e.DateTime
        };
    }
}
