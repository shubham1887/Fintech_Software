using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.NPCIBank;
using FintechApp.Responses.NPCIBank;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class NPCIBankService : INPCIBankService
    {
        private readonly INPCIBankRepository _repository;

        public NPCIBankService(INPCIBankRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<NPCIBankResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<NPCIBankResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateNPCIBankRequest request)
        {
            var entity = new NPCIBank
            {
                BankCode = request.BankCode,
                BankName = request.BankName,
                IFSC = request.IFSC,
                IIN = request.IIN,
                ParentBank = request.ParentBank,
                BankType = request.BankType,
                IsActive = request.IsActive
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateNPCIBankRequest request)
        {
            var entity = await _repository.GetByID(request.NPCIBankID);
            if (entity == null) return Messages.NPCIBankMessages.NotFound;
            entity.BankCode = request.BankCode;
            entity.BankName = request.BankName;
            entity.IFSC = request.IFSC;
            entity.IIN = request.IIN;
            entity.ParentBank = request.ParentBank;
            entity.BankType = request.BankType;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.NPCIBankMessages.NotFound;
        }

        private static NPCIBankResponse MapToResponse(NPCIBank e) => new()
        {
            NPCIBankID = e.NPCIBankID,
            BankCode = e.BankCode,
            BankName = e.BankName,
            IFSC = e.IFSC,
            IIN = e.IIN,
            ParentBank = e.ParentBank,
            BankType = e.BankType,
            IsActive = e.IsActive
        };
    }
}
