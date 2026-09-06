using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.HitachiYesBankAEPS;
using FintechApp.Responses.HitachiYesBankAEPS;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class HitachiYesBankAEPSService : IHitachiYesBankAEPSService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAEPSSettingRepository _aepsSettingRepository;
        private readonly ITransactionMasterRepository _transactionMasterRepository;
        private readonly IWalletServiceMappingRepository _walletServiceMappingRepository;
        private readonly IProviderMasterRepository _providerMasterRepository;
        private readonly IDailyAuthenticationMasterRepository _dailyAuthRepository;

        private const string YesBankSettingName = "YesBank";
        private const int AEPSServiceID = 4;
        private const int WithdrawOperatorID = 5;
        private const int BalanceOperatorID = 4;
        private const int MiniStatementOperatorID = 9;

        public HitachiYesBankAEPSService(
            IUserRepository userRepository,
            IAEPSSettingRepository aepsSettingRepository,
            ITransactionMasterRepository transactionMasterRepository,
            IWalletServiceMappingRepository walletServiceMappingRepository,
            IProviderMasterRepository providerMasterRepository,
            IDailyAuthenticationMasterRepository dailyAuthRepository)
        {
            _userRepository = userRepository;
            _aepsSettingRepository = aepsSettingRepository;
            _transactionMasterRepository = transactionMasterRepository;
            _walletServiceMappingRepository = walletServiceMappingRepository;
            _providerMasterRepository = providerMasterRepository;
            _dailyAuthRepository = dailyAuthRepository;
        }

        public async Task<(string failureMessage, YesBank_AuthenticationResponse data)> Authentication(YesBank_AuthenticationRequest request, int userID)
        {
            var response = new YesBank_AuthenticationResponse { Status = "FAILURE" };

            if (string.IsNullOrWhiteSpace(request.AadhaarNumber) || request.AadhaarNumber.Length != 12)
            {
                response.Message = "Invalid Aadhaar number.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.FingerPrint) || request.FingerPrint.Length < 20)
            {
                response.Message = "Invalid fingerprint data.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.Latitude) || string.IsNullOrWhiteSpace(request.Longitude))
            {
                response.Message = "Location data is required.";
                return (response.Message, response);
            }

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
            {
                response.Message = Messages.UserMessages.UserNotFound;
                return (response.Message, response);
            }

            if (!user.IsActive)
            {
                response.Message = Messages.Common.Message.AccountInactive;
                return (response.Message, response);
            }

            // YesBankCore Hitachi AEPS SDK integration required for Authentication.
            // When SDK is available: call Hitachi_AEPSProcessing().Authentication(request, requestID, ipAddress, user).
            // On SUCCESS: insert DailyAuthenticationMaster record for this user.
            response.Message = "YesBank AEPS (Hitachi) SDK integration pending. Contact system administrator.";
            return (response.Message, response);
        }

        public async Task<(string failureMessage, AEPSViewModelResponse data)> CashWithdrawal(YesBank_CashWithdrawalRequest request, int userID, string ipAddress, string userAgent)
        {
            var response = new AEPSViewModelResponse { Status = "FAILURE" };

            if (string.IsNullOrWhiteSpace(request.IIN))
            {
                response.Message = "Invalid IIN.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.AadhaarNumber) || request.AadhaarNumber.Length != 12)
            {
                response.Message = "Invalid Aadhaar number.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.FingerPrint) || request.FingerPrint.Length < 20)
            {
                response.Message = "Invalid fingerprint data.";
                return (response.Message, response);
            }

            if (request.Amount <= 0)
            {
                response.Message = Messages.Common.Message.InvalidAmount;
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.Latitude) || string.IsNullOrWhiteSpace(request.Longitude))
            {
                response.Message = "Location data is required.";
                return (response.Message, response);
            }

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
            {
                response.Message = Messages.UserMessages.UserNotFound;
                return (response.Message, response);
            }

            if (!user.IsActive)
            {
                response.Message = Messages.Common.Message.AccountInactive;
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.ConsumerNumber) || request.ConsumerNumber.Length <= 5)
                request.ConsumerNumber = user.MobileNumber;

            var aepsSetting = await _aepsSettingRepository.GetByName(YesBankSettingName);
            if (aepsSetting == null || !aepsSetting.IsActive)
            {
                response.Message = "YesBank AEPS service is not active.";
                return (response.Message, response);
            }

            if (request.Amount < aepsSetting.MinimumAmount || request.Amount > aepsSetting.MaximumAmount)
            {
                response.Message = $"Amount must be between {aepsSetting.MinimumAmount} and {aepsSetting.MaximumAmount}.";
                return (response.Message, response);
            }

            // YesBankCore Hitachi AEPS SDK integration required.
            // When SDK is available: call FunctionalValidationProcessing.FunctionalValidation() → insert AEPS TransactionMaster
            // → call Hitachi_AEPSProcessing().Withdraw(request, ipAddress, requestID, user)
            // → update TransactionMaster status + AEPSBal.UpdateAEPS() + ProviderRequestResponseLog().
            response.Message = "YesBank AEPS (Hitachi) SDK integration pending. Contact system administrator.";
            return (response.Message, response);
        }

        public async Task<(string failureMessage, AEPSViewModelResponse data)> BalanceEnquiry(YesBank_BalanceEnquiryRequest request, int userID, string ipAddress, string userAgent)
        {
            var response = new AEPSViewModelResponse { Status = "FAILURE" };

            if (string.IsNullOrWhiteSpace(request.IIN))
            {
                response.Message = "Invalid IIN.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.AadhaarNumber) || request.AadhaarNumber.Length != 12)
            {
                response.Message = "Invalid Aadhaar number.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.FingerPrint) || request.FingerPrint.Length < 20)
            {
                response.Message = "Invalid fingerprint data.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.Latitude) || string.IsNullOrWhiteSpace(request.Longitude))
            {
                response.Message = "Location data is required.";
                return (response.Message, response);
            }

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
            {
                response.Message = Messages.UserMessages.UserNotFound;
                return (response.Message, response);
            }

            if (!user.IsActive)
            {
                response.Message = Messages.Common.Message.AccountInactive;
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.ConsumerNumber) || request.ConsumerNumber.Length <= 5)
                request.ConsumerNumber = user.MobileNumber;

            var aepsSetting = await _aepsSettingRepository.GetByName(YesBankSettingName);
            if (aepsSetting == null || !aepsSetting.IsActive)
            {
                response.Message = "YesBank AEPS service is not active.";
                return (response.Message, response);
            }

            // YesBankCore Hitachi AEPS SDK integration required.
            // When SDK is available: call FunctionalValidationProcessing.FunctionalValidation() (amount=0)
            // → insert AEPS TransactionMaster → call Hitachi_AEPSProcessing().BalanceEnquiry()
            // → update TransactionMaster + ProviderRequestResponseLog().
            response.Message = "YesBank AEPS (Hitachi) SDK integration pending. Contact system administrator.";
            return (response.Message, response);
        }

        public async Task<(string failureMessage, AEPSViewModelResponse data)> MiniStatement(YesBank_MinistatementRequest request, int userID, string ipAddress, string userAgent)
        {
            var response = new AEPSViewModelResponse { Status = "FAILURE" };

            if (string.IsNullOrWhiteSpace(request.IIN))
            {
                response.Message = "Invalid IIN.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.AadhaarNumber) || request.AadhaarNumber.Length != 12)
            {
                response.Message = "Invalid Aadhaar number.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.FingerPrint) || request.FingerPrint.Length < 20)
            {
                response.Message = "Invalid fingerprint data.";
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.Latitude) || string.IsNullOrWhiteSpace(request.Longitude))
            {
                response.Message = "Location data is required.";
                return (response.Message, response);
            }

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
            {
                response.Message = Messages.UserMessages.UserNotFound;
                return (response.Message, response);
            }

            if (!user.IsActive)
            {
                response.Message = Messages.Common.Message.AccountInactive;
                return (response.Message, response);
            }

            if (string.IsNullOrWhiteSpace(request.ConsumerNumber) || request.ConsumerNumber.Length <= 5)
                request.ConsumerNumber = user.MobileNumber;

            var aepsSetting = await _aepsSettingRepository.GetByName(YesBankSettingName);
            if (aepsSetting == null || !aepsSetting.IsActive)
            {
                response.Message = "YesBank AEPS service is not active.";
                return (response.Message, response);
            }

            // YesBankCore Hitachi AEPS SDK integration required.
            // When SDK is available: validate fingerprint via Hitachi_AEPSProcessing().isValidCapturedFingerPrint()
            // → call FunctionalValidationProcessing.FunctionalValidation() (amount=0)
            // → insert AEPS TransactionMaster → call Hitachi_AEPSProcessing().MiniStatement()
            // → parse ministatement string (split by '|', extract Date/Narration/TxnType/Amount)
            // → update TransactionMaster + ProviderRequestResponseLog().
            response.Message = "YesBank AEPS (Hitachi) SDK integration pending. Contact system administrator.";
            return (response.Message, response);
        }
    }
}
