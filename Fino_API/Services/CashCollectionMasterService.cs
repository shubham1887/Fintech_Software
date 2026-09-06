using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CashCollection;
using FintechApp.Requests.CashCollectionMaster;
using FintechApp.Responses.CashCollection;
using FintechApp.Responses.CashCollectionMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CashCollectionMasterService : ICashCollectionMasterService
    {
        private readonly ICashCollectionMasterRepository _repository;
        private readonly ICashCollectionDetailRepository _detailRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUserBalanceMasterRepository _balanceRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly Logger _logger;

        public CashCollectionMasterService(
            ICashCollectionMasterRepository repository,
            ICashCollectionDetailRepository detailRepo,
            IUserRepository userRepo,
            IUserBalanceMasterRepository balanceRepo,
            ISettingMasterRepository settingRepo)
        {
            _repository = repository;
            _detailRepo = detailRepo;
            _userRepo = userRepo;
            _balanceRepo = balanceRepo;
            _settingRepo = settingRepo;
            _logger = new Logger();
        }

        private static CashCollectionMasterResponse MapToResponse(CashCollectionMaster e) => new()
        {
            CashCollectionMasterID = e.CashCollectionMasterID,
            UserID = e.UserID,
            Outstanding = e.Outstanding,
            MaxOutstanding = e.MaxOutstanding,
        };

        public async Task<List<CashCollectionMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CashCollectionMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<CashCollectionMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateCashCollectionMasterRequest request)
        {
            var entity = new CashCollectionMaster
            {
                UserID = request.UserID,
                Outstanding = request.Outstanding,
                MaxOutstanding = request.MaxOutstanding,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCashCollectionMasterRequest request)
        {
            var entity = await _repository.GetByID(request.CashCollectionMasterID);
            if (entity == null) return Messages.CashCollectionMasterMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Outstanding = request.Outstanding;
            entity.MaxOutstanding = request.MaxOutstanding;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CashCollectionMasterMessages.NotFound;
        }

        public async Task<(string failureMessage, string result)> AddCashCollection(AddCashCollectionRequest request, int senderID)
        {
            if (request.ReceiverID <= 0 || request.Amount <= 0)
                return (Messages.Common.Message.InvalidRequest, string.Empty);

            var allUsers = await _userRepo.GetAllUsers();
            var sender = allUsers.FirstOrDefault(u => u.UserMasterID == senderID);
            if (sender == null)
                return (Messages.Common.Message.InvalidData, string.Empty);

            if (!int.TryParse(request.Pin, out int pinInt) || sender.Mpin != pinInt)
                return ("Invalid PIN.", string.Empty);

            var ccSetting = await _settingRepo.GetByKey("CashCollection");
            if (ccSetting != null && ccSetting.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return ("Cash collection not allowed, please contact customer care", string.Empty);

            var masterList = await _repository.GetByUserID(request.ReceiverID);
            var master = masterList.FirstOrDefault();
            if (master == null)
            {
                await _repository.Insert(new CashCollectionMaster
                {
                    UserID = request.ReceiverID,
                    Outstanding = 0,
                    MaxOutstanding = 999999
                });
                masterList = await _repository.GetByUserID(request.ReceiverID);
                master = masterList.FirstOrDefault();
                if (master == null) return ("Cash collection master not found.", string.Empty);
            }

            decimal closingBalance = master.Outstanding - request.Amount;
            if (closingBalance < 0)
                return ("Invalid Cash collection Amount", string.Empty);

            decimal openingBalance = master.Outstanding;
            master.Outstanding = closingBalance;
            int updateResult = await _repository.Update(master);
            if (updateResult <= 0)
                return ("Cash not collected", string.Empty);

            string description = $"Outstanding Decreased, by: {request.Amount} and new outstanding is: {Math.Round(closingBalance, 2)}";
            if (!string.IsNullOrWhiteSpace(request.Remark) && request.Remark.Length > 2)
                description = $"{request.Remark} {description}";

            try
            {
                await _detailRepo.Insert(new CashCollectionDetail
                {
                    UserID = request.ReceiverID,
                    ParentID = senderID,
                    OutstandingOpening = openingBalance,
                    OutstandingAmount = request.Amount,
                    OutstandingClosing = closingBalance,
                    ParentRemark = description,
                    PayMode = request.PayMode,
                    Credit = string.Empty,
                    Debit = "DEBIT",
                    UTRNumber = request.UTRNumber,
                    Type = "CASH_COLLECTION_DECREASED"
                });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

            return (StatusType.Ok, "Cash collected successfully");
        }

        public async Task<(string failureMessage, string result)> ReverseCashCollection(ReverseCashCollectionRequest request, int receiverID)
        {
            if (request.ReceiverID <= 0 || request.Amount <= 0)
                return (Messages.Common.Message.InvalidRequest, string.Empty);

            int senderID = request.ReceiverID;

            var masterList = await _repository.GetByUserID(senderID);
            var master = masterList.FirstOrDefault();
            if (master == null)
                return (Messages.CashCollectionMasterMessages.NotFound, string.Empty);

            decimal openingBalance = master.Outstanding;
            decimal closingBalance = openingBalance + request.Amount;
            master.Outstanding = closingBalance;

            int updateResult = await _repository.Update(master);
            if (updateResult <= 0)
                return ("Cash collection not reversed", string.Empty);

            string description = !string.IsNullOrWhiteSpace(request.Remark) && request.Remark.Length > 2
                ? $"{request.Remark} Outstanding increased, by: {request.Amount} and new outstanding is: {Math.Round(closingBalance, 2)}"
                : $"Outstanding increased (reverse), by: {request.Amount} and new outstanding is: {Math.Round(closingBalance, 2)}";

            try
            {
                await _detailRepo.Insert(new CashCollectionDetail
                {
                    UserID = senderID,
                    ParentID = receiverID,
                    OutstandingOpening = openingBalance,
                    OutstandingAmount = request.Amount,
                    OutstandingClosing = closingBalance,
                    ParentRemark = description,
                    PayMode = request.PayMode,
                    Credit = "CREDIT",
                    Debit = string.Empty,
                    Type = "CASH_COLLECTION_REVERSE"
                });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

            return (StatusType.Ok, "Cash collection reversed successfully");
        }

        public async Task<(string failureMessage, SelectByUserIDResponse? data)> SelectUserOutstanding(int userID)
        {
            var masterList = await _repository.GetByUserID(userID);
            var master = masterList.FirstOrDefault();
            if (master == null)
                return (Messages.Common.Message.DataNotFound, null);

            string balanceDetail = string.Empty;
            try
            {
                var wallets = await _balanceRepo.SelectByUserID(userID);
                balanceDetail = string.Join(", ", wallets.Select(w => $"{w.ShortName} : {(int)w.Balance}"));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

            return (StatusType.Ok, new SelectByUserIDResponse
            {
                BalanceDetail = balanceDetail,
                Outstanding = master.Outstanding,
                MaxOutStanding = master.MaxOutstanding
            });
        }

        public async Task<string> SendCollectionMessage(int targetUserID, int adminUserID)
        {
            var allUsers = await _userRepo.GetAllUsers();

            var adminUser = allUsers.FirstOrDefault(u => u.UserMasterID == adminUserID);
            if (adminUser == null || adminUser.UserTypeID != 1)
                return Messages.Common.Message.Unauthorized;

            var targetUser = allUsers.FirstOrDefault(u => u.UserMasterID == targetUserID);
            if (targetUser == null)
                return Messages.Common.Message.InvalidData;

            var masterList = await _repository.GetByUserID(targetUserID);
            var master = masterList.FirstOrDefault();
            decimal outstanding = master?.Outstanding ?? 0;

            _logger.Write($"SendCollectionMessage: UserID={targetUserID}, FirmName={targetUser.FirmName}, Outstanding={outstanding}");

            return StatusType.Ok;
        }

        public async Task<(string failureMessage, OutstandingWrapper? data)> OutstandingReport(SelectCashCollectionRequest request)
        {
            var allMasters = await _repository.GetAll();
            var allUsers = await _userRepo.GetAllUsers();

            var report = allMasters
                .Where(c => c.Outstanding != 0)
                .OrderByDescending(c => c.Outstanding)
                .Select(c =>
                {
                    var user = allUsers.FirstOrDefault(u => u.UserMasterID == c.UserID);
                    string name = user != null ? $"{user.FirmName} ({user.MobileNumber})" : c.UserID.ToString();
                    return new OutstandingViewModel { UserID = c.UserID, UserName = name, Outstanding = c.Outstanding };
                })
                .ToList();

            if (request.UserID > 0)
                report = report.Where(r => r.UserID == request.UserID).ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                report = report.Where(r => r.UserName.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

            var wrapper = new OutstandingWrapper
            {
                data = report,
                IsExcelDownload = request.IsExcelDownload
            };

            return (StatusType.Ok, wrapper);
        }
    }
}
