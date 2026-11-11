using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Sender;
using FintechApp.Requests.SenderBeneficiary;
using FintechApp.Responses.Sender;
using FintechApp.Responses.SenderBeneficiary;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SenderService : ISenderService
    {
        private readonly ISenderRepository _repository;
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly IUserRepository _userRepository;
        public SenderService(ISenderRepository repository, IBeneficiaryRepository beneficiaryRepository, IUserRepository userRepository)
        {
            _repository = repository;
            _beneficiaryRepository = beneficiaryRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Sender / Beneficiary report — filtered, scoped and paged, with the owning entity's name
        /// and each sender's beneficiary count resolved.
        /// </summary>
        public async Task<(string failureMessage, SenderReportWrapper data)> SelectReport(
            SelectSenderReportRequest request, int callerUserID, int callerUserTypeID)
        {
            var wrapper = new SenderReportWrapper();

            // Shared scoped DbContext — await sequentially.
            var senders = (await _repository.GetAll()).Where(s => !s.IsDeleted).AsEnumerable();
            var allUsers = await _userRepository.GetAllUsers();

            // Visibility: admin/support see everything, an upline sees its own network, everyone
            // else only the senders they registered.
            bool isAdmin = callerUserTypeID == UserTypeCode.Admin || callerUserTypeID == UserTypeCode.SupportUser;
            if (!isAdmin)
            {
                if (callerUserTypeID == UserTypeCode.Distributor || callerUserTypeID == UserTypeCode.SuperDistributor)
                {
                    var downline = UserHierarchy.GetDownlineIds(allUsers, callerUserID);
                    downline.Add(callerUserID);
                    senders = senders.Where(s => downline.Contains(s.UserID));
                }
                else
                {
                    senders = senders.Where(s => s.UserID == callerUserID);
                }
            }

            if (request.UserID > 0) senders = senders.Where(s => s.UserID == request.UserID);
            if (DateTime.TryParse(request.FromDate, out var from)) senders = senders.Where(s => s.DateTime >= from.Date);
            if (DateTime.TryParse(request.ToDate, out var to)) senders = senders.Where(s => s.DateTime < to.Date.AddDays(1));

            string search = (request.Search ?? string.Empty).Trim();
            if (search.Length > 0)
            {
                senders = senders.Where(s =>
                    (s.Name ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (s.Mobile ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (s.Pincode ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = senders.OrderByDescending(s => s.DateTime).ToList();
            wrapper.Count = filtered.Count;
            if (filtered.Count == 0) return (StatusType.Ok, wrapper);

            var page = request.TakeCount > 0
                ? filtered.Skip(Math.Max(request.SkipCount, 0)).Take(request.TakeCount).ToList()
                : filtered;

            // Beneficiary counts, resolved once for the page rather than per row.
            var pageSenderIds = page.Select(s => s.SenderID).ToHashSet();
            var beneficiaryCounts = (await _beneficiaryRepository.GetAll())
                .Where(b => !b.IsDeleted && pageSenderIds.Contains(b.SenderID))
                .GroupBy(b => b.SenderID)
                .ToDictionary(g => g.Key, g => g.Count());

            var userById = allUsers.GroupBy(u => u.UserMasterID).ToDictionary(g => g.Key, g => g.First());

            wrapper.list = page.Select(s =>
            {
                userById.TryGetValue(s.UserID, out var owner);
                return new SenderReportViewModel
                {
                    SenderID = s.SenderID,
                    Name = s.Name,
                    Mobile = s.Mobile,
                    Address = s.Address,
                    Pincode = s.Pincode,
                    UserID = s.UserID,
                    UserName = owner == null ? string.Empty
                        : (!string.IsNullOrWhiteSpace(owner.FirmName) ? owner.FirmName : owner.PersonName),
                    UserMobile = owner?.MobileNumber ?? string.Empty,
                    BeneficiaryCount = beneficiaryCounts.TryGetValue(s.SenderID, out var c) ? c : 0,
                    IsActive = s.IsActive,
                    IsOTPVerified = s.IsOTPVerified,
                    IsKYC = s.IsKYC,
                    DOC = s.DateTime,
                };
            }).ToList();

            return (StatusType.Ok, wrapper);
        }

        private static SenderResponse MapToResponse(Sender e) => new() { SenderID = e.SenderID, Name = e.Name, Mobile = e.Mobile, GUID = e.GUID, Address = e.Address, UserID = e.UserID, IsActive = e.IsActive, IsOTPVerified = e.IsOTPVerified, IsKYC = e.IsKYC, KYCDocumentURL = e.KYCDocumentURL, IPAddress = e.IPAddress, Pincode = e.Pincode, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<SenderResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SenderResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<SenderResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateSenderRequest request) { var entity = new Sender { Name = request.Name, Mobile = request.Mobile, GUID = request.GUID, Address = request.Address, UserID = request.UserID, IsActive = request.IsActive, IsOTPVerified = request.IsOTPVerified, IsKYC = request.IsKYC, KYCDocumentURL = request.KYCDocumentURL, IPAddress = request.IPAddress, Pincode = request.Pincode, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSenderRequest request) { var entity = await _repository.GetByID(request.SenderID); if (entity == null) return Messages.SenderMessages.NotFound; entity.Name = request.Name; entity.Mobile = request.Mobile; entity.GUID = request.GUID; entity.Address = request.Address; entity.UserID = request.UserID; entity.IsActive = request.IsActive; entity.IsOTPVerified = request.IsOTPVerified; entity.IsKYC = request.IsKYC; entity.KYCDocumentURL = request.KYCDocumentURL; entity.IPAddress = request.IPAddress; entity.Pincode = request.Pincode; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SenderMessages.NotFound; }
    }
}
