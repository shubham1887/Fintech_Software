using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BlackListBankMaster;
using FintechApp.Responses.BlackListBankMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class BlackListBankMasterService : IBlackListBankMasterService
    {
        private readonly IBlackListBankMasterRepository _repository;
        public BlackListBankMasterService(IBlackListBankMasterRepository repository) { _repository = repository; }
        private static BlackListBankMasterResponse MapToResponse(BlackListBankMaster e) => new() { BlackListBankMasterID = e.BlackListBankMasterID, BankName = e.BankName, IFSC = e.IFSC, AccountNumber = e.AccountNumber, };
        public async Task<List<BlackListBankMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<BlackListBankMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateBlackListBankMasterRequest request) { var entity = new BlackListBankMaster { BankName = request.BankName, IFSC = request.IFSC, AccountNumber = request.AccountNumber, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateBlackListBankMasterRequest request) { var entity = await _repository.GetByID(request.BlackListBankMasterID); if (entity == null) return Messages.BlackListBankMasterMessages.NotFound; entity.BankName = request.BankName; entity.IFSC = request.IFSC; entity.AccountNumber = request.AccountNumber; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.BlackListBankMasterMessages.NotFound; }
    }
}
