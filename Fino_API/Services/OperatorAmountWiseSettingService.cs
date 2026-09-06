using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OperatorAmountWiseSetting;
using FintechApp.Responses.OperatorAmountWiseSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OperatorAmountWiseSettingService : IOperatorAmountWiseSettingService
    {
        private readonly IOperatorAmountWiseSettingRepository _repository;
        public OperatorAmountWiseSettingService(IOperatorAmountWiseSettingRepository repository) { _repository = repository; }

        private static OperatorAmountWiseSettingResponse MapToResponse(OperatorAmountWiseSetting e) => new()
        {
            OperatorAmountWiseSettingID = e.OperatorAmountWiseSettingID,
            OperatorID = e.OperatorID,
            Amount = e.Amount,
            SerialCount = e.SerialCount
        };

        public async Task<List<OperatorAmountWiseSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OperatorAmountWiseSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<OperatorAmountWiseSettingResponse>> GetByOperatorID(int operatorID) { var list = await _repository.GetByOperatorID(operatorID); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateOperatorAmountWiseSettingRequest request)
        {
            var entity = new OperatorAmountWiseSetting
            {
                OperatorID = request.OperatorID,
                Amount = request.Amount,
                SerialCount = request.SerialCount
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateOperatorAmountWiseSettingRequest request)
        {
            var entity = await _repository.GetByID(request.OperatorAmountWiseSettingID);
            if (entity == null) return Messages.OperatorAmountWiseSettingMessages.NotFound;
            entity.OperatorID = request.OperatorID;
            entity.Amount = request.Amount;
            entity.SerialCount = request.SerialCount;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.OperatorAmountWiseSettingMessages.NotFound;
        }
    }
}
