using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.RangeWiseSequence;
using FintechApp.Responses.RangeWiseSequence;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class RangeWiseSequenceService : IRangeWiseSequenceService
    {
        private readonly IRangeWiseSequenceRepository _repository;

        public RangeWiseSequenceService(IRangeWiseSequenceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RangeWiseSequenceResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<RangeWiseSequenceResponse>> GetByOperatorID(int operatorId)
        {
            var list = await _repository.GetByOperatorID(operatorId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<RangeWiseSequenceResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateRangeWiseSequenceRequest request)
        {
            var entity = new RangeWiseSequence
            {
                ProviderID = request.ProviderID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                FromAmount = request.FromAmount,
                ToAmount = request.ToAmount,
                IsDelete = request.IsDelete,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateRangeWiseSequenceRequest request)
        {
            var entity = await _repository.GetByID(request.RangeWiseSequenceID);
            if (entity == null) return RangeWiseSequenceMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.FromAmount = request.FromAmount;
            entity.ToAmount = request.ToAmount;
            entity.IsDelete = request.IsDelete;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : RangeWiseSequenceMessages.NotFound;
        }

        private static RangeWiseSequenceResponse MapToResponse(RangeWiseSequence e) => new()
        {
            RangeWiseSequenceID = e.RangeWiseSequenceID,
            ProviderID = e.ProviderID,
            ServiceID = e.ServiceID,
            OperatorID = e.OperatorID,
            FromAmount = e.FromAmount,
            ToAmount = e.ToAmount,
            IsDelete = e.IsDelete,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
