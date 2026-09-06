using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderDetails;
using FintechApp.Responses.ProviderDetails;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderDetailsService : IProviderDetailsService
    {
        private readonly IProviderDetailsRepository _repository;

        public ProviderDetailsService(IProviderDetailsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProviderDetailsResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<ProviderDetailsResponse>> GetByProviderID(int providerId)
        {
            var list = await _repository.GetByProviderID(providerId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ProviderDetailsResponse?> GetByID(int providerDetailId)
        {
            var entity = await _repository.GetByID(providerDetailId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateProviderDetailsRequest request)
        {
            var entity = new ProviderDetails
            {
                ProviderID = request.ProviderID,
                ServiceID = request.ServiceID,
                UrlType = request.UrlType,
                ProviderURL = request.ProviderURL,
                MethodType = request.MethodType,
                Balance = request.Balance,
                HeaderJson = request.HeaderJson,
                DateFormat = request.DateFormat,
                IsActive = request.IsActive,
                TransactionIDPrefix = request.TransactionIDPrefix,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateProviderDetailsRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderDetailID);
            if (entity == null) return Messages.ProviderDetailsMessages.NotFound;

            entity.ProviderID = request.ProviderID;
            entity.ServiceID = request.ServiceID;
            entity.UrlType = request.UrlType;
            entity.ProviderURL = request.ProviderURL;
            entity.MethodType = request.MethodType;
            entity.Balance = request.Balance;
            entity.HeaderJson = request.HeaderJson;
            entity.DateFormat = request.DateFormat;
            entity.IsActive = request.IsActive;
            entity.TransactionIDPrefix = request.TransactionIDPrefix;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int providerDetailId)
        {
            var entity = await _repository.GetByID(providerDetailId);
            if (entity == null) return Messages.ProviderDetailsMessages.NotFound;

            int result = await _repository.Delete(providerDetailId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static ProviderDetailsResponse MapToResponse(ProviderDetails e) => new()
        {
            ProviderDetailID = e.ProviderDetailID,
            ProviderID = e.ProviderID,
            ProviderName = e.ProviderMaster?.ProviderName ?? string.Empty,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            UrlType = e.UrlType,
            ProviderURL = e.ProviderURL,
            MethodType = e.MethodType,
            Balance = e.Balance,
            HeaderJson = e.HeaderJson,
            DateFormat = e.DateFormat,
            IsActive = e.IsActive,
            TransactionIDPrefix = e.TransactionIDPrefix,
            DateTime = e.DateTime
        };
    }
}
