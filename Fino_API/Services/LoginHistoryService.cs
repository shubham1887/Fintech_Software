using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.LoginHistory;
using FintechApp.Responses.LoginHistory;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly ILoginHistoryRepository _repository;

        public LoginHistoryService(ILoginHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LoginHistoryResponse>> GetAll(GetLoginHistoryRequest request)
        {
            var all = await _repository.GetByUserID(
                request.UserID,
                request.FromDate,
                request.ToDate,
                request.PageNumber,
                request.PageSize
            );

            if (!string.IsNullOrWhiteSpace(request.Through))
                all = all.Where(x => x.Through.Equals(request.Through, StringComparison.OrdinalIgnoreCase)).ToList();

            return all.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateLoginHistoryRequest request)
        {
            var entity = new LoginHistory
            {
                UserID = request.UserID,
                IPAddress = request.IPAddress,
                Token = request.Token,
                Through = request.Through,
                Date = request.Date == default ? DateTime.UtcNow : request.Date,
                LoginMessage = request.LoginMessage,
                Location = request.Location,
                SimCardDetails = request.SimCardDetails,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        private static LoginHistoryResponse MapToResponse(LoginHistory x) => new()
        {
            LoginHistoryID = x.LoginHistoryID,
            UserID = x.UserID,
            IPAddress = x.IPAddress,
            Token = x.Token,
            Through = x.Through,
            Date = x.Date,
            LoginMessage = x.LoginMessage,
            Location = x.Location,
            SimCardDetails = x.SimCardDetails,
            DateTime = x.DateTime,
            UpdatedateTime = x.UpdatedateTime
        };
    }
}
