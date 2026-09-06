using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserType;
using FintechApp.Responses.UserType;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserTypeMasterService : IUserTypeMasterService
    {
        private readonly IUserTypeMasterRepository _repository;

        public UserTypeMasterService(IUserTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserTypeResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<UserTypeResponse?> GetByID(int userTypeId)
        {
            var entity = await _repository.GetByID(userTypeId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<UserTypeDropdownResponse>> GetDropdown()
        {
            var all = await _repository.GetAll();
            return all.Where(x => x.IsActive)
                      .Select(x => new UserTypeDropdownResponse
                      {
                          UserTypeID = x.UserTypeID,
                          UserType = x.UserType,
                          AliasName = x.AliasName
                      }).ToList();
        }

        public async Task<string> Create(CreateUserTypeRequest request)
        {
            var existing = await _repository.GetByName(request.UserType);
            if (existing != null)
                return Messages.UserTypeMasterMessages.AlreadyExists;

            var entity = new UserTypeMaster
            {
                UserType = request.UserType,
                AliasName = request.AliasName,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateUserTypeRequest request)
        {
            var entity = await _repository.GetByID(request.UserTypeID);
            if (entity == null) return Messages.UserTypeMasterMessages.NotFound;

            entity.UserType = request.UserType;
            entity.AliasName = request.AliasName;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int userTypeId)
        {
            var entity = await _repository.GetByID(userTypeId);
            if (entity == null) return Messages.UserTypeMasterMessages.NotFound;

            int result = await _repository.Delete(userTypeId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static UserTypeResponse MapToResponse(UserTypeMaster e) => new()
        {
            UserTypeID = e.UserTypeID,
            UserType = e.UserType,
            AliasName = e.AliasName,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };
    }
}
