using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.eKYC;
using FintechApp.Responses.eKYC;
using FintechApp.StaticModels;
using Newtonsoft.Json;
using System.Xml;

namespace FintechApp.Services
{
    public class eKYCService : IeKYCService
    {
        private readonly IUserRepository _userRepository;
        private readonly IeKYCMasterRepository _eKYCMasterRepository;
        private readonly IeKYCLogRepository _eKYCLogRepository;
        private readonly IeKYCSettingRepository _eKYCSettingRepository;
        private readonly IProviderUserMappingRepository _providerUserMappingRepository;
        private readonly ISettingMasterRepository _settingMasterRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string YesBankEkycSettingName = "YesBank";
        private const string UidTokenKey = "UidToken";

        public eKYCService(
            IUserRepository userRepository,
            IeKYCMasterRepository eKYCMasterRepository,
            IeKYCLogRepository eKYCLogRepository,
            IeKYCSettingRepository eKYCSettingRepository,
            IProviderUserMappingRepository providerUserMappingRepository,
            ISettingMasterRepository settingMasterRepository,
            IHttpClientFactory httpClientFactory)
        {
            _userRepository = userRepository;
            _eKYCMasterRepository = eKYCMasterRepository;
            _eKYCLogRepository = eKYCLogRepository;
            _eKYCSettingRepository = eKYCSettingRepository;
            _providerUserMappingRepository = providerUserMappingRepository;
            _settingMasterRepository = settingMasterRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(string failureMessage, VerifyPanResponse? data)> VerifyPan(VerifyPanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PanNumber))
                return (Messages.Common.Message.InvalidRequest, null);

            var agencyIDSetting = await _settingMasterRepository.GetByKey("ZoopeKYCAgenecyID");
            var apiKeySetting = await _settingMasterRepository.GetByKey("ZoopeKYCAPIKey");

            string agencyID = agencyIDSetting?.Value ?? string.Empty;
            string apiKey = apiKeySetting?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(agencyID) || string.IsNullOrEmpty(apiKey))
                return ("Invalid credentials.", null);

            string url = $"https://oneproject7.consisty.com/ZoopeKYC/GetPANDetails?AgencyID={agencyID}&APIKey={apiKey}&PanNumber={request.PanNumber.Trim()}";

            string httpResponse;
            try
            {
                var client = _httpClientFactory.CreateClient();
                httpResponse = await client.GetStringAsync(url);
            }
            catch (Exception)
            {
                return (Messages.Common.Message.ExceptionOccured, null);
            }

            VerifyPanResponse? response;
            try
            {
                response = JsonConvert.DeserializeObject<VerifyPanResponse>(httpResponse);
            }
            catch
            {
                return (Messages.Common.Message.ExceptionOccured, null);
            }

            if (response == null)
                return (Messages.Common.Message.ExceptionOccured, null);

            if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("Request is from invalid IP"))
                return (response.Message, response);

            if (response.Status.Equals("FAILURE", StringComparison.OrdinalIgnoreCase))
                return (response.Message, response);

            return (StatusType.Ok, response);
        }

        public async Task<(string failureMessage, Yes_eKYCInitiateResponse data)> YesBankEkycInitiate(int userID)
        {
            var response = new Yes_eKYCInitiateResponse();

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
            {
                response.Message = Messages.UserMessages.UserNotFound;
                return (response.Message, response);
            }

            if (user.IseKYC || user.IsKYCApproved)
            {
                response.Message = "User eKYC already completed.";
                return (response.Message, response);
            }

            var allSettings = await _eKYCSettingRepository.GetAll();
            var yesBankSetting = allSettings.FirstOrDefault(s =>
                s.Name.Equals(YesBankEkycSettingName, StringComparison.OrdinalIgnoreCase) && s.IsActive);

            if (yesBankSetting == null)
            {
                response.Message = "YesBank eKYC is not active.";
                return (response.Message, response);
            }

            // YesBankCore SDK integration required for actual API call.
            // The SDK handles RSA-encrypted communication with YesBank's KYC API.
            // Credentials: MerchantID=yesBankSetting.MerchantID, AuthKey=yesBankSetting.AuthKey, Token=yesBankSetting.Token
            // When SDK is available: call AgentCreation API and store OTPToken + RequestID in eKYCMaster.
            response.Message = "YesBank eKYC SDK integration pending. Contact system administrator.";
            return (response.Message, response);
        }

        public async Task<(string failureMessage, Yes_eKYCValidateResponse data)> YesBankEkycValidate(eKYCValidateRequest request, int userID)
        {
            var response = new Yes_eKYCValidateResponse();

            if (string.IsNullOrWhiteSpace(request.Otp))
            {
                response.Message = "Invalid OTP.";
                return (response.Message, response);
            }

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
            {
                response.Message = Messages.UserMessages.UserNotFound;
                return (response.Message, response);
            }

            if (user.IseKYC || user.IsKYCApproved)
            {
                response.Message = "User eKYC already completed.";
                return (response.Message, response);
            }

            var eKYCRecords = await _eKYCMasterRepository.GetByUserID(userID);
            var eKYCMasterDetails = eKYCRecords.FirstOrDefault();

            if (eKYCMasterDetails == null)
            {
                response.Message = "OTP token not found. Please initiate eKYC first.";
                return (response.Message, response);
            }

            request.otpToken = eKYCMasterDetails.OTPToken;
            request.RequestID = eKYCMasterDetails.OrderID;

            // YesBankCore SDK integration required for OTP validation and AadharWadh API calls.
            // When SDK is available: call VerifyOTP API → get KYCToken → call AadharWadh API → get Wadh → update eKYCMaster.
            response.Message = "YesBank eKYC SDK integration pending. Contact system administrator.";
            return (response.Message, response);
        }

        public async Task<(string failureMessage, Yes_eKYCValidateResponse data)> YesBankBioMatricVerification(YesBank_AadharBiometricRequest request, int userID)
        {
            var response = new Yes_eKYCValidateResponse();

            var user = await _userRepository.GetUserById(userID);
            if (user == null)
            {
                response.Message = Messages.UserMessages.UserNotFound;
                return (response.Message, response);
            }

            if (user.IseKYC || user.IsKYCApproved)
            {
                response.Message = "User eKYC already completed.";
                return (response.Message, response);
            }

            var eKYCRecords = await _eKYCMasterRepository.GetByUserID(userID);
            var eKYCMasterDetails = eKYCRecords.FirstOrDefault();

            if (eKYCMasterDetails == null || string.IsNullOrEmpty(eKYCMasterDetails.KYCToken))
            {
                response.Message = "KYC token not found. Please validate OTP first.";
                return (response.Message, response);
            }

            request.RequestID = eKYCMasterDetails.OrderID;
            request.KYCToken = eKYCMasterDetails.KYCToken;
            request.WadhValue = eKYCMasterDetails.wadh;

            if (!string.IsNullOrEmpty(request.FingerPrint))
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(request.FingerPrint);
                    var paramNode = xmlDoc.SelectSingleNode("//Param[@name='sysid']");
                    if (paramNode?.Attributes?["value"] != null)
                        request.SerialNumber = paramNode.Attributes["value"]!.Value;
                }
                catch
                {
                    response.Message = "Invalid fingerprint XML data.";
                    return (response.Message, response);
                }
            }

            // YesBankCore SDK integration required for AadharBiometricKYC API call.
            // When SDK is available: call AadharBioMertic API → on success: update eKYCMaster,
            // insert eKYCLog, insert ProviderUserMapping with UidToken=apiResponse.Token, and call
            // IKycApprovalService.AutoApproveEkyc(userID) — that sets IseKYC + IsKYCApproved, writes
            // the KYCApprovedDetails audit row and sends the "KYC Approved" message. Do NOT set the
            // two user flags by hand here, or the approval leaves no audit trail and the user is
            // never told. (Until the SDK lands, the admin KYC queue reconciles any user already
            // flagged IseKYC on load, so the auto-approval rule still holds.)
            response.Message = "YesBank eKYC SDK integration pending. Contact system administrator.";
            return (response.Message, response);
        }
    }
}
