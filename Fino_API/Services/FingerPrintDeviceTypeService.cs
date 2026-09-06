using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.FingerPrintDeviceType;
using FintechApp.Responses.FingerPrintDeviceType;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class FingerPrintDeviceTypeService : IFingerPrintDeviceTypeService
    {
        private readonly IFingerPrintDeviceTypeRepository _repository;
        public FingerPrintDeviceTypeService(IFingerPrintDeviceTypeRepository repository) { _repository = repository; }

        private static FingerPrintDeviceTypeResponse MapToResponse(FingerPrintDeviceType e) => new()
        {
            FingerPrintDeviceTypeID = e.FingerPrintDeviceTypeID,
            DeviceName = e.DeviceName,
            Value = e.Value,
        };

        public async Task<List<FingerPrintDeviceTypeResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<FingerPrintDeviceTypeResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateFingerPrintDeviceTypeRequest request)
        {
            var entity = new FingerPrintDeviceType
            {
                DeviceName = request.DeviceName,
                Value = request.Value,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateFingerPrintDeviceTypeRequest request)
        {
            var entity = await _repository.GetByID(request.FingerPrintDeviceTypeID);
            if (entity == null) return Messages.FingerPrintDeviceTypeMessages.NotFound;
            entity.DeviceName = request.DeviceName;
            entity.Value = request.Value;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.FingerPrintDeviceTypeMessages.NotFound;
        }
    }
}
