using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PaySprintBank;
using FintechApp.Responses.PaySprintBank;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class PaySprintBankService : IPaySprintBankService
    {
        private readonly IPaySprintBankRepository _repository;
        public PaySprintBankService(IPaySprintBankRepository repository) { _repository = repository; }
        private static PaySprintBankResponse MapToResponse(PaySprintBank e) => new() { ID = e.ID, BankName = e.BankName, Sequence = e.Sequence, };
        public async Task<List<PaySprintBankResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PaySprintBankResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreatePaySprintBankRequest request) { var entity = new PaySprintBank { BankName = request.BankName, Sequence = request.Sequence, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdatePaySprintBankRequest request) { var entity = await _repository.GetByID(request.ID); if (entity == null) return Messages.PaySprintBankMessages.NotFound; entity.BankName = request.BankName; entity.Sequence = request.Sequence; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.PaySprintBankMessages.NotFound; }
    }
}
