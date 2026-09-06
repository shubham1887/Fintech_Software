using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Mode;
using FintechApp.Responses.Mode;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ModeService : IModeService
    {
        private readonly IModeRepository _repository;
        private readonly ISubModeRepository _subModeRepository;

        public ModeService(IModeRepository repository, ISubModeRepository subModeRepository)
        {
            _repository = repository;
            _subModeRepository = subModeRepository;
        }

        public async Task<List<ModeResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<ModeResponse>> GetActive()
        {
            var all = await _repository.GetAll();
            return all.Where(x => x.IsActive).Select(MapToResponse).ToList();
        }

        public async Task<ModeResponse?> GetByID(int modeId)
        {
            var entity = await _repository.GetByID(modeId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateModeRequest request, int userId)
        {
            if (await _repository.ExistsByName(request.ModeName, 0))
                return Messages.ModeMessages.AlreadyExists;

            var entity = new Mode
            {
                ModeName = request.ModeName.Trim(),
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                IsDelete = false,
                CreatedDate = DateTime.Now,
                CreatedBy = userId
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateModeRequest request, int userId)
        {
            var entity = await _repository.GetByID(request.ModeID);
            if (entity == null) return Messages.ModeMessages.NotFound;

            if (await _repository.ExistsByName(request.ModeName, request.ModeID))
                return Messages.ModeMessages.AlreadyExists;

            entity.ModeName = request.ModeName.Trim();
            entity.IsActive = request.IsActive;
            entity.DisplayOrder = request.DisplayOrder;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = userId;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int modeId)
        {
            var entity = await _repository.GetByID(modeId);
            if (entity == null) return Messages.ModeMessages.NotFound;

            // Guard the FK: a mode with sub modes cannot be dropped from under them.
            if (await _subModeRepository.AnyByModeID(modeId))
                return Messages.ModeMessages.HasSubModes;

            int result = await _repository.Delete(modeId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static ModeResponse MapToResponse(Mode e) => new()
        {
            ModeID = e.ModeID,
            ModeName = e.ModeName,
            IsActive = e.IsActive,
            DisplayOrder = e.DisplayOrder,
            CreatedDate = e.CreatedDate
        };
    }
}
