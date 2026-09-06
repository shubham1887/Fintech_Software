using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.MicroATM;
using FintechApp.Responses.MicroATM;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    public class MicroATMService : IMicroATMService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMicroATMSettingRepository _microATMSettingRepository;
        private readonly ITransactionMasterRepository _transactionMasterRepository;
        private readonly IWalletServiceMappingRepository _walletServiceMappingRepository;
        private readonly IProviderMasterRepository _providerMasterRepository;

        private const int MATMServiceID = 5;

        public MicroATMService(
            IUserRepository userRepository,
            IMicroATMSettingRepository microATMSettingRepository,
            ITransactionMasterRepository transactionMasterRepository,
            IWalletServiceMappingRepository walletServiceMappingRepository,
            IProviderMasterRepository providerMasterRepository)
        {
            _userRepository = userRepository;
            _microATMSettingRepository = microATMSettingRepository;
            _transactionMasterRepository = transactionMasterRepository;
            _walletServiceMappingRepository = walletServiceMappingRepository;
            _providerMasterRepository = providerMasterRepository;
        }

        public async Task<(string failureMessage, InitiateResponse? data)> Initiate(InitiateImproveRequest request, int userID, string ipAddress)
        {
            if (request.Amount <= 0)
                return (Messages.Common.Message.InvalidAmount, null);

            if (string.IsNullOrWhiteSpace(request.Name))
                return (Messages.Common.Message.InvalidData, null);

            if (request.OperatorID <= 0)
                return (Messages.Common.Message.InvalidData, null);

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
                return (Messages.UserMessages.UserNotFound, null);

            if (!user.IsActive)
                return (Messages.Common.Message.AccountInactive, null);

            var settings = await _microATMSettingRepository.GetAll();
            var setting = settings.FirstOrDefault(s => s.IsActive);
            if (setting == null)
                return ("MicroATM service is currently not active.", null);

            if (request.Amount < setting.MinimumAmount)
                return ($"Minimum transaction amount is {setting.MinimumAmount}.", null);

            if (request.Amount > setting.MaximumAmount)
                return ($"Maximum transaction amount is {setting.MaximumAmount}.", null);

            // Get WalletID for MATM service from mapping
            var walletMappings = await _walletServiceMappingRepository.GetAll();
            var walletMapping = walletMappings.FirstOrDefault(w => w.ServiceID == MATMServiceID);
            int walletID = walletMapping?.WalletID ?? 1;

            // Get ProviderID by setting name
            var provider = await _providerMasterRepository.GetByName(setting.Name);
            int providerID = provider?.ProviderID ?? 1;

            // Generate unique request ID for this transaction
            string requestID = "MATM" + DateTime.UtcNow.Ticks.ToString()[^12..];

            // Insert pending TransactionMaster record
            // NOTE: Slab commission, AccountProcessing (debit balance), Inbox and LocationLookup
            // inserts are pending — these require the full accounting infrastructure (SlabDetailBAL,
            // AccountProcessing) to be ported. When available, debit user wallet here.
            var transaction = new TransactionMaster
            {
                UserID = userID,
                ServiceID = MATMServiceID,
                OperatorID = request.OperatorID,
                WalletID = walletID,
                ConsumerNumber = request.ConsumerNumber,
                Amount = request.Amount,
                DebitAmount = request.Amount,
                Status = "Pending",
                ProviderID = providerID,
                ProviderRequestID = requestID,
                Through = "A",
                IPAddress = ipAddress,
            };

            await _transactionMasterRepository.Insert(transaction);

            // PaySwiff-specific response: app uses these credentials to call PaySwiff SDK directly.
            // Slat = Amount * 100 (paise), UserName = user's UniqueReferenceNo (merchant identifier).
            var response = new InitiateResponse
            {
                RequestId = requestID,
                Amount = request.Amount,
                CustomerNumber = request.CustomerNumber,
                UniqueRefNo = user.UniqueReferenceNo,
                UserMobileNumber = user.MobileNumber,
                Name = request.Name,
                IsActive = setting.IsActive,
                Slat = (request.Amount * 100).ToString("F0"),
                UserName = user.UniqueReferenceNo,
                Password = setting.Password,
                Token = setting.Token,
                PublicKey = setting.PublicKey,
                MerchantID = setting.MerchantID,
            };

            return (StatusType.Ok, response);
        }

        public async Task<(string failureMessage, string responseJson)> TransactionStatusImprove(TransactionStatusRequest request, int userID)
        {
            if (string.IsNullOrWhiteSpace(request.OrderID))
                return (Messages.Common.Message.InvalidData, string.Empty);

            var transaction = await _transactionMasterRepository.GetByProviderRequestID(request.OrderID);
            if (transaction == null)
                return (Messages.Common.Message.DataNotFound, string.Empty);

            // PaySwiff SDK integration required for live status check.
            // When SDK is available: call PaySwiff TransactionStatus API with MerchantID + OrderID/TransactionID.
            // On SUCCESS response: update TransactionMaster status, trigger AccountProcessing to
            // credit/debit user and admin wallets, send FCM notification, send SMS/WhatsApp receipt.
            var result = new
            {
                Status = transaction.Status,
                Amount = transaction.Amount,
                ProviderRequestID = transaction.ProviderRequestID,
                ReferenceNo = transaction.ReferenceNo,
                Message = "PaySwiff status check integration pending."
            };

            return (StatusType.Ok, JsonConvert.SerializeObject(result));
        }
    }
}
