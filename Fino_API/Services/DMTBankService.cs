using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DMTBank;
using FintechApp.Responses.DMTBank;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DMTBankService : IDMTBankService
    {
        private readonly IDMTBankRepository _repository;

        public DMTBankService(IDMTBankRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DMTBankResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<DMTBankResponse?> GetByID(int dmtBankId)
        {
            var entity = await _repository.GetByID(dmtBankId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<DMTBankDropdownResponse>> GetDropdown()
        {
            var all = await _repository.GetAll();
            return all.Where(x => x.IsActive)
                      .Select(x => new DMTBankDropdownResponse
                      {
                          DMTBankID = x.DMTBankID,
                          Name = x.Name,
                          DefaultIFSC = x.DefaultIFSC
                      }).ToList();
        }

        public async Task<string> Create(CreateDMTBankRequest request)
        {
            var existing = await _repository.GetByName(request.Name);
            if (existing != null)
                return Messages.DMTBankMessages.AlreadyExists;

            var entity = new DMTBank
            {
                Name = request.Name,
                DefaultIFSC = request.DefaultIFSC,
                IsActive = request.IsActive,
                IsIMPS = request.IsIMPS,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateDMTBankRequest request)
        {
            var entity = await _repository.GetByID(request.DMTBankID);
            if (entity == null) return Messages.DMTBankMessages.NotFound;

            entity.Name = request.Name;
            entity.DefaultIFSC = request.DefaultIFSC;
            entity.IsActive = request.IsActive;
            entity.IsIMPS = request.IsIMPS;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int dmtBankId)
        {
            var entity = await _repository.GetByID(dmtBankId);
            if (entity == null) return Messages.DMTBankMessages.NotFound;

            int result = await _repository.Delete(dmtBankId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static DMTBankResponse MapToResponse(DMTBank e) => new()
        {
            DMTBankID = e.DMTBankID,
            Name = e.Name,
            DefaultIFSC = e.DefaultIFSC,
            IsActive = e.IsActive,
            IsIMPS = e.IsIMPS,
            DateTime = e.DateTime
        };
    }
}
