using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Inbox;
using FintechApp.Responses.Inbox;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class InboxService : IInboxService
    {
        private readonly IInboxRepository _repository;
        public InboxService(IInboxRepository repository) { _repository = repository; }

        private static InboxResponse MapToResponse(Inbox e) => new()
        {
            InboxID = e.InboxID,
            ServiceID = e.ServiceID,
            OperatorID = e.OperatorID,
            UserID = e.UserID,
            Amount = e.Amount,
            ConsumerNumber = e.ConsumerNumber,
            Status = e.Status,
            Through = e.Through,
            IPAddress = e.IPAddress,
            Param1 = e.Param1,
        };

        public async Task<List<InboxResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<InboxResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<InboxResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateInboxRequest request)
        {
            var entity = new Inbox
            {
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                UserID = request.UserID,
                Amount = request.Amount,
                ConsumerNumber = request.ConsumerNumber,
                Status = request.Status,
                Through = request.Through,
                IPAddress = request.IPAddress,
                Param1 = request.Param1,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateInboxRequest request)
        {
            var entity = await _repository.GetByID(request.InboxID);
            if (entity == null) return Messages.InboxMessages.NotFound;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.UserID = request.UserID;
            entity.Amount = request.Amount;
            entity.ConsumerNumber = request.ConsumerNumber;
            entity.Status = request.Status;
            entity.Through = request.Through;
            entity.IPAddress = request.IPAddress;
            entity.Param1 = request.Param1;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.InboxMessages.NotFound;
        }
    }
}
