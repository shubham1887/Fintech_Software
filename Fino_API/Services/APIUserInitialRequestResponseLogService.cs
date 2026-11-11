using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Responses.APIUserInitialRequestResponseLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class APIUserInitialRequestResponseLogService : IAPIUserInitialRequestResponseLogService
    {
        private readonly IAPIUserInitialRequestResponseLogRepository _repository;
        private readonly IUserRepository _userRepo;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IOperatorRepository _operatorRepo;
        public APIUserInitialRequestResponseLogService(
            IAPIUserInitialRequestResponseLogRepository repository,
            IUserRepository userRepo,
            ITransactionMasterRepository txnRepo,
            IServiceRepository serviceRepo,
            IOperatorRepository operatorRepo)
        {
            _repository = repository;
            _userRepo = userRepo;
            _txnRepo = txnRepo;
            _serviceRepo = serviceRepo;
            _operatorRepo = operatorRepo;
        }

        /// <summary>
        /// The log the way a support desk reads it: a day, a partner, a service, an operator, a
        /// free-text term — with the names resolved. The screen this serves replaced a dump of
        /// the raw table that showed a log id and a user id and could not be filtered at all.
        /// </summary>
        public async Task<APIUserLogWrapper> Select(SelectAPIUserLogRequest request)
        {
            var wrapper = new APIUserLogWrapper();
            IEnumerable<APIUserInitialRequestResponseLog> rows = await _repository.GetAll();

            if (DateTime.TryParse(request.FromDate, out var from)) rows = rows.Where(r => r.DateTime >= from.Date);
            if (DateTime.TryParse(request.ToDate, out var to)) rows = rows.Where(r => r.DateTime < to.Date.AddDays(1));
            if (request.UserID > 0) rows = rows.Where(r => r.UserID == request.UserID);

            var list = rows.OrderByDescending(r => r.DateTime).ThenByDescending(r => r.APIUserInitialRequestResponseLogID).ToList();

            // The transaction each call became — that is where the service, operator, amount and
            // outcome live. One query for the page's ids, not one per row.
            var txnById = (await _txnRepo.GetByIDs(list.Select(r => r.TransactionMasterID)))
                .GroupBy(t => t.TransactionMasterID).ToDictionary(g => g.Key, g => g.First());
            if (request.ServiceID > 0)
                list = list.Where(r => txnById.TryGetValue(r.TransactionMasterID, out var t) && t.ServiceID == request.ServiceID).ToList();
            if (request.OperatorID > 0)
                list = list.Where(r => txnById.TryGetValue(r.TransactionMasterID, out var t) && t.OperatorID == request.OperatorID).ToList();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim();
                list = list.Where(r =>
                    (r.ConsumerNumber ?? string.Empty).Contains(s, StringComparison.OrdinalIgnoreCase)
                    || (r.APIUserRequestID ?? string.Empty).Contains(s, StringComparison.OrdinalIgnoreCase)
                    || r.TransactionMasterID.ToString() == s
                    || (r.Request ?? string.Empty).Contains(s, StringComparison.OrdinalIgnoreCase)
                    || (r.Response ?? string.Empty).Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            wrapper.Count = list.Count;
            if (request.TakeCount > 0) list = list.Skip(Math.Max(0, request.SkipCount)).Take(request.TakeCount).ToList();

            var users = (await _userRepo.GetAllUsersWithDeleted()).GroupBy(u => u.UserMasterID).ToDictionary(g => g.Key, g => g.First());
            var services = (await _serviceRepo.GetAllServices()).GroupBy(x => x.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);
            var operators = (await _operatorRepo.GetAllOperators()).GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First().OperatorName);

            wrapper.list = list.Select(e =>
            {
                var r = MapToResponse(e);
                if (users.TryGetValue(e.UserID, out var u))
                {
                    r.UserName = !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName : u.PersonName;
                    r.MobileNumber = u.MobileNumber ?? string.Empty;
                }
                if (txnById.TryGetValue(e.TransactionMasterID, out var t))
                {
                    r.ServiceName = services.TryGetValue(t.ServiceID, out var sn) ? sn : string.Empty;
                    r.OperatorName = operators.TryGetValue(t.OperatorID, out var on) ? on : string.Empty;
                    r.Amount = t.Amount;
                    r.Status = t.Status;
                    if (string.IsNullOrWhiteSpace(r.ConsumerNumber)) r.ConsumerNumber = t.ConsumerNumber;
                }
                return r;
            }).ToList();
            return wrapper;
        }

        private static APIUserInitialRequestResponseLogResponse MapToResponse(APIUserInitialRequestResponseLog e) => new()
        {
            APIUserInitialRequestResponseLogID = e.APIUserInitialRequestResponseLogID,
            UserID = e.UserID,
            Request = e.Request,
            Response = e.Response,
            TransactionMasterID = e.TransactionMasterID,
            APIUserRequestID = e.APIUserRequestID,
            ConsumerNumber = e.ConsumerNumber,
            DateTime = e.DateTime,
        };

        public async Task<List<APIUserInitialRequestResponseLogResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<APIUserInitialRequestResponseLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<APIUserInitialRequestResponseLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        /// <summary>
        /// The API traffic for ONE transaction: what the integrator sent us and what we answered
        /// (and, on the callback log, what we pushed back to them). Admin and support read any
        /// transaction; every other caller — the API user itself — only ever reads its own rows,
        /// so one integrator can never read another's payload by quoting a transaction id.
        /// </summary>
        public async Task<List<APIUserInitialRequestResponseLogResponse>> GetByTransaction(int transactionMasterID, int callerUserID, int callerUserTypeID)
        {
            var list = await _repository.GetByTransaction(transactionMasterID);
            bool isAdmin = callerUserTypeID == UserTypeCode.Admin || callerUserTypeID == UserTypeCode.SupportUser;
            if (!isAdmin) list = list.Where(x => x.UserID == callerUserID).ToList();
            return list.Select(MapToResponse).ToList();
        }
        public async Task<string> Create(CreateAPIUserInitialRequestResponseLogRequest request)
        {
            var entity = new APIUserInitialRequestResponseLog
            {
                UserID = request.UserID,
                Request = request.Request,
                Response = request.Response,
                TransactionMasterID = request.TransactionMasterID,
                APIUserRequestID = request.APIUserRequestID,
                ConsumerNumber = request.ConsumerNumber,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateAPIUserInitialRequestResponseLogRequest request)
        {
            var entity = await _repository.GetByID(request.APIUserInitialRequestResponseLogID);
            if (entity == null) return Messages.APIUserInitialRequestResponseLogMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Request = request.Request;
            entity.Response = request.Response;
            entity.TransactionMasterID = request.TransactionMasterID;
            entity.APIUserRequestID = request.APIUserRequestID;
            entity.ConsumerNumber = request.ConsumerNumber;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.APIUserInitialRequestResponseLogMessages.NotFound;
        }
    }
}
