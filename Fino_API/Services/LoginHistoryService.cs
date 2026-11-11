using FintechApp.Entities;
using FintechApp.Helpers;
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
        private readonly IUserRepository _userRepository;

        public LoginHistoryService(ILoginHistoryRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public Task<List<LoginHistoryResponse>> GetAll(GetLoginHistoryRequest request)
            => GetAllForCaller(request, callerUserID: 0, callerUserTypeID: UserTypeCode.Admin);

        /// <summary>
        /// Login history as this caller may see it.
        ///
        /// Admin and Support see every sign-in; a distributor or super-distributor sees their own
        /// network; everyone else sees only their own. Without this, passing any UserID returned
        /// somebody else's sign-in times, IPs and devices — which is exactly the kind of thing a
        /// login history exists to protect.
        ///
        /// Filtering and paging both happen in the query (see the repository) so the channel filter
        /// no longer thins out an already-paged result.
        /// </summary>
        public async Task<List<LoginHistoryResponse>> GetAllForCaller(GetLoginHistoryRequest request, int callerUserID, int callerUserTypeID)
        {
            List<int>? allowedUserIds = null;
            bool isAdmin = callerUserTypeID == UserTypeCode.Admin || callerUserTypeID == UserTypeCode.SupportUser;
            if (!isAdmin && callerUserID > 0)
            {
                if (callerUserTypeID == UserTypeCode.Distributor || callerUserTypeID == UserTypeCode.SuperDistributor)
                {
                    var downline = UserHierarchy.GetDownlineIds(await _userRepository.GetAllUsers(), callerUserID);
                    downline.Add(callerUserID);
                    allowedUserIds = downline.ToList();
                }
                else
                {
                    allowedUserIds = new List<int> { callerUserID };
                }
            }

            var all = await _repository.GetFiltered(
                request.UserID,
                request.FromDate,
                request.ToDate,
                request.Through,
                allowedUserIds,
                request.PageNumber,
                request.PageSize);

            // Names are resolved here, from the full user list (deleted ones included), so a
            // sign-in by someone since deactivated still reads as a person rather than an id.
            var users = (await _userRepository.GetAllUsersWithDeleted())
                .GroupBy(u => u.UserMasterID).ToDictionary(g => g.Key, g => g.First());

            return all.Select(x => MapToResponse(x, users.TryGetValue(x.UserID, out var u) ? u : null)).ToList();
        }

        public async Task<string> Create(CreateLoginHistoryRequest request)
        {
            var entity = new LoginHistory
            {
                UserID = request.UserID,
                IPAddress = request.IPAddress,
                Token = request.Token,
                Through = request.Through,
                Date = request.Date == default ? DateTime.Now : request.Date,
                LoginMessage = request.LoginMessage,
                Location = request.Location,
                SimCardDetails = request.SimCardDetails,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
                IsDeleted = false
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        private static LoginHistoryResponse MapToResponse(LoginHistory x, UserMaster? user)
        {
            // Older rows (and any client that sends neither) carry the pair inside Location as
            // "lat,long". Split it out so the report has the two columns either way, rather than
            // showing coordinates in a column headed "Location" and two empty ones beside it.
            string latitude = x.Latitude ?? string.Empty;
            string longitude = x.Longitude ?? string.Empty;
            string location = x.Location ?? string.Empty;
            if (latitude.Length == 0 && longitude.Length == 0 && location.Contains(','))
            {
                var parts = location.Split(',', 2);
                if (double.TryParse(parts[0].Trim(), out _) && double.TryParse(parts[1].Trim(), out _))
                {
                    latitude = parts[0].Trim();
                    longitude = parts[1].Trim();
                }
            }

            return new LoginHistoryResponse
            {
                LoginHistoryID = x.LoginHistoryID,
                UserID = x.UserID,
                UserName = user == null ? string.Empty
                    : (!string.IsNullOrWhiteSpace(user.PersonName) ? user.PersonName
                        : user.FirmName ?? string.Empty),
                FirmName = user?.FirmName ?? string.Empty,
                MobileNumber = user?.MobileNumber ?? string.Empty,
                UserCode = user?.AliasName ?? string.Empty,
                UserType = RoleName(user?.UserTypeID ?? 0),
                IPAddress = x.IPAddress,
                Through = x.Through,
                Date = x.Date,
                LoginMessage = x.LoginMessage,
                Location = location,
                Latitude = latitude,
                Longitude = longitude,
                SimCardDetails = x.SimCardDetails,
                DateTime = x.DateTime,
                UpdatedateTime = x.UpdatedateTime
            };
        }

        private static string RoleName(int userTypeID) => userTypeID switch
        {
            UserTypeCode.Admin => "Administrator",
            UserTypeCode.SuperDistributor => "Super Distributor",
            UserTypeCode.Distributor => "Distributor",
            UserTypeCode.Retailer => "Retailer",
            UserTypeCode.ApiUser => "API User",
            UserTypeCode.SupportUser => "Support",
            UserTypeCode.Client => "Client",
            UserTypeCode.Accountant => "Accountant",
            UserTypeCode.SDFOS => "SD Field Officer",
            UserTypeCode.DFOS => "D Field Officer",
            _ => string.Empty,
        };
    }
}
