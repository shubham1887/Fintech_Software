using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserServiceCharge;
using FintechApp.Responses.UserServiceCharge;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserServiceChargeService : IUserServiceChargeService
    {
        private readonly IUserServiceChargeRepository _repository;

        public UserServiceChargeService(IUserServiceChargeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserServiceChargeResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserServiceChargeResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserServiceChargeResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserServiceChargeRequest request)
        {
            var entity = new UserServiceCharge
            {
                UserID = request.UserID,
                ServiceID = request.ServiceID,
                Charge = request.Charge,
                Remark = request.Remark,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserServiceChargeRequest request)
        {
            var entity = await _repository.GetByID(request.UserServiceChargeID);
            if (entity == null) return UserServiceChargeMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ServiceID = request.ServiceID;
            entity.Charge = request.Charge;
            entity.Remark = request.Remark;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserServiceChargeMessages.NotFound;
        }

        private static UserServiceChargeResponse MapToResponse(UserServiceCharge e) => new()
        {
            UserServiceChargeID = e.UserServiceChargeID,
            UserID = e.UserID,
            ServiceID = e.ServiceID,
            Charge = e.Charge,
            Remark = e.Remark,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
