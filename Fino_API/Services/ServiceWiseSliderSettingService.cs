using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ServiceWiseSliderSetting;
using FintechApp.Responses.ServiceWiseSliderSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ServiceWiseSliderSettingService : IServiceWiseSliderSettingService
    {
        private readonly IServiceWiseSliderSettingRepository _repository;
        public ServiceWiseSliderSettingService(IServiceWiseSliderSettingRepository repository) { _repository = repository; }

        private static ServiceWiseSliderSettingResponse MapToResponse(ServiceWiseSliderSetting e) => new()
        {
            ServiceWiseSliderSettingID = e.ServiceWiseSliderSettingID,
            ServiceID = e.ServiceID,
            ImagePath = e.ImagePath,
            Description = e.Description,
            ReferenceLink = e.ReferenceLink,
            Priority = e.Priority,
            IsActive = e.IsActive,
            IsDelete = e.IsDelete,
            Type = e.Type,
        };

        public async Task<List<ServiceWiseSliderSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ServiceWiseSliderSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ServiceWiseSliderSettingResponse>> GetByServiceID(int serviceid) { var list = await _repository.GetByServiceID(serviceid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateServiceWiseSliderSettingRequest request)
        {
            var entity = new ServiceWiseSliderSetting
            {
                ServiceID = request.ServiceID,
                ImagePath = request.ImagePath,
                Description = request.Description,
                ReferenceLink = request.ReferenceLink,
                Priority = request.Priority,
                IsActive = request.IsActive,
                IsDelete = request.IsDelete,
                Type = request.Type,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateServiceWiseSliderSettingRequest request)
        {
            var entity = await _repository.GetByID(request.ServiceWiseSliderSettingID);
            if (entity == null) return Messages.ServiceWiseSliderSettingMessages.NotFound;
            entity.ServiceID = request.ServiceID;
            entity.ImagePath = request.ImagePath;
            entity.Description = request.Description;
            entity.ReferenceLink = request.ReferenceLink;
            entity.Priority = request.Priority;
            entity.IsActive = request.IsActive;
            entity.IsDelete = request.IsDelete;
            entity.Type = request.Type;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ServiceWiseSliderSettingMessages.NotFound;
        }
    }
}
