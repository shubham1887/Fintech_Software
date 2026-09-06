using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DenominationMaster;
using FintechApp.Responses.DenominationMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DenominationMasterService : IDenominationMasterService
    {
        private readonly IDenominationMasterRepository _repository;

        public DenominationMasterService(IDenominationMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DenominationMasterResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<DenominationMasterResponse?> GetByID(int denominationMasterId)
        {
            var entity = await _repository.GetByID(denominationMasterId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateDenominationMasterRequest request)
        {
            var entity = new DenominationMaster
            {
                Description = request.Description,
                DenominationRange = request.DenominationRange,
                IsStop = request.IsStop,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateDenominationMasterRequest request)
        {
            var entity = await _repository.GetByID(request.DenominationMasterID);
            if (entity == null) return Messages.DenominationMasterMessages.NotFound;

            entity.Description = request.Description;
            entity.DenominationRange = request.DenominationRange;
            entity.IsStop = request.IsStop;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int denominationMasterId)
        {
            var entity = await _repository.GetByID(denominationMasterId);
            if (entity == null) return Messages.DenominationMasterMessages.NotFound;
            int result = await _repository.Delete(denominationMasterId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static DenominationMasterResponse MapToResponse(DenominationMaster e) => new()
        {
            DenominationMasterID = e.DenominationMasterID,
            Description = e.Description,
            DenominationRange = e.DenominationRange,
            IsStop = e.IsStop,
            DateTime = e.DateTime
        };
    }
}
