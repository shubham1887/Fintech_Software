using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.OperatorSequence;
using FintechApp.Requests.OperatorSequenceUserWise;
using FintechApp.Responses.OperatorSequence;
using FintechApp.Responses.OperatorSequenceUserWise;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OperatorSequenceUserWiseService : IOperatorSequenceUserWiseService
    {
        private readonly IOperatorSequenceUserWiseRepository _repository;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IUserRepository _userRepo;

        public OperatorSequenceUserWiseService(
            IOperatorSequenceUserWiseRepository repository,
            IOperatorRepository operatorRepo,
            IProviderMasterRepository providerRepo,
            IServiceRepository serviceRepo,
            IUserRepository userRepo)
        {
            _repository = repository;
            _operatorRepo = operatorRepo;
            _providerRepo = providerRepo;
            _serviceRepo = serviceRepo;
            _userRepo = userRepo;
        }

        public async Task<List<OperatorSequenceUserWiseResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<OperatorSequenceUserWiseResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<OperatorSequenceUserWiseResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateOperatorSequenceUserWiseRequest request)
        {
            var entity = new OperatorSequenceUserWise
            {
                UserID = request.UserID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                ProviderID = request.ProviderID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateOperatorSequenceUserWiseRequest request)
        {
            var entity = await _repository.GetByID(request.OperatorSequenceUserWiseID);
            if (entity == null) return OperatorSequenceUserWiseMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.ProviderID = request.ProviderID;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : OperatorSequenceUserWiseMessages.NotFound;
        }

        private static OperatorSequenceUserWiseResponse MapToResponse(OperatorSequenceUserWise e) => new()
        {
            OperatorSequenceUserWiseID = e.OperatorSequenceUserWiseID,
            UserID = e.UserID,
            ServiceID = e.ServiceID,
            OperatorID = e.OperatorID,
            ProviderID = e.ProviderID,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };

        // ── Legacy user-wise switching contract ──────────────────────────────────────

        private static string FormatUserName(UserMaster? user) =>
            user == null ? string.Empty :
            string.IsNullOrWhiteSpace(user.MobileNumber) ? user.FirmName : $"{user.FirmName} [{user.MobileNumber}]";

        // The user's assigned providers for one operator, in insertion (priority) order.
        private List<SequenceProviderItemResponse> BuildAssignedProviders(
            List<OperatorSequenceUserWise> rows, List<ProviderMaster> providers, int operatorID, int userID)
        {
            return rows
                .Where(r => r.OperatorID == operatorID && r.UserID == userID)
                .OrderBy(r => r.OperatorSequenceUserWiseID)
                .Select(r => new SequenceProviderItemResponse
                {
                    OperatorID = operatorID,
                    ProviderID = r.ProviderID,
                    ProviderName = providers.FirstOrDefault(p => p.ProviderID == r.ProviderID)?.ProviderName ?? $"#{r.ProviderID}",
                    Count = 5,
                    IsActive = true
                })
                .ToList();
        }

        public async Task<List<OperatorSequenceGridResponse>> Select(SelectUserWiseSequenceRequest request)
        {
            var result = new List<OperatorSequenceGridResponse>();
            if (request.ServiceID <= 0) return result;

            var user = await _userRepo.GetUserById(request.UserID);
            string userName = FormatUserName(user);

            var service = await _serviceRepo.GetServiceById(request.ServiceID);
            string serviceName = service?.Name?.Trim() ?? string.Empty;

            var operators = (await _operatorRepo.GetActiveOperators())
                .Where(o => o.ServiceID == request.ServiceID)
                .OrderBy(o => o.OperatorName)
                .ToList();

            var rows = await _repository.GetAll();
            var providers = await _providerRepo.GetAll();

            foreach (var op in operators)
            {
                result.Add(new OperatorSequenceGridResponse
                {
                    UserID = request.UserID,
                    UserName = userName,
                    ServiceID = request.ServiceID,
                    ServiceName = serviceName,
                    OperatorID = op.OperatorID,
                    OperatorName = op.OperatorName,
                    Detail = $"{serviceName}{Environment.NewLine}{op.OperatorName}",
                    ProviderList = BuildAssignedProviders(rows, providers, op.OperatorID, request.UserID)
                });
            }

            return result;
        }

        public async Task<OperatorSequenceGridResponse?> SelectByOperator(SelectUserWiseByOperatorRequest request)
        {
            var op = await _operatorRepo.GetOperatorById(request.OperatorID);
            if (op == null) return null;

            var user = await _userRepo.GetUserById(request.UserID);
            var service = await _serviceRepo.GetServiceById(op.ServiceID);
            var rows = await _repository.GetAll();
            var providers = await _providerRepo.GetAll();

            return new OperatorSequenceGridResponse
            {
                UserID = request.UserID,
                UserName = FormatUserName(user),
                ServiceID = op.ServiceID,
                ServiceName = service?.Name?.Trim() ?? string.Empty,
                OperatorID = op.OperatorID,
                OperatorName = op.OperatorName,
                ProviderList = BuildAssignedProviders(rows, providers, op.OperatorID, request.UserID)
            };
        }

        public async Task<string> InsertSequence(InsertUserWiseSequenceRequest request)
        {
            if (request.UserID <= 0) return OperatorSequenceUserWiseMessages.InvalidUser;
            if (request.OperatorID <= 0) return OperatorSequenceMessages.InvalidOperator;

            int serviceID = request.ServiceID;
            if (serviceID <= 0)
            {
                var op = await _operatorRepo.GetOperatorById(request.OperatorID);
                serviceID = op?.ServiceID ?? 0;
            }

            await _repository.DeleteByUserAndOperator(request.UserID, request.OperatorID);

            foreach (var provider in (request.ProviderList ?? new List<SequenceProviderItem>()).Where(p => p.ProviderID > 0))
            {
                int result = await _repository.Insert(new OperatorSequenceUserWise
                {
                    UserID = request.UserID,
                    ProviderID = provider.ProviderID,
                    OperatorID = request.OperatorID,
                    ServiceID = serviceID,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                    IsDeleted = false
                });
                if (result <= 0) return OperatorSequenceUserWiseMessages.SaveFailed;
            }

            return StatusType.Ok;
        }

        public async Task<string> DeleteByUser(DeleteUserWiseByUserRequest request)
        {
            if (request.UserID <= 0) return OperatorSequenceUserWiseMessages.InvalidUser;
            await _repository.DeleteByUserAndOperator(request.UserID, request.OperatorID);
            return StatusType.Ok;
        }

        public async Task<string> DeleteAllSequences()
        {
            await _repository.DeleteAllRows();
            return StatusType.Ok;
        }

        public async Task<List<OperatorSequenceGridResponse>> SelectAllUserwiseSequenceList()
        {
            var rows = await _repository.GetAll();
            var providers = await _providerRepo.GetAll();
            var operators = await _operatorRepo.GetAllOperators();
            var services = await _serviceRepo.GetAllServices();
            var users = await _userRepo.GetAllUsers();

            var result = new List<OperatorSequenceGridResponse>();

            foreach (var group in rows.GroupBy(r => new { r.UserID, r.OperatorID }))
            {
                var first = group.OrderBy(r => r.OperatorSequenceUserWiseID).First();
                var op = operators.FirstOrDefault(o => o.OperatorID == group.Key.OperatorID);
                var user = users.FirstOrDefault(u => u.UserMasterID == group.Key.UserID);
                var service = services.FirstOrDefault(s => s.ServiceID == first.ServiceID)
                              ?? services.FirstOrDefault(s => s.ServiceID == op?.ServiceID);

                result.Add(new OperatorSequenceGridResponse
                {
                    OperatorSequenceUserWiseID = first.OperatorSequenceUserWiseID,
                    UserID = group.Key.UserID,
                    UserName = FormatUserName(user),
                    OperatorID = group.Key.OperatorID,
                    OperatorName = op?.OperatorName ?? $"#{group.Key.OperatorID}",
                    ServiceID = service?.ServiceID ?? first.ServiceID,
                    ServiceName = service?.Name?.Trim() ?? string.Empty,
                    ProviderList = BuildAssignedProviders(rows, providers, group.Key.OperatorID, group.Key.UserID)
                });
            }

            return result
                .OrderBy(r => r.UserName)
                .ThenBy(r => r.ServiceName)
                .ThenBy(r => r.OperatorName)
                .ToList();
        }
    }
}
