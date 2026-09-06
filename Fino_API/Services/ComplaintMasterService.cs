using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ComplaintMaster;
using FintechApp.Responses.ComplaintMaster;
using FintechApp.StaticModels;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace FintechApp.Services
{
    public class ComplaintMasterService : IComplaintMasterService
    {
        private readonly IComplaintMasterRepository _repository;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IProviderDetailsRepository _endpointRepo;
        private readonly IAccountCoreProcessingService _accountCore;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger = new();

        public ComplaintMasterService(
            IComplaintMasterRepository repository,
            IProviderMasterRepository providerRepo,
            IProviderDetailsRepository endpointRepo,
            IAccountCoreProcessingService accountCore,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _providerRepo = providerRepo;
            _endpointRepo = endpointRepo;
            _accountCore = accountCore;
            _httpClientFactory = httpClientFactory;
        }

        // The three spellings the frontend may send for an accepted complaint.
        private static bool IsAccepted(string? status)
        {
            string s = (status ?? string.Empty).Trim().ToUpperInvariant();
            return s == "ACCEPT" || s == "ACCEPTED" || s == "SUCCESS";
        }

        // User/Service/Operator arrive as navigation properties on the entity; only the provider
        // needs a separate lookup, so it is resolved once per list rather than per row.
        private async Task<List<ComplaintMasterResponse>> WithNames(List<ComplaintMaster> list)
        {
            if (list.Count == 0) return new List<ComplaintMasterResponse>();
            var providerById = (await _providerRepo.GetAll()).ToDictionary(p => p.ProviderID, p => p.ProviderName);
            return list.Select(e =>
            {
                var r = MapToResponse(e);
                r.ProviderName = providerById.TryGetValue(e.ProviderID, out var n) ? n : string.Empty;
                return r;
            }).ToList();
        }

        public async Task<List<ComplaintMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return await WithNames(list);
        }

        public async Task<List<ComplaintMasterResponse>> GetByUserID(int userID)
        {
            var list = await _repository.GetByUserID(userID);
            return await WithNames(list);
        }

        public async Task<ComplaintMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            if (entity == null) return null;
            return (await WithNames(new List<ComplaintMaster> { entity })).FirstOrDefault();
        }

        public async Task<string> Create(CreateComplaintMasterRequest request)
        {
            // Best-effort: if the provider has a COMPLAINT endpoint configured, forward the complaint
            // to it and keep whatever it replied alongside the admin remark. Never fails the complaint.
            string providerNote = await ForwardToProviderAsync(request);
            string adminRemark = string.IsNullOrWhiteSpace(providerNote)
                ? request.AdminRemark
                : string.IsNullOrWhiteSpace(request.AdminRemark) ? providerNote : $"{request.AdminRemark}\n{providerNote}";

            var entity = new ComplaintMaster
            {
                UserID = request.UserID,
                TransactionMasterID = request.TransactionMasterID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                ConsumerNumber = request.ConsumerNumber,
                Amount = request.Amount,
                CustomerRemark = request.CustomerRemark,
                AdminRemark = adminRemark,
                ComplaintStatus = request.ComplaintStatus,
                TransactionMasterDOCInt = request.TransactionMasterDOCInt,
                TransactionDate = request.TransactionDate,
                ComplaintType = request.ComplaintType,
                NewConsumerNumber = request.NewConsumerNumber,
                ProviderID = request.ProviderID,
                ProviderRequestID = request.ProviderRequestID,
                ReferenceNo = request.ReferenceNo,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        // Forward a newly-raised complaint to the provider's configured COMPLAINT endpoint (a
        // ProviderDetails row with UrlType=COMPLAINT). Placeholders in the URL are filled from the
        // complaint. Returns a short note (provider reply or failure) to store on the complaint, or
        // "" when no complaint endpoint is configured. Never throws.
        private async Task<string> ForwardToProviderAsync(CreateComplaintMasterRequest request)
        {
            try
            {
                if (request.ProviderID <= 0) return string.Empty;
                var endpoints = await _endpointRepo.GetByProviderID(request.ProviderID);
                var ep = endpoints?.FirstOrDefault(e => e.IsActive && IsComplaintKind(e.UrlType));
                if (ep == null || string.IsNullOrWhiteSpace(ep.ProviderURL)) return string.Empty;

                string url = FillComplaintPlaceholders(ep.ProviderURL, request);
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);

                string reply;
                if (ep.MethodType.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(url);
                    string baseUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
                    string body = url.Contains('?') ? url[(url.IndexOf('?') + 1)..] : string.Empty;
                    if (!string.IsNullOrWhiteSpace(ep.HeaderJson))
                    {
                        try
                        {
                            var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(ep.HeaderJson);
                            if (headers != null)
                                foreach (var h in headers.Where(h => h.Key != "Content-Type"))
                                    client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
                        }
                        catch { }
                    }
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var resp = await client.PostAsync(baseUrl, content);
                    reply = await resp.Content.ReadAsStringAsync();
                }
                else
                {
                    reply = await client.GetStringAsync(url);
                }

                if (reply.Length > 400) reply = reply[..400];
                return $"[Provider complaint sent {DateTime.Now:dd/MMM/yyyy hh:mm tt}] {reply}";
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return $"[Provider complaint forward failed: {ex.Message}]";
            }
        }

        private static bool IsComplaintKind(string? urlType)
        {
            string k = (urlType ?? string.Empty).Trim().ToUpperInvariant();
            return k is "COMPLAINT" or "COMPLAIN" or "DISPUTE" or "GRIEVANCE";
        }

        private static string FillComplaintPlaceholders(string template, CreateComplaintMasterRequest r)
        {
            string Rep(string input, string token, string value) =>
                string.IsNullOrEmpty(input) ? input
                : Regex.Replace(input, Regex.Escape(token), _ => Uri.EscapeDataString(value ?? string.Empty), RegexOptions.IgnoreCase);

            string url = template;
            url = Rep(url, "[TRANSACTIONID]", r.TransactionMasterID.ToString());
            url = Rep(url, "[CONSUMERNUMBER]", r.ConsumerNumber);
            url = Rep(url, "[NUMBER]", r.ConsumerNumber);
            url = Rep(url, "[MOBILE]", r.ConsumerNumber);
            url = Rep(url, "[AMOUNT]", r.Amount.ToString("0.##"));
            url = Rep(url, "[REFERENCENO]", r.ReferenceNo ?? string.Empty);
            url = Rep(url, "[PROVIDERREQUESTID]", r.ProviderRequestID ?? string.Empty);
            url = Rep(url, "[REMARK]", r.CustomerRemark ?? string.Empty);
            return url;
        }

        public async Task<string> Update(UpdateComplaintMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ComplaintMasterID);
            if (entity == null) return Messages.ComplaintMasterMessages.NotFound;
            entity.UserID = request.UserID;
            entity.TransactionMasterID = request.TransactionMasterID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.ConsumerNumber = request.ConsumerNumber;
            entity.Amount = request.Amount;
            entity.CustomerRemark = request.CustomerRemark;
            entity.AdminRemark = request.AdminRemark;
            entity.ComplaintStatus = request.ComplaintStatus;
            entity.TransactionMasterDOCInt = request.TransactionMasterDOCInt;
            entity.TransactionDate = request.TransactionDate;
            entity.ComplaintType = request.ComplaintType;
            entity.NewConsumerNumber = request.NewConsumerNumber;
            entity.ProviderID = request.ProviderID;
            entity.ProviderRequestID = request.ProviderRequestID;
            entity.ReferenceNo = request.ReferenceNo;
            entity.UpdatedateTime = DateTime.Now;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        // Accepting a complaint reverses the disputed transaction: the principal is refunded to the
        // user's wallet and every commission credited on it is debited back (retailer / distributor /
        // super-distributor / admin). RefundTransaction performs the whole symmetric reversal and is
        // idempotent, so re-accepting (or accepting a txn that already failed) never double-refunds.
        // Rejecting only records the outcome and moves no money.
        public async Task<string> Resolve(ResolveComplaintMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ComplaintMasterID);
            if (entity == null) return Messages.ComplaintMasterMessages.NotFound;

            bool acceptingNow = IsAccepted(request.ComplaintStatus) && !IsAccepted(entity.ComplaintStatus);
            if (acceptingNow && entity.TransactionMasterID > 0)
            {
                string remark = string.IsNullOrWhiteSpace(request.AdminRemark)
                    ? "Complaint accepted — transaction refunded"
                    : request.AdminRemark;
                string refund = await _accountCore.RefundTransaction(
                    entity.TransactionMasterID, remark, DOCTypeConstants.Recharge);

                // Ok = refunded now; "already failed/refunded" = the money was returned earlier (still
                // valid to accept). Any other result (e.g. txn not found) is a real failure — surface
                // it and leave the complaint open so the admin can investigate.
                bool refunded = refund.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase)
                    || refund.Contains("already", StringComparison.OrdinalIgnoreCase);
                if (!refunded) return refund;
            }

            entity.ComplaintStatus = request.ComplaintStatus;
            entity.AdminRemark = request.AdminRemark ?? string.Empty;
            entity.UpdatedateTime = DateTime.Now;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ComplaintMasterMessages.NotFound;
        }

        private static ComplaintMasterResponse MapToResponse(ComplaintMaster e) => new()
        {
            ComplaintMasterID = e.ComplaintMasterID,
            UserID = e.UserID,
            TransactionMasterID = e.TransactionMasterID,
            ServiceID = e.ServiceID,
            OperatorID = e.OperatorID,
            ConsumerNumber = e.ConsumerNumber,
            Amount = e.Amount,
            CustomerRemark = e.CustomerRemark,
            AdminRemark = e.AdminRemark,
            ComplaintStatus = e.ComplaintStatus,
            TransactionMasterDOCInt = e.TransactionMasterDOCInt,
            TransactionDate = e.TransactionDate,
            ComplaintType = e.ComplaintType,
            NewConsumerNumber = e.NewConsumerNumber,
            ProviderID = e.ProviderID,
            ProviderRequestID = e.ProviderRequestID,
            ReferenceNo = e.ReferenceNo,
            DateTime = e.DateTime,
            UserName = !string.IsNullOrWhiteSpace(e.UserMaster?.FirmName) ? e.UserMaster!.FirmName : e.UserMaster?.PersonName ?? string.Empty,
            UserMobile = e.UserMaster?.MobileNumber ?? string.Empty,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            OperatorName = e.OperatorMaster?.OperatorName ?? string.Empty
        };
    }
}
