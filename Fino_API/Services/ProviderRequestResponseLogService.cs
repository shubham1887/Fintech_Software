using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderRequestResponseLog;
using FintechApp.Responses.ProviderRequestResponseLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderRequestResponseLogService : IProviderRequestResponseLogService
    {
        private readonly IProviderRequestResponseLogRepository _repository;
        private readonly IUserRepository _userRepo;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IServiceRepository _serviceRepo;
        public ProviderRequestResponseLogService(
            IProviderRequestResponseLogRepository repository,
            IUserRepository userRepo,
            IProviderMasterRepository providerRepo,
            IServiceRepository serviceRepo)
        {
            _repository = repository;
            _userRepo = userRepo;
            _providerRepo = providerRepo;
            _serviceRepo = serviceRepo;
        }

        // Fill UserName / ProviderName / ServiceName so the log table can show names (and be
        // filtered by provider + service type) instead of raw ids.
        private async Task Enrich(List<ProviderRequestResponseLogResponse> rows)
        {
            if (rows.Count == 0) return;
            var providerNames = (await _providerRepo.GetAll())
                .GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);
            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);
            var users = await _userRepo.GetAllUsers();
            var userNames = users.GroupBy(u => u.UserMasterID).ToDictionary(
                g => g.Key,
                g => !string.IsNullOrWhiteSpace(g.First().FirmName) ? g.First().FirmName : g.First().PersonName);
            foreach (var r in rows)
            {
                if (providerNames.TryGetValue(r.ProviderID, out var pn)) r.ProviderName = pn;
                if (serviceNames.TryGetValue(r.ServiceID, out var sn)) r.ServiceName = sn;
                if (userNames.TryGetValue(r.UserID, out var un)) r.UserName = un;
            }
        }

        private static ProviderRequestResponseLogResponse MapToResponse(ProviderRequestResponseLog e) => new()
        {
            ProviderRequestResponseLogID = e.ProviderRequestResponseLogID,
            UserID = e.UserID ?? 0,
            ProviderID = e.ProviderID,
            TransactionID = e.TransactionID,
            Request = e.Request,
            Response = e.Response,
            IPAddress = e.IPAddress,
            Through = e.Through,
            ServiceID = e.ServiceID,
            DateTime = e.DateTime,
        };

        public async Task<List<ProviderRequestResponseLogResponse>> GetAll() { var list = await _repository.GetAll(); var rows = list.OrderByDescending(e => e.ProviderRequestResponseLogID).Select(MapToResponse).ToList(); await Enrich(rows); return rows; }

        /// <summary>
        /// Filtered, paged provider log — date range, provider, service, entity, call type, free text.
        /// Only the requested page is mapped and enriched, so the screen no longer has to download the
        /// whole request/response history to show 25 rows.
        /// </summary>
        public async Task<ProviderRequestResponseLogPage> Search(SearchProviderRequestResponseLogRequest request)
        {
            var all = await _repository.GetAll();
            IEnumerable<Entities.ProviderRequestResponseLog> query = all;

            if (DateTime.TryParse(request.FromDate, out var from))
                query = query.Where(e => e.DateTime >= from.Date);
            if (DateTime.TryParse(request.ToDate, out var to))
                query = query.Where(e => e.DateTime < to.Date.AddDays(1));

            if (request.ProviderID > 0) query = query.Where(e => e.ProviderID == request.ProviderID);
            if (request.ServiceID > 0) query = query.Where(e => e.ServiceID == request.ServiceID);
            if (request.UserID > 0) query = query.Where(e => e.UserID == request.UserID);
            if (request.TransactionID > 0) query = query.Where(e => e.TransactionID == request.TransactionID);

            string through = (request.Through ?? string.Empty).Trim();
            if (through.Length > 0 && !through.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                query = query.Where(e => (e.Through ?? string.Empty).Equals(through, StringComparison.OrdinalIgnoreCase));

            // The payloads are where an operator actually looks for a reference number or an error
            // string, so free text searches them as well as the IP and the transaction id.
            string search = (request.Search ?? string.Empty).Trim();
            if (search.Length > 0)
            {
                query = query.Where(e =>
                    (e.Request ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (e.Response ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (e.IPAddress ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || e.TransactionID.ToString() == search);
            }

            var filtered = query.OrderByDescending(e => e.DateTime).ThenByDescending(e => e.ProviderRequestResponseLogID).ToList();

            int take = request.TakeCount > 0 ? request.TakeCount : 25;
            var page = filtered.Skip(Math.Max(request.SkipCount, 0)).Take(take).Select(MapToResponse).ToList();
            await Enrich(page);

            return new ProviderRequestResponseLogPage
            {
                Count = filtered.Count,
                list = page,
                // Offered by the screen's "Call type" filter — taken from the whole table rather than
                // the filtered set so choosing one type does not remove every other option.
                ThroughOptions = all
                    .Select(e => (e.Through ?? string.Empty).Trim())
                    .Where(t => t.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(t => t)
                    .ToList(),
            };
        }
        public async Task<ProviderRequestResponseLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ProviderRequestResponseLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        /// <summary>
        /// Status-check / dispatch logs for one transaction (newest first) — shows whether and when
        /// the status was re-checked against the provider.
        ///
        /// Admin and support ONLY. These rows are our side of the wire: the upstream endpoint, the
        /// vendor id, our own opening/closing balance and the margin we earn on the transfer. An
        /// API user or a retailer opening "Log" on their own transaction must never be shown the
        /// JaiJui (or any other provider's) exchange — they get their own API request/response from
        /// APIUserInitialRequestResponseLog instead.
        /// </summary>
        public async Task<List<ProviderRequestResponseLogResponse>> GetByTransaction(int transactionId, int callerUserTypeID)
        {
            if (callerUserTypeID != UserTypeCode.Admin && callerUserTypeID != UserTypeCode.SupportUser)
                return new List<ProviderRequestResponseLogResponse>();

            var list = await _repository.GetAll();
            return list.Where(e => e.TransactionID == transactionId)
                .OrderByDescending(e => e.DateTime)
                .Select(MapToResponse).ToList();
        }
        public async Task<string> Create(CreateProviderRequestResponseLogRequest request)
        {
            var entity = new ProviderRequestResponseLog
            {
                UserID = request.UserID,
                ProviderID = request.ProviderID,
                TransactionID = request.TransactionID,
                Request = request.Request,
                Response = request.Response,
                IPAddress = request.IPAddress,
                Through = request.Through,
                ServiceID = request.ServiceID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderRequestResponseLogRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderRequestResponseLogID);
            if (entity == null) return Messages.ProviderRequestResponseLogMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ProviderID = request.ProviderID;
            entity.TransactionID = request.TransactionID;
            entity.Request = request.Request;
            entity.Response = request.Response;
            entity.IPAddress = request.IPAddress;
            entity.Through = request.Through;
            entity.ServiceID = request.ServiceID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderRequestResponseLogMessages.NotFound;
        }
    }
}
