using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SlabMaster;
using FintechApp.Responses.SlabMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class SlabMasterService : ISlabMasterService
    {
        private readonly ISlabMasterRepository _repository;

        public SlabMasterService(ISlabMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SlabMasterResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<SlabMasterResponse?> GetByID(int slabMasterId)
        {
            var entity = await _repository.GetByID(slabMasterId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<SlabMasterDropdownResponse>> GetDropdown()
        {
            var all = await _repository.GetAll();
            return all.Where(x => x.IsActive)
                      .Select(x => new SlabMasterDropdownResponse
                      {
                          SlabMasterID = x.SlabMasterID,
                          SlabName = x.SlabName
                      }).ToList();
        }

        public async Task<string> Create(CreateSlabMasterRequest request)
        {
            var existing = await _repository.GetByName(request.SlabName);
            if (existing != null)
                return Messages.SlabMasterMessages.AlreadyExists;

            var entity = new SlabMaster
            {
                SlabName = request.SlabName,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateSlabMasterRequest request)
        {
            var entity = await _repository.GetByID(request.SlabMasterID);
            if (entity == null) return Messages.SlabMasterMessages.NotFound;

            entity.SlabName = request.SlabName;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int slabMasterId)
        {
            var entity = await _repository.GetByID(slabMasterId);
            if (entity == null) return Messages.SlabMasterMessages.NotFound;

            int result = await _repository.Delete(slabMasterId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static SlabMasterResponse MapToResponse(SlabMaster e) => new()
        {
            SlabMasterID = e.SlabMasterID,
            SlabName = e.SlabName,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };
    }
}
