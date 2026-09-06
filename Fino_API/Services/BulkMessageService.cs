using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BulkMessage;
using FintechApp.Responses.BulkMessage;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class BulkMessageService : IBulkMessageService
    {
        private readonly IBulkMessageRepository _repository;
        public BulkMessageService(IBulkMessageRepository repository) { _repository = repository; }

        private static BulkMessageResponse MapToResponse(BulkMessage e) => new()
        {
            BulkMessageID = e.BulkMessageID,
            UserID = e.UserID,
            Message = e.Message,
            IsSMS = e.IsSMS,
            IsEmail = e.IsEmail,
            IsWhatsUp = e.IsWhatsUp,
            IsDelete = e.IsDelete,
        };

        public async Task<List<BulkMessageResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<BulkMessageResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<BulkMessageResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateBulkMessageRequest request)
        {
            var entity = new BulkMessage
            {
                UserID = request.UserID,
                Message = request.Message,
                IsSMS = request.IsSMS,
                IsEmail = request.IsEmail,
                IsWhatsUp = request.IsWhatsUp,
                IsDelete = request.IsDelete,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateBulkMessageRequest request)
        {
            var entity = await _repository.GetByID(request.BulkMessageID);
            if (entity == null) return Messages.BulkMessageMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Message = request.Message;
            entity.IsSMS = request.IsSMS;
            entity.IsEmail = request.IsEmail;
            entity.IsWhatsUp = request.IsWhatsUp;
            entity.IsDelete = request.IsDelete;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.BulkMessageMessages.NotFound;
        }
    }
}
