using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CircleWiseOperatorLimit;
using FintechApp.Responses.CircleWiseOperatorLimit;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CircleWiseOperatorLimitService : ICircleWiseOperatorLimitService
    {
        private readonly ICircleWiseOperatorLimitRepository _repository;
        public CircleWiseOperatorLimitService(ICircleWiseOperatorLimitRepository repository) { _repository = repository; }

        private static CircleWiseOperatorLimitResponse MapToResponse(CircleWiseOperatorLimit e) => new()
        {
            CircleWiseOperatorLimitID = e.CircleWiseOperatorLimitID,
            UserID = e.UserID,
            StateID = e.StateID,
            OperatorID = e.OperatorID,
            AmountLimit = e.AmountLimit,
        };

        public async Task<List<CircleWiseOperatorLimitResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CircleWiseOperatorLimitResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<CircleWiseOperatorLimitResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateCircleWiseOperatorLimitRequest request)
        {
            var entity = new CircleWiseOperatorLimit
            {
                UserID = request.UserID,
                StateID = request.StateID,
                OperatorID = request.OperatorID,
                AmountLimit = request.AmountLimit,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCircleWiseOperatorLimitRequest request)
        {
            var entity = await _repository.GetByID(request.CircleWiseOperatorLimitID);
            if (entity == null) return Messages.CircleWiseOperatorLimitMessages.NotFound;
            entity.UserID = request.UserID;
            entity.StateID = request.StateID;
            entity.OperatorID = request.OperatorID;
            entity.AmountLimit = request.AmountLimit;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CircleWiseOperatorLimitMessages.NotFound;
        }
    }
}
