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

        // Amount bands decide which provider a transfer is routed through. Two bands that overlap
        // for the same operator make the routing ambiguous — whichever row the engine happens to
        // read first wins — so an overlap is rejected at the point it is entered rather than
        // discovered later as transfers going out through the wrong provider.
        private async Task<string> ValidateRange(int operatorId, decimal from, decimal to, int ignoreId)
        {
            if (to <= from) return RangeWiseSequenceMessages.InvalidRange;
            var existing = await _repository.GetByOperatorID(operatorId);
            bool clashes = existing.Any(x =>
                x.RangeWiseSequenceID != ignoreId && !x.IsDelete && from <= x.ToAmount && to >= x.FromAmount);
            return clashes ? RangeWiseSequenceMessages.OverlappingRange : StatusType.Ok;
        }

        public async Task<string> Create(CreateRangeWiseSequenceRequest request)
        {
            string invalid = await ValidateRange(request.OperatorID, request.FromAmount, request.ToAmount, 0);
            if (invalid != StatusType.Ok) return invalid;

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

            string invalid = await ValidateRange(request.OperatorID, request.FromAmount, request.ToAmount, request.RangeWiseSequenceID);
            if (invalid != StatusType.Ok) return invalid;

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
            ProviderName = e.ProviderMaster?.ProviderName ?? string.Empty,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName ?? string.Empty,
            FromAmount = e.FromAmount,
            ToAmount = e.ToAmount,
            IsDelete = e.IsDelete,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
