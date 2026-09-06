using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderConfigurationAdminCommission;
using FintechApp.Responses.ProviderConfigurationAdminCommission;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderConfigurationAdminCommissionService : IProviderConfigurationAdminCommissionService
    {
        private readonly IProviderConfigurationAdminCommissionRepository _repository;
        public ProviderConfigurationAdminCommissionService(IProviderConfigurationAdminCommissionRepository repository) { _repository = repository; }

        private static ProviderConfigurationAdminCommissionResponse MapToResponse(ProviderConfigurationAdminCommission e) => new()
        {
            ProviderConfigurationAdminCommissionID = e.ProviderConfigurationAdminCommissionID,
            ProviderID = e.ProviderID,
            ServiceID = e.ServiceID,
            OperatorID = e.OperatorID,
            FromAmount = e.FromAmount,
            ToAmount = e.ToAmount,
            AdminComm = e.AdminComm,
            AdminCommType = e.AdminCommType,
            AdminCommissionCategory = e.AdminCommissionCategory,
        };

        public async Task<List<ProviderConfigurationAdminCommissionResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ProviderConfigurationAdminCommissionResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ProviderConfigurationAdminCommissionResponse>> GetByProviderID(int providerid) { var list = await _repository.GetByProviderID(providerid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateProviderConfigurationAdminCommissionRequest request)
        {
            var entity = new ProviderConfigurationAdminCommission
            {
                ProviderID = request.ProviderID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                FromAmount = request.FromAmount,
                ToAmount = request.ToAmount,
                AdminComm = request.AdminComm,
                AdminCommType = request.AdminCommType,
                AdminCommissionCategory = request.AdminCommissionCategory,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderConfigurationAdminCommissionRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderConfigurationAdminCommissionID);
            if (entity == null) return Messages.ProviderConfigurationAdminCommissionMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.FromAmount = request.FromAmount;
            entity.ToAmount = request.ToAmount;
            entity.AdminComm = request.AdminComm;
            entity.AdminCommType = request.AdminCommType;
            entity.AdminCommissionCategory = request.AdminCommissionCategory;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderConfigurationAdminCommissionMessages.NotFound;
        }
    }
}
