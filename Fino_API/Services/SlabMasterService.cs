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
        // Only to put a NAME on the owner column of the admin listing — a reseller's card shows
        // whose it is, not a bare user id.
        private readonly IUserRepository _userRepository;

        public SlabMasterService(ISlabMasterRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<List<SlabMasterResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            var users = await _userRepository.GetAllUsers();
            return all.Select(e => MapToResponse(e, all, users)).ToList();
        }

        public async Task<SlabMasterResponse?> GetByID(int slabMasterId)
        {
            var entity = await _repository.GetByID(slabMasterId);
            if (entity == null) return null;
            var all = await _repository.GetAll();
            var users = await _userRepository.GetAllUsers();
            return MapToResponse(entity, all, users);
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

        private static SlabMasterResponse MapToResponse(
            SlabMaster e, List<SlabMaster> allSlabs, List<UserMaster> users)
        {
            var owner = e.OwnerUserID is int oid && oid > 0
                ? users.FirstOrDefault(u => u.UserMasterID == oid) : null;

            return new SlabMasterResponse
            {
                SlabMasterID = e.SlabMasterID,
                SlabName = e.SlabName,
                IsActive = e.IsActive,
                DateTime = e.DateTime,
                // No owner recorded = the platform operator's own card, which is what every
                // pre-delegation row reads as.
                OwnerName = owner == null
                    ? "Admin"
                    : (!string.IsNullOrWhiteSpace(owner.FirmName) ? owner.FirmName
                        : owner.PersonName ?? $"User {owner.UserMasterID}"),
                IsDelegatedSlab = e.IsDelegatedSlab,
                ParentSlabName = e.ParentSlabID is int pid
                    ? allSlabs.FirstOrDefault(s => s.SlabMasterID == pid)?.SlabName ?? string.Empty
                    : string.Empty,
                Status = SlabRowStatus.Normalize(e.Status),
            };
        }
    }
}
