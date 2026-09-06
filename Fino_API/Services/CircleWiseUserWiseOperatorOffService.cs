using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CircleWiseUserWiseOperatorOff;
using FintechApp.Responses.CircleWiseUserWiseOperatorOff;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CircleWiseUserWiseOperatorOffService : ICircleWiseUserWiseOperatorOffService
    {
        private readonly ICircleWiseUserWiseOperatorOffRepository _repository;
        public CircleWiseUserWiseOperatorOffService(ICircleWiseUserWiseOperatorOffRepository repository) { _repository = repository; }

        private static CircleWiseUserWiseOperatorOffResponse MapToResponse(CircleWiseUserWiseOperatorOff e) => new()
        {
            CircleWiseUserWiseOperatorOffID = e.CircleWiseUserWiseOperatorOffID,
            UserID = e.UserID,
            StateID = e.StateID,
            OperatorID = e.OperatorID,
        };

        public async Task<List<CircleWiseUserWiseOperatorOffResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CircleWiseUserWiseOperatorOffResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<CircleWiseUserWiseOperatorOffResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateCircleWiseUserWiseOperatorOffRequest request)
        {
            var entity = new CircleWiseUserWiseOperatorOff
            {
                UserID = request.UserID,
                StateID = request.StateID,
                OperatorID = request.OperatorID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCircleWiseUserWiseOperatorOffRequest request)
        {
            var entity = await _repository.GetByID(request.CircleWiseUserWiseOperatorOffID);
            if (entity == null) return Messages.CircleWiseUserWiseOperatorOffMessages.NotFound;
            entity.UserID = request.UserID;
            entity.StateID = request.StateID;
            entity.OperatorID = request.OperatorID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CircleWiseUserWiseOperatorOffMessages.NotFound;
        }
    }
}
