using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DateFormatMaster;
using FintechApp.Responses.DateFormatMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DateFormatMasterService : IDateFormatMasterService
    {
        private readonly IDateFormatMasterRepository _repository;
        public DateFormatMasterService(IDateFormatMasterRepository repository) { _repository = repository; }

        private static DateFormatMasterResponse MapToResponse(DateFormatMaster e) => new()
        {
            DateFormatMasterkID = e.DateFormatMasterkID,
            DateFormat = e.DateFormat,
            IsActive = e.IsActive,
        };

        public async Task<List<DateFormatMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<DateFormatMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateDateFormatMasterRequest request)
        {
            var entity = new DateFormatMaster
            {
                DateFormat = request.DateFormat,
                IsActive = request.IsActive,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateDateFormatMasterRequest request)
        {
            var entity = await _repository.GetByID(request.DateFormatMasterkID);
            if (entity == null) return Messages.DateFormatMasterMessages.NotFound;
            entity.DateFormat = request.DateFormat;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.DateFormatMasterMessages.NotFound;
        }
    }
}
