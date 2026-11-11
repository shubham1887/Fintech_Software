using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserwiseDMTMapping;
using FintechApp.Responses.UserwiseDMTMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserwiseDMTMappingService : IUserwiseDMTMappingService
    {
        private readonly IUserwiseDMTMappingRepository _repository;

        public UserwiseDMTMappingService(IUserwiseDMTMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserwiseDMTMappingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserwiseDMTMappingResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserwiseDMTMappingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserwiseDMTMappingRequest request)
        {
            var entity = new UserwiseDMTMapping
            {
                UserID = request.UserID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                ProviderID = request.ProviderID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserwiseDMTMappingRequest request)
        {
            var entity = await _repository.GetByID(request.UserwiseDMTMappingID);
            if (entity == null) return UserwiseDMTMappingMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.ProviderID = request.ProviderID;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserwiseDMTMappingMessages.NotFound;
        }

        private static UserwiseDMTMappingResponse MapToResponse(UserwiseDMTMapping e) => new()
        {
            UserwiseDMTMappingID = e.UserwiseDMTMappingID,
            UserID = e.UserID,
            UserName = e.UserMaster?.PersonName ?? string.Empty,
            FirmName = e.UserMaster?.FirmName ?? string.Empty,
            MobileNumber = e.UserMaster?.MobileNumber ?? string.Empty,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName ?? string.Empty,
            ProviderID = e.ProviderID,
            ProviderName = e.ProviderMaster?.ProviderName ?? string.Empty,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
