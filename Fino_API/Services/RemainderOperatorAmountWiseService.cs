using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.RemainderOperatorAmountWise;
using FintechApp.Responses.RemainderOperatorAmountWise;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class RemainderOperatorAmountWiseService : IRemainderOperatorAmountWiseService
    {
        private readonly IRemainderOperatorAmountWiseRepository _repository;
        public RemainderOperatorAmountWiseService(IRemainderOperatorAmountWiseRepository repository) { _repository = repository; }
        private static RemainderOperatorAmountWiseResponse MapToResponse(RemainderOperatorAmountWise e) => new() { RemainderOperatorAmountWiseID = e.RemainderOperatorAmountWiseID, OperatorID = e.OperatorID, Amount = e.Amount, Days = e.Days, };
        public async Task<List<RemainderOperatorAmountWiseResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<RemainderOperatorAmountWiseResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateRemainderOperatorAmountWiseRequest request) { var entity = new RemainderOperatorAmountWise { OperatorID = request.OperatorID, Amount = request.Amount, Days = request.Days, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateRemainderOperatorAmountWiseRequest request) { var entity = await _repository.GetByID(request.RemainderOperatorAmountWiseID); if (entity == null) return Messages.RemainderOperatorAmountWiseMessages.NotFound; entity.OperatorID = request.OperatorID; entity.Amount = request.Amount; entity.Days = request.Days; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.RemainderOperatorAmountWiseMessages.NotFound; }
    }
}
