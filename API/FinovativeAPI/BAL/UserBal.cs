
using Google.Authenticator;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Processing.Processing;
using project.Response.User;
using project.StaticModel;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;

using Project.Requests;
using Project.Requests.User;
using Project.Response.CashCollection;
using Project.Response.FCMResponse;
using Project.Response.ServiceSubscription;
using Project.Response.User;
using Project.Response.UserBalanceCapping;
using Project.Response.Wallet;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class UserBal
    {
        private IConfiguration _config;

        public UserBal(IConfiguration config)
        {
            _config = config;
        }

        public UserBal()
        {
        }

        private string isPasswordExpired(int UserID)
        {
            string failureMessage = UserMessage.InvalidUserNameOrPassword;

            try
            {
                if (UserID <= 0)
                {
                    return failureMessage;
                }

                if (!new PasswordChangeHistoryBal().SelectPasswordExpiry(UserID))
                {
                    failureMessage = UserMessage.PasswordExpired;
                    return failureMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }

        internal bool IsUplineActive(int UserID)
        {
            if (!Company.IsUplineActive.EqualsIgnoreCase("Yes"))
                return true;
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user == null)
                return false;
            UserMaster parentuser = new UserDAL().SelectByIDFromCache(user.ParentID);
            if (parentuser == null)
                return false;
            if (parentuser.UserID == Company.AdminUserID)
                return true;
            if (parentuser.IsActive)
                return true;

            return false;
        }

        private bool isValidAgentCode(UserMaster user, string AgentCode)
        {
            if (user == null)
            {
                return false;
            }
            if (AgentCode == null)
            {
                return false;
            }
            if (AgentCode.Trim().Length <= 0)
            {
                return false;
            }

            return true;
        }

        public Response.Agent.ViewModel CheckAgentRegistration(int userID, int ProviderID, string iPAddress, out string failureMessage)
        {
            failureMessage = Message.UserMessage.InvalidUser;
            Response.Agent.ViewModel response = new Response.Agent.ViewModel();
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (user == null)
                return response;

            response.IsRegister = false;
            response.AgentCode = "";
            response.Message = UserMessage.AgentNotRegister;
            failureMessage = UserMessage.AgentNotRegister;

            if (ProviderID == Company.PayOneMoneyProviderID)
            {
                if (user.PayOneMoneyAgentCode == null)
                    return response;

                if (!isValidAgentCode(user, user.PayOneMoneyAgentCode))
                    return response;

                response.AgentCode = user.PayOneMoneyAgentCode;
            }

            if (ProviderID == Company.RoyalPayProviderID)
            {
                if (user.RoyalPayAgentCode == null)
                    return response;

                if (!isValidAgentCode(user, user.RoyalPayAgentCode))
                    return response;

                response.AgentCode = user.RoyalPayAgentCode;
            }

            if (ProviderID == Company.RMGPayProviderID)
            {
                if (user.RMGPayAgentCode == null)
                    return response;

                if (!isValidAgentCode(user, user.RMGPayAgentCode))
                    return response;

                response.AgentCode = user.RMGPayAgentCode;
            }

            if (ProviderID == Company.RMGPay5LProviderID)
            {
                if (user.RMGPayAgentCode == null)
                    return response;

                if (!isValidAgentCode(user, user.RMGPayAgentCode))
                    return response;

                response.AgentCode = user.RMGPayAgentCode;
            }

            response.IsRegister = true;
            response.Message = StatusType.Ok;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal UserViewModel SelectUserByMahagramAEPSAgentID(string bCID)
        {
            UserViewModel response = new UserViewModel();
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectUserByMahagramAEPSAgentID(bCID);
                response.EmailID = ObjModel.EmailID;
                response.UserID = ObjModel.UserID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return response;
        }

        private string BlockAccountIfWrongPasswrodLimitExceeds(int UserID)
        {
            string failureMessage = UserMessage.InvalidUserNameOrPassword;

            int WrongPasscount = new WrongPasswordHistoryDal().SelectWrongPasswordCount(UserID);

            int WrongPasswordLimit = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.WrongPasswordCount), 5);

            if (WrongPasscount < WrongPasswordLimit)
            {
                return StatusType.Ok;
            }

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }

            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsActive = false;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
            if (rowsAffected <= 0)
            {
                return failureMessage;
            }

            Task.Run(() => new MessageSelection().SendDeactivateMessage(ObjModel));

            return Message.UserMessage.BlockAccount;
        }

        internal int SelectUserIDFromAEPSMerchantID(string merchant_id)
        {
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectUserByPaysprintAEPSMerchantID(merchant_id);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return 0;
            }

            if (ObjModel == null)
            {
                return 0;
            }

            return ObjModel.UserID;
        }

        public async Task<(string, LoginResponse)> Login(LoginRequest loginRequest, string IPAddress)
        {
            LoginResponse ReturnObj = null;
            loginRequest.CaptchaToken = "00000";

            await Task.Delay(1);

            string failureMessage = loginRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return (failureMessage, ReturnObj);

            failureMessage = UserMessage.UpdateToLatestVersion;
            try
            {
                string AndroidAppVersion = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AndroidAppVersion), "0");

                if (!AndroidAppVersion.EqualsIgnoreCase("0") &&
                    loginRequest.Device.EqualsIgnoreCase(PlatformCode.Android) &&
                    !loginRequest.AndroidAppVersion.EqualsIgnoreCase(AndroidAppVersion))
                {
                    failureMessage = UserMessage.UpdateToLatestVersion;
                    failureMessage = failureMessage.Replace("[platform]", loginRequest.Device);
                    return (failureMessage, ReturnObj);
                }

                string iOSAppVersion = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.iOSAppVersion), "0");

                if (!iOSAppVersion.EqualsIgnoreCase("0") &&
                    loginRequest.Device.EqualsIgnoreCase(PlatformCode.iOS) &&
                    !loginRequest.iOSAppVersion.EqualsIgnoreCase(iOSAppVersion))
                {
                    failureMessage = UserMessage.UpdateToLatestVersion;

                    failureMessage = failureMessage.Replace("[platform]", loginRequest.Device);

                    return (failureMessage, ReturnObj);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return (failureMessage, ReturnObj);
            }

            bool IsFaliAttempt = new DeviceWiseFailLoginHistoryDAL().IsReturnFailAttempts(loginRequest.UserName, loginRequest.DeviceID);
            if (IsFaliAttempt)
            {
                failureMessage = UserMessage.AppLock;
                return (failureMessage, ReturnObj);
            }

            failureMessage = UserMessage.InvalidUserNameOrPassword;

            loginRequest.Password = new Hashing().Hash(loginRequest.Password);
            try
            {
                string cashCollection = IsCashCollection();
                string TargetOnSale = IsTargetOnSale();
                string isEkycUserProfileUpdate = IsEkycUserProfileUpdate();
                string isUserListOnDashboard = IsUserListOnDashboard();

                string pin = IsPin();
                ReturnObj = new UserDAL().SelectForLogin(loginRequest.UserName, loginRequest.Password, loginRequest.Device, loginRequest.DeviceID, loginRequest.Pin, cashCollection, pin, TargetOnSale, isEkycUserProfileUpdate, isUserListOnDashboard);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return (failureMessage, ReturnObj);
            }


            if (ReturnObj == null)
            {
                try
                {
                    bool IsDeactive = await new UserDAL().SelectActiveUserByMobileNumber(loginRequest.UserName);
                    if (!IsDeactive)
                    {
                        failureMessage = "User is deactived, Please contact to customer care";
                        return (failureMessage, ReturnObj);
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return (failureMessage, ReturnObj);
                }

                try
                {
                    int UserID = await new UserDAL().SelectUserIDByMobileNumber(loginRequest.UserName);

                    if (UserID <= 0)
                    {
                        return (failureMessage, ReturnObj);
                    }

                    failureMessage = BlockAccountIfWrongPasswrodLimitExceeds(UserID);
                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    {
                        AddToLoginHistory(ReturnObj.UserID, IPAddress, loginRequest.Device, failureMessage, loginRequest.Latitude, loginRequest.Longitude);
                        return (failureMessage, ReturnObj);
                    }

                    WrongPasswordHistory history = new WrongPasswordHistory
                    {
                        MobileNumber = loginRequest.UserName,
                        IPAddress = IPAddress,
                        UserID = UserID
                    };

                    Parallel.Invoke(
                     () =>
                     {
                         new WrongPasswordHistoryDal().Insert(history);
                     }
                      ,
                      () =>
                      {
                          AddToLoginHistory(UserID, IPAddress, loginRequest.Device, Message.UserMessage.InvalidUserNameOrPassword, loginRequest.Latitude, loginRequest.Longitude);
                      });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return (failureMessage, ReturnObj);
                }

                return (failureMessage, ReturnObj);
            }

            if (ReturnObj.Message == "Contact customer service for assistance with the PIN or app being locked")
            {
                failureMessage = ReturnObj.Message;
                return (failureMessage, ReturnObj);
            }

            ParentViewModel viewModel = new UserDAL().SelectParent(ReturnObj.UserID);

            if (viewModel == null)
            {
                failureMessage = "Your upline is deactive, Please contact to customer care";
                ReturnObj = null;
                return (failureMessage, ReturnObj);
            }

            failureMessage = isPasswordExpired(ReturnObj.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                AddToLoginHistory(ReturnObj.UserID, IPAddress, loginRequest.Device, failureMessage, loginRequest.Latitude, loginRequest.Longitude);
                failureMessage = StatusType.Ok;
                ReturnObj = new LoginResponse
                {
                    UserID = 0,
                    IsPasswordExpired = true,
                };
                return (failureMessage, ReturnObj);
            }

            failureMessage = StatusType.Ok;

            if (ReturnObj.IsInitialPassword)
            {
                ReturnObj = new LoginResponse
                {
                    UserID = 0,
                    IsInitialPassword = true,
                };
                return (failureMessage, ReturnObj);
            }

            if (ReturnObj.IsTwoFA)
            {
                ReturnObj = new LoginResponse
                {
                    UserID = 0,
                    IsTwoFA = true,
                    IsNewDevice = false,
                    IsOTPSent = false,
                    Message = "Open 2FA app, and submit 2FA"
                };
                return (failureMessage, ReturnObj);
            }

            if (ReturnObj.IsLoginOTP)
            {
                new MessageSelection().SendLoginOTP(ReturnObj);
                ReturnObj = new LoginResponse
                {
                    UserID = 0,
                    IsNewDevice = true,
                    IsOTPSent = true,
                    IsTwoFA = false,
                    Message = "OTP sent on your number/email"
                };
                return (failureMessage, ReturnObj);
            }

            bool NewDeviceOTP = isOTPForLoginForFingerprint(ReturnObj.UserID, loginRequest.FingerPrint);

            if (NewDeviceOTP)
            {
                new MessageSelection().SendLoginOTP(ReturnObj);
                ReturnObj = new LoginResponse
                {
                    UserID = 0,
                    IsNewDevice = true,
                    IsOTPSent = true,
                    IsTwoFA = false,
                    Message = "OTP sent on your number/email"
                };
                return (failureMessage, ReturnObj);
            }

            AddToLoginHistory(ReturnObj.UserID, IPAddress, loginRequest.Device, UserMessage.LoginSuccess, loginRequest.Latitude, loginRequest.Longitude);

            if (ReturnObj.UserID != Company.AdminUserID)
                ReturnObj.NewsCount = new NewsBAL().SelectNewsCount(ReturnObj.UserID);

            if (ReturnObj.UserTypeID == (int)UserType.Developer)
            {
                ReturnObj.RedirectTo = "/Company";
                return (failureMessage, ReturnObj);
            }
            if (ReturnObj.UserTypeID == (int)UserType.Support)
            {
                List<Response.SupportUserAccessDetail.Parent> menus = new MenuMasterBal().GetMenuHeirarchy();
                ReturnObj.MenuResponse.data = menus;
            }
            if (loginRequest.Device.EqualsIgnoreCase(PlatformCode.Web))
            {
                ReturnObj.UISettings = new UISettingsBal().SelectAll();
            }
            UpdateFlagAfterLoginSuccess(loginRequest.Device, ReturnObj);

            try
            {
                new WrongPasswordHistoryDal().DeleteWrongPasswordHistory(ReturnObj.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return (failureMessage, ReturnObj);
        }

        private void UpdateFlagAfterLoginSuccess(string Device, LoginResponse ReturnObj)
        {
            ReturnObj.ActiveDMTNeft = "";
            ReturnObj.ActiveDMTList = new DMTSettingsBal().SelectActiveDMTList();

            if (ReturnObj.ActiveDMTList == null || ReturnObj.ActiveDMTList.Count <= 0)
            {
                ReturnObj.ActiveDMT = "";
            }
            else
            {
                ReturnObj.ActiveDMT = ReturnObj.ActiveDMTList[0].Name;
            }

            string ActiveAEPS = new AEPSSettingDal().SelectActiveAEPSName();
            ReturnObj.ActiveAEPS = ActiveAEPS;

            ReturnObj.CommissionOnTransfer = Company.CommissionOnTransfer;
            ReturnObj.IsWhiteLabel = Company.IsWhiteLabel.ToLower();
            ReturnObj.IsPendingProvider = Company.IsPendingProvider.ToLower();
            string WhatsappURL = GetWhatsappURL(Device);
            ReturnObj.WhatsappURL = WhatsappURL;

            if (Util.isRoleAdminOrSupport(ReturnObj.UserTypeID))
            {
                ReturnObj.IsChannel = true;
            }

            ReturnObj.RedirectTo = GetRedirectionByUserID(ReturnObj.UserTypeID);

            if (Company.OverDraft.EqualsIgnoreCase("Yes"))
            {
                ReturnObj.IsOverDraftActive = new OverdraftMasterDAL().SelectActiveOverdraft(ReturnObj.UserID);
            }

            if (ReturnObj.UserTypeID == (int)UserType.Admin || Company.GSTHold.EqualsIgnoreCase("Yes"))
            {
                ReturnObj.IsGSTHold = true;
            }

            if (ReturnObj.UserTypeID == (int)UserType.Admin)
            {
                ReturnObj.CommissionHold = true;
            }

            if (!Company.CommissionHold.EqualsIgnoreCase("Yes"))
            {
                ReturnObj.CommissionHold = false;
            }

            if (Company.AutoBalanceTransfer.EqualsIgnoreCase("Yes"))
            {
                ReturnObj.AutoBalanceTransfer = true;
            }
            if (Company.BackDateFundTransfer.EqualsIgnoreCase("Yes"))
            {
                ReturnObj.BackDateFundTransfer = true;
            }

            if (Company.ServicewiseSubscription.EqualsIgnoreCase("Yes"))
            {
                ReturnObj.ServicewiseSubscription = true;
            }
            if (Company.IsPGSettlementHold.EqualsIgnoreCase("Yes"))
            {
                ReturnObj.IsPGSettlementHold = true;
            }

            string ServiceCharge = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsServiceCharge), "No");

            if (ServiceCharge.EqualsIgnoreCase("Yes"))
                ReturnObj.IsServiceCharge = true;

            string Voucher = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.Voucher), "No");

            if (Voucher.EqualsIgnoreCase("Yes"))
                ReturnObj.Voucher = true;

            string IsProviderPerformanceGraph = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsProviderPerformanceGraph), "No");

            if (IsProviderPerformanceGraph.EqualsIgnoreCase("Yes"))
                ReturnObj.IsProviderPerformanceGraph = true;

            string SignupReferral = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SignupReferral), "No");

            if (SignupReferral.EqualsIgnoreCase("Yes"))
                ReturnObj.IsSignupReferral = true;

            string IsMobikwikBBPS = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsMobikwikBBPS), "No");

            if (IsMobikwikBBPS.EqualsIgnoreCase("Yes"))
                ReturnObj.IsMobikwikBBPS = true;
            string IsFundRequestAmountEdit = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsFundRequestAmountEdit), "No");

            if (IsFundRequestAmountEdit.EqualsIgnoreCase("true"))
                ReturnObj.IsFundRequestAmountEdit = true;
            AddRefreshTokenToUser(ReturnObj);

            ReturnObj.SubscriptionDays = "0";

            List<string> whiteList = new List<string>(new string[] { "WithFAB", "WithoutFAB" });
            string AppDashboardType = "";
            AppDashboardType = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AppDashboardType), "WithFAB", whiteList);

            ReturnObj.AppDashboardType = AppDashboardType;
            int QuickSearchBackDays = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.QuickSearchBackDays), 7);
            ReturnObj.QuickSearchBackDays = QuickSearchBackDays;

            int DMTSenderNumberLength = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DMTSenderNumberLength), 10);
            ReturnObj.DMTSenderNumberLength = DMTSenderNumberLength;

            int CashFreeAiUserID = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.CashFreeAiUserID), 0);
            if (ReturnObj.UserID == CashFreeAiUserID)
                ReturnObj.IsCashFreeAiUser = true;
            string W2OWithSenderWallet = new SettingMasterBal().getW2OWithSenderWallet();
            if (W2OWithSenderWallet.EqualsIgnoreCase("Yes"))
                ReturnObj.IsW2OWithSenderWallet = true;

            if (Util.isRoleSuperDistOrDist(ReturnObj.UserTypeID))
            {
                List<string> whiteforFundReverseList = new List<string>(new string[] { "Yes", "No" });
                string FundReversalUpline = "";
                FundReversalUpline = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FundReversalForUpline), "No", whiteforFundReverseList);

                ReturnObj.FundReversalUpline = FundReversalUpline;
            }
        }

        private string GetWhatsappURL(string device)
        {
            string WhatsappNumber = new SettingMasterBal().SelectByKeyWithoutDefault(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SupportWhatsappNumber));

            string WhatsappURL = "";

            if (device.EqualsIgnoreCase(PlatformCode.Web))
            {
                WhatsappURL = "https://web.whatsapp.com/send?phone=91[Number]&text=Hi";
            }
            else
            {
                WhatsappURL = "whatsapp://send?phone=91[Number]&text=Hi";
            }

            WhatsappURL = WhatsappURL.Replace("[Number]", WhatsappNumber);

            return WhatsappURL;
        }

        private bool isOTPForLoginForFingerprint(int userID, string fingerPrint)
        {
            if (!new SettingMasterBal().SelectByKeyWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.OTPForNewDevice)))
                return false;

            bool b = IsTrustableDevice(userID, fingerPrint);
            return !b;
        }

        private bool isValid(UserMaster user, int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Client)
                return true;

            if (UserTypeID == (int)UserType.SuperDistributer)
                return true;

            if (UserTypeID == (int)UserType.Distributer)
                return true;

            if (user.IsInvoiceApplicable == true)
                return true;

            return false;
        }

        private bool isValidWithGST(string gstNo, int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Client)
                return true;

            if (UserTypeID == (int)UserType.SuperDistributer)
                return true;

            if (UserTypeID == (int)UserType.Distributer)
                return true;

            if (gstNo.Length > 5)
                return true;

            return false;
        }

        internal bool IsValidForInvoice(int userID, out string failureMessage)
        {
            failureMessage = UserMessage.InvoiceNotApplicable;
            UserMaster user = new UserDAL().SelectByIDFromCache(userID);
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (!isValid(user, UserTypeID))
                return false;

            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string InvoiceForNonGSTUser = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.InvoiceForNonGSTUser),
                                                                UserMessage.InvoiceForNonGSTUserDefaultValue, whiteList);

            if (InvoiceForNonGSTUser.EqualsIgnoreCase(InvoiceMessages.InvoiceForNonGSTUser))
            {
                failureMessage = StatusType.Ok;
                if (user.StateID != 1)
                    return true;
            }

            failureMessage = UserMessage.InvalidGST;

            if (!isValidWithGST(user.GSTNo, UserTypeID))
                return false;

            failureMessage = StatusType.Ok;
            return true;
        }

        internal UserMaster SelectByID(int userID)
        {
            UserMaster user = new UserDAL().SelectByIDFromCache(userID);
            return user;
        }

        internal UserMaster SelectByMobileFromCache(string MobileNumber)
        {
            UserMaster user = new UserDAL().SelectByMobileFromCache(MobileNumber);
            return user;
        }

        private void AddRefreshTokenToUser(LoginResponse ReturnObj)
        {
            RefreshTokenMaster refreshToken = null;
            try
            {
                refreshToken = new RefreshTokenDal().SelectByID(ReturnObj.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (refreshToken == null)
            {
                refreshToken = CreateRefreshToken();
                ReturnObj.RefreshToken = refreshToken.RefreshToken;
                ReturnObj.RefreshTokenExpiration = refreshToken.Expires;
                refreshToken.UserID = ReturnObj.UserID;
                refreshToken.IsActive = true;
                new RefreshTokenDal().Insert(refreshToken);
            }
            else
            {
                RefreshTokenMaster newToken = CreateRefreshToken();
                ReturnObj.RefreshToken = newToken.RefreshToken;
                ReturnObj.RefreshTokenExpiration = newToken.Expires;
                refreshToken.RefreshToken = newToken.RefreshToken;
                refreshToken.Expires = newToken.Expires;
                refreshToken.IsActive = true;
                new RefreshTokenDal().Update(refreshToken);
            }
        }

        internal LoginResponse RefreshJWTToken(KeyDataModel keyDataModel)
        {
            LoginResponse res = new LoginResponse
            {
                UserID = keyDataModel.UserID
            };
            int currentUserID = keyDataModel.UserID;

            string RefreshToken = keyDataModel.RefreshToken;

            RefreshTokenMaster ObjModel = new RefreshTokenDal().SelectByToken(RefreshToken, currentUserID);

            if (ObjModel == null) return res;

            DateTime currentDate = DateTime.Now;
            DateTime ExpiryDate = ObjModel.Expires;
            if (currentDate > ExpiryDate)
                return res;

            try
            {
                res = new UserDAL().SelectForLoginByUserID(currentUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            AddRefreshTokenToUser(res);
            return res;
        }

        private RefreshTokenMaster CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            return new RefreshTokenMaster
            {
                RefreshToken = Convert.ToBase64String(randomNumber),
                // Expires = DateTime.UtcNow.AddDays(10),
                Expires = DateTime.UtcNow.AddMinutes(2),
            };
        }

        public void AddToLoginHistory(int userID, string IPAddress, string device, string LoginMessage, string Latitude, string Longitude)
        {
            LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(IPAddress);

            if (locationLookUp == null)
            {
                locationLookUp = new LocationLookUp();
                locationLookUp.SimCardDetails = "0";
                locationLookUp.Location = $"{Latitude},{Longitude}";
            }

            LoginHistrory history = new LoginHistrory
            {
                UserID = userID,
                IPAddress = IPAddress,
                Token = "0",
                Through = device,
                LoginMessage = LoginMessage,
                Location = locationLookUp.Location.TrimIfNotNullWithDefault("0"),
                SimCardDetails = locationLookUp.SimCardDetails.TrimIfNotNullWithDefault("0")
            };

            Task.Run(() =>
            {
                int LoginHistoryTableID = new LoginHistoryDal().Insert(history);

                new LocationLookupBAL().InsertCore(LocationLookUpType.Login, Latitude, Longitude, IPAddress, LoginHistoryTableID);
            });
        }

        private string GetRedirectionByUserID(int userTypeID)
        {
            string redirectionPath = string.Empty;
            switch (userTypeID)
            {
                case (int)UserType.Admin:
                    redirectionPath = "/AdminDashboard";
                    break;

                case (int)UserType.SuperDistributer:
                    redirectionPath = "/SuperDistributorDashboard";
                    break;

                case (int)UserType.Distributer:
                    redirectionPath = "/DistributorDashboard";
                    break;

                case (int)UserType.Retailer:
                    redirectionPath = "/RetailerDashboard";
                    break;

                case (int)UserType.Support:
                    redirectionPath = "/Users/Profile";
                    break;

                case (int)UserType.APIUser:
                    redirectionPath = "/Daybook/Report";
                    break;

                case (int)UserType.Client:
                    redirectionPath = "/ClientDashboard";
                    break;

                case (int)UserType.Accountant:
                    redirectionPath = "/Fund/Report";
                    break;

                default:
                    redirectionPath = "/Users/Profile";
                    break;
            }
            return redirectionPath;
        }

        private bool IsTrustableDevice(int UserID, string Fingerprint)
        {
            TrustableDevice device = null;
            try
            {
                device = new TrustableDeviceDal().Select(UserID, Fingerprint);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (device == null)
                return false;

            return true;
        }

        public string IsCashCollection()
        {
            List<string> whiteList = new List<string>(new string[] { "true", "false" });

            string IsCashCollection = new SettingMasterBal().SelectByKeyWithDefaultValue(
                                                               Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsCashCollection),
                                                               "true",
                                                               whiteList);

            return IsCashCollection.ToLower();
        }
        public string IsVoucher()
        {
            List<string> whiteList = new List<string>(new string[] { "true", "false" });

            string IsVoucher = new SettingMasterBal().SelectByKeyWithDefaultValue(
                                                               Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.Voucher),
                                                               "true",
                                                               whiteList);

            return IsVoucher.ToLower();
        }

        public string IsUserParentChangeThenParentSlabSet()
        {
            string flag = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(
                                                               Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.UserParentChangeThenParentSlabSet),
                                                               "No");
            return flag.ToLower();

        }
        public string IsEkycUserProfileUpdate()
        {
            string flag = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(
                                                               Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsEkycUserProfileUpdate),
                                                               "Yes");
            return flag.ToLower();

        }
        public string IsUserListOnDashboard()
        {
            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });
            string flag = new SettingMasterBal().SelectByKeyWithDefaultValue(
                                                               Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsUserListOnDashboard),
                                                               "No", whiteList);
            return flag.ToLower();

        }

        public string IsTargetOnSale()
        {
            string TargetOnSale = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.TargetOnSale), "No");
            return TargetOnSale.ToLower();
        }

        public string IsShowCommission()
        {
            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string flag = new SettingMasterBal().SelectByKeyWithDefaultValue(
                                                               Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission),
                                                               "Yes",
                                                               whiteList);

            return flag.ToLower();
        }

        private string IsPin()
        {
            List<string> whiteList = new List<string>(new string[] { "true", "false" });

            string IsPin = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsPin),
                                                                              "true",
                                                                              whiteList);

            return IsPin.ToLower();
        }
        public string IsFundRequestAmountEdit()
        {
            string FundRequestAmountEdit = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsFundRequestAmountEdit), "No");
            return FundRequestAmountEdit.ToLower();
        }
        public string IsIServeUDMRAutoStatusCheck()
        {
            string IsIServeUDMRAutoStatusCheck = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsIServeUDMRAutoStatusCheck), "Yes");
            return IsIServeUDMRAutoStatusCheck.ToLower();
        }


        internal LoginResponse NewDeviceLogin(NewDeviceLoginRequest mainRequest, string IPAddress, out string failureMessage)
        {
            LoginResponse ReturnObj = null;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ReturnObj;

            mainRequest.Password = new Hashing().Hash(mainRequest.Password);
            try
            {
                string cashCollection = IsCashCollection();
                string pin = IsPin();
                string TargetOnSale = IsTargetOnSale();
                string isEkycUserProfileUpdate = IsEkycUserProfileUpdate();
                string isUserListOnDashboard = IsUserListOnDashboard();
                ReturnObj = new UserDAL().SelectForLogin(mainRequest.UserName, mainRequest.Password, mainRequest.Device, mainRequest.DeviceID, mainRequest.Pin, cashCollection, pin, TargetOnSale, isEkycUserProfileUpdate, isUserListOnDashboard);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return ReturnObj;
            }

            if (ReturnObj == null)
            {
                failureMessage = UserMessage.InvalidUser;
                return ReturnObj;
            }

            bool IsValidOTP = new OTPMasterBal().IsValid(ReturnObj.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.Login));

            LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(IPAddress);

            if (!IsValidOTP)
            {
                failureMessage = UserMessage.InvalidOTP;
                int LoginHistoryTableID = new LoginHistoryDal().Insert(new LoginHistrory
                {
                    Through = mainRequest.Device,
                    Token = "0",
                    UserID = ReturnObj.UserID,
                    SimCardDetails = locationLookUp.SimCardDetails.TrimIfNotNullWithDefault("0"),
                    IPAddress = IPAddress,
                    Location = locationLookUp.Location.TrimIfNotNullWithDefault("0"),
                    LoginMessage = failureMessage,
                });

                new LocationLookupBAL().InsertCore(LocationLookUpType.Login, "", "", IPAddress, LoginHistoryTableID);

                ReturnObj = new LoginResponse();
                return ReturnObj;
            }

            new TrustableDeviceDal().Insert(new TrustableDevice
            {
                IPAddress = IPAddress,
                UserID = ReturnObj.UserID,
                Device = mainRequest.Device,
                IsVerified = true,
                Browser = mainRequest.FingerPrint
            });
            bool isKYCApproved = IsKyc(ReturnObj.UserID);
            if (!isKYCApproved)
            {
                ReturnObj.RedirectTo = "/KYC/Upload";
                return ReturnObj;
            }
            ReturnObj.RedirectTo = GetRedirectionByUserID(ReturnObj.UserTypeID);

            if (mainRequest.Device.EqualsIgnoreCase(PlatformCode.Web))
            {
                ReturnObj.UISettings = new UISettingsBal().SelectAll();
            }

            UpdateFlagAfterLoginSuccess(mainRequest.Device, ReturnObj);

            ReturnObj.IsNewDevice = false;
            ReturnObj.IsOTPSent = false;
            failureMessage = StatusType.Ok;
            return ReturnObj;
        }

        internal int SelectUserIDByAPIToken(string mobileNumber, string aPIToken)
        {
            int Result;
            try
            {
                Result = new UserDAL().SelectUserIDByAPIToken(mobileNumber, aPIToken);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                Result = -1;
            }
            return Result;
        }

        private void setParnetUsingRole(RegistrationRequest registrationRequest)
        {
            try
            {
                if (registrationRequest.ParentID <= 0)
                {
                    UserMaster user = new UserDAL().SelectByIDFromCache(registrationRequest.JoiningByUserID);
                    if (user == null)
                    {
                        return;
                    }

                    if (user.UserTypeID == (int)UserType.Distributer ||
                        user.UserTypeID == (int)UserType.SuperDistributer ||
                        user.UserTypeID == (int)UserType.Support)
                    {
                        registrationRequest.ParentID = registrationRequest.JoiningByUserID;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal async Task<string> Insert(RegistrationRequest registrationRequest, string IPAddress)
        {
            string failureMessage = registrationRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (!IsValidKYC(registrationRequest.ParentID))
                return UserMessage.KYCNotApproved;

            if (!IsValidReferalOutsideRegistration(registrationRequest))
            {
                failureMessage = UserMessage.InvalidReferralOutsideSignUp;
                return failureMessage;
            }

            int ParentUserTypeID = new UserDAL().SelectUserTypeID(registrationRequest.ParentID);
            string IsUserIDPurchase = "No";
            if (ParentUserTypeID != (int)UserType.Admin)
            {
                IsUserIDPurchase = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.UserIDPurchase), "No");
                if (IsUserIDPurchase.EqualsIgnoreCase("Yes"))
                {
                    bool IsValid = new UserCreationWalletBal().IsValidForUserCreation(registrationRequest.JoiningByUserID);
                    if (!IsValid)
                        return UserMessage.InvalidUserCreate;
                }
            }

            registrationRequest.ParentID = new Util().GetDFOSOrSDFOSParentID(registrationRequest.JoiningByUserID, registrationRequest.ParentID);

            failureMessage = UserMessage.InvalidParent;
            if (registrationRequest.ParentID <= 0) return failureMessage;

            UserMaster referralparent = null;
            referralparent = new UserDAL().SelectByReferralCode(registrationRequest.ReferralCode);

            if (referralparent != null)
            {
                registrationRequest.JoiningByUserID = referralparent.UserID;
            }

            setParnetUsingRole(registrationRequest);

            if (registrationRequest.RegistrationType.EqualsIgnoreCase(RegistrationType.OutsideSignUp))
            {
                string IsAllowedOutsideSignUp = "";
                try
                {
                    IsAllowedOutsideSignUp = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.OutsideSignUp), "No");
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return failureMessage;
                }

                failureMessage = UserMessage.OutsideSignUpnotallowed;
                if (!IsAllowedOutsideSignUp.EqualsIgnoreCase(UserMessage.OutsideSignUpDefaultValue))
                    return failureMessage;
            }

            failureMessage = CheckIsChannelValidation(registrationRequest.JoiningByUserID, registrationRequest.IsChannel, registrationRequest.UserTypeID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = await DuplicateValidation(-99, registrationRequest.MobileNumber, registrationRequest.EmailID, "", registrationRequest.AliasName, registrationRequest.WhatsAppNumber);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            string Password = new Util().GetRandomPassword();
            string HashPassword = new Hashing().Hash(Password);

            if (registrationRequest.WhatsAppNumber.EqualsIgnoreCase(""))
                registrationRequest.WhatsAppNumber = registrationRequest.MobileNumber;

            string responseType = "";
            if (registrationRequest.UserTypeID == (int)UserType.APIUser)
                responseType = ResponseType.Callback.ToString();

            decimal DefaultCommissionOnTransferInPercentage = 0;
            if ((registrationRequest.UserTypeID == (int)UserType.Retailer ||
                registrationRequest.UserTypeID == (int)UserType.Distributer ||
                 registrationRequest.UserTypeID == (int)UserType.SuperDistributer)
                 && registrationRequest.RegistrationType.EqualsIgnoreCase(RegistrationType.Inside))
            {
                DefaultCommissionOnTransferInPercentage = new SettingMasterBal().SelectByKeyWithDecimalDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.CommissionOnTransferInPercentage), 0);

                if (DefaultCommissionOnTransferInPercentage < 0M)
                    DefaultCommissionOnTransferInPercentage = 0;
            }
            bool IsActive = true;
            if (Company.AutoEnableUser.EqualsIgnoreCase("No"))
                IsActive = false;

            int DefaultPayoutAccountCount = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DefaultPayoutAccountCount), 10);

            string IsSignUPOTP = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsSignUPOTP), "No");

            if (IsSignUPOTP.EqualsIgnoreCase("Yes"))
            {
                if (!IsValidOTP(registrationRequest.RegistrationType, registrationRequest.OTPMasterID, registrationRequest.OTP, registrationRequest.MobileNumber))
                    return Message.UserMessage.InvalidOTP;
            }

            UserMaster ObjModel = new UserMaster
            {
                AliasName = registrationRequest.AliasName,
                FirmName = registrationRequest.FirmName,
                PersonName = registrationRequest.PersonName,
                MobileNumber = registrationRequest.MobileNumber,
                EmailID = registrationRequest.EmailID,
                Password = HashPassword,
                GSTNo = registrationRequest.GSTNo,
                PANNo = registrationRequest.PANNo,
                AadharNo = registrationRequest.AadharNo.ToUpper(),
                UserTypeID = registrationRequest.UserTypeID,
                Address = registrationRequest.Address,
                StateID = registrationRequest.StateID,
                ParentID = registrationRequest.ParentID,
                JoiningByUserID = registrationRequest.JoiningByUserID,
                RegistrationType = registrationRequest.RegistrationType,
                IsActive = IsActive,
                IseKYC = registrationRequest.IseKYC,
                IsDelete = false,
                IsKYCApproved = false,
                IsInitialPassword = false,
                Mpin = Messages.DefaultMPin,
                WebFCMToken = "",
                AppFCMToken = "",
                UniqueReferenceNo = "".GetGUID().Replace("-", ""),
                IsWeb = true,
                IsApp = true,
                UpdatedDOC = DateTime.Now,
                IsIPSecure = true,
                AllowRealAPI = registrationRequest.AllowRealAPI,
                IsChannel = registrationRequest.IsChannel,
                ProfileImagePath = Messages.DefaultImagePath,
                IsTwoFA = registrationRequest.IsTwoFA,
                TwoFASecretKey = Guid.NewGuid().ToString("N"),
                CityName = registrationRequest.CityName,
                Pincode = registrationRequest.Pincode,
                ResponseType = responseType,
                PayoutAccountCount = DefaultPayoutAccountCount,
                WhatsAppNumber = registrationRequest.WhatsAppNumber,
                PaysprintAEPSAgentID = "".GetAlphaNumbericGUID(15),
                ICICI_Static_GUID = "".GetAlphaNumbericGUID(Messages.ICICI_Static_GUID_LENGETH),
                EkoAgentID = "",
                GSTHoldCutting = Messages.GSTMonthly,
                Comm = DefaultCommissionOnTransferInPercentage,
                EzyPayRetailerID = "".GetAlphaNumbericGUID(20),
                ReferralCode = "".GetAlphaNumbericGUID(6),
                ShopAddress = registrationRequest.ShopAddress,

            };
            int userID;
            try
            {
                userID = new UserDAL().Insert(ObjModel);
            }

            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return failureMessage;
            }
            if (userID <= 0)
            {
                failureMessage = Message.UserMessage.Registrationfailure;
                return failureMessage;
            }
            if (ParentUserTypeID != (int)UserType.Admin && IsUserIDPurchase.EqualsIgnoreCase("Yes"))
            {
                new UserCreationMasterBal().UpdateTransaction(registrationRequest.JoiningByUserID, userID, 1, IDPurchase.Consumed, IDPurchase.Debit);
            }
            if (!registrationRequest.ReferralCode.EqualsIgnoreCase(""))
            {
                ReferralUserInsert(registrationRequest, userID);
            }
            if (registrationRequest.UserServiceList == null)
            {
                registrationRequest.UserServiceList = new List<UserServiceMappingDetailList>();
            }
            //registrationRequest.UserServiceList.AddRange(Defaultlist);
            if (registrationRequest.RegistrationType.EqualsIgnoreCase(RegistrationType.Inside))
            {
                List<UserServiceMappingDetailList> DefaultList = new ServiceMasterDal().SelectDefaultServices();

                registrationRequest.UserServiceList.AddRange(DefaultList);
            }
            if (registrationRequest.RegistrationType.EqualsIgnoreCase(RegistrationType.OutsideSignUp))
            {
                //registrationRequest.UserServiceList.AddRange(Defaultlist);
                registrationRequest.UserServiceList = new ServiceMasterDal().SelectOutsideServices();
            }

            AssignSlabToUser(userID, registrationRequest.SlabID, registrationRequest.ParentID);

            if (registrationRequest.ChannelSlabMasterID > 0)
                AssignChannelSlabToUser(userID, registrationRequest.ChannelSlabMasterID, registrationRequest.ParentID, registrationRequest.JoiningByUserID);

            AddDefaultWalletsToUser(userID);


            AddUserServiceMapping(userID, registrationRequest.RegistrationType, registrationRequest.UserServiceList);

            await Task.Run(() => new MessageSelection().SendSignupMessage(userID, Password, ObjModel.MobileNumber, ObjModel.WhatsAppNumber, ObjModel.FirmName, ObjModel.EmailID, ObjModel.Mpin.ToString()));

            await Task.Run(() => new WebHookProcessing().SignupWebHook(userID));

            await Task.Run(() => new MessageSelection().SendSignupMailToAdmin(ObjModel.MobileNumber, ObjModel.FirmName));

            await Task.Run(() => new FCMSelection().SendSignUpMessageToAdmin(ObjModel.MobileNumber, ObjModel.FirmName));

            await Task.Run(() => AddDefaultVirtualAccountNumberAndAliasName(userID));

            new LocationLookupBAL().InsertCore(LocationLookUpType.Signup, "", "", IPAddress, userID);

            if (registrationRequest.UserTypeID == (int)UserType.APIUser)
                await Task.Run(() => new SlabBAL().GenerateSlabForAPIUser(registrationRequest.FirmName, userID));

            failureMessage = StatusType.Ok;
            return failureMessage;


        }

        private bool IsValidOTP(string RegistrationType, int OTPMasterID, string OTP, string Mobile)
        {
            if (RegistrationType.EqualsIgnoreCase("Registration"))
                return true;

            if (Company.IsCreditInstitution.EqualsIgnoreCase("Yes"))
                return true;

            OTPMaster otpMaster = new OTPMaster();
            try
            {
                otpMaster = new OTPMasterDal().SelectOtp(OTPMasterID, OTP, Mobile);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            if (otpMaster == null)
                return false;

            if (!otpMaster.OTP.EqualsIgnoreCase(OTP))
                return false;

            int Result;

            otpMaster.IsVerified = true;

            try
            {
                Result = new OTPMasterDal().Update(otpMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return true;

        }
        private bool IsValidKYC(int UserID)
        {
            if (Company.AdminUserID == UserID)
                return true;
            bool isKYCApproved = new UserBal().IsKyc(UserID);
            string KYCForAllService = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.KYCForAllService), "Yes");
            if (isKYCApproved == false && KYCForAllService.EqualsIgnoreCase("Yes"))
                return false;
            return true;
        }

        private string CheckIsChannelValidation(int JoiningByUserID, bool IsChannel, int UserTypeID)
        {
            if (IsChannel == true && JoiningByUserID == Company.AdminUserID)
            {
                if (UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Retailer)
                    return UserMessage.InvalidChannelUser;
            }
            return StatusType.Ok;
        }

        private void AddDefaultVirtualAccountNumberAndAliasName(int userID)
        {
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (ObjModel == null)
                return;

            if (ObjModel.AliasName == null || ObjModel.AliasName.Length <= 0)
                ObjModel.AliasName = DefaultAliasName(ObjModel.UserTypeID, ObjModel.UserID);

            if (ObjModel.VirtualAccountNumber == null || ObjModel.VirtualAccountNumber.Length <= 0)
                ObjModel.VirtualAccountNumber = DefaultVirtualAccountNumber(ObjModel.MobileNumber);

            try
            {
                new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        private void ReferralUserInsert(RegistrationRequest registrationRequest, int userID)
        {
            UserMaster referralparent = null;
            referralparent = new UserDAL().SelectByReferralCode(registrationRequest.ReferralCode);
            if (referralparent == null) return;

            ReferralRegistration ObjModel = new ReferralRegistration
            {
                UserID = userID,
                CreatedByUserID = referralparent.UserID,
                IsBenefitUtilize = false,
                ReferralCode = registrationRequest.ReferralCode,
                MobileNumber = referralparent.MobileNumber,
                ReferralAmount = referralparent.ReferralAmount,
            };
            new ReferralRegistrationDal().Insert(ObjModel);
        }

        private void AssignSlabToUser(int userID, int slabID, int ParentID)
        {
            Project.Requests.UserSlabMapping.InsertRequest insertRequest = new Requests.UserSlabMapping.InsertRequest();

            insertRequest.UserID = userID;
            insertRequest.SlabID = slabID;
            if (ParentID == Company.AdminUserID && slabID <= 0)
                insertRequest.SlabID = Company.DefaultSlabID;
            try
            {
                new UserSlabMappingBal().Insert(insertRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void AssignChannelSlabToUser(int userID, int ChannelSlabMasterID, int ParentID, int JoiningByUserID)
        {
            Project.Requests.UserChannelSlabMapping.InsertRequest insertRequest = new Requests.UserChannelSlabMapping.InsertRequest();
            if (ParentID != JoiningByUserID)
                return;
            insertRequest.UserID = userID;
            insertRequest.ChannelSlabMasterID = ChannelSlabMasterID;
            try
            {
                new UserChannelSlabMappingBal().Insert(insertRequest, ParentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal async Task<string> Update(Requests.User.UpdateRequest updateRequest, int AdminID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = await DuplicateValidation(updateRequest.UserID, updateRequest.MobileNumber, updateRequest.EmailID, updateRequest.VirtualAccountNumber, "", updateRequest.WhatsAppNumber);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            int UserTypeID = new UserDAL().SelectUserTypeID(AdminID);
            failureMessage = Message.UserMessage.UserNotFound;
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByID(updateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return failureMessage;

            failureMessage = Message.UserMessage.UpdateFail;

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                ObjModel.EmailID = updateRequest.EmailID;
                ObjModel.MobileNumber = updateRequest.MobileNumber;
                ObjModel.WhatsAppNumber = updateRequest.WhatsAppNumber;
                ObjModel.WalletIDforEcollection = updateRequest.WalletIDforEcollection;
                ObjModel.StaticQRWalletID = updateRequest.StaticQRWalletID;
                ObjModel.AliasName = updateRequest.AliasName;
                ObjModel.PayoutAccountCount = updateRequest.PayoutAccountCount;
                if (updateRequest.AliasName.Length <= 0)
                    ObjModel.AliasName = DefaultAliasName(updateRequest.UserTypeID, updateRequest.UserID);
            }

            ObjModel.FirmName = updateRequest.FirmName;
            ObjModel.PersonName = updateRequest.PersonName;

            if (UserTypeID != (int)UserType.Distributer)
            {
                ObjModel.ParentID = updateRequest.ParentID;
            }
            ObjModel.UserTypeID = updateRequest.UserTypeID;
            ObjModel.Address = updateRequest.Address;
            ObjModel.ShopAddress = updateRequest.ShopAddress;
            ObjModel.StateID = updateRequest.StateID;
            ObjModel.GSTNo = updateRequest.GSTNo;
            ObjModel.PANNo = updateRequest.PANNo;
            ObjModel.AadharNo = updateRequest.AadharNo;
            ObjModel.TargetAmount = updateRequest.TargetAmount;
            ObjModel.AllowRealAPI = updateRequest.AllowRealAPI;
            ObjModel.IsChannel = updateRequest.IsChannel;
            ObjModel.VirtualAccountNumber = updateRequest.VirtualAccountNumber;
            if (updateRequest.VirtualAccountNumber.Length <= 0)
                ObjModel.VirtualAccountNumber = DefaultVirtualAccountNumber(updateRequest.MobileNumber);

            ObjModel.IsTwoFA = updateRequest.IsTwoFA;
            ObjModel.CityName = updateRequest.CityName;
            ObjModel.Pincode = updateRequest.Pincode;
            ObjModel.ResponseType = updateRequest.ResponseType;

            int UserID = -1;
            try
            {
                UserID = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (UserID <= 0)
                return failureMessage;


            string isParentSlab = IsUserParentChangeThenParentSlabSet();
            if (isParentSlab.EqualsIgnoreCase("Yes"))
            {

                Project.Response.Slab.UserSlabMappingViewModel userSlab = new UserSlabMappingDal().SelectByUserID(UserID);

                if (userSlab != null)
                {
                    Project.Requests.UserSlabMapping.SelectRequest deleteRequest = new Project.Requests.UserSlabMapping.SelectRequest();
                    deleteRequest.UserSlabMappingID = userSlab.UserSlabMappingID;
                    failureMessage = new UserSlabMappingBal().Delete(deleteRequest);
                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    {
                        return failureMessage;
                    }
                }
            }

            //AddUserServiceMapping(UserID, ObjModel.RegistrationType, updateRequest.UserServiceList);
            await UpdateUserServiceMapping(UserID, ObjModel.RegistrationType, updateRequest.UserServiceList);

            UpdateMaxOutstanding(UserID, updateRequest.MaxOutstanding);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        private async Task UpdateUserServiceMapping(int userID, string registrationType, List<UserServiceMappingDetailList> userServiceList)
        {
            UserServiceMappingDal dal = new UserServiceMappingDal();
            try
            {
                await dal.DeactiveAllServiceByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            userServiceList = userServiceList.GroupBy(x => x.ServiceID).Select(x => x.FirstOrDefault()).ToList();
            UserServiceMappingDal mappingDal = new UserServiceMappingDal();

            List<UserServiceMapping> newMappings = new List<UserServiceMapping>();
            List<UserServiceMapping> updatedMappings = new List<UserServiceMapping>();

            foreach (var service in userServiceList)
            {
                UserServiceMapping mapping = new UserServiceMappingDal().SelectByUserIDAndServiceID(userID, service.ServiceID);

                if (mapping == null)
                {
                    mapping = new UserServiceMapping()
                    {
                        UserID = userID,
                        ServiceID = service.ServiceID,
                        IsActive = true
                    };
                    newMappings.Add(mapping);
                }

                if (mapping != null)
                {
                    mapping.IsActive = true;
                    updatedMappings.Add(mapping);
                }
            }

            if (newMappings.Count > 0)
            {
                await new UserServiceMappingDal().BulkInsert(newMappings);
            }

            if (updatedMappings.Count > 0)
            {
                await new UserServiceMappingDal().BulkUpdate(updatedMappings);
            }
        }

        private string DefaultVirtualAccountNumber(string MobileNumber)
        {
            string VirtualAccountNumber = "";
            VirtualAccountMaster ObjModel = null;
            try
            {
                ObjModel = new VirtualAccountMasterDal().SelectBySettingID(1);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return VirtualAccountNumber;

            if (ObjModel.Prefix == null) return VirtualAccountNumber;

            if (ObjModel.Prefix.Length >= 3)
                VirtualAccountNumber = $"{ObjModel.Prefix.Trim()}{MobileNumber.Trim()}";

            return VirtualAccountNumber.Trim();
        }

        private string DefaultAliasName(int UserTypeID, int UserID)
        {
            string AliasName = "";
            UserTypeMaster ObjtypeModel = null;
            try
            {
                ObjtypeModel = new UserTypeDAL().SelectByID(UserTypeID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex); return AliasName;
            }
            if (ObjtypeModel.AliasName == null) return AliasName;

            if (ObjtypeModel.AliasName.Length > 0)
                AliasName = $"{ObjtypeModel.AliasName.Trim()}{UserID.ToString().Trim()}";

            return AliasName.Trim();
        }

        private void UpdateMaxOutstanding(int UserID, decimal MaxOutstanding)
        {
            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(UserID);
            if (ObjModel == null)
            {
                new CashCollectionBal().InsertCashCollectionMaster(UserID);
                ObjModel = new CashCollectionMasterDal().SelectByUserID(UserID);
            }

            ObjModel.MaxOutstanding = MaxOutstanding;
            try
            {
                new CashCollectionMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void UpdateResponseType(int UserID, string ResponseType)
        {
            UserMaster ObjModel = new UserBal().SelectByID(UserID);

            ObjModel.ResponseType = ResponseType;
            try
            {
                new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal string UpdateUserCommission(UpdateUserCommissionRequest updateRequest, int userID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(updateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (user == null)
                return failureMessage;

            int ParentUserTypeID = new UserDAL().SelectUserTypeID(user.ParentID);
            if (ParentUserTypeID != (int)UserType.Admin)
            {
                SelectUserCommissionByUserIDResponse parentCommission = SelectCommissionByUserID(new SelectUserCommissionByUserIDRequest
                {
                    UserID = user.ParentID
                }, out failureMessage);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;

                if (parentCommission.Comm < updateRequest.Comm && parentCommission.CommissionCategory == updateRequest.CommissionCategory)
                {
                    failureMessage = "Commission must be less than parents commission (" + parentCommission.Comm.ToString() + ")";
                    return failureMessage;
                }
            }

            user.Comm = updateRequest.Comm;
            user.CommissionCategory = updateRequest.CommissionCategory;

            int UserID = -1;
            try
            {
                UserID = new UserDAL().Update(user);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            failureMessage = Message.UserMessage.UpdateFail;
            if (UserID <= 0)
                return failureMessage;
            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string Activate(ActivateRequest userActivateRequest)
        {
            string failureMessage = userActivateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(userActivateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsActive = userActivateRequest.IsActive;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
            {
                return Message.UserMessage.UpdateFail;
            }
            try
            {
                new WrongPasswordHistoryDal().DeleteWrongPasswordHistory(userActivateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (userActivateRequest.IsActive)
                Task.Run(() => new MessageSelection().SendActivateMessage(ObjModel));

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string Deactivate(int UserID)
        {
            string failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsActive = false;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
            {
                return Message.UserMessage.UpdateFail;
            }

            Task.Run(() => new MessageSelection().SendDeactivateMessage(ObjModel));

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string LoginOTPActivate(LoginOTPActivateRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsLoginOTP = mainRequest.IsLoginOTP;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0) return Message.UserMessage.UpdateFail;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string CommissionHoldActivate(ActivateRequest userActivateRequest)
        {
            string failureMessage = userActivateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(userActivateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsCommissionHold = userActivateRequest.IsCommissionHold;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0) return Message.UserMessage.UpdateFail;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal async Task<string> ForgotPassword(ForgotPasswordRequest forgotPasswordRequest, string IPAddress)
        {
            string failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;
            try
            {
                userMaster = await new UserDAL().SelectUserByMobileNumber(forgotPasswordRequest.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (userMaster == null) return failureMessage;

            try
            {
                failureMessage = Message.UserMessage.ForgotPassLimitExceed;
                int ForgotPasscount = 0;

                ForgotPasscount = await new PasswordChangeHistoryDal().SelectForgotPassCount(userMaster.UserID, SecurityParam.ForgotPassword);

                int ForgotPasswordLimit = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ForgotPasswordCount), 5);
                if (ForgotPasscount >= ForgotPasswordLimit)
                    return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            string Password = new Util().GetRandomPassword();
            string HashPassword = new Hashing().Hash(Password);
            userMaster.Password = HashPassword;

            failureMessage = Message.UserMessage.ForgotPasswordFailure;
            int Result;
            try
            {
                Result = new UserDAL().Update(userMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return failureMessage;

            new MessageSelection().ForgetPasswordMessage(Password, userMaster.UserID, forgotPasswordRequest.MobileNumber, userMaster.WhatsAppNumber, userMaster.EmailID, userMaster.FirmName);

            try
            {
                PasswordChangeHistory passwordhistory = new PasswordChangeHistory();
                passwordhistory.UserID = userMaster.UserID;
                passwordhistory.IPAddress = IPAddress;
                passwordhistory.Type = SecurityParam.ForgotPassword;
                new PasswordChangeHistoryDal().Insert(passwordhistory);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            new LocationLookupBAL().InsertCore(LocationLookUpType.ForgotPassword, "", "", IPAddress, userMaster.UserID);

            failureMessage = StatusType.Ok;

            return failureMessage;
        }

        internal async Task<string> ForgotPasswordGenerateOTP(ForgotPasswordRequest forgotPasswordRequest, string IPAddress)
        {
            string failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;
            try
            {
                userMaster = await new UserDAL().SelectUserByMobileNumber(forgotPasswordRequest.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (userMaster == null) return failureMessage;

            string AadharValidationFlag = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ForgotPasswordAadharPANValidation), "No");
            if (AadharValidationFlag.EqualsIgnoreCase("Yes"))
            {
                if (!userMaster.PANNo.EqualsIgnoreCase(forgotPasswordRequest.PANNo)) return Message.UserMessage.InvalidPan;
            }

            if (!userMaster.IsActive)
            {
                return UserMessage.DeactivatedUser;
            }
            new MessageSelection().SendForgotPasswordOTP(userMaster);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal async Task<string> VerifyForgotPasswordOTP(ForgotPasswordOTPRequest mainRequest, string IPAddress)
        {
            string failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;
            try
            {
                userMaster = await new UserDAL().SelectUserByMobileNumber(mainRequest.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (userMaster == null) return failureMessage;

            bool IsValidOTP = new OTPMasterBal().IsValid(userMaster.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.ForgotPasswordOTP));
            if (!IsValidOTP)
                return UserMessage.InvalidOTP;
            try
            {
                failureMessage = Message.UserMessage.ForgotPassLimitExceed;
                int ForgotPasscount = 0;

                ForgotPasscount = await new PasswordChangeHistoryDal().SelectForgotPassCount(userMaster.UserID, SecurityParam.ForgotPassword);

                int ForgotPasswordLimit = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ForgotPasswordCount), 5);
                if (ForgotPasscount >= ForgotPasswordLimit)
                    return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            string Password = new Util().GetRandomPassword();
            string HashPassword = new Hashing().Hash(Password);
            userMaster.Password = HashPassword;
            userMaster.IsInitialPassword = true;

            failureMessage = Message.UserMessage.ForgotPasswordFailure;
            int Result;
            try
            {
                Result = new UserDAL().Update(userMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return failureMessage;

            new MessageSelection().ForgetPasswordMessage(Password, userMaster.UserID, mainRequest.MobileNumber, userMaster.WhatsAppNumber, userMaster.EmailID, userMaster.FirmName);

            try
            {
                PasswordChangeHistory passwordhistory = new PasswordChangeHistory();
                passwordhistory.UserID = userMaster.UserID;
                passwordhistory.IPAddress = IPAddress;
                passwordhistory.Type = SecurityParam.ForgotPassword;
                new PasswordChangeHistoryDal().Insert(passwordhistory);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            new LocationLookupBAL().InsertCore(LocationLookUpType.ForgotPassword, "", "", IPAddress, userMaster.UserID);

            failureMessage = StatusType.Ok;

            return failureMessage;
        }





        private void AddDefaultWalletsToUser(int userID)
        {
            List<WalletMaster> walletList = new WalletMasterDal().Select();

            int DefaultCappingAmount = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DefaultCappingAmount), 0);

            foreach (WalletMaster wallet in walletList)
            {
                decimal zeroWithPrecision = 0.00000M;
                UserWalletMaster uw = new UserWalletMaster
                {
                    UserID = userID,
                    WalletID = wallet.WalletID,
                    Balance = zeroWithPrecision,
                    BalanceHash = new UserWalletMasterBal().HashBalance(userID, wallet.WalletID, zeroWithPrecision),
                };
                new UserWalletMasterDal().Insert(uw);

                if (DefaultCappingAmount >= 1)
                {
                    UserBalanceCapping ObjModel = new UserBalanceCapping
                    {
                        WalletID = wallet.WalletID,
                        Amount = Convert.ToInt32(DefaultCappingAmount),
                        UserID = userID,
                    };
                    new UserBalanceCappingDal().Insert(ObjModel);
                }
            }

            new CashCollectionBal().InsertCashCollectionMaster(userID);
        }

        private void AddUserServiceMapping(int userID, string Type, List<UserServiceMappingDetailList> UserServiceList)
        {
            UserServiceMappingDal dal = new UserServiceMappingDal();
            try
            {
                dal.Delete(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserServiceList = UserServiceList.GroupBy(x => x.ServiceID).Select(x => x.FirstOrDefault()).ToList();

            Parallel.ForEach(UserServiceList, async (service) =>
            {
                int result = await new UserServiceMappingDal().Insert(new UserServiceMapping()
                {
                    UserID = userID,
                    ServiceID = service.ServiceID,
                    IsActive = true
                });
            });
        }

        internal void AddServiceMapping(int userID, List<SubscriptionDetailResponse> UserServiceList)
        {
            bool hasBBPS = UserServiceList.Any(s => s.ServiceID == -999);
            if (hasBBPS)
            {
                // Remove the -999 entry
                UserServiceList = UserServiceList.Where(s => s.ServiceID != -999).ToList();

                List<Response.Service.ViewModel> bbpsServices = new ServiceMasterDal().SelectBBPSServices();

                // Convert bbpsServices to SubscriptionDetailResponse objects
                List<SubscriptionDetailResponse> convertedBbpsServices = bbpsServices.Select(s => new SubscriptionDetailResponse
                {
                    ServiceID = s.ServiceID,
                    ServiceName = s.ServiceName
                }).ToList();

                UserServiceList.AddRange(convertedBbpsServices);
            }
            // Remove duplicates
            UserServiceList = UserServiceList.GroupBy(x => x.ServiceID)
                                             .Select(x => x.FirstOrDefault())
                                             .ToList();
            // Insert services
            Parallel.ForEach(UserServiceList, (service) =>
            {
                new UserServiceMappingDal().Insert(new UserServiceMapping()
                {
                    UserID = userID,
                    ServiceID = service.ServiceID,
                    IsActive = true
                });
            });
        }

        private void AddUserServiceMappingBulkUpdate(int userID, List<UserServiceMappingDetailList> userServiceList)
        {
            try
            {
                userServiceList = userServiceList.GroupBy(x => x.ServiceID).Select(x => x.FirstOrDefault()).ToList();
                List<UserServiceMapping> newMappings = new List<UserServiceMapping>();
                List<UserServiceMapping> updatedMappings = new List<UserServiceMapping>();

                foreach (var service in userServiceList)
                {
                    UserServiceMapping mapping = new UserServiceMappingDal().SelectByUserIDAndServiceID(userID, service.ServiceID);

                    if (mapping == null)
                    {
                        mapping = new UserServiceMapping()
                        {
                            UserID = userID,
                            ServiceID = service.ServiceID,
                            IsActive = true
                        };
                        newMappings.Add(mapping);
                    }

                    if (mapping != null)
                    {
                        mapping.IsActive = true;
                        updatedMappings.Add(mapping);
                    }
                }

                if (newMappings.Count > 0)
                {
                    var Taskinsert = new UserServiceMappingDal().BulkInsert(newMappings);
                    Taskinsert.Wait();
                }

                if (updatedMappings.Count > 0)
                {
                    var Taskupdate = new UserServiceMappingDal().BulkUpdate(updatedMappings);
                    Taskupdate.Wait();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        private void RemoveUserServiceMapping(int UserID, List<UserServiceMappingDetailList> UserServiceList)
        {
            try
            {
                UserServiceList = UserServiceList.GroupBy(x => x.ServiceID).Select(x => x.FirstOrDefault()).ToList();

                Parallel.ForEach(UserServiceList, async (service) =>
                {
                    new UserServiceMappingDal().DeActiveByServiceIDAndUserID(UserID, service.ServiceID);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private async Task<string> CheckDuplicateFirmName(int CurrentUserID, string firmName)
        {
            try
            {
                int UserID = await new UserDAL().SelectUserIDByFirmName(firmName);
                if (UserID > 0 && UserID != CurrentUserID)
                    return Message.UserMessage.DuplicateFirmName;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        private async Task<string> CheckDuplicateVirtualAccountNumber(int CurrentUserID, string VirtualAccountNumber)
        {
            try
            {
                if (VirtualAccountNumber.Length <= 0)
                {
                    return StatusType.Ok;
                }

                int UserID = await new UserDAL().SelectUserIDByVirtualAccountNumber(VirtualAccountNumber);
                if (UserID > 0 && UserID != CurrentUserID)
                    return Message.UserMessage.DuplicateVirtualAccountNumber;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        private async Task<string> CheckDuplicateAliasName(string AliasName)
        {
            try
            {
                if (AliasName.Length <= 0)
                {
                    return StatusType.Ok;
                }

                int UserID = await new UserDAL().SelectUserIDByAliasName(AliasName);
                if (UserID > 0)
                    return Message.UserMessage.DuplicateAliasName;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        private async Task<string> CheckDuplicateMobileNumber(int CurrentUserID, string mobileNumber)
        {
            try
            {
                int UserID = await new UserDAL().SelectUserIDByMobileNumber(mobileNumber);
                if (UserID > 0 && UserID != CurrentUserID)
                    return Message.UserMessage.DuplicateMobileNumber;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }
        private async Task<string> CheckDuplicateWhatsAppNumber(int CurrentUserID, string WhatsAppNumber)
        {
            try
            {
                int UserID = await new UserDAL().SelectUserIDByWhatsAppNumber(WhatsAppNumber);
                if (UserID > 0 && UserID != CurrentUserID)
                    return Message.UserMessage.DuplicateWhatsAppNumber;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        private async Task<string> CheckDuplicateEmailID(int CurrentUserID, string emailID)
        {
            int UserID = 0;
            try
            {
                UserID = await new UserDAL().SelectUserIDByEmailID(emailID);
                if (UserID > 0 && UserID != CurrentUserID)
                    return Message.UserMessage.DuplicateEmailID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            try
            {
                bool IsDuplicate = IsDuplicateCleanemailID(UserID, emailID);
                if (IsDuplicate)
                {
                    return Message.UserMessage.DuplicateEmailID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        private string getCleanedEmailID(string emailID)
        {
            string CleanemailID = emailID.Replace(" ", String.Empty);
            CleanemailID = Regex.Replace(CleanemailID, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
            CleanemailID = CleanemailID.Replace("_", "");
            CleanemailID = CleanemailID.Replace(".", "");
            return CleanemailID;
        }

        internal bool IsDuplicateCleanemailID(int userID, string emailID)
        {
            List<UserEmailIDViewModel> allEmails = null;
            string CleanemailID = "";
            try
            {
                CleanemailID = getCleanedEmailID(emailID);

                allEmails = new UserDAL().SelectAllEmailIDs();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }
            if (allEmails == null) return true;

            try
            {
                foreach (UserEmailIDViewModel DBEmailID in allEmails)
                {
                    if (userID == DBEmailID.UserID)
                    {
                        continue;
                    }

                    string EmailID = getCleanedEmailID(DBEmailID.EmailID);

                    if (CleanemailID.EqualsIgnoreCase(EmailID))
                        return true;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            return false;
        }

        private async Task<string> DuplicateValidation(int userID, string mobileNumber, string emailID, string VirtualAccountNumber, string AliasName, string WhatsAppNumber)
        {
            //2. Mobile Number validation
            var DuplicateMobileNumber = CheckDuplicateMobileNumber(userID, mobileNumber);
            //3. EmailID validation
            var DuplicateEmailID = CheckDuplicateEmailID(userID, emailID);
            //4. Virtual Account Number validation
            var DuplicateVirtualAccountNumber = CheckDuplicateVirtualAccountNumber(userID, VirtualAccountNumber);
            //5.AliasName validation
            var DuplicateAliasName = CheckDuplicateAliasName(AliasName);
            //6. WhatsAppNumber Number validation
            var DuplicateWhatsAppNumber = CheckDuplicateWhatsAppNumber(userID, WhatsAppNumber);
            await Task.WhenAll(DuplicateMobileNumber, DuplicateEmailID, DuplicateVirtualAccountNumber, DuplicateAliasName);

            string mobileMessage = await DuplicateMobileNumber;
            string WhatsAppNumberMessage = await DuplicateWhatsAppNumber;
            string emailMessage = await DuplicateEmailID;
            string virtualAccountMessage = await DuplicateVirtualAccountNumber;
            string AliasNameMessage = await DuplicateAliasName;

            if (!mobileMessage.EqualsIgnoreCase(StatusType.Ok))
                return mobileMessage;

            if (!WhatsAppNumberMessage.EqualsIgnoreCase(StatusType.Ok))
                return WhatsAppNumberMessage;

            if (!emailMessage.EqualsIgnoreCase(StatusType.Ok))
                return emailMessage;

            if (!virtualAccountMessage.EqualsIgnoreCase(StatusType.Ok))
                return virtualAccountMessage;

            if (!AliasNameMessage.EqualsIgnoreCase(StatusType.Ok))
                return AliasNameMessage;

            return StatusType.Ok;
        }

        internal List<UserViewModel> SelectByType(UserTypeSelectRequest selectByTypeRequest, out string failureMessage)
        {
            List<UserViewModel> ReturnResponse = null;

            failureMessage = selectByTypeRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ReturnResponse;

            try
            {
                ReturnResponse = new UserDAL().SelectByType(selectByTypeRequest.UserTypeID);
            }
            catch (Exception ex) { new Logger().Write(ex); }

            return ReturnResponse;
        }

        internal List<UserViewModel> SelectUpline(UplineSelectRequest selectUplineRequest, int UserID, out string failureMessage)
        {
            List<UserViewModel> ReturnResponse = null;
            failureMessage = selectUplineRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ReturnResponse;

            UserMaster user = null;
            int UserTypeID = 0;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);

                if (user == null)
                {
                    failureMessage = Messages.DataNotFound;
                    return ReturnResponse;
                }

                UserTypeID = user.UserTypeID;

                if (UserTypeID == (int)UserType.APIUser)
                {
                    failureMessage = Messages.InvalidUserInput;
                    return ReturnResponse;
                }
                if (UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
                {
                    return ReturnResponse = new UserDAL().SelectUplineByUserID(UserID);
                }

                ReturnResponse = new UserDAL().SelectUpline(selectUplineRequest.UserTypeID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ReturnResponse == null)
            {
                failureMessage = Messages.DataNotFound;
                return ReturnResponse;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                ReturnResponse = (from item in ReturnResponse
                                  where item.ParentID == UserID ||
                                  item.UserID == UserID
                                  select item).ToList();
            }

            if (UserTypeID != (int)UserType.Admin && UserTypeID != (int)UserType.Support && UserTypeID != (int)UserType.SuperDistributer)
            {
                ReturnResponse = new List<UserViewModel>();
                UserViewModel item = new UserViewModel();
                item.UserID = UserID;
                item.FirmName = user.FirmName;
                ReturnResponse.Add(item);
            }

            failureMessage = StatusType.Ok;
            return ReturnResponse;
        }

        internal MemberDetailResponseWrapper SelectAll(SelectAllRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            MemberDetailResponseWrapper resonse = null;
            UserMaster ObjModel = null;

            List<MemberDetailResponse> members = null;
            failureMessage = "Members not found.";
            if (mainRequest.UserTypeID <= -1)
                return resonse;

            try
            {
                members = new UserDAL().SelectAllByParentID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return resonse;
            }

            if (members.Count <= 0)
                return resonse;

            if (mainRequest.UserTypeID != 0 && mainRequest.UserTypeID != 999)
                members = members.Where(m => m.UserTypeID == mainRequest.UserTypeID).ToList();

            if (mainRequest.UserTypeID == 999)
                members = members.Where(m => m.IsActive == false).ToList();

            if (mainRequest.CriteriaText.Length >= 1)
                members = members.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            if (mainRequest.Criteria.EqualsIgnoreCase("Date"))
            {
                int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
                int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
                members = members.Where(P => P.DOCInt >= From && P.DOCInt <= To).ToList();
            }
            if (!mainRequest.CriteriaText.EqualsIgnoreCase(""))
            {
                if (mainRequest.Criteria.EqualsIgnoreCase("Name"))
                {
                    members = members.Where(P => P.FirmName.Contains(mainRequest.CriteriaText)).ToList();
                }
                if (mainRequest.Criteria.EqualsIgnoreCase("AliasName"))
                {
                    members = members.Where(P => P.AliasName.Contains(mainRequest.CriteriaText)).ToList();
                }
                if (mainRequest.Criteria.EqualsIgnoreCase("PANNo"))
                {
                    members = members.Where(P => P.PANNo.Contains(mainRequest.CriteriaText)).ToList();
                }
                if (mainRequest.Criteria.EqualsIgnoreCase("CityName"))
                {
                    members = members.Where(P => P.CityName.Contains(mainRequest.CriteriaText)).ToList();
                }
            }

            if (!mainRequest.IsTextValue)
            {
                if (mainRequest.Criteria.EqualsIgnoreCase("IsCommissionHold"))
                {
                    members = members.Where(P => P.IsCommissionHold.Equals(mainRequest.CriteriaValue)).ToList();

                }
            }

            if (mainRequest.IsExcelDownload)
            {
                mainRequest.Search = "";
                members = members.Where(P => P.IsActive).ToList();
            }
            if (mainRequest.Search.Trim().Length >= 3)
            {
                members = members.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            }


            if (!mainRequest.CriteriaNumber.EqualsIgnoreCase(""))
            {
                if (mainRequest.Criteria.EqualsIgnoreCase("MobileNumber"))
                {
                    members = members.Where(P => P.MobileNumber.Contains(mainRequest.CriteriaNumber)).ToList();
                }

                if (mainRequest.Criteria.EqualsIgnoreCase("ParentMobile"))
                {
                    members = members.Where(P => P.ParentMobileNumber.Contains(mainRequest.CriteriaNumber)).ToList();
                }

                if (mainRequest.Criteria.EqualsIgnoreCase("AadharNo"))
                {
                    members = members.Where(P => P.AadharNo.Contains(mainRequest.CriteriaNumber)).ToList();
                }

                if (mainRequest.Criteria.EqualsIgnoreCase("Pincode"))
                {
                    members = members.Where(P => P.Pincode.Contains(mainRequest.CriteriaNumber)).ToList();
                }

            }
            int count = members.Count();
            members = members.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            if (!mainRequest.IsExcelDownload && mainRequest.Search.Trim().Length <= 3)
            {
                members = members.AsQueryable().Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 };

            string cashCollection = new UserBal().IsCashCollection();
            List<Model.CashCollectionMaster> outstandingList = new List<CashCollectionMaster>();
            if (cashCollection.EqualsIgnoreCase("true"))
            {
                outstandingList = new CashCollectionMasterDal().Select();
            }

            Parallel.ForEach(members, options, member =>
            {
                member.Outstanding = 0;

                if (cashCollection.EqualsIgnoreCase("true"))
                {
                    Model.CashCollectionMaster cashItem = outstandingList
                        .Where(item => item.UserID == member.UserID)
                        .FirstOrDefault();

                    if (cashItem != null)
                    {
                        member.Outstanding = (float)System.Math.Round(cashItem.Outstanding, 2);
                    }
                }
                member.SlabName = new UserBal().SelectSlabName(member.UserID);

                string IsKYCApprovedStr = new UserBal().SelectKYCStatus(member.IsKYCApproved, member.IseKYC, member.UserID);
                member.IsKYCApprovedStr = IsKYCApprovedStr;

                member.Details = $"{member.FirmName}{Environment.NewLine} KYC : {IsKYCApprovedStr}";

                List<DetailViewModel> wallets = new UserWalletMasterDal().SelectByUserID(member.UserID);

                foreach (DetailViewModel item in wallets)
                {
                    if (item.WalletID == (int)WalletCode.Recharge)
                    {
                        member.RechargeBalance = (float)System.Math.Round(item.Balance);
                        member.Balances = $"{item.ShortName}:{member.RechargeBalance}";
                    }
                    if (item.WalletID == (int)WalletCode.DMT)
                    {
                        member.DMTBalance = (float)System.Math.Round(item.Balance);
                        member.Balances = $"{member.Balances} {Environment.NewLine}{item.ShortName} :{member.DMTBalance}";
                    }
                    if (item.WalletID == (int)WalletCode.AEPS)
                    {
                        member.AEPSBalance = (float)System.Math.Round(item.Balance);
                        member.Balances = $"{member.Balances} {Environment.NewLine}{item.ShortName} :{member.AEPSBalance}";
                    }
                }

                List<CappingViewModel> capping = new UserBalanceCappingDal().SelectBalanceCapping(member.UserID);
                foreach (CappingViewModel item in capping)
                {
                    if (item.WalletID == (int)WalletCode.Recharge)
                    {
                        member.CappRechargeBalance = (float)System.Math.Round(item.Amount);
                        member.BalanceCappings = $"{item.ShortName}:{member.CappRechargeBalance}";
                    }
                    if (item.WalletID == (int)WalletCode.DMT)
                    {
                        member.CappDMTBalance = (float)System.Math.Round(item.Amount);
                        member.BalanceCappings = $"{member.BalanceCappings} {Environment.NewLine}{item.ShortName} :{member.CappDMTBalance}";
                    }
                    if (item.WalletID == (int)WalletCode.AEPS)
                    {
                        member.CappAEPSBalance = (float)System.Math.Round(item.Amount);
                        member.BalanceCappings = $"{member.BalanceCappings} {Environment.NewLine}{item.ShortName} :{member.CappAEPSBalance}";
                    }
                }
            });
            List<Headerlist> headerlist = new UserDAL().SelectTotalBalanceSummary(mainRequest.UserTypeID, keyDataModel.UserID);

            resonse = new MemberDetailResponseWrapper
            {
                IsExcelDownload = mainRequest.IsExcelDownload,
                Count = count,
                data = members,
                headerlist = headerlist
            };

            failureMessage = StatusType.Ok;
            return resonse;
        }


        internal MemberDetailResponseWrapper SelectAgent(SelectAllRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            MemberDetailResponseWrapper resonse = null;
            UserMaster ObjModel = null;

            List<AgentDetailResponseBBPS> members = null;
            failureMessage = "Members not found.";
            if (mainRequest.UserTypeID <= -1)
                return resonse;

            try
            {
                members = new UserDAL().SelectAgent(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return resonse;
            }

            if (members.Count <= 0)
                return resonse;

            if (mainRequest.UserTypeID != 0 && mainRequest.UserTypeID != 999)
                members = members.Where(m => m.UserTypeID == mainRequest.UserTypeID).ToList();


            if (mainRequest.IsExcelDownload)
            {
                mainRequest.Search = "";
                members = members.Where(P => P.IsActive).ToList();
            }
            if (mainRequest.Search.Trim().Length >= 3)
            {
                members = members.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            }


            int count = members.Count();
            members = members.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            if (!mainRequest.IsExcelDownload && mainRequest.Search.Trim().Length <= 3)
            {
                members = members.AsQueryable().Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }


            resonse = new MemberDetailResponseWrapper
            {
                IsExcelDownload = mainRequest.IsExcelDownload,
                Count = count,
                dataResBBPS = members,
            };

            failureMessage = StatusType.Ok;
            return resonse;
        }
        internal MemberDetailResponseWrapper SelectAgentForMATM(SelectAllRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            MemberDetailResponseWrapper resonse = null;
            UserMaster ObjModel = null;

            List<AgentDetailResponse> members = null;
            failureMessage = "Members not found.";
            if (mainRequest.UserTypeID <= -1)
                return resonse;

            try
            {
                members = new UserDAL().SelectAgentForMATM(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return resonse;
            }

            if (members.Count <= 0)
                return resonse;

            if (mainRequest.UserTypeID != 0 && mainRequest.UserTypeID != 999)
                members = members.Where(m => m.UserTypeID == mainRequest.UserTypeID).ToList();


            if (mainRequest.IsExcelDownload)
            {
                mainRequest.Search = "";
                members = members.Where(P => P.IsActive).ToList();
            }
            if (mainRequest.Search.Trim().Length >= 3)
            {
                members = members.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            }

            int count = members.Count();
            members = members.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            if (!mainRequest.IsExcelDownload && mainRequest.Search.Trim().Length <= 3)
            {
                members = members.AsQueryable().Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            resonse = new MemberDetailResponseWrapper
            {
                IsExcelDownload = mainRequest.IsExcelDownload,
                Count = count,
                dataRes = members,
            };

            failureMessage = StatusType.Ok;
            return resonse;
        }

        private string SelectKYCStatus(bool isKYCApproved, bool IseKYC, int UserID)
        {
            string KYCStatus = "No Document";

            if (IsValidManualKycAndeKycBoth())
            {
                if (!isKYCApproved || !IseKYC)
                    return KYCStatus;
            }
            if (IseKYC)
                return "eKYCApproved";

            if (isKYCApproved)
                return "Approved";

            List<Response.KYC.ViewModel> list = null;
            try
            {
                list = new UserDocumentMasterDal().SelectDocumentsByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (list == null || list.Count <= 0) return KYCStatus;

            foreach (Response.KYC.ViewModel item in list)
            {
                if (item.Status.EqualsIgnoreCase(StatusType.Reject))
                {
                    return KYCStatus = "Rejected";
                }

                KYCStatus = "Pending";
            }

            return KYCStatus;
        }

        private bool CheckRemainingBalanceForUser(int UserID)
        {
            List<DetailViewModel> wallets = new UserWalletMasterDal().SelectByUserID(UserID);

            foreach (DetailViewModel item in wallets)
            {
                if (item.Balance > 1)
                    return false;
            }
            return true;
        }

        internal string Delete(DeleteRequest deleteUserRequest)
        {
            string failureMessage = deleteUserRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(deleteUserRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            if (CheckRemainingBalanceForUser(ObjModel.UserID) == false)
            {
                return UserMessage.ReverseWalletBal;
            }

            ObjModel.IsDelete = true;
            ObjModel.IsActive = false;
            ObjModel.UpdatedDOC = DateTime.Now;
            ObjModel.MobileNumber = ObjModel.MobileNumber + "_Deleted";
            ObjModel.EmailID = ObjModel.EmailID + "_Deleted";

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)

                return Message.UserMessage.DeleteFail;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string ReKyc(DeleteRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            failureMessage = Message.UserMessage.UserNotFound;

            string result = new UserDocumentMasterBal().ReKyc(mainRequest.UserID);

            if (!result.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserMaster ObjModel;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsKYCApproved = false;
            ObjModel.IseKYC = false;
            ObjModel.UpdatedDOC = DateTime.Now;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
                return Message.UserMessage.RekycFail;

            try
            {
                new eKYCMasterBal().Delete(mainRequest.UserID);
                new eKYCLogBal().Delete(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal FetchDetailsForPincodeResponse FetchDetailsForPincode(FetchDetailsForPincodeRequest mainRequest, out string failureMessage)
        {
            FetchDetailsForPincodeResponse response = new FetchDetailsForPincodeResponse();

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            try
            {
                string Url = "https://api.postalpincode.in/pincode/[Pincode]";

                Url = Url.Replace("[Pincode]", mainRequest.Pincode);

                string res = HttpProcessing.CallGetMethod(Url);

                if (res == null)
                {
                    failureMessage = "Invalid PinCode";
                    return response;
                }

                if (res.Length <= 35)
                {
                    failureMessage = "Invalid PinCode";
                    return response;
                }

                List<PincodeAPIResponse> apiRespnse = JsonConvert.DeserializeObject<List<PincodeAPIResponse>>(res);
                if (apiRespnse == null || apiRespnse.Count <= 0)
                {
                    failureMessage = "Invalid PinCode";
                    return response;
                }

                if (apiRespnse[0].Status.EqualsIgnoreCase("Error"))
                {
                    failureMessage = apiRespnse[0].Message;
                    return response;
                }

                if (apiRespnse[0].PostOffice == null || apiRespnse[0].PostOffice.Count <= 0)
                {
                    failureMessage = "Invalid PinCode";
                    return response;
                }

                response.State = apiRespnse[0].PostOffice[0].State;
                response.CityName = apiRespnse[0].PostOffice[0].Block + ", " + apiRespnse[0].PostOffice[0].Name;
                response.District = apiRespnse[0].PostOffice[0].District;
                response.Circle = apiRespnse[0].PostOffice[0].Circle;
                response.Division = apiRespnse[0].PostOffice[0].Division;
                List<StateMaster> ReturnList = new List<StateMaster>();
                try
                {
                    ReturnList = new StateDAL().Select();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                response.StateID = 0;
                foreach (StateMaster state in ReturnList)
                {
                    if (response.State.Contains(state.Name))
                    {
                        response.StateID = state.ID;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = "Invalid PinCode";
                return response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal MemberDetailResponse SelectDetailByUserID(SelectRequest selectByIDRequest, out string failureMessage)
        {
            failureMessage = selectByIDRequest.IsValidModalState();

            MemberDetailResponse member = null;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return member;
            try
            {
                member = new UserDAL().SelectDetailByUserID(selectByIDRequest.UserID);
                if (member == null)
                {
                    failureMessage = "Data not found";
                    return member;
                }

                member.UserServiceList = new UserServiceMappingDal().SelectByUserID(selectByIDRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Message.UserMessage.UserNotFound;
                return member;
            }

            bool IsChannel = new UserDAL().SelectIsChannel(selectByIDRequest.UserID);
            if (IsChannel)
                member.IsChannel = IsChannel;

            SelectByUserIDResponse returnresponse = null;
            try
            {
                returnresponse = new CashCollectionBal().SelectByUserID(selectByIDRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (returnresponse == null)
                return member;

            member.MaxOutstanding = returnresponse.MaxOutStanding;

            failureMessage = StatusType.Ok;
            return member;
        }

        internal ProfileResponse SelectProfile(int UserID, out string failureMessage)
        {
            ProfileResponse member = null; failureMessage = UserMessage.InvalidUser;
            try
            {
                member = new UserDAL().SelectProfile(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Message.UserMessage.UserNotFound;
                return member;
            }
            if (member == null)
                return member;

            member.KYCStatus = StatusType.Success;
            if (!member.IsKYCApproved)
            {
                member.KYCStatus = StatusType.Process;
                List<Project.Response.KYC.ViewModel> list = null;
                try
                {
                    list = new UserDocumentMasterDal().SelectDocumentsByUserID(UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (list == null || list.Count <= 0)
                {
                    member.KYCStatus = "Not uploaded";
                }
            }

            if (member.GSTNo.Length < 5)
                member.IsInvoiceApplicable = false;

            ParentViewModel parent = new UserDAL().SelectParent(UserID);
            if (member != null)
            {
                member.ParentID = parent.ParentID;
                member.ParentFirmName = parent.FirmName;
                member.ParentMobileNumber = parent.MobileNumber;
                member.ParentRole = parent.ParentRole;

                MemberDetailResponse details = new UserDAL().SelectDetailByUserID(parent.ParentID);
                if (details != null)
                    member.ParentRole = details.UserType;
            }
            bool IsChannel = new UserDAL().SelectIsChannel(UserID);
            if (IsChannel)
                member.IsChannel = IsChannel;
            member.SlabName = new UserBal().SelectSlabName(UserID);

            if (member.ReferralURL.Length >= 5)
            {
                member.ReferralURL = Company.UIURL + "Signup/" + member.ReferralURL;
            }
            else { member.ReferralURL = ""; }

            failureMessage = StatusType.Ok;
            return member;
        }

        internal ParentViewModel SelectParent(int UserID)
        {
            ParentViewModel returnList = new ParentViewModel();
            try
            {
                returnList = new UserDAL().SelectParent(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return returnList;
        }

        internal List<DownlineForDropDownResponse> SelectDownlineForDropDown(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;

            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
            }

            try
            {
                list = new UserDAL().SelectDownlineForDropDown(userid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();

                if (mainRequest.UserTypeID > 0)
                {
                    list = (from item in list
                            where item.UserTypeID == mainRequest.UserTypeID
                            select item).ToList();
                }
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineForDropDownResponse> SelectDownlineForLedger(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;

            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
            }

            try
            {
                list = new UserDAL().SelectDownlineForLedger(userid, usertypeid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();

                if (mainRequest.UserTypeID > 0)
                {
                    list = (from item in list
                            where item.UserTypeID == mainRequest.UserTypeID
                            select item).ToList();
                }
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineForDropDownResponse> SelectDownlinForFundTransfer(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;
            bool IsDFOSOrSDFOS = false;
            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
                IsDFOSOrSDFOS = true;
            }
            try
            {
                list = new UserDAL().SelectDownlinForFundTransfer(userid, IsDFOSOrSDFOS);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineForDropDownResponse> SelectDirectDownline(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;

            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
            }

            try
            {
                list = new UserDAL().SelectDirectDownline(userid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();

                if (mainRequest.UserTypeID > 0)
                {
                    list = (from item in list
                            where item.UserTypeID == mainRequest.UserTypeID
                            select item).ToList();
                }
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineForDropDownResponse> SelectDirectDownlineForUserBulk(KeyDataModel keyDataModel, SelectUserForBulkUpdateRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;

            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
            }

            try
            {
                list = new UserDAL().SelectDirectDownlineForUserBulk(userid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (mainRequest.UserTypeID > 0)
                {
                    list = (from item in list
                            where item.UserTypeID == mainRequest.UserTypeID
                            select item).ToList();
                }
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineForDropDownResponse> SelectDownlineWithOutStanding(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;

            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
            }
            try
            {
                list = new UserDAL().SelectDownlineWithOutStanding(userid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();

                if (mainRequest.UserTypeID > 0)
                {
                    list = (from item in list
                            where item.UserTypeID == mainRequest.UserTypeID
                            select item).ToList();
                }
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineWithBalanceResponse> SelectDownlinWithBalances(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineWithBalanceResponse> resultList = null;

            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(userid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;

            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
            }

            List<DownlineForDropDownResponse> list = null;
            try
            {
                list = new UserDAL().SelectDownlineForDropDownExceptClientRole(userid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return resultList;
            }

            if (list == null)
                return resultList;

            if (list.Count <= 0)
                return resultList;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }

            resultList = new List<DownlineWithBalanceResponse>();

            string IsCashCollection = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsCashCollection));

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(list, parallelOptions, mainItem =>
            {
                // foreach (DownlineForDropDownResponse mainItem in list)            {
                DownlineWithBalanceResponse obj = new DownlineWithBalanceResponse();
                obj.Role = mainItem.UserType;
                obj.UserID = mainItem.UserID;
                obj.FirmName = mainItem.FirmName;
                obj.MobileNumber = new UserDAL().SelectMobileNumber(obj.UserID);
                obj.Balances = "";

                string bal = "";

                List<DetailViewModel> wallets = new UserWalletMasterDal().SelectByUserID(mainItem.UserID);

                foreach (DetailViewModel item in wallets)
                {
                    bal += item.ShortName + ":" + Math.Round(item.Balance, 2) + " ";

                    if (item.WalletID == (int)WalletCode.Recharge)
                    {
                        obj.RechargeBalance = (float)System.Math.Round(item.Balance, 2);
                    }
                    if (item.WalletID == (int)WalletCode.DMT)
                    {
                        obj.DMTBalance = (float)System.Math.Round(item.Balance, 2);
                    }
                    if (item.WalletID == (int)WalletCode.AEPS)
                    {
                        obj.AEPSBalance = (float)System.Math.Round(item.Balance, 2);
                    }
                }

                obj.Outstanding = 0;
                if (IsCashCollection.EqualsIgnoreCase("true"))
                {
                    obj.Outstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(mainItem.UserID);
                }

                obj.Balances = bal;
                resultList.Add(obj);
            });

            resultList = resultList.OrderBy(x => x.UserID).ToList();

            failureMessage = StatusType.Ok;
            return resultList;
        }

        internal List<DownlineForDropDownResponse> SelectUserForDropDown(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            int usertypeid = 0;
            int userid = keyDataModel.UserID;
            try
            {
                usertypeid = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertypeid == (int)UserType.Support)
                userid = Company.AdminUserID;

            if (Util.isDFOSOrSDFOS(usertypeid))
            {
                userid = new UserDAL().SelectParentID(keyDataModel.UserID);
            }

            try
            {
                list = new UserDAL().SelectUserForDropDown(userid);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineForDropDownResponse> SelectUserForDropDownForCashCollection(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";
            int UserTypeID = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            if (UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
            {
                int ParentID = new UserDAL().SelectParentID(keyDataModel.UserID);
                keyDataModel.UserID = ParentID;
            }
            try
            {
                list = new UserDAL().SelectUserForDropDownForCashCollection(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;
            if (keyDataModel.UserID != Company.AdminUserID)
            {
                DownlineForDropDownResponse item = new DownlineForDropDownResponse();
                item.UserID = keyDataModel.UserID;
                item.FirmName = "Select Own";
                list.Insert(0, item);
                if (mainRequest != null)
                {
                    if (!string.IsNullOrEmpty(mainRequest.FirmName))
                        list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
                }
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal string UpdatePassword(UpdatePasswordRequest updateRequest, int UserID, string IPAddress)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (updateRequest.NewPassword == updateRequest.OldPassword)
                return UserMessage.ShouldDifferentNewPassword;

            if (!new Validation().IsComplexPassword(updateRequest.NewPassword))
            {
                return UserMessage.ComplexPass;
            }

            if (updateRequest.NewPassword != updateRequest.ConfirmPassword)
                return UserMessage.InvalidConfirmPass;

            if (updateRequest.MobileNumber != null)
            {
                if (updateRequest.MobileNumber.Length >= 1)
                    UserID = new UserDAL().SelectOnlyUserIDByMobileNumber(updateRequest.MobileNumber);
            }
            if (UserID <= 0)
                return UserMessage.InvalidUser;

            UserMaster ObjModel = null;
            try
            {
                updateRequest.OldPassword = new Hashing().Hash(updateRequest.OldPassword);
                ObjModel = new UserDAL().SelectByIDWithPassword(UserID, updateRequest.OldPassword);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return UserMessage.InvalidOldPass;

            ObjModel.Password = new Hashing().Hash(updateRequest.NewPassword);
            ObjModel.IsInitialPassword = false;

            int Result = -1;
            try
            {
                Result = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return Message.UserMessage.UpdatepassFail;

            Parallel.Invoke(
               () =>
               {
                   new MessageSelection().ChangePasswordMessage(updateRequest.UserID, ObjModel.MobileNumber, ObjModel.WhatsAppNumber, ObjModel.EmailID, ObjModel.FirmName);
               }
                ,
                () =>
                {
                    insertPasswordChangeHistoryWithLocaion(UserID, IPAddress, SecurityParam.ChangePassword);
                }

            );

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal SelectUserCommissionByUserIDResponse SelectCommissionByUserID(SelectUserCommissionByUserIDRequest selectRequest, out string failureMessage)
        {
            SelectUserCommissionByUserIDResponse response = null;

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            try
            {
                response = new UserDAL().SelectUserCommissionByUserID(selectRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;

            return response;
        }

        internal string SendPassword(SendPasswordRequest mainRequest, int userID, out string failureMessage)
        {
            failureMessage = "Send request with admin login";

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return failureMessage;

            string Password = new Util().GetRandomPassword();
            string HashPassword = new Hashing().Hash(Password);

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (user == null)
                return failureMessage;

            user.Password = HashPassword;
            int Result = -1;
            try
            {
                Result = new UserDAL().Update(user);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.UserMessage.UserNotFound;
            if (Result <= 0)
                return failureMessage;

            new MessageSelection().SendPasswordMessage(Password, user.UserID, user.MobileNumber, user.WhatsAppNumber, user.EmailID, user.FirmName);

            failureMessage = StatusType.Ok;
            return Message.UserMessage.UpdatePasswordSuccess;
        }

        internal List<ParentViewModel> SelectParentWithAdmin(int UserID)
        {
            List<ParentViewModel> returnList = new List<ParentViewModel>();
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            if (user == null)
                return returnList;

            try
            {
                ParentViewModel parent = new UserDAL().SelectParent(UserID);

                returnList.Add(parent);

                bool IsFundRequestUplineWithAdmin = new SettingMasterBal().GetFundRequestUplineWithAdmin();
                bool IsFundRequestUplineWithDFOSOrSDFOS = new SettingMasterBal().GetFundRequestUplineDFOSOrSDFOS();

                if (IsFundRequestUplineWithAdmin)
                {
                    ParentViewModel admin = new UserDAL().SelectAdmin();

                    if (parent.ParentID != admin.ParentID)
                    {
                        returnList.Add(admin);
                    }

                }
                if (IsFundRequestUplineWithDFOSOrSDFOS && user.UserTypeID != (int)UserType.SDFOS && user.UserTypeID != (int)UserType.DFOS)
                {
                    List<ParentViewModel> DFOSlist = new UserDAL().SelectDFOSOrSDFOSByParentID(parent.ParentID);
                    foreach (ParentViewModel item in DFOSlist)
                    {
                        returnList.Add(item);
                    }

                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return returnList;
        }

        internal List<DownlineForDropDownResponse> SelectSupportUserForDropDown(out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            try
            {
                list = new UserDAL().SelectSupportUserForDropDown();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        internal string IsWebActivate(ActivateRequest activateRequest)
        {
            string failureMessage = activateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(activateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsWeb = activateRequest.IsActive;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
            {
                return Message.UserMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal DownlineDetailResponseWrapper SelectDownlineReport(SelectDownlineReportRequest mainRequest, out string failureMessage)
        {
            DownlineDetailResponseWrapper wraperresonse = null;
            List<SelectDownlineReportResponse> response = new List<SelectDownlineReportResponse>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wraperresonse;
            try
            {
                response = new UserDAL().SelectDownlineReport(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = UserMessage.UserNotFound;
            if (response.Count <= 0)
            {
                return wraperresonse;
            }
            decimal RCH = 0;
            decimal DMT = 0;
            decimal AEPS = 0;
            decimal Outstand = 0;
            List<Headerlist> headerlistdata = new List<Headerlist>();

            foreach (SelectDownlineReportResponse item in response)
            {
                Outstand += item.Outstanding;
                foreach (UserWallet wallet in item.Wallets)
                {
                    if (wallet.WalletID == (int)WalletCode.Recharge)
                    {
                        item.RechargeBalance += wallet.Balance;
                        RCH += wallet.Balance;
                    }
                    if (wallet.WalletID == (int)WalletCode.DMT)
                    {
                        item.DMTBalance += wallet.Balance;
                        DMT += wallet.Balance;
                    }
                    if (wallet.WalletID == (int)WalletCode.AEPS)
                    {
                        item.AEPSBalance += wallet.Balance;
                        AEPS += wallet.Balance;
                    }
                }
            }

            List<WalletMaster> wallets = new WalletMasterDal().Select();
            foreach (WalletMaster item in wallets)
            {
                if (item.WalletID == (int)WalletCode.Recharge)
                {
                    headerlistdata.Add(new Headerlist
                    {
                        name = item.ShortName,
                        amount = RCH
                    });
                }

                if (item.WalletID == (int)WalletCode.DMT)
                {
                    headerlistdata.Add(new Headerlist
                    {
                        name = item.ShortName,
                        amount = DMT
                    });
                }

                if (item.WalletID == (int)WalletCode.AEPS)
                {
                    headerlistdata.Add(new Headerlist
                    {
                        name = item.ShortName,
                        amount = AEPS
                    });
                }
            }

            headerlistdata.Add(new Headerlist
            {
                name = "TotalBal",
                amount = RCH + DMT + AEPS
            });

            string IsCashCollection = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsCashCollection));
            if (IsCashCollection.EqualsIgnoreCase("true"))
            {
                headerlistdata.Add(new Headerlist
                {
                    name = "OutstandBal",
                    amount = Outstand
                });
            }
            wraperresonse = new DownlineDetailResponseWrapper()
            {
                data = response,
                IsExcelDownload = mainRequest.IsExcelDownload,
                headerlist = headerlistdata
            };

            failureMessage = StatusType.Ok;
            return wraperresonse;
        }

        internal string IsInvoiceActivate(ActivateRequest activateRequest)
        {
            string failureMessage = activateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(activateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsInvoiceApplicable = activateRequest.IsActive;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
            {
                return Message.UserMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string IsAppActivate(ActivateRequest activateRequest)
        {
            string failureMessage = activateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(activateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsApp = activateRequest.IsActive;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
            {
                return Message.UserMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<UserSummaryDashboardWrapper> SelectUserBalanceSummaryForAdminDashborad(int UserID, string Date)
        {
            List<UserSummaryDashboardWrapper> usersummary = new List<UserSummaryDashboardWrapper>();
            try
            {
                if (UserID != Company.AdminUserID)
                {
                    return usersummary;
                }
                List<UserTypeWithCount> usertypes = new List<UserTypeWithCount>();
                usertypes = new UserDAL().SelectUserTypeWithCount();

                if (usertypes == null)
                {
                    return usersummary;
                }

                int Totalcount = usertypes
                         .Sum(x => x.Count);

                foreach (UserTypeWithCount item in usertypes)
                {
                    List<WalletView> WalletList = new List<WalletView>();
                    UserSummaryDashboardWrapper response = new UserSummaryDashboardWrapper();
                    WalletList = new UserDAL().SelectWalletBalanceUserTypewise(item.UserTypeID, Date);

                    response.UserType = item.UserType;
                    response.Count = item.Count;
                    response.WalletList = WalletList;

                    usersummary.Add(response);
                }

                List<WalletMaster> Activewallet = new WalletMasterDal().Select();

                List<WalletView> WalletList1 = new List<WalletView>();
                foreach (var item in Activewallet.Select((value, i) => new { i, value }))
                {
                    WalletView WList1 = new WalletView();

                    var value = item.value;
                    var index = item.i;
                    WList1.WalletName = value.Name;
                    WList1.WalletBalance = usersummary
                         .Sum(x => x.WalletList[index].WalletBalance);

                    WalletList1.Add(WList1);
                }

                UserSummaryDashboardWrapper responsetotal = new UserSummaryDashboardWrapper();
                responsetotal.UserType = "Total";
                responsetotal.Count = Totalcount;
                responsetotal.WalletList = WalletList1;
                usersummary.Add(responsetotal);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return usersummary;
        }

        internal void UpdateFCMToken(FCMViewModel mainRequest, KeyDataModel keyDataModel)
        {
            FCMViewModel userFCM = null;
            try
            {
                userFCM = new UserDAL().SelectFCMTokenByUserID(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (userFCM == null)
                return;

            string webToken = mainRequest.WebFCMToken;
            string appToken = mainRequest.AppFCMToken;

            if (string.IsNullOrEmpty(mainRequest.WebFCMToken) && string.IsNullOrEmpty(mainRequest.AppFCMToken))
            {
                return;
            }

            if (string.IsNullOrEmpty(mainRequest.WebFCMToken))
            {
                webToken = userFCM.WebFCMToken;
            }

            if (string.IsNullOrEmpty(mainRequest.AppFCMToken))
            {
                appToken = userFCM.AppFCMToken;
            }

            if (!string.IsNullOrEmpty(mainRequest.WebFCMToken))
            {
                if (userFCM.WebFCMToken.EqualsIgnoreCase(mainRequest.WebFCMToken))
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(mainRequest.AppFCMToken))
            {
                if (userFCM.AppFCMToken.EqualsIgnoreCase(mainRequest.AppFCMToken))
                {
                    return;
                }
            }

            try
            {
                UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
                if (user == null)
                    return;

                user.WebFCMToken = webToken;
                user.AppFCMToken = appToken;

                Task.Run(() =>
                {
                    new UserDAL().Update(user);
                }
                );
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal string TokenGeneration(TokenRequest mainRequest, KeyDataModel keyDataModel)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int UserTypeID = new UserDAL().SelectUserTypeID(keyDataModel.UserID);

            if (UserTypeID != (int)UserType.APIUser)
            {
                return UserMessage.InvalidUserType;
            }

            bool IsValidOTP = new OTPMasterBal().IsValid(keyDataModel.UserID, mainRequest.OTP, Enum.GetName(typeof(StaticModel.OTPType), StaticModel.OTPType.APIToken));

            if (!IsValidOTP)
            {
                return Messages.InvalidOTP;
            }
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return UserMessage.UserNotFound;

            ObjModel.APIToken = Guid.NewGuid().ToString("N").Substring(0, 20);
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new UserDAL().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return UserMessage.TokenFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal TokenResponse SelectAPIToken(int UserID, out string failureMessage)
        {
            TokenResponse response = new TokenResponse();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID != (int)UserType.APIUser)
            {
                failureMessage = UserMessage.InvalidUserType;
                return response;
            }
            try
            {
                response.APIToken = new UserDAL().SelectAPIToken(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null)
            {
                failureMessage = UserMessage.TokenNotFound;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal ProfileUploadResponse ProfileUpload(ProfileUploadRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            ProfileUploadResponse response = new ProfileUploadResponse();
            response.Message = UserMessage.UploadFail;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            failureMessage = UserMessage.UploadFail;
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return response;
            }
            string path = "";
            try
            {
                path = new MediaProcessing().SaveImage(mainRequest.Document, "ProfileImage", out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;
            if (string.IsNullOrEmpty(path))
            {
                return response;
            }
            ObjModel.ProfileImagePath = path;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new UserDAL().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            response.Message = UserMessage.UploadSuccess;
            response.ProfileImagePath = String.Concat(Company.WebAPIURL, path);
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string UpdateIsSecureIP(UpdateIsSecureIPRequest mainRequest, int UserID, out string failureMessage)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            failureMessage = UserMessage.InvalidUser;
            if (UserTypeID != (int)UserType.APIUser)
                return failureMessage;

            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return UserMessage.UserNotFound;
            }
            failureMessage = UserMessage.UserNotFound;
            if (user == null)
                return failureMessage;

            if (!mainRequest.IsIPSecure)
            {
                bool IsValidOTP = new OTPMasterBal().IsValid(UserID, mainRequest.OTP, Enum.GetName(typeof(StaticModel.OTPType), StaticModel.OTPType.SecureIPOTP));

                failureMessage = Messages.InvalidOTP;
                if (!IsValidOTP) return failureMessage;
            }
            user.IsIPSecure = mainRequest.IsIPSecure;
            int Result = -1;
            failureMessage = UserMessage.UpdateFail;
            try
            {
                Result = new UserDAL().Update(user);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return "";
            }
            if (Result <= 0)
                return failureMessage;
            failureMessage = StatusType.Ok;
            return UserMessage.UpdateSuccess;
        }

        internal SelectIsSecureIPResponse SelectIsSecureIP(int UserID, out string failureMessage)
        {
            SelectIsSecureIPResponse viewMdel = null;

            failureMessage = UserMessage.InvalidUser;
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID != (int)UserType.APIUser)
                return viewMdel;

            failureMessage = UserMessage.UserNotFound;
            UserMaster user;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewMdel;
            }
            failureMessage = UserMessage.UserNotFound;
            if (user == null)
                return viewMdel;

            viewMdel = new SelectIsSecureIPResponse()
            {
                IsIPSecure = user.IsIPSecure
            };

            failureMessage = StatusType.Ok;
            return viewMdel;
        }

        internal List<DownlineForDropDownResponse> SelectChannelUserForDropDown(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            try
            {
                list = new UserDAL().SelectChannelUserForDropDown(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<ParentViewModel> SelectParentWithAdminForCallMeRequest(int UserID)
        {
            List<ParentViewModel> returnList = new List<ParentViewModel>();
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            if (user == null)
                return returnList;
            try
            {
                ParentViewModel adminModel = new UserDAL().SelectAdmin();
                returnList.Add(adminModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user.ParentID == Company.AdminUserID)
                return returnList;

            try
            {
                ParentViewModel parent = new UserDAL().SelectParent(UserID);
                returnList.Add(parent);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return returnList;
        }

        private string SelectSlabName(int UserID)
        {
            bool IsChannel = new UserDAL().SelectIsChannel(UserID);

            if (IsChannel)
            {
                Response.UserChannelSlabMapping.ViewModel UserSlabMapping = new UserChannelSlabMappingDal().SelectSlabNameByUserID(UserID);
                if (UserSlabMapping == null)
                    return DefaultSlab.Default + " (Channel)";
                return UserSlabMapping.SlabName + " (Channel)";
            }
            else
            {
                Response.Slab.UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingDal().SelectSlabNameByUserID(UserID);
                if (UserSlabMapping == null)
                    return DefaultSlab.Default + " (General)";
                return UserSlabMapping.SlabName + " (General)";
            }
        }

        internal InactiveUserWrapper SelectInActiveUsers(SelectInActiveUsersRequest mainRequest, out string failureMessage)
        {
            failureMessage = "";
            List<SelectInActiveUserResponse> list = new List<SelectInActiveUserResponse>();
            InactiveUserWrapper wrapper = new InactiveUserWrapper();

            try
            {
                list = new UserDAL().SelectInActiveUsers(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = UserMessage.NotFoundOrActive;

            if (list.Count <= 0) return wrapper;

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            wrapper.Count = list[0].Count;
            //if (!mainRequest.IsExcelDownload)
            //{
            //    list = list.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            //}
            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<UserSummaryDashboardWrapper> SelectUserBalanceSummaryForDistributorDashborad(int UserID)
        {
            List<UserSummaryDashboardWrapper> usersummary = new List<UserSummaryDashboardWrapper>();
            try
            {
                List<UserTypeWithCount> usertypes = new List<UserTypeWithCount>();
                usertypes = new UserDAL().SelectUserTypeWithCountForDist(UserID);

                if (usertypes == null)
                {
                    return usersummary;
                }

                int Totalcount = usertypes
                      .Sum(x => x.Count);

                foreach (UserTypeWithCount item in usertypes)
                {
                    List<WalletView> WalletList = new List<WalletView>();
                    UserSummaryDashboardWrapper response = new UserSummaryDashboardWrapper();
                    WalletList = new UserDAL().SelectWalletBalanceUserTypewiseForDist(item.UserTypeID, UserID);
                    response.UserType = item.UserType;
                    response.Count = item.Count;
                    response.WalletList = WalletList;

                    usersummary.Add(response);
                }

                List<WalletMaster> Activewallet = new WalletMasterDal().Select();

                List<WalletView> WalletList1 = new List<WalletView>();
                foreach (var item in Activewallet.Select((value, i) => new { i, value }))
                {
                    WalletView WList1 = new WalletView();

                    var value = item.value;
                    var index = item.i;
                    WList1.WalletName = value.Name;
                    WList1.WalletBalance = usersummary
                         .Sum(x => x.WalletList[index].WalletBalance);

                    WalletList1.Add(WList1);
                }

                UserSummaryDashboardWrapper responsetotal = new UserSummaryDashboardWrapper();
                //  responsetotal.UserType = "Total";
                //  responsetotal.Count = Totalcount;
                //  responsetotal.WalletList = WalletList1;
                //   usersummary.Add(responsetotal);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return usersummary;
        }

        internal string UpdatePin(UpdatePinRequest updateRequest, int UserID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (updateRequest.NewPin != updateRequest.ConfirmPin)
                return UserMessage.InvalidConfirmPin;

            UserMaster ObjModel = null;
            try
            {
                updateRequest.CurrentPassword = new Hashing().Hash(updateRequest.CurrentPassword);

                ObjModel = new UserDAL().SelectByIDWithPin(updateRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return UserMessage.InvalidPassword;

            ObjModel.Mpin = updateRequest.NewPin;
            int Result = -1;
            try
            {
                Result = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return Message.UserMessage.UpdatePinFail;

            new MessageSelection().SendMpinMessage(updateRequest.NewPin.ToString(), updateRequest.UserID, ObjModel.MobileNumber, ObjModel.WhatsAppNumber, ObjModel.EmailID, ObjModel.FirmName);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<DownlineForDropDownResponse> SelectRetailerAndAPIUserForDropDown(SelectUserForDropDownRequest mainRequest, int UserID, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            try
            {
                list = new UserDAL().SelectRetailerAndAPIUserForDropDown(UserID, mainRequest.UserTypeID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal string UpdateProfile(UpdateProfileRequest updateRequest, int UserID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return UserMessage.UserNotFound;

            ObjModel.PersonName = updateRequest.PersonName;
            ObjModel.Address = updateRequest.Address;
            ObjModel.ShopAddress = updateRequest.ShopAddress;
            ObjModel.AadharNo = updateRequest.AadharNo;
            ObjModel.PANNo = updateRequest.PANNo;
            ObjModel.GSTNo = updateRequest.GSTNo;
            ObjModel.IsTwoFA = false;
            ObjModel.AllowRealAPI = false; //updateRequest.AllowRealAPI;
            ObjModel.WalletIDforEcollection = updateRequest.WalletIDforEcollection;
            ObjModel.StaticQRWalletID = updateRequest.StaticQRWalletID;
            ObjModel.IsPin = updateRequest.IsPin;
            ObjModel.NumberFirst = updateRequest.NumberFirst;
            ObjModel.WhatsAppNumber = updateRequest.WhatsAppNumber;
            ObjModel.ResponseType = updateRequest.ResponseType;
            ObjModel.IsReferralPopup = updateRequest.IsReferralPopup;
            ObjModel.IsCommissionHold = updateRequest.IsCommissionHold;
            ObjModel.CityName = updateRequest.City;
            ObjModel.Pincode = updateRequest.PinCode;
            int Result = -1;
            try
            {
                Result = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return Message.UserMessage.UpdateProfileFail;

            new FCMSelection().SendAPPBarRefreshFCMInBackground(UserID);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal UserWrapper SelectDatewise(SelectDatewiseRequest mainRequest, out string failureMessage)
        {
            UserWrapper wrapper = new UserWrapper();
            failureMessage = "";
            List<SelectDatewiseResponse> list = new List<SelectDatewiseResponse>();

            try
            {
                list = new UserDAL().SelectDatewise(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }


            failureMessage = UserMessage.UserNotFound;
            if (list.Count <= 0) return wrapper;


            foreach (SelectDatewiseResponse user in list)
            {
                string kycStatus = new UserBal().SelectKYCStatus(user.IsKYCApproved, user.IsKYC, user.UserID);
                user.IsKYCApprovedStr = kycStatus;

                List<DetailViewModel> wallets = new UserWalletMasterDal().SelectByUserID(user.UserID);

                foreach (DetailViewModel item in wallets)
                {
                    if (item.WalletID == (int)WalletCode.Recharge)
                    {
                        user.Balances = $"{item.ShortName}:{(float)System.Math.Round(item.Balance)}";
                    }
                    if (item.WalletID == (int)WalletCode.DMT)
                    {
                        user.Balances = $"{user.Balances} {Environment.NewLine}{item.ShortName} :{(float)System.Math.Round(item.Balance)}";
                    }
                    if (item.WalletID == (int)WalletCode.AEPS)
                    {
                        user.Balances = $"{user.Balances} {Environment.NewLine}{item.ShortName} :{item.Balance}";
                    }
                }
            }
            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            wrapper.Count = list.Count();
            if (!mainRequest.IsExcelDownload)
            {
                list = list.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string SelectResponseMessage(int UserID)
        {
            DateTime lastChangedDOC = DateTime.Now;

            PasswordChangeHistory ObjModel = null;
            try
            {
                ObjModel = new PasswordChangeHistoryDal().SelectLastDate(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel != null)
            {
                lastChangedDOC = ObjModel.DOC;
            }
            UserMaster user = null;
            if (ObjModel == null)
            {
                try
                {
                    user = new UserDAL().SelectByIDFromCache(UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            if (user != null)
            {
                lastChangedDOC = user.DOC;
            }

            DateTime today = DateTime.Now;
            int days = (lastChangedDOC - today).Days;

            days = Math.Abs(days);

            int PasswordExpireLimit = new SettingMasterBal().SelectMax_PasswordExpireDays();

            int reaminingdays = (days - PasswordExpireLimit);
            reaminingdays = Math.Abs(reaminingdays);
            string Message = UserMessage.PasswordExpiry;
            Message = Message.Replace("[ReaminingDays]", reaminingdays.ToString());

            return Message;
        }

        internal BarCodeViewModel Select2FAQRImage(KeyDataModel keyDataModel, out string failureMessage)

        {
            UserMaster user = null;

            try
            {
                user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            BarCodeViewModel response = null;

            try
            {
                string UserUniqueKey = user.MobileNumber + user.TwoFASecretKey;

                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                var setupInfo = tfa.GenerateSetupCode(Company.ClientName, user.MobileNumber, UserUniqueKey, false, 300);

                response = new BarCodeViewModel();
                response.BarCodeImageURL = setupInfo.QrCodeSetupImageUrl;
                response.SetUpCode = setupInfo.ManualEntryKey;
                response.UserUniqueKey = UserUniqueKey;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string TwoFAActivate(TwoFAActivateRequest mainRequest, KeyDataModel keyDatamodel)
        {
            string failureMessage = "";
            failureMessage = Message.UserMessage.UserNotFound;

            if (keyDatamodel.UserID <= 0)
                return failureMessage;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(keyDatamodel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsTwoFA = mainRequest.IsTwoFA;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0) return Message.UserMessage.UpdateFail;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal bool Send2FAOTP(KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = "";
            failureMessage = Message.UserMessage.UserNotFound;
            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

            if (user == null)
                return false;

            Task.Run(() => new MessageSelection().Send2FAOTP(user));

            failureMessage = StatusType.Ok;
            return true;
        }

        internal string Deactivate2FAWithOTP(Deactivate2FAWithOTPRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;
            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

            if (user == null)
                return failureMessage;

            bool IsValidOTP = new OTPMasterBal().IsValid(user.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.TwoFactorAuthentication));

            failureMessage = Messages.InvalidOTP;
            if (!IsValidOTP)
                return failureMessage;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return failureMessage;

            ObjModel.IsTwoFA = false;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0) return Message.UserMessage.UpdateFail;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal bool VerifyTwoFA(TwoFAVerifyRequest VerifyRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = VerifyRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return false;

            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;

            try
            {
                userMaster = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (userMaster == null) return false;
            bool IsValid = false;
            try
            {
                string UserUniqueKey = userMaster.MobileNumber + userMaster.TwoFASecretKey;

                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                IsValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, VerifyRequest.TwoFAPasscode);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }
            failureMessage = Message.UserMessage.Invalid2FAPasscode;
            if (!IsValid)
            {
                return false;
            }

            userMaster.IsTwoFA = true;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(userMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            if (rowsAffected <= 0) return false;

            failureMessage = StatusType.Ok;
            return true;
        }

        internal bool IsValid2FA(string MobileNumber, string TwoFAPasscode)
        {
            UserMaster userMaster = null;
            try
            {
                userMaster = new UserDAL().SelectUserByMobileNumberSynchronization(MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }
            if (userMaster == null) return false;

            bool IsValid = false;
            try
            {
                string UserUniqueKey = MobileNumber + userMaster.TwoFASecretKey;

                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                IsValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, TwoFAPasscode);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            if (!IsValid)
            {
                return false;
            }
            return true;
        }

        internal LoginResponse VerifyTwoFAatLogin(VerifyTwoFARequest mainRequest, string IPAddress, out string failureMessage)
        {
            LoginResponse ReturnObj = null;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ReturnObj;

            mainRequest.Password = new Hashing().Hash(mainRequest.Password);
            try
            {
                string cashCollection = IsCashCollection();
                string TargetOnSale = IsTargetOnSale();
                string pin = IsPin();
                string isEkycUserProfileUpdate = IsEkycUserProfileUpdate();
                string isUserListOnDashboard = IsUserListOnDashboard();

                ReturnObj = new UserDAL().SelectForLogin(mainRequest.UserName, mainRequest.Password, mainRequest.Device, mainRequest.DeviceID, mainRequest.Pin, cashCollection, pin, TargetOnSale, isEkycUserProfileUpdate, isUserListOnDashboard);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return ReturnObj;
            }

            if (ReturnObj == null)
            {
                failureMessage = UserMessage.InvalidUser;
                return ReturnObj;
            }

            failureMessage = Message.UserMessage.Invalid2FAPasscode;
            bool Is2FAValid = IsValid2FA(ReturnObj.MobileNumber, mainRequest.TwoFAPasscode);

            LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(IPAddress);

            if (!Is2FAValid)
            {
                failureMessage = UserMessage.InvalidSecurityCode;

                int LoginHistoryTableID = new LoginHistoryDal().Insert(new LoginHistrory
                {
                    Through = mainRequest.Device,
                    Token = "0",
                    UserID = ReturnObj.UserID,
                    SimCardDetails = locationLookUp.SimCardDetails.TrimIfNotNullWithDefault("0"),
                    IPAddress = IPAddress,
                    Location = locationLookUp.Location.TrimIfNotNullWithDefault("0"),
                    LoginMessage = failureMessage,
                });

                new LocationLookupBAL().InsertCore(LocationLookUpType.Login, "", "", IPAddress, LoginHistoryTableID);

                ReturnObj = new LoginResponse();
                return ReturnObj;
            }

            new TrustableDeviceDal().Insert(new TrustableDevice
            {
                IPAddress = IPAddress,
                UserID = ReturnObj.UserID,
                Device = mainRequest.Device,
                IsVerified = true,
                Browser = mainRequest.FingerPrint
            });
            if (mainRequest.Device.EqualsIgnoreCase(PlatformCode.Web))
            {
                ReturnObj.UISettings = new UISettingsBal().SelectAll();
            }
            ReturnObj.RedirectTo = GetRedirectionByUserID(ReturnObj.UserTypeID);

            ReturnObj.IsNewDevice = false;
            ReturnObj.IsOTPSent = false;
            failureMessage = StatusType.Ok;

            AddToLoginHistory(ReturnObj.UserID, IPAddress, "", UserMessage.LoginSuccess, "0", "0");

            if (ReturnObj.UserID != Company.AdminUserID)
                ReturnObj.NewsCount = new NewsBAL().SelectNewsCount(ReturnObj.UserID);

            if (ReturnObj.UserTypeID == (int)UserType.Developer)
            {
                ReturnObj.RedirectTo = "/Company";
                return (ReturnObj);
            }

            if (ReturnObj.UserTypeID == (int)UserType.Support)
            {
                List<Response.SupportUserAccessDetail.Parent> menus = new MenuMasterBal().GetMenuHeirarchy();
                ReturnObj.MenuResponse.data = menus;
            }

            if (mainRequest.Device.EqualsIgnoreCase(PlatformCode.Web))
            {
                ReturnObj.UISettings = new UISettingsBal().SelectAll();
            }

            UpdateFlagAfterLoginSuccess(mainRequest.Device, ReturnObj);

            try
            {
                new WrongPasswordHistoryDal().DeleteWrongPasswordHistory(ReturnObj.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return ReturnObj;
        }

        private void insertPasswordChangeHistoryWithLocaion(int UserID, string IPAddress, string type)
        {
            try
            {
                PasswordChangeHistory passwordhistory = new PasswordChangeHistory();
                passwordhistory.UserID = UserID;
                passwordhistory.IPAddress = IPAddress;
                passwordhistory.Type = type;
                new PasswordChangeHistoryDal().Insert(passwordhistory);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            new LocationLookupBAL().InsertCore(LocationLookUpType.PasswordChangeHistory, "", "", IPAddress, UserID);
        }

        internal string ChangePasswordAfterExpiry(ChangePasswordAfterExpiryRequest updateRequest, string IPAddress)
        {
            int UserID = 0;
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserMaster ObjModel = null;
            try
            {
                updateRequest.OldPassword = new Hashing().Hash(updateRequest.OldPassword);
                ObjModel = new UserDAL().SelectByMobileWithPassword(updateRequest.MobileNumber, updateRequest.OldPassword);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return UserMessage.InvalidOldPass;

            UserID = ObjModel.UserID;

            if (UserID <= 0)
                return UserMessage.InvalidUser;

            ObjModel.Password = new Hashing().Hash(updateRequest.NewPassword);

            int Result = -1;
            try
            {
                Result = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return Message.UserMessage.UpdatepassFail;

            Parallel.Invoke(
                () =>
                {
                    new MessageSelection().ChangePasswordMessage(UserID, ObjModel.MobileNumber, ObjModel.WhatsAppNumber, ObjModel.EmailID, ObjModel.FirmName);
                }
               ,
               () =>
               {
                   insertPasswordChangeHistoryWithLocaion(UserID, IPAddress, SecurityParam.ChangePasswordAfterExpiry);
               }
            );

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string SelectResponseType(int UserID)
        {
            string respoType = "";
            try
            {
                respoType = new UserDAL().SelectResponseTypeByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (respoType == null || respoType.Length <= 5)
            {
                return UserResponseType.Initial;
            }

            if (respoType.EqualsIgnoreCase(UserResponseType.Callback))
            {
                return UserResponseType.Callback;
            }

            return UserResponseType.Initial;
        }

        internal List<DeletedUsersResponse> SelectDeletedUsers(SelectDeletedUsersRequest mainRequest, int UserID, out string failureMessage)
        {
            List<DeletedUsersResponse> members = null;
            failureMessage = "Members not found.";
            try
            {
                members = new UserDAL().SelectAllDeleteUsers(mainRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 };

                Parallel.ForEach(members, options, member =>
                {
                    member.Outstanding = new CashCollectionMasterDal().SelectOutstandingForDeletedUser(member.UserID);

                    List<DetailViewModel> wallets = new UserWalletMasterDal().SelectByUserID(member.UserID);

                    foreach (DetailViewModel item in wallets)
                    {
                        if (item.WalletID == (int)WalletCode.Recharge)
                        {
                            member.RechargeBalance = (float)System.Math.Round(item.Balance);
                            member.Balances = $"{item.ShortName}:{member.RechargeBalance}";
                        }
                        if (item.WalletID == (int)WalletCode.DMT)
                        {
                            member.DMTBalance = (float)System.Math.Round(item.Balance);
                            member.Balances = $"{member.Balances} {Environment.NewLine}{item.ShortName} :{member.DMTBalance}";
                        }
                        if (item.WalletID == (int)WalletCode.AEPS)
                        {
                            member.AEPSBalance = (float)System.Math.Round(item.Balance);
                            member.Balances = $"{member.Balances} {Environment.NewLine}{item.ShortName} :{member.AEPSBalance}";
                        }
                    }
                }
          );
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = UserMessage.UserNotFound;
            if (members.Count <= 0) return members;

            failureMessage = StatusType.Ok;
            return members;
        }

        internal async Task<string> UserRevert(DeleteRequest userRequest)
        {
            string failureMessage = userRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectDeletedUsersByID(userRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsActive = true;
            ObjModel.IsDelete = false;
            ObjModel.MobileNumber = ObjModel.MobileNumber.Replace("_Deleted", "").Trim();

            try
            {
                int UserID = await new UserDAL().SelectUserIDByMobileNumber(ObjModel.MobileNumber);
                if (UserID > 0 && UserID != userRequest.UserID)
                    return failureMessage = UserMessage.UnableToRevert;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
            {
                return Message.UserMessage.UpdateFail;
            }
            try
            {
                new WrongPasswordHistoryDal().DeleteWrongPasswordHistory(userRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<DownlineForDropDownResponse> SelectSDAndDistForDropDown(SelectUserForDropDownRequest mainRequest, int UserID, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            try
            {
                list = new UserDAL().SelectSDAndDistForDropDown();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<DownlineForDropDownResponse> SelectSDAndDistForDropDownByOldUserID(SelectUserForDropDownByUserIDRequest mainRequest, int UserID, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = UserMessage.UserNotFound;

            if (mainRequest.UserID <= 0)
                return list;

            int UserTypeID = 0;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (UserTypeID <= 0)
                return list;

            try
            {
                list = new UserDAL().SelectByUserTypeID(UserTypeID, mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal string TransferUsers(List<TransferUsersRequest> mainRequest, out string failureMessge)
        {
            string Result = UserMessage.UserTransferSuccess;

            failureMessge = UserMessage.UserNotFound;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            int FailureCount = 0, SuccessCount = 0;

            foreach (TransferUsersRequest request in mainRequest)
            {
                failureMessge = UpdateUser(request);

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            }

            failureMessge = StatusType.Ok;
            if (FailureCount <= 0 && SuccessCount > 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = UserMessage.UserTransferFail;
                return Result;
            }

            Result = UserMessage.UserTransferSuccessFail;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        internal string UpdateUser(TransferUsersRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return failureMessage;

            failureMessage = Message.UserMessage.UpdateFail;

            ObjModel.ParentID = mainRequest.NewParentID;

            int UserID = -1;
            try
            {
                UserID = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (UserID <= 0)
                return failureMessage;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<RoleWiseUserViewModel> SelectByRole(UserTypeSelectRequest selectByTypeRequest, out string failureMessage)
        {
            List<RoleWiseUserViewModel> ReturnResponse = null;

            failureMessage = selectByTypeRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ReturnResponse;

            try
            {
                ReturnResponse = new UserDAL().SelectByRole(selectByTypeRequest.UserTypeID);
            }
            catch (Exception ex) { new Logger().Write(ex); }

            return ReturnResponse;
        }

        internal RoleWiseUserWrapper SelectByRoleUserBulk(UserTypeSelectRequest selectByTypeRequest, out string failureMessage)
        {
            RoleWiseUserWrapper wrapper = new RoleWiseUserWrapper();
            List<RoleWiseUserViewModel> ReturnResponse = null;

            failureMessage = selectByTypeRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                ReturnResponse = new UserDAL().SelectByRoleUserBulk(selectByTypeRequest);
            }
            catch (Exception ex) { new Logger().Write(ex); }

            wrapper.Count = ReturnResponse.Count;

            ReturnResponse = ReturnResponse.Skip(selectByTypeRequest.SkipCount).Take(selectByTypeRequest.TakeCount).ToList();

            wrapper.list = ReturnResponse;

            return wrapper;
        }

        internal bool CheckValidUserForUpdate(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Admin) return true;
            if (UserTypeID == (int)UserType.Support) return true;

            if (UserTypeID == (int)UserType.Distributer) return true;

            if (UserTypeID == (int)UserType.SuperDistributer) return true;

            return false;
        }

        internal string BulkUpdate(List<BulkUpdateUsersRequest> mainRequest, int LoginUserID, out string failureMessge)
        {
            string Result = UserMessage.UpdateSuccess;

            failureMessge = UserMessage.UserNotFound;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            UserMaster loginuser = new UserDAL().SelectByIDFromCache(LoginUserID);

            if (loginuser == null) return failureMessge;

            if (!CheckValidUserForUpdate(loginuser.UserTypeID))
            { return failureMessge; }

            int FailureCount = 0, SuccessCount = 0;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(mainRequest.AsEnumerable(), parallelOptions, request =>
            {
                string failureMessge = CheckCompanyMasterSetting(request);
                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    return;

                failureMessge = BulkUpdateUser(request, loginuser.UserTypeID, LoginUserID);

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            });

            failureMessge = StatusType.Ok;
            if (FailureCount <= 0 && SuccessCount > 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = UserMessage.UpdateFail;
                return Result;
            }

            Result = UserMessage.UserTransferSuccessFail;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        private bool SelectValidUserForResponseType(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Retailer)
            {
                return true;
            }
            if (UserTypeID == (int)UserType.APIUser)
            {
                return true;
            }

            return false;
        }

        internal string CheckCompanyMasterSetting(BulkUpdateUsersRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (mainRequest.Criteria.EqualsIgnoreCase("CommissionOnTransfer"))
            {
                failureMessage = UserMessage.CommissionOnTransferOff;
                if (!Company.CommissionOnTransfer.EqualsIgnoreCase("Yes"))
                    return failureMessage;
            }

            if (mainRequest.Criteria.EqualsIgnoreCase("Overdraft"))
            {
                failureMessage = UserMessage.OverdraftOff;
                if (!Company.OverDraft.EqualsIgnoreCase("Yes"))
                    return failureMessage;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string BulkUpdateUser(BulkUpdateUsersRequest mainRequest, int UserTypeID, int LoginUserID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return failureMessage;

            if ((UserTypeID == (int)UserType.Distributer) || (UserTypeID == (int)UserType.SuperDistributer))
            {
                failureMessage = Message.UserMessage.UpdateFail;
                if (LoginUserID != ObjModel.ParentID) return failureMessage;
            }

            failureMessage = Message.UserMessage.UpdateFail;

            if (mainRequest.Criteria.EqualsIgnoreCase("ResponseType"))
            {
                bool IsValid = SelectValidUserForResponseType(ObjModel.UserTypeID);
                if (IsValid == false)
                {
                    failureMessage = "Invalid User";
                    return failureMessage;
                }
                Task.Run(() => UpdateResponseType(mainRequest.UserID, mainRequest.ResponseType));

                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            if (mainRequest.Criteria.EqualsIgnoreCase("MaxOutstanding"))
            {
                failureMessage = UserMessage.InvalidMaxOs;
                if (mainRequest.CriteriaText.EqualsIgnoreCase("")) return failureMessage;

                Task.Run(() => UpdateMaxOutstanding(mainRequest.UserID, Convert.ToDecimal(mainRequest.CriteriaText)));
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            if (mainRequest.Criteria.EqualsIgnoreCase("AddServices"))
            {
                Task.Run(() => AddUserServiceMappingBulkUpdate(mainRequest.UserID, mainRequest.UserServiceList));
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            if (mainRequest.Criteria.EqualsIgnoreCase("RemoveServices"))
            {
                Task.Run(() => RemoveUserServiceMapping(mainRequest.UserID, mainRequest.UserServiceList));
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            if (mainRequest.Criteria.EqualsIgnoreCase("BalanceCappingAmount"))
            {
                Task.Run(() => InsertBalanceCapping(mainRequest));
                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            if (mainRequest.Criteria.EqualsIgnoreCase("LowBalance"))
            {
                Task.Run(() => InsertLowBalance(mainRequest));
                failureMessage = StatusType.Ok;
                return failureMessage;
            }



            if (mainRequest.Criteria.EqualsIgnoreCase("PayoutAccountCount"))
                ObjModel.PayoutAccountCount = Convert.ToInt32(mainRequest.CriteriaText);

            if (mainRequest.Criteria.EqualsIgnoreCase("CommissionOnTransfer"))
            {
                ObjModel.Comm = Convert.ToDecimal(mainRequest.CriteriaText);
                ObjModel.CommissionCategory = CommissionCategory.Commission;
            }

            if (mainRequest.Criteria.EqualsIgnoreCase("GSTHoldCutting"))
            {
                if (Company.GSTHold.EqualsIgnoreCase("Yes"))
                    ObjModel.GSTHoldCutting = mainRequest.GSTHoldCutting;
            }

            if (mainRequest.Criteria.EqualsIgnoreCase("GSTType"))
            {
                if (!mainRequest.GSTType.EqualsIgnoreCase(""))
                    ObjModel.GSTPrinciple = mainRequest.GSTType;
            }

            if (mainRequest.Criteria.EqualsIgnoreCase("Referral"))
            {
                decimal ReferralBenefitMinAmount = Convert.ToDecimal(mainRequest.ReferralBenefitMinAmount);
                if (ReferralBenefitMinAmount > 0)
                    ObjModel.ReferralBenefitMinAmount = ReferralBenefitMinAmount;

                decimal ReferralAmount = Convert.ToDecimal(mainRequest.ReferralAmount);
                if (ReferralAmount > 0)
                    ObjModel.ReferralAmount = ReferralAmount;
            }

            if (!mainRequest.IsTextValue)
            {
                if (mainRequest.Criteria.EqualsIgnoreCase("Deactivate"))
                    ObjModel.IsActive = !mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsDelete"))
                    ObjModel.IsDelete = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsLoginOTP"))
                    ObjModel.IsLoginOTP = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsTwoFA"))
                    ObjModel.IsTwoFA = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsReferralPopup"))
                    ObjModel.IsReferralPopup = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsInvoiceApplicable"))
                    ObjModel.IsInvoiceApplicable = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("AllowRealAPI"))
                    ObjModel.AllowRealAPI = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsChannel"))
                    ObjModel.IsChannel = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsMpin"))
                    ObjModel.IsPin = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("NumberFirst"))
                    ObjModel.NumberFirst = mainRequest.CriteriaValue;

                if (mainRequest.Criteria.EqualsIgnoreCase("IsCommissionHold"))
                    ObjModel.IsCommissionHold = mainRequest.CriteriaValue;
            }

            int UserID = -1;
            try
            {
                UserID = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (UserID <= 0)
                return failureMessage;

            if (mainRequest.Criteria.EqualsIgnoreCase("MaxOutstanding"))
                Task.Run(() => UpdateMaxOutstanding(mainRequest.UserID, Convert.ToDecimal(mainRequest.CriteriaText)));

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string InsertLowBalance(BulkUpdateUsersRequest mainRequest)
        {
            string failureMessage = string.Empty;
            if (mainRequest.WalletID <= 0)
                return failureMessage;

            Requests.UserwiseLowBalance.InsertRequest balance = new Requests.UserwiseLowBalance.InsertRequest
            {
                WalletID = mainRequest.WalletID,
                Amount = Convert.ToDecimal(mainRequest.CriteriaText)
            };

            try
            {
                failureMessage = new UserwiseLowBalanceBAL().Insert(balance, mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return failureMessage;
        }

        internal string InsertBalanceCapping(BulkUpdateUsersRequest mainRequest)
        {
            string failureMessage = string.Empty;
            if (mainRequest.WalletID <= 0)
                return failureMessage;

            UserBalanceCapping model = null;
            try
            {
                model = new UserBalanceCappingDal().SelectByWalletIDAndUserID(mainRequest.UserID, mainRequest.WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int affectedrow = 0;
            failureMessage = UserBalanceCappingMessages.InsertDeaultCappingFailure;

            if (model == null)
            {
                UserBalanceCapping ObjModel = new UserBalanceCapping
                {
                    WalletID = mainRequest.WalletID,
                    Amount = Convert.ToDecimal(mainRequest.CriteriaText),
                    UserID = mainRequest.UserID,
                };
                try
                {
                    affectedrow = new UserBalanceCappingDal().Insert(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (affectedrow <= 0) return failureMessage;

                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            if (model != null)
            {
                model.WalletID = model.WalletID;
                model.Amount = Convert.ToDecimal(mainRequest.CriteriaText);
                model.UpdatedDOC = DateTime.Now;
                try
                {
                    affectedrow = new UserBalanceCappingDal().Update(model);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (affectedrow <= 0) return failureMessage;

                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            return failureMessage;
        }

        internal string SendPasswordToAll()
        {
            List<Response.User.MemberDetailResponse> userList = new UserDAL().SelectAllExceptAdmin();
            foreach (MemberDetailResponse item in userList)
            {
                if (item.UserID <= 79)
                {
                    continue;
                }

                string Password = new Util().GetRandomPassword();
                string HashPassword = new Hashing().Hash(Password);

                UserMaster user = new UserMaster();
                try
                {
                    user = new UserDAL().SelectByIDFromCache(item.UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (user == null)
                    continue;

                user.Password = HashPassword;
                int Result = -1;
                try
                {
                    Result = new UserDAL().Update(user);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                new MessageSelection().SendPasswordMessage(Password, user.UserID, user.MobileNumber, user.WhatsAppNumber, user.EmailID, user.FirmName);
            }
            return StatusType.Ok;
        }

        internal List<Response.UserOnbording.UserOnbordingListResponse> SelectUserOnbordingList(Requests.UserOnbording.SelectUserOnbordingListRequest mainRequest, int UserID, out string failureMessage)
        {
            List<Response.UserOnbording.UserOnbordingListResponse> members = null;
            failureMessage = "Members not found.";
            try
            {
                members = new UserDAL().SelectUserOnbordingList(mainRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = UserMessage.UserNotFound;
            if (members.Count <= 0) return members;

            failureMessage = StatusType.Ok;
            return members;
        }
        internal List<Response.UserOnbording.UserOnbordingListResponse> SelectUserOnbordingStatusCheckList(Requests.UserOnbording.SelectUserOnbordingListRequest mainRequest, int UserID, out string failureMessage)
        {
            List<Response.UserOnbording.UserOnbordingListResponse> members = null;
            failureMessage = "Members not found.";
            try
            {
                members = new UserDAL().SelectUserOnbordingStatusCheckList(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = UserMessage.UserNotFound;
            if (members.Count <= 0) return members;

            failureMessage = StatusType.Ok;
            return members;
        }

        internal List<DownlineForDropDownResponse> SelectOnlyParentUserForDropdown(KeyDataModel keyDataModel, SelectUserForDropDownRequest mainRequest, out string failureMessage)
        {
            List<DownlineForDropDownResponse> list = null;
            failureMessage = "Downline not found.";

            try
            {
                list = new UserDAL().SelectOnlyParentUserForDropdown(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.FirmName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.FirmName).ToList();
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<eKYCViewModel> SelecteKYCUserDetailsByUserID(int UserID, out string failureMessage)
        {
            List<eKYCViewModel> member = null;

            try
            {
                member = new UserDAL().SelecteKYCUserDetailsByUserID(UserID);
                if (member == null || member.Count <= 0)
                {
                    failureMessage = "Data not found";
                    return member;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Message.UserMessage.UserNotFound;
                return member;
            }

            failureMessage = StatusType.Ok;
            return member;
        }

        internal bool IsKyc(int UserID)
        {
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (IsValidManualKycAndeKycBoth())
            {
                if (!user.IseKYC || !user.IsKYCApproved)
                    return false;
            }
            if (user.IseKYC)
                return true;
            if (user.IsKYCApproved)
                return true;
            return false;
        }
        private bool IsValidManualKycAndeKycBoth()
        {
            string IsManualKycAndeKycBoth = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsManualKycAndeKycBoth), "No");
            if (!IsManualKycAndeKycBoth.EqualsIgnoreCase("Yes"))
                return false;
            return true;
        }



        internal bool IsValidReferalOutsideRegistration(RegistrationRequest registrationRequest)
        {
            UserMaster referralparent = null;
            referralparent = new UserDAL().SelectByReferralCode(registrationRequest.ReferralCode);
            if (referralparent == null) return true;

            if (referralparent.UserTypeID == (int)UserType.Client)
            {
                if (registrationRequest.UserTypeID == (int)UserType.Client)
                    return true;
            }
            if (referralparent.UserTypeID == (int)UserType.APIUser)
            {
                if (registrationRequest.UserTypeID == (int)UserType.APIUser)
                    return true;
                if (registrationRequest.UserTypeID == (int)UserType.Retailer)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Client)
                    return true;
            }

            if (referralparent.UserTypeID == (int)UserType.Retailer)
            {
                if (registrationRequest.UserTypeID == (int)UserType.APIUser)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Retailer)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Client)
                    return true;
            }

            if (referralparent.UserTypeID == (int)UserType.Distributer)
            {
                if (registrationRequest.UserTypeID == (int)UserType.APIUser)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Retailer)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Client)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.DFOS)
                    return true;
            }

            if (referralparent.UserTypeID == (int)UserType.SuperDistributer)
            {
                if (registrationRequest.UserTypeID == (int)UserType.APIUser)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Retailer)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Client)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.SDFOS)
                    return true;

                if (registrationRequest.UserTypeID == (int)UserType.Distributer)
                    return true;
            }

            return false;
        }

        internal MorePayUserOnBoardingResponse SelectUserMorePayOnBoarding(int UserID, out string failureMessage)
        {
            MorePayUserOnBoardingResponse response = new MorePayUserOnBoardingResponse();
            failureMessage = Message.UserMessage.InvalidUser;
            string result = "";
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.Message = failureMessage;
                return response;
            }
            if (user == null)
            {
                response.Message = failureMessage;
                return response;
            }

            if (user.MorePayMerchantCode == null)
            {
                response.Message = UserMessage.OpenOnboarding;
                failureMessage = StatusType.Ok;
                return response;
            }
            if (user.MorePayMerchantCode.Length <= 2)
                response.Message = UserMessage.OpenOnboarding;
            else if (user.MorePayMerchantCode.Length >= 5 && user.IsMorePayEKYC == false)
            {
                response.MorePayMerchantCode = user.MorePayMerchantCode;
                response.Message = UserMessage.OpenOTP;
            }
            else if (user.MorePayMerchantCode.Length >= 5 && user.IsMorePayEKYC == true)
            {
                response.MorePayMerchantCode = user.MorePayMerchantCode;
                response.Message = UserMessage.Transaction;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string UpdateOutlatID(int UserID, string OutlatID)
        {
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            ObjModel.InstantPayOutletID = OutlatID;
            int Result = -1;
            try
            {
                Result = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return Message.UserMessage.UpdateOutletIDFail;

            return StatusType.Ok;
        }


        internal async Task<GenerateOTPResponse> SignUpGenerateOTP(RegistrationRequest registrationRequest, string IPAddress, string failureMessage)
        {
            GenerateOTPResponse response = new GenerateOTPResponse();

            registrationRequest.ParentID = Company.AdminUserID;
            registrationRequest.JoiningByUserID = Company.AdminUserID;
            registrationRequest.RegistrationType = RegistrationType.OutsideSignUp;
            registrationRequest.SlabID = 0;
            registrationRequest.AllowRealAPI = false;
            registrationRequest.IsChannel = false;
            registrationRequest.IsTwoFA = false;

            failureMessage = registrationRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;


            string IsSignUPOTP = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsSignUPOTP), "No");

            if (IsSignUPOTP.EqualsIgnoreCase("No"))
            {
                UserBal registrationProcessor = new UserBal();
                failureMessage = await registrationProcessor.Insert(registrationRequest, IPAddress);
                if (failureMessage.EqualsIgnoreCase("Ok"))
                {
                    response.Message = Message.UserMessage.RegistrationSuccess;
                    return response;
                }
                response.Message = failureMessage;
                return response;
            }

            failureMessage = "Invalid OTP";
            response.OTPMasterID = new MessageSelection().SendSignUPPasswordOTP(registrationRequest.MobileNumber);
            if (response.OTPMasterID <= 0)
                return response;

            failureMessage = StatusType.Ok;
            response.Message = Message.UserMessage.SignUpOTP;
            response.Status = StatusType.Ok;
            return response;
        }

        public async Task<string> CheckckingDuplicateAliasName(string AliasName)
        {
            try
            {
                if (AliasName.Length <= 0)
                {
                    return StatusType.Ok;
                }

                int UserID = await new UserDAL().SelectUserIDByAliasName(AliasName);
                if (UserID > 0)
                    return Message.UserMessage.DuplicateAliasName;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }


    }
}