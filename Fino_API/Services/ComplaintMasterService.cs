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
        private readonly IUserRepository _userRepo;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger = new();

        public ComplaintMasterService(
            IComplaintMasterRepository repository,
            IProviderMasterRepository providerRepo,
            IProviderDetailsRepository endpointRepo,
            IAccountCoreProcessingService accountCore,
            IUserRepository userRepo,
            ITransactionMasterRepository txnRepo,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _providerRepo = providerRepo;
            _endpointRepo = endpointRepo;
            _accountCore = accountCore;
            _userRepo = userRepo;
            _txnRepo = txnRepo;
            _httpClientFactory = httpClientFactory;
        }

        // The three spellings the frontend may send for an accepted complaint.
        private static bool IsAccepted(string? status) => ComplaintStatus.IsAccepted(status);

        private static bool IsAdminTier(int userTypeID) =>
            userTypeID == UserTypeCode.Admin || userTypeID == UserTypeCode.SupportUser;

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

        /// <summary>
        /// The complaint queue as this caller is allowed to see it. Admin and Support get the whole
        /// company queue; a distributor or super-distributor gets their own network (themselves plus
        /// every downline); everybody else gets only the complaints they raised. Without this the
        /// list endpoint handed every retailer the full customer-dispute history of the platform.
        /// </summary>
        public async Task<List<ComplaintMasterResponse>> GetAllForCaller(int callerUserID, int callerUserTypeID)
        {
            if (IsAdminTier(callerUserTypeID)) return await GetAll();

            var list = await _repository.GetAll();

            if (callerUserTypeID == UserTypeCode.Distributor || callerUserTypeID == UserTypeCode.SuperDistributor)
            {
                var downline = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), callerUserID);
                downline.Add(callerUserID);
                list = list.Where(c => downline.Contains(c.UserID)).ToList();
            }
            else
            {
                list = list.Where(c => c.UserID == callerUserID).ToList();
            }

            return await WithNames(list);
        }

        /// <summary>
        /// Complaints raised against one transaction, newest first. Used by the manual-update flow:
        /// before an admin settles a transaction by hand they need to see whether the customer has
        /// an open dispute on it, and afterwards that dispute is closed from the same decision.
        /// </summary>
        public async Task<List<ComplaintMasterResponse>> GetByTransaction(int transactionMasterID, int callerUserID, int callerUserTypeID)
        {
            if (transactionMasterID <= 0) return new List<ComplaintMasterResponse>();

            var list = (await _repository.GetAll())
                .Where(c => c.TransactionMasterID == transactionMasterID)
                .OrderByDescending(c => c.DateTime)
                .ToList();

            // Same visibility rule as the queue — a complaint is only ever shown to its raiser,
            // their upline or an admin.
            if (!IsAdminTier(callerUserTypeID))
            {
                if (callerUserTypeID == UserTypeCode.Distributor || callerUserTypeID == UserTypeCode.SuperDistributor)
                {
                    var downline = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), callerUserID);
                    downline.Add(callerUserID);
                    list = list.Where(c => downline.Contains(c.UserID)).ToList();
                }
                else
                {
                    list = list.Where(c => c.UserID == callerUserID).ToList();
                }
            }

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
            // Fill in which upstream rail this complaint belongs to, and its reference there, from
            // the transaction being complained about.
            //
            // This is what makes forwarding AUTOMATIC. Nothing on the "raise a complaint" form asks
            // a retailer which provider carried their recharge - they have no idea, and should not -
            // so ProviderID arrived as 0 on every complaint raised from a transaction and the
            // forwarder below returned immediately. The provider, its request id and its reference
            // are all already recorded on the transaction; this just reads them.
            await ResolveProviderFromTransactionAsync(request);

            // Best-effort: if the provider has a COMPLAINT endpoint configured, forward the complaint
            // to it and keep whatever it replied alongside the admin remark. Never fails the complaint.
            var (providerNote, providerComplaintId) = await ForwardToProviderAsync(request);
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
                // The provider's own complaint number. Stored in its own column because it is the
                // only handle that exists for chasing the complaint with them later - buried in a
                // free-text admin remark it cannot be searched, filtered or shown on the row.
                ProviderComplaintID = providerComplaintId,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        /// <summary>
        /// Fills ProviderID / ProviderRequestID / ReferenceNo on a complaint from the transaction
        /// it is about, when the caller did not supply them.
        ///
        /// Anything the caller DID send wins - an admin raising a complaint against a specific rail
        /// is making a deliberate choice and must not have it overwritten.
        ///
        /// Never throws: a complaint must be recorded even if the transaction cannot be read, and a
        /// complaint with no provider is simply one that does not get forwarded.
        /// </summary>
        private async Task ResolveProviderFromTransactionAsync(CreateComplaintMasterRequest request)
        {
            try
            {
                if (request.TransactionMasterID <= 0) return;
                if (request.ProviderID > 0 && !string.IsNullOrWhiteSpace(request.ProviderRequestID)) return;

                var txn = await _txnRepo.GetByID(request.TransactionMasterID);
                if (txn == null) return;

                if (request.ProviderID <= 0) request.ProviderID = txn.ProviderID;
                if (string.IsNullOrWhiteSpace(request.ProviderRequestID))
                    request.ProviderRequestID = txn.ProviderRequestID;
                if (string.IsNullOrWhiteSpace(request.ReferenceNo))
                    request.ReferenceNo = txn.ReferenceNo;
                if (string.IsNullOrWhiteSpace(request.ConsumerNumber))
                    request.ConsumerNumber = txn.ConsumerNumber;
                if (request.Amount <= 0) request.Amount = txn.Amount;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
        }

        // Forward a newly-raised complaint to the provider's configured COMPLAINT endpoint (a
        // ProviderDetails row with UrlType=COMPLAINT). Placeholders in the URL are filled from the
        // complaint. Returns a short note (provider reply or failure) to store on the complaint, or
        // "" when no complaint endpoint is configured. Never throws.
        private async Task<(string note, string complaintId)> ForwardToProviderAsync(CreateComplaintMasterRequest request)
        {
            try
            {
                if (request.ProviderID <= 0) return (string.Empty, string.Empty);
                var endpoints = await _endpointRepo.GetByProviderID(request.ProviderID);
                var ep = endpoints?.FirstOrDefault(e => e.IsActive && IsComplaintKind(e.UrlType));
                if (ep == null || string.IsNullOrWhiteSpace(ep.ProviderURL)) return (string.Empty, string.Empty);

                // The provider's own credentials, so a configured URL can carry [UserName] and
                // [Token] exactly as the recharge URLs on the same rails already do - the admin
                // pastes the vendor's documented URL and nothing has to be retyped per client.
                var provider = await _providerRepo.GetByID(request.ProviderID);
                string url = FillComplaintPlaceholders(ep.ProviderURL, request, provider);

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);

                string reply;
                if (ep.MethodType.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(url);
                    string baseUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";

                    // The configured query string IS the body, converted to JSON.
                    //
                    // It used to be posted verbatim - "APIUserRequestID=x&Amount=10" under a
                    // Content-Type of application/json - which no JSON API accepts, so the POST
                    // variant of this feature could never have worked. Writing it as one object
                    // means the same single URL in the Provider Endpoints screen configures both
                    // the GET and the POST form of a vendor's complaint API.
                    string body = BuildJsonBody(url);

                    if (!string.IsNullOrWhiteSpace(ep.HeaderJson))
                    {
                        try
                        {
                            // Headers get the placeholders too: these APIs put the credentials in a
                            // header (JaiJui / Payin send KeyData: {"Mobile":...,"APIToken":...}),
                            // so without this the one place they are actually needed was the one
                            // place they were not filled in.
                            string headerJson = FillComplaintPlaceholders(ep.HeaderJson, request, provider, urlEncode: false);
                            var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerJson);
                            if (headers != null)
                                foreach (var h in headers.Where(h => !h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
                                    client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
                        }
                        catch (Exception ex) { _logger.Write(ex); }
                    }

                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var resp = await client.PostAsync(baseUrl, content);
                    reply = await resp.Content.ReadAsStringAsync();
                }
                else
                {
                    var resp = await client.GetAsync(url);
                    reply = await resp.Content.ReadAsStringAsync();
                }

                // Read out of the reply BEFORE it is truncated for storage.
                string complaintId = ExtractComplaintId(reply);

                if (reply.Length > 400) reply = reply[..400];
                return ($"[Provider complaint sent {DateTime.Now:dd/MMM/yyyy hh:mm tt}] {reply}", complaintId);
            }
            catch (Exception ex)
            {
                // A provider that is down must never stop a retailer recording a complaint - the
                // complaint is the customer's, and chasing the vendor is a separate problem. The
                // failure is written onto the complaint so an operator can see it needs re-sending.
                _logger.Write(ex);
                return ($"[Provider complaint forward failed: {ex.Message}]", string.Empty);
            }
        }

        /// <summary>
        /// Turns the configured query string into a JSON object body.
        ///
        /// Values that parse as a number are written unquoted, because that is how these vendors
        /// document their own payload (`"Amount": 10`, not `"Amount": "10"`) and a strict
        /// deserialiser on their side will reject the quoted form.
        /// </summary>
        private static string BuildJsonBody(string url)
        {
            int q = url.IndexOf('?');
            if (q < 0 || q == url.Length - 1) return "{}";

            var fields = new Dictionary<string, object>();
            foreach (string pair in url[(q + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = pair.IndexOf('=');
                string key = eq < 0 ? pair : pair[..eq];
                string raw = eq < 0 ? string.Empty : pair[(eq + 1)..];
                if (string.IsNullOrWhiteSpace(key)) continue;

                // The query string was built with escaped values; the JSON body wants them plain.
                string value = Uri.UnescapeDataString(raw);
                if (decimal.TryParse(value, out decimal number)) fields[key] = number;
                else fields[key] = value;
            }

            return JsonConvert.SerializeObject(fields);
        }

        /// <summary>
        /// Pulls the provider's complaint number out of whatever they replied.
        ///
        /// Tries JSON first (the documented `{"Status":"SUCCESS","ComplaintID":"34669"}`) and falls
        /// back to a regex, because these gateways answer with plain text often enough that losing
        /// the one number worth keeping to a stray non-JSON reply is not worth it.
        /// </summary>
        private static string ExtractComplaintId(string reply)
        {
            if (string.IsNullOrWhiteSpace(reply)) return string.Empty;
            try
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(reply);
                if (parsed != null)
                    foreach (var key in new[] { "ComplaintID", "ComplaintId", "complaintID", "complaintid", "Complaint_ID" })
                        if (parsed.TryGetValue(key, out var v) && v != null)
                        {
                            string s = v.ToString() ?? string.Empty;
                            if (s.Length > 0) return s.Length > 100 ? s[..100] : s;
                        }
            }
            catch { /* not JSON - fall through to the regex */ }

            var m = Regex.Match(reply, "complaint[_ ]?id\\W{1,4}([A-Za-z0-9-]{1,100})", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : string.Empty;
        }

        private static bool IsComplaintKind(string? urlType)
        {
            string k = (urlType ?? string.Empty).Trim().ToUpperInvariant();
            return k is "COMPLAINT" or "COMPLAIN" or "DISPUTE" or "GRIEVANCE";
        }

        /// <summary>
        /// Fills the placeholders in a configured complaint URL (or header) from the complaint and
        /// the provider's own credentials, so an admin can paste the vendor's documented URL
        /// verbatim into Provider Endpoints and nothing has to be retyped per client.
        ///
        /// <paramref name="urlEncode"/> is off for a header: a header value is not a URL, and
        /// percent-escaping a credential there sends the wrong credential.
        /// </summary>
        private static string FillComplaintPlaceholders(
            string template, CreateComplaintMasterRequest r, ProviderMaster? provider, bool urlEncode = true)
        {
            string Rep(string input, string token, string value) =>
                string.IsNullOrEmpty(input) ? input
                : Regex.Replace(input, Regex.Escape(token),
                    _ => urlEncode ? Uri.EscapeDataString(value ?? string.Empty) : (value ?? string.Empty),
                    RegexOptions.IgnoreCase);

            string url = template;

            // The account WE hold with the provider. LoginID is the registered mobile and Token the
            // API token - the same two fields the recharge URLs on these rails already read, which
            // is why the vendor's own [UserName] / [Token] spelling is honoured as-is.
            string userName = provider?.LoginID ?? string.Empty;
            string token = provider?.Token ?? string.Empty;
            url = Rep(url, "[USERNAME]", userName);
            url = Rep(url, "[USERID]", userName);
            url = Rep(url, "[APITOKEN]", token);
            url = Rep(url, "[TOKEN]", token);

            // The provider's own id for the transaction being complained about. This is the field
            // the vendor matches a complaint on, so it is the one that must not be missing - our
            // TransactionMasterID means nothing to them.
            string providerRequestId = r.ProviderRequestID ?? string.Empty;
            url = Rep(url, "[APIUSERREQUESTID]", providerRequestId);
            url = Rep(url, "[PROVIDERREQUESTID]", providerRequestId);

            url = Rep(url, "[TRANSACTIONID]", r.TransactionMasterID.ToString());
            url = Rep(url, "[CONSUMERNUMBER]", r.ConsumerNumber);
            url = Rep(url, "[NUMBER]", r.ConsumerNumber);
            // [MOBILE] stays the CUSTOMER's number, which is what it has always meant here. The
            // vendor's own docs use [UserName] for our registered mobile, handled above - the two
            // are different numbers and swapping them silently sends a complaint for the wrong
            // account.
            url = Rep(url, "[MOBILE]", r.ConsumerNumber);
            url = Rep(url, "[AMOUNT]", r.Amount.ToString("0.##"));
            url = Rep(url, "[REFERENCENO]", r.ReferenceNo ?? string.Empty);
            url = Rep(url, "[REMARK]", r.CustomerRemark ?? string.Empty);
            url = Rep(url, "[CUSTOMERREMARK]", r.CustomerRemark ?? string.Empty);
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

        /// <summary>
        /// Withdraw a complaint. This is what a retailer (or any non-admin) gets instead of
        /// Accept / Reject: it closes the complaint and moves no money, so it is safe to expose to
        /// the person who raised it. Only the raiser — or an admin acting for them — may cancel,
        /// and only while it is still open; a decided complaint is a record and stays as it is.
        /// </summary>
        public async Task<string> Cancel(CancelComplaintMasterRequest request, int callerUserID, int callerUserTypeID)
        {
            var entity = await _repository.GetByID(request.ComplaintMasterID);
            if (entity == null) return Messages.ComplaintMasterMessages.NotFound;

            if (!IsAdminTier(callerUserTypeID) && entity.UserID != callerUserID)
                return Messages.ComplaintMasterMessages.CancelNotYours;

            if (!ComplaintStatus.IsOpen(entity.ComplaintStatus))
                return Messages.ComplaintMasterMessages.AlreadyDecided;

            string remark = string.IsNullOrWhiteSpace(request.Remark)
                ? "Withdrawn by the user who raised it"
                : request.Remark.Trim();

            entity.ComplaintStatus = ComplaintStatus.Cancel;
            // Keep the admin remark; append rather than overwrite so any provider reply survives.
            entity.AdminRemark = string.IsNullOrWhiteSpace(entity.AdminRemark)
                ? $"[Cancelled] {remark}"
                : $"{entity.AdminRemark}\n[Cancelled] {remark}";
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
            ProviderComplaintID = e.ProviderComplaintID,
            DateTime = e.DateTime,
            UserName = !string.IsNullOrWhiteSpace(e.UserMaster?.FirmName) ? e.UserMaster!.FirmName : e.UserMaster?.PersonName ?? string.Empty,
            UserMobile = e.UserMaster?.MobileNumber ?? string.Empty,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            OperatorName = e.OperatorMaster?.OperatorName ?? string.Empty
        };
    }
}
