using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using project.Response.Invoice;
using Project.BAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.User;
using Project.Requests.UserOnbording;
using Project.Response;
using Project.Response.FCMResponse;
using Project.Response.User;
using Project.Response.UserOnbording;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class UserDAL
    {
        private readonly string UserMasterKEY = "_UserMaster";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserMaster>>();
            cache.Remove(UserMasterKEY);
        }

        internal List<UserMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserMaster>>();
            List<UserMaster> list = _list.GetOrCreate(UserMasterKEY, () => SelectWithoutCache());
            return list;
        }

        private List<UserMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();

            return context.UserMaster.Where(U => U.IsDelete == false).ToList();
        }

        internal int Update(UserMaster ObjModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.UserMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.UserID;
        }

        internal int Insert(UserMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserMaster.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.UserID;
        }


        internal LoginResponse SelectForLogin(string UserName, string Password, string Device, string DeviceID, int Pin, string isCashCollection, string isPin, string TargetOnSale, string isEkycUserProfileUpdate, string isUserListOnDashboard)
        {

            LoginResponse ReturnResponse = new LoginResponse();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var query = from User in context.UserMaster
                            where (User.MobileNumber == UserName || User.AliasName == UserName)
                            && User.Password == Password
                            && User.IsDelete == false
                            && User.IsActive == true
                            select User;

                if (Device.EqualsIgnoreCase(PlatformCode.Android) || Device.EqualsIgnoreCase(PlatformCode.iOS))
                {
                    query = query.Where(User => User.Mpin == Pin);
                }

                ReturnResponse = query.Select(User => new LoginResponse
                {

                    IsLoginOTP = User.IsLoginOTP,
                    EmailID = User.EmailID,
                    UserType = User.UserType.UserType,
                    MobileNumber = User.MobileNumber,
                    WhatsAppNumber = User.WhatsAppNumber,
                    UserTypeID = User.UserTypeID,
                    UserID = User.UserID,
                    ParentID = User.ParentID,
                    AliasName = User.AliasName,
                    FirmName = User.FirmName,
                    ProfileImagePath = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                    IsInitialPassword = User.IsInitialPassword,
                    IsKYCApproved = User.IsKYCApproved,
                    IseKYC = User.IseKYC,
                    IsChannel = User.IsChannel,
                    IsTwoFA = User.IsTwoFA,
                    IsReferralPopup = User.IsReferralPopup,
                    NumberFirst = User.NumberFirst,
                   CommissionHold = User.IsCommissionHold,
                    AadharNo = User.AadharNo,
                    PANNo = User.PANNo,

                }).FirstOrDefault();
                if (ReturnResponse == null)
                {
                    if (Device.EqualsIgnoreCase(PlatformCode.Android) || Device.EqualsIgnoreCase(PlatformCode.iOS))
                        IncrementFailedAttempt(UserName, DeviceID);

                    return ReturnResponse;
                }

                ReturnResponse.IsCashCollection = isCashCollection.ToLower();

                ReturnResponse.IsEkycUserProfileUpdate = isEkycUserProfileUpdate.ToLower();
                ReturnResponse.IsUserListOnDashboard = isUserListOnDashboard.ToLower();
                ReturnResponse.IsTargetOnSale = TargetOnSale.ToLower();
                ReturnResponse.IsOverDraft = Company.OverDraft.ToLower();
                ReturnResponse.PlanApiForAPIUser = Company.PlanApiForAPIUser.ToLower();
                ReturnResponse.IsCreditInstitution = Company.IsCreditInstitution.ToLower();
                ReturnResponse.IsFintechLeadGeneration = Company.IsFintechLeadGeneration.ToLower();

                ReturnResponse.IsW2OFundTransfer = Company.IsW2OFundTransfer.ToLower();
                ReturnResponse.IsPin = IsValidPin(ReturnResponse.UserID);
                ReturnResponse.IsUPIVerify = IsUPIVerify();
                ReturnResponse.IsReferralPopup = IsValidReferralPopup(ReturnResponse.IsReferralPopup);
                ReturnResponse.ReportFontSize = new UISettingDal().SelectByKey(Enum.GetName(typeof(UISettingKeys), UISettingKeys.ReportFontSize), "13") + "px";
                List<WalletMaster> walletList = new WalletMasterDal().Select();
                ReturnResponse.Wallets = (from walletMaster in walletList
                                          join wallet in context.UserWalletMaster
                                          on walletMaster.WalletID equals (wallet.WalletID)
                                          where wallet.UserID == ReturnResponse.UserID

                                          select new UserWallet
                                          {
                                              Name = walletMaster.Name,
                                              Balance = wallet.Balance,
                                              ShortName = walletMaster.ShortName
                                          }).ToList();
                try
                {
                    Response.News.ViewModel news = new NewsDAL().SelectNewsMessage(ReturnResponse.UserID);
                    if (news != null)
                    {
                        if (news.HyperLink.Length != 0)
                        {
                            ReturnResponse.NewsMessage = $"{news.Title}. click here - {news.HyperLink}";
                        }
                        else
                        {
                            ReturnResponse.NewsMessage = news.Title;
                        }
                        ReturnResponse.HyperLink = news.HyperLink;
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (ReturnResponse.IsCashCollection.EqualsIgnoreCase("true"))
                {
                    float Outstanding = new CashCollectionMasterDal().SelectOutstandingForUIByUserID(ReturnResponse.UserID);

                    ReturnResponse.Wallets.Add(new UserWallet
                    {
                        Balance = Convert.ToDecimal(Outstanding),
                        Name = "OS",
                        ShortName = "OS"
                    });

                    if ((ReturnResponse.UserTypeID == (int)UserType.Admin) ||
                        (ReturnResponse.UserTypeID == (int)UserType.Retailer) ||
                        (ReturnResponse.UserTypeID == (int)UserType.APIUser))
                        return ReturnResponse;

                    float rOS = new CashCollectionMasterDal().SelectOutstandingByUserID(ReturnResponse.UserID);

                    ReturnResponse.Wallets.Add(new UserWallet
                    {
                        Balance = Convert.ToDecimal(rOS),
                        Name = "R.OS",
                        ShortName = "R.OS"
                    });
                }
            }

            return ReturnResponse;
        }

        internal LoginResponse SelectForMpinLogin(string UserName, string Password)
        {
            LoginResponse ReturnResponse = new LoginResponse();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                ReturnResponse = (from User in context.UserMaster
                                  where (User.MobileNumber == UserName || User.AliasName == UserName)
                                  && User.Password == Password
                                  && User.IsDelete == false
                                  && User.IsActive == true

                                  select new LoginResponse
                                  {
                                      IsLoginOTP = User.IsLoginOTP,
                                      EmailID = User.EmailID,
                                      UserType = User.UserType.UserType,
                                      MobileNumber = User.MobileNumber,
                                      WhatsAppNumber = User.WhatsAppNumber,
                                      UserTypeID = User.UserTypeID,
                                      UserID = User.UserID,
                                      ParentID = User.ParentID,
                                      FirmName = User.FirmName,
                                      ProfileImagePath = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                                      IsInitialPassword = User.IsInitialPassword,
                                      IsKYCApproved = User.IsKYCApproved,
                                      IseKYC = User.IseKYC,
                                      IsChannel = User.IsChannel,
                                      IsTwoFA = User.IsTwoFA,
                                      IsReferralPopup = User.IsReferralPopup,
                                      NumberFirst = User.NumberFirst,
                                      CommissionHold = User.IsCommissionHold,
                                      AadharNo = User.AadharNo,
                                      PANNo = User.PANNo,

                                  }).FirstOrDefault();

                return ReturnResponse;
            }
        }


        private string IncrementFailedAttempt(string userName, string DeviceID)
        {

            DeviceWiseFailLoginHistory Obj = new DeviceWiseFailLoginHistory
            {
                UserName = userName,
                DeviceID = DeviceID,
            };

            int Result = -1;
            try
            {
                Result = new DeviceWiseFailLoginHistoryDAL().Insert(Obj);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        private string ValidatePinChange(List<Response.PinGeneration.ViewModel> response, string deviceID)
        {
            int today = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));

            // Check pin changes today
            var pinChangesToday = response.Count(pin => pin.DOCInt == today && pin.DeviceID == deviceID);

            if (pinChangesToday >= 3)
            {
                return UserMessage.AppLock;
            }

            return StatusType.Ok;
        }



        internal UserMaster SelectUserByPaysprintAEPSMerchantID(string merchant_id)
        {
            return Select().FirstOrDefault(U => U.PaysprintAEPSAgentID == merchant_id);
        }
        internal UserMaster SelectUserByFingpayAgentID(string bcLoginID)
        {
            return Select().FirstOrDefault(U => U.FingpayAgentID == bcLoginID);
        }

        internal UserMaster SelectUserByMahagramAEPSAgentID(string bCID)
        {
            return Select().FirstOrDefault(U => U.MahagramAEPSAgentID == bCID);
        }

        internal List<UserMaster> GetUserInformationForBillPay(string outletPan)
        {
            List<UserMaster> list = Select();
            List<UserMaster> listRet = null;
            try
            {
                listRet = (from user in list
                           where user.PANNo == outletPan
                           select user).ToList();
            }
            catch (Exception ex)
            {
            }
            return listRet;
        }

        internal LoginResponse SelectForLoginByUserID(int currentUserID)
        {
            LoginResponse ReturnResponse = new LoginResponse();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            List<UserMaster> list = Select();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                ReturnResponse = (from User in list
                                  join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                                  where User.UserID == currentUserID
                                  && User.IsDelete == false
                                  && User.IsActive == true
                                  select new LoginResponse
                                  {
                                      IsLoginOTP = User.IsLoginOTP,
                                      EmailID = User.EmailID,
                                      UserType = usertype.UserType,
                                      MobileNumber = User.MobileNumber,
                                      UserTypeID = User.UserTypeID,
                                      UserID = User.UserID,
                                      FirmName = User.FirmName,
                                      IsKYCApproved = User.IsKYCApproved,
                                  }).FirstOrDefault();

                if (ReturnResponse == null)
                    return ReturnResponse;

                List<WalletMaster> walletList = new WalletMasterDal().Select();

                ReturnResponse.IsCashCollection = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsCashCollection)); //Company.IsCashCollection;

                ReturnResponse.IsPin = IsValidPin(ReturnResponse.UserID);
                ReturnResponse.IsUPIVerify = IsUPIVerify();

                ReturnResponse.Wallets = (from walletMaster in walletList
                                          join wallet in context.UserWalletMaster
                                          on walletMaster.WalletID equals (wallet.WalletID)
                                          where wallet.UserID == ReturnResponse.UserID
                                          select new UserWallet
                                          {
                                              Name = walletMaster.Name,
                                              Balance = wallet.Balance
                                          }).ToList();
            }
            return ReturnResponse;
        }

        public string IsValidPin(int UserID)
        {
            string IsPin = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsPin));//Company.IsPin;

            if (!IsPin.EqualsIgnoreCase("true"))
            {
                bool ispin = SelectUserIsPin(UserID);
                if (ispin == true)
                    return "true";
                return "false";
            }
            return "true";
        }


        public string IsValidUserPin(int UserID, string RechargePin)
        {
            string IsPin = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsPin));//Company.IsPin;

            if (IsPin.EqualsIgnoreCase("true"))
            {
                bool ispin = SelectUserIsPin(UserID);
                if (ispin == true)
                {
                    Validation valid = new Validation();
                    if (!valid.IsDigitsOnly(RechargePin))
                        return UserMessage.InvalidPin;

                    int Pin = new UserDAL().SelectUserPin(UserID);
                    if (Pin != Convert.ToInt32(RechargePin))
                        return UserMessage.InvalidPin;
                }
                return StatusType.Ok;
            }
            return StatusType.Ok;
        }

        private bool IsUPIVerify()
        {
            string IsUPIVerify = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsUPIVerify), "No");
            if (!IsUPIVerify.EqualsIgnoreCase("Yes"))
                return false;
            return true;
        }

        private bool IsValidReferralPopup(bool IsReferralPopup)
        {
            string SignupReferral = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SignupReferral), "No");
            if (!SignupReferral.EqualsIgnoreCase("Yes"))
                return false;
            if (!IsReferralPopup)
                return false;
            return true;
        }

        internal string SelectAdminEmailID()
        {
            UserMaster item = Select().FirstOrDefault(U => U.UserID == Company.AdminUserID);

            if (item == null)
                return "";

            return item.EmailID;
        }

        internal string SelectAdminMobileNumber()
        {
            UserMaster item = Select().FirstOrDefault(U => U.UserID == Company.AdminUserID);

            if (item == null)
                return "";

            return item.MobileNumber;
        }

        internal string SelectEmailID(int UserID)
        {
            UserMaster item = Select().FirstOrDefault(U => U.UserID == UserID);

            if (item == null)
                return "";

            return item.EmailID;
        }

        internal string SelectMobileNumber(int UserID)
        {
            UserMaster item = Select().FirstOrDefault(U => U.UserID == UserID);
            if (item == null)
            {
                return "";
            }

            return item.MobileNumber;
        }

        internal string SelectFirmNameByUserID(int UserID)
        {
            UserMaster item = Select().FirstOrDefault(U => U.UserID == UserID);
            if (item == null)
            {
                return "";
            }

            return item.FirmName.ConcatNameMobileAliasName(item.MobileNumber, item.AliasName);
        }

        internal int SelectUserIDByAPIToken(string mobileNumber, string aPIToken)
        {
            List<UserMaster> list = Select();

            UserViewModel obj = (from user in list
                                 where user.UserTypeID == (int)UserType.APIUser
                                 && user.IsDelete == false
                                 && user.IsActive == true
                                 && user.MobileNumber == mobileNumber
                                 && user.APIToken == aPIToken
                                 select new UserViewModel
                                 {
                                     FirmName = user.FirmName,
                                     UserID = user.UserID
                                 }).FirstOrDefault();
            if (obj == null)
                return 0;

            return obj.UserID;
        }

        internal List<UserViewModel> SelectByType(int UserTypeID)
        {
            List<UserMaster> list = Select();

            List<UserViewModel> ReturnResponse = new List<UserViewModel>();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            ReturnResponse = (from user in list
                              join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                              where user.UserTypeID == UserTypeID
                              && user.IsDelete == false
                              && user.IsActive == true
                              && user.UserID != 1
                              select new UserViewModel
                              {
                                  FirmName = user.FirmName.ConcatNameMobileAliasNameRole(user.MobileNumber, user.AliasName, usertype.UserType),
                                  UserID = user.UserID,
                                  ParentID = user.ParentID,
                                  MobileNumber = user.MobileNumber,
                                  WhatsAppNumber = user.WhatsAppNumber,
                                  EmailID = user.EmailID,
                                  FcmViewModel = new FCMViewModel
                                  {
                                      AppFCMToken = user.AppFCMToken,
                                      WebFCMToken = user.WebFCMToken
                                  }
                              }
                          ).ToList();

            return ReturnResponse;
        }

        internal List<UserViewModel> SelectUplineByUserID(int UserID)
        {
            int ParentID = SelectParentID(UserID);
            List<UserMaster> list = Select().Where(x => x.UserID == ParentID).ToList();

            List<UserViewModel> ReturnResponse = new List<UserViewModel>();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            ReturnResponse = (from user in list
                              where user.IsDelete == false
                              && user.IsActive == true
                              && user.UserID != 1

                              select new UserViewModel
                              {
                                  FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                  UserID = user.UserID,
                                  ParentID = user.ParentID,
                                  MobileNumber = user.MobileNumber,
                                  WhatsAppNumber = user.WhatsAppNumber,
                                  EmailID = user.EmailID,
                                  FcmViewModel = new FCMViewModel
                                  {
                                      AppFCMToken = user.AppFCMToken,
                                      WebFCMToken = user.WebFCMToken
                                  }
                              }
                          ).ToList();

            return ReturnResponse;
        }

        internal UserMaster SelectByICICIStaticGUID(string ICICIStaticGUID)
        {
            return Select().FirstOrDefault(U => U.ICICI_Static_GUID == ICICIStaticGUID);
        }

        internal List<UserViewModel> SelectUpline(int UserTypeID)
        {
            List<UserViewModel> ReturnResponse = new List<UserViewModel>();

            switch (UserTypeID)
            {
                case (int)UserType.SuperDistributer:
                    ReturnResponse = SelectByType((int)UserType.Admin);
                    break;

                case (int)UserType.Support:
                    ReturnResponse = SelectByType((int)UserType.Admin);
                    break;

                case (int)UserType.Distributer:
                    ReturnResponse = SelectByType((int)UserType.Admin);
                    ReturnResponse.AddRange(SelectByType((int)UserType.SuperDistributer));
                    break;

                case (int)UserType.Retailer:
                    ReturnResponse = SelectByType((int)UserType.Admin);
                    ReturnResponse.AddRange(SelectByType((int)UserType.SuperDistributer));
                    ReturnResponse.AddRange(SelectByType((int)UserType.Distributer));
                    break;

                case (int)UserType.SDFOS:
                    ReturnResponse.AddRange(SelectByType((int)UserType.SuperDistributer));
                    break;

                case (int)UserType.DFOS:
                    ReturnResponse.AddRange(SelectByType((int)UserType.Distributer));
                    break;

                case (int)UserType.APIUser:
                    ReturnResponse = SelectByType((int)UserType.Admin);
                    break;

                case (int)UserType.Client:
                    ReturnResponse = SelectByType((int)UserType.Admin);
                    ReturnResponse.AddRange(SelectByType((int)UserType.SuperDistributer));
                    ReturnResponse.AddRange(SelectByType((int)UserType.Distributer));
                    ReturnResponse.AddRange(SelectByType((int)UserType.Retailer));
                    break;

                case (int)UserType.Accountant:
                    ReturnResponse = SelectByType((int)UserType.Admin);
                    break;
            }
            return ReturnResponse;
        }

        public int SelectUserTypeID(int UserID)
        {
            List<UserMaster> list = Select();

            if (list.Count <= 0)
                return 0;

            UserMaster item = list.FirstOrDefault(U => U.UserID == UserID);

            if (item == null) return 0;

            return item.UserTypeID;
        }

        public int SelectUserPin(int UserID)
        {
            UserMaster item = Select().FirstOrDefault(U => U.UserID == UserID);

            if (item == null) return 0;

            return item.Mpin;
        }

        public bool SelectUserIsPin(int UserID)
        {
            UserMaster item = Select().FirstOrDefault(U => U.UserID == UserID);

            if (item == null) return false;

            return item.IsPin;
        }

        internal List<MemberDetailResponse> SelectAllExceptAdmin()
        {
            List<UserMaster> list = Select();
            List<StateMaster> stateList = new StateDAL().Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            List<MemberDetailResponse> members = null;

            members = (from User in list
                       join joineduser in list on User.JoiningByUserID equals joineduser.UserID
                       into JoiningDetails
                       from ljoined in JoiningDetails.DefaultIfEmpty()
                       join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                       join parent in list on User.ParentID equals parent.UserID
                       join superparent in list on parent.ParentID equals superparent.UserID
                       into superparentdetilas
                       from leftsuper in superparentdetilas.DefaultIfEmpty()
                       join state in stateList on User.StateID equals state.ID

                       where User.UserTypeID != (int)UserType.Admin
                       && User.IsDelete == false

                       select new MemberDetailResponse
                       {
                           UserID = User.UserID,
                           UserTypeID = User.UserTypeID,
                           UserType = usertype.UserType,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           ProfileImagePath = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                           MobileNumber = User.MobileNumber,
                           ParentName = parent?.FirmName.ConcatNameMobileAliasName(parent.MobileNumber, parent.AliasName) ?? "",
                           SuperParentName = leftsuper?.FirmName.ConcatNameMobileAliasName(leftsuper?.MobileNumber, leftsuper?.AliasName) ?? "",
                           IsLoginOTP = User.IsLoginOTP,
                           EmailID = User.EmailID,
                           GSTNo = User.GSTNo,
                           PANNo = User.PANNo,
                           AadharNo = User.AadharNo,
                           IsKYCApproved = User.IsKYCApproved,
                           IseKYC = User.IseKYC,
                           IsActive = Convert.ToBoolean(User.IsActive),
                           IsWeb = User.IsWeb,
                           IsApp = User.IsApp,
                           IsInvoiceApplicable = User.IsInvoiceApplicable,
                           JoiningByUser = ljoined?.FirmName.ConcatNameMobileAliasName(ljoined.MobileNumber, ljoined.AliasName) ?? "",
                           DOC = User.DOC.ToString(Messages.DateShortFormat),
                           StateName = state.Name,
                           DOCInt = User.DOCInt,
                           CityName = User.CityName,
                           PersonName = User.PersonName,
                           Pincode = User.Pincode,
                           AliasName = User.AliasName,
                           Address = User.Address,
                           ParentAliasName = parent.AliasName,
                           ParentFirmName = parent.FirmName,
                           WhatsAppNumber = User.WhatsAppNumber,
                           VirtualAccountNumber = User.VirtualAccountNumber,
                           ResponseType = User.ResponseType,
                           ParentMobileNumber = parent.MobileNumber,
                           IsCommissionHold = User.IsCommissionHold,
                           SuperParentAliasName = leftsuper?.AliasName,
                           SuperParentMobileNumber = leftsuper?.MobileNumber,
                           SuperParentFirmName = leftsuper?.FirmName,
                           ShopAddress = User.ShopAddress,
                       }).ToList();

            return members;
        }

        internal List<ReportViewModel> SelectForInvoice(Requests.Invoice.SelectRequest mainRequest)
        {
            List<UserMaster> list = Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            return (from user in list
                    join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                    where user.UserID > 2
                    && (mainRequest.UserTypeID == 0 ? 2 > 1 : user.UserTypeID == mainRequest.UserTypeID)
                    && (mainRequest.UserID == 0 ? 2 > 1 : user.UserID == mainRequest.UserID)
                    select new ReportViewModel
                    {
                        UserType = usertype.UserType,
                        FirmName = user.FirmName,
                        MobileNumber = user.MobileNumber,

                        EmailID = user.EmailID,
                        UserID = user.UserID,
                        GSTNo = user.GSTNo,
                    }).ToList();
        }

        internal async Task<int> SelectUserIDByFirmName(string firmName)
        {
            List<UserMaster> list = Select();

            UserMaster user = null;

            user = list.FirstOrDefault(U => U.FirmName == firmName);

            return GetOnlyUserID(user);
        }

        internal async Task<int> SelectUserIDByVirtualAccountNumber(string VirtualAccountNumber)
        {
            List<UserMaster> list = Select();

            UserMaster user = null;

            user = list.FirstOrDefault(U => U.VirtualAccountNumber == VirtualAccountNumber);

            return GetOnlyUserID(user);
        }

        internal async Task<int> SelectUserIDByAliasName(string AliasName)
        {
            List<UserMaster> list = Select();

            UserMaster user = null;

            user = list.FirstOrDefault(U => U.AliasName == AliasName);

            return GetOnlyUserID(user);
        }

        private int GetOnlyUserID(UserMaster user)
        {
            if (user == null)
                return -1;

            return user.UserID;
        }

        internal int SelectUserIDByAliasNames(string AliasName)
        {
            List<UserMaster> list = Select();

            UserMaster user = null;

            user = list.FirstOrDefault(U => U.AliasName == AliasName);

            return GetOnlyParentUserID(user);
        }


        private int GetOnlyParentUserID(UserMaster user)
        {
            if (user == null)
                return Company.AdminUserID;

            return user.UserID;
        }


        internal UserMaster SelectByUniqueRefNo(string uniqueRefNo)
        {
            return Select().FirstOrDefault(U => U.UniqueReferenceNo == uniqueRefNo);
        }

        internal async Task<int> SelectUserIDByEmailID(string emailID)
        {
            List<UserMaster> list = Select();

            UserMaster user = list.FirstOrDefault(U => U.EmailID == emailID.Trim());

            return GetOnlyUserID(user);
        }

        internal List<MemberDetailResponse> SelectAllByParentID(int ParentID)
        {
            List<UserMaster> list = Select();

            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            int ParentUserTypeID = SelectUserTypeID(ParentID);

            if (Util.isDFOSOrSDFOS(ParentUserTypeID))
            {
                ParentID = SelectParentID(ParentID);
            }

            List<MemberDetailResponse> members = null;

            if (Util.isRoleAdminOrSupport(ParentUserTypeID))
            {
                members = SelectAllExceptAdmin();
                return members;
            }

            List<StateMaster> stateList = new StateDAL().Select();

            members = (from User in list
                       join joineduser in list on User.JoiningByUserID equals joineduser.UserID
                       into JoiningDetails
                       from ljoined in JoiningDetails.DefaultIfEmpty()
                       join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                       join parent in list on User.ParentID equals parent.UserID
                       join superparent in list on parent.ParentID equals superparent.UserID
                       into superparentdetilas
                       from leftsuper in superparentdetilas.DefaultIfEmpty()
                       join state in stateList on User.StateID equals state.ID
                       where User.UserTypeID != (int)UserType.Admin
                       && User.ParentID == ParentID
                       && User.IsDelete == false

                       select new MemberDetailResponse
                       {
                           UserID = User.UserID,
                           UserTypeID = User.UserTypeID,
                           UserType = usertype.UserType,
                           FirmName = User.FirmName.ConcatNameAliasName(User.AliasName),
                           ProfileImagePath = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                           MobileNumber = User.MobileNumber,
                           ParentName = parent.FirmName.ConcatNameMobileAliasName(parent.MobileNumber, parent.AliasName),
                           SuperParentName = leftsuper?.FirmName.ConcatNameMobileAliasName(leftsuper.MobileNumber, leftsuper.AliasName) ?? "",
                           EmailID = User.EmailID,
                           GSTNo = User.GSTNo,
                           PANNo = User.PANNo,
                           AadharNo = User.AadharNo,
                           IsKYCApproved = User.IsKYCApproved,
                           IseKYC = User.IseKYC,
                           IsActive = User.IsActive,
                           IsWeb = User.IsWeb,
                           IsApp = User.IsApp,
                           IsInvoiceApplicable = User.IsInvoiceApplicable,
                           StateName = state.Name,
                           JoiningByUser = ljoined?.FirmName.ConcatNameMobileAliasName(ljoined.MobileNumber, ljoined.AliasName) ?? "",
                           DOCInt = User.DOCInt,
                           DOC = User.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                           CityName = User.CityName,
                           PersonName = User.PersonName,
                           Pincode = User.Pincode,
                           AliasName = User.AliasName,
                           Address = User.Address,
                           ParentFirmName = parent.FirmName,
                           ParentAliasName = parent.AliasName,
                           WhatsAppNumber = User.WhatsAppNumber,
                           VirtualAccountNumber = User.VirtualAccountNumber,
                           ResponseType = User.ResponseType,
                           ParentMobileNumber = parent.MobileNumber,
                           GSTPrinciple = User.GSTPrinciple,
                           IsCommissionHold = User.IsCommissionHold,
                           SuperParentAliasName = leftsuper.AliasName,
                           SuperParentMobileNumber = leftsuper.MobileNumber,
                           SuperParentFirmName = leftsuper.FirmName,
                       }).ToList();
            return members;
        }



        internal List<AgentDetailResponseBBPS> SelectAgent(int ParentID)
        {
            List<UserMaster> list = Select();
            List<StateMaster> stateList = new StateDAL().Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            List<AgentDetailResponseBBPS> members = null;

            members = (from User in list
                       join joineduser in list on User.JoiningByUserID equals joineduser.UserID
                       into JoiningDetails
                       from ljoined in JoiningDetails.DefaultIfEmpty()
                       join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                       join parent in list on User.ParentID equals parent.UserID
                       join superparent in list on parent.ParentID equals superparent.UserID
                       into superparentdetilas
                       from leftsuper in superparentdetilas.DefaultIfEmpty()
                       join state in stateList on User.StateID equals state.ID

                       where User.UserTypeID != (int)UserType.Admin
                       && User.IsDelete == false
                       && User.NPCIAgentID == null

                       select new AgentDetailResponseBBPS
                       {
                           UserID = User.UserID,
                           AgentName = User.PersonName,
                           MobileNumber = User.MobileNumber,
                           AgentShopName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           EmailID = User.EmailID,
                           IsActive = Convert.ToBoolean(User.IsActive),
                           EffectiveFormDate = User.DOC.ToString(Messages.DateShortFormat),
                           State = state.Name,
                           City = User.CityName,
                           Pincode = User.Pincode,
                           AgentReferenceId = User.AliasName,
                           Address = User.Address,
                           AgentInstituteID = "YB53",
                           PaymentChannel = "[{\"paymentChannel\":\"Business_Correspondent\",\"minLimit\":1,\"maxLimit\":null}]",
                           PaymentMode = "[{\"paymentMode\":\"Debit Card\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"Credit Card\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"Prepaid Card\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"IMPS\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"Cash\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"UPI\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"Wallet\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"NEFT\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"AEPS\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"Account Transfer\",\"minLimit\":1,\"maxLimit\":null},{\"paymentMode\":\"Bharat QR\",\"minLimit\":1,\"maxLimit\":null}]"
                       }).ToList();

            return members;
        }

        internal List<AgentDetailResponse> SelectAgentForMATM(int ParentID)
        {
            List<UserMaster> list = Select();
            List<StateMaster> stateList = new StateDAL().Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            List<AgentDetailResponse> members = null;

            members = (from User in list
                       join joineduser in list on User.JoiningByUserID equals joineduser.UserID
                       into JoiningDetails
                       from ljoined in JoiningDetails.DefaultIfEmpty()
                       join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                       join parent in list on User.ParentID equals parent.UserID
                       join superparent in list on parent.ParentID equals superparent.UserID
                       into superparentdetilas
                       from leftsuper in superparentdetilas.DefaultIfEmpty()
                       join state in stateList on User.StateID equals state.ID

                       where User.UserTypeID != (int)UserType.Admin
                       && User.IsDelete == false
                     && User.PaySwiffMerchantId == null

                       select new AgentDetailResponse
                       {
                           UserID = User.UserID,
                           UserTypeID = User.UserTypeID,
                           UserType = usertype.UserType,
                           MobileNumber = User.MobileNumber,
                           EmailID = User.EmailID,
                           BusinessName = "Hitachi-payment Service Private Limited",
                           DBAName = "Hitachi-payment Service Private Limited",
                           BusinessFilingType = "Private",
                           BusinessPAN = "AABCI4082J",
                           GSTIN = "27AABCI4082J1ZG",
                           BusinessAddress = "4th Floor, 401, Silver Metropolis, Jai Coach Compound, Goregoan East",
                           State = "Maharashtra",
                           City = "Mumbai",
                           ZipCode = "400063",
                           Categorycode = "8",
                           Subcategorycode = "67",
                           OwnerSignatoryName = "Hitachi",
                           OwnerPAN = "AABCI4082J",
                           OwnerDOB = "12/9/2005",
                           AuthorisedSignatoryAddress = "4th Floor, 401, Silver Metropolis, Jai Coach Compound, Goregoan East",
                           OwnerState = "Maharashtra",
                           OwnerCity = "Mumbai",
                           OwnerZipCode = "400063",
                           IsActive = Convert.ToBoolean(User.IsActive),
                           AgentCode = User.AliasName,
                           DeviceNo = "K206Y037757"

                       }).ToList();

            return members;
        }

        internal List<DownlineForDropDownResponse> SelectDownlineForDropDown(int userID)
        {
            List<UserMaster> list = Select();

            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       where User.UserTypeID != (int)UserType.Admin
                       && User.UserTypeID != (int)UserType.Developer
                       && User.ParentID == userID
                       && User.IsDelete == false
                       && User.IsActive == true
                       orderby User.FirmName ascending
                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           UserTypeID = User.UserTypeID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();
            return members;
        }

        internal List<DownlineForDropDownResponse> SelectDownlineForLedger(int userID, int UserTypeID)
        {
            List<UserMaster> list = Select();

            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       where User.UserTypeID != (int)UserType.Admin
                       && User.UserTypeID != (int)UserType.Developer
                       && User.IsDelete == false
                       && User.IsActive == true
                       orderby User.FirmName ascending
                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           ParentID = User.ParentID,
                           UserTypeID = User.UserTypeID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                members = (from item in members
                           where item.ParentID == userID
                           select item).ToList();

                return members;
            }
            return members;
        }

        internal List<DownlineForDropDownResponse> SelectDirectDownline(int userID)
        {
            List<UserMaster> list = Select();
            int UserTypeID = SelectUserTypeID(userID);
            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       where (User.UserTypeID == (int)UserType.SuperDistributer
                       || User.UserTypeID == (int)UserType.Distributer
                       || User.UserTypeID == (int)UserType.Retailer)
                       && User.ParentID == userID
                       && User.IsDelete == false
                       && User.IsActive == true
                       orderby User.FirmName ascending
                       select new DownlineForDropDownResponse
                       {
                           UserTypeID = User.UserTypeID,
                           UserID = User.UserID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();

            return members;
        }

        internal List<DownlineForDropDownResponse> SelectDirectDownlineForUserBulk(int userID)
        {
            List<UserMaster> list = Select();
            List<UserTypeMaster> typelist = new UserTypeDAL().Select();
            int UserTypeID = SelectUserTypeID(userID);
            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       join usertype in typelist on User.UserTypeID equals usertype.UserTypeID
                       where User.ParentID == userID
                       && User.IsDelete == false
                       && User.IsActive == true
                       orderby User.FirmName ascending
                       select new DownlineForDropDownResponse
                       {
                           UserTypeID = User.UserTypeID,
                           UserType = usertype.UserType,
                           UserID = User.UserID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();

            return members;
        }

        internal List<DownlineForDropDownResponse> SelectDownlinForFundTransfer(int userID, bool IsDFOSOrSDFOS = false)
        {
            List<UserMaster> list = Select();
            int UserTypeID = SelectUserTypeID(userID);
            string cashCollection = new FundBal().IsCashCollection();

            List<DownlineForDropDownResponse> members = null;
            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();

                if (cashCollection.EqualsIgnoreCase("true"))
                {
                    members = (from User in list
                               where User.UserTypeID != (int)UserType.Admin
                               && User.IsDelete == false
                                && User.ParentID == userID
                               && User.IsActive == true

                               select new DownlineForDropDownResponse
                               {
                                   UserID = User.UserID,
                                   FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                               }).ToList();
                }
                return members;
            }

            members = (from User in list
                       where User.UserTypeID != (int)UserType.Admin
                       && User.UserTypeID != (int)UserType.Developer
                       && User.ParentID == userID
                       && User.IsDelete == false
                       && User.IsActive == true
                       && (IsDFOSOrSDFOS ? User.UserTypeID != (int)UserType.SDFOS : 2 > 1)
                        && (IsDFOSOrSDFOS ? User.UserTypeID != (int)UserType.DFOS : 2 > 1)
                       orderby User.FirmName ascending
                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();

            return members;
        }

        internal List<DownlineForDropDownResponse> SelectDownlineWithOutStanding(int userID)
        {
            List<UserMaster> list = Select();

            List<DownlineForDropDownResponse> members = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                members = (from User in list
                           join collection in context.CashCollectionMaster on User.UserID equals collection.UserID
                           orderby collection.Outstanding descending
                           where User.UserTypeID != (int)UserType.Admin
                           && collection.Outstanding != 0
                           && User.UserTypeID != (int)UserType.Developer
                           && User.ParentID == userID
                           && User.IsDelete == false
                           && User.IsActive == true
                           orderby User.FirmName
                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               UserTypeID = User.UserTypeID,
                               FirmName = User.FirmName.ConcatNameMobileAliasNameOutstanding(User.MobileNumber, User.AliasName, collection.Outstanding),
                           }).ToList();
                return members;
            }
        }

        internal List<DownlineForDropDownResponse> SelectDownlineForDropDownExceptClientRole(int userID)
        {
            List<UserMaster> list = Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                       where User.UserTypeID != (int)UserType.Admin && User.UserTypeID != (int)UserType.Client
                       && User.ParentID == userID
                       && User.IsDelete == false && User.IsActive == true

                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           UserTypeID = User.UserTypeID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           UserType = usertype.UserType,
                       }).ToList();
            return members;
        }

        internal List<DownlineForDropDownResponse> SelectUserForDropDown(int userID)
        {
            List<UserMaster> list = Select();

            int UserTypeID = SelectUserTypeID(userID);

            List<DownlineForDropDownResponse> members = null;

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
                return members;
            }

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                var distList = (from User in list
                                where User.UserTypeID != (int)UserType.Admin
                                && User.ParentID == userID
                                && User.IsDelete == false
                                && User.IsActive == true
                                select new
                                {
                                    UserID = User.UserID,
                                    MobileNumber = User.MobileNumber,
                                    AliasName = User.AliasName,
                                    FirmName = User.FirmName,
                                }).ToList();

                var retList = (from User in list
                               join d in distList //inner sequence
                               on User.ParentID equals d.UserID
                               select new
                               {
                                   UserID = User.UserID,
                                   MobileNumber = User.MobileNumber,
                                   AliasName = User.AliasName,
                                   FirmName = User.FirmName,
                               }).ToList();

                var listFinal = distList.Union(retList).ToList();

                members = (from User in listFinal
                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.ParentID == userID
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }

            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.UserID == userID
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }
            return members;
        }

        internal UserMaster SelectByIDFromCache(int UserID)
        {
            List<UserMaster> list = Select();

            return list.FirstOrDefault(U => U.UserID == UserID);
        }

        internal UserMaster SelectByMobileFromCache(string MobileNumber)
        {
            List<UserMaster> list = Select();

            return list.FirstOrDefault(U => U.MobileNumber == MobileNumber);
        }

        internal UserMaster SelectByID(int UserID)
        {
            using (var context = new DatabaseContext())
            {
                return (from User in context.UserMaster
                        where User.UserID == UserID
                        && User.IsDelete == false
                        select User).FirstOrDefault();
            }
        }

        internal UserMaster SelectByIDWithPassword(int UserID, string Password)
        {
            List<UserMaster> list = Select();

            return (from item in list
                    where item.UserID == UserID && item.Password == Password
                    select item).FirstOrDefault();
        }

        internal UserMaster SelectByMobileWithPassword(string Mobile, string Password)
        {
            List<UserMaster> list = Select();

            return (from item in list
                    where item.MobileNumber == Mobile && item.Password == Password
                    select item).FirstOrDefault();
        }

        internal UserMaster SelectByIDWithPin(UpdatePinRequest updateRequest, int UserID)
        {
            List<UserMaster> list = Select();

            return (from item in list
                    where item.UserID == UserID //&& item.Mpin == updateRequest.OldPin
                    && item.Password == updateRequest.CurrentPassword
                    select item).FirstOrDefault();
        }

        internal MemberDetailResponse SelectDetailByUserID(int userID, bool isWithAdmin = false)
        {
            List<UserMaster> list = Select().Where(User => User.UserID == userID && User.IsDelete == false).ToList();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            List<UserMaster> allList = Select();
            MemberDetailResponse member = (from User in list
                                           join userparent in allList on User.ParentID equals userparent.UserID
                                           join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID

                                           select new MemberDetailResponse
                                           {
                                               UserID = User.UserID,
                                               UserTypeID = User.UserTypeID,
                                               UserType = usertype.UserType,
                                               ParentID = User.ParentID,
                                               ParentFirmName = userparent.FirmName.ConcatNameMobileAliasName(userparent.MobileNumber, userparent.AliasName),
                                               FirmName = User.FirmName,
                                               PersonName = User.PersonName,
                                               MobileNumber = User.MobileNumber,
                                               EmailID = User.EmailID,
                                               GSTNo = User.GSTNo,
                                               PANNo = User.PANNo,
                                               AadharNo = User.AadharNo,
                                               IsKYCApproved = User.IsKYCApproved,
                                               IsActive = User.IsActive,
                                               Address = User.Address,
                                               ShopAddress = User.ShopAddress,
                                               StateID = User.StateID,
                                               TargetAmount = User.TargetAmount,
                                               AllowRealAPI = User.AllowRealAPI,
                                               IsChannel = User.IsChannel,
                                               IsInvoiceApplicable = User.IsInvoiceApplicable,
                                               IsLoginOTP = User.IsLoginOTP,
                                               VirtualAccountNumber = User.VirtualAccountNumber,
                                               IsTwoFA = User.IsTwoFA,
                                               CityName = User.CityName,
                                               Pincode = User.Pincode,
                                               AliasName = User.AliasName,
                                               WalletIDforEcollection = User.WalletIDforEcollection,
                                               StaticQRWalletID = User.StaticQRWalletID,
                                               PayoutAccountCount = User.PayoutAccountCount,
                                               ResponseType = User.ResponseType,
                                               WhatsAppNumber = User.WhatsAppNumber,
                                               ProfileImagePath = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                                               Logo = String.Concat(Company.WebAPIURL, "Media/Logo.png"),
                                               YesBankLogo = String.Concat(Company.WebAPIURL, "Media/YesBankLogo.png"),
                                               IsCommissionHold = User.IsCommissionHold
                                           }).FirstOrDefault();

            if (isWithAdmin == false)
                if (member?.UserTypeID == (int)UserType.Admin)
                    return null;

            return member;
        }

        internal ProfileResponse SelectProfile(int UserID)
        {
            List<UserMaster> list = Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            ProfileResponse member = null;
            List<StateMaster> stateList = new StateDAL().Select();
            member = (from User in list
                      join stateMaster in stateList on User.StateID equals stateMaster.ID
                      join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                      where User.UserID == UserID

                      select new ProfileResponse
                      {
                          FirmName = User.FirmName,
                          PersonName = User.PersonName,
                          MobileNumber = User.MobileNumber,
                          WhatsAppNumber = User.WhatsAppNumber,
                          EmailID = User.EmailID,
                          UserType = usertype.UserType,
                          StateName = stateMaster.Name,
                          GSTNo = User.GSTNo,
                          PANNo = User.PANNo,
                          AadharNo = User.AadharNo,
                          Address = User.Address,
                          ShopAddress = User.ShopAddress,
                          IsKYCApproved = User.IsKYCApproved,
                          Pin = User.Mpin,
                          IsReferralPopup = User.IsReferralPopup,
                          PinCode = User.Pincode,
                          City = User.CityName,
                          CommissionOnTransfer = User.Comm,
                          ProfileImagePath = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                          VirtualAccountNumber = User.VirtualAccountNumber,
                          IsInvoiceApplicable = User.IsInvoiceApplicable,
                          IsTwoFA = User.IsTwoFA,
                          AllowRealAPI = User.AllowRealAPI,
                          WalletIDforEcollection = User.WalletIDforEcollection,
                          StaticQRWalletID = User.StaticQRWalletID,
                          IsPin = User.IsPin,
                          NumberFirst = User.NumberFirst,
                          ReferralURL = User.ReferralCode,
                          ReferralAmount = User.ReferralAmount,
                          ReferralBenefitMinAmount = User.ReferralBenefitMinAmount,
                          ResponseType = User.ResponseType,
                          IsCommissionHold = User.IsCommissionHold
                      }).FirstOrDefault();

            return member;
        }

        internal int SelectOnlyUserIDByMobileNumber(string mobileNumber)
        {
            List<UserMaster> list = Select();

            UserViewModel obj = (from user in list
                                 where user.UserID != 1
                                 && user.MobileNumber == mobileNumber

                                 select new UserViewModel
                                 {
                                     FirmName = user.FirmName,
                                     UserID = user.UserID
                                 }).FirstOrDefault();

            if (obj == null)
            {
                return 0;
            }

            return obj.UserID;
        }

        internal async Task<int> SelectUserIDByMobileNumber(string mobileNumber)
        {
            UserMaster user = await SelectUserByMobileNumber(mobileNumber);
            return GetOnlyUserID(user);
        }
        internal async Task<int> SelectUserIDByWhatsAppNumber(string WhatsAppNumber)
        {
            UserMaster user = await SelectUserByWhatsAppNumber(WhatsAppNumber);
            return GetOnlyUserID(user);
        }

        internal async Task<int> SelectUserTypeIDByMobileNumber(string mobileNumber)
        {
            UserMaster user = await SelectUserByMobileNumber(mobileNumber);
            return GetOnlyUserTypeID(user);
        }

        private int GetOnlyUserTypeID(UserMaster user)
        {
            if (user == null)
                return -1;

            return user.UserTypeID;
        }

        internal async Task<UserMaster> SelectUserByMobileNumber(string mobileNumber)
        {
            List<UserMaster> list = Select();

            return list.FirstOrDefault(U => U.MobileNumber == mobileNumber);
        }


        internal async Task<UserMaster> SelectUserByWhatsAppNumber(string WhatsAppNumber)
        {
            List<UserMaster> list = Select();

            return list.FirstOrDefault(U => U.WhatsAppNumber == WhatsAppNumber);
        }

        internal UserMaster SelectUserByMobileNumberSynchronization(string mobileNumber)
        {
            List<UserMaster> list = Select();

            return list.FirstOrDefault(U => U.MobileNumber == mobileNumber);
        }

        internal ParentViewModel SelectParent(int userID)
        {
            List<UserMaster> list = Select();
            List<UserTypeMaster> ReturnList = new UserTypeDAL().Select();
            ParentViewModel obj = new ParentViewModel();

            obj = (from Child in list
                   join Parent in list
                   on Child.ParentID equals Parent.UserID
                   join userType in ReturnList
                   on Parent.UserTypeID equals userType.UserTypeID
                   where Child.UserID == userID

                   select new ParentViewModel
                   {
                       FirmName = Parent.FirmName,
                       ParentID = Parent.UserID,
                       MobileNumber = Parent.MobileNumber,
                       ParentRole = userType.UserType,
                       AadharNo = Parent.AadharNo,
                       PanNo = Parent.PANNo
                   }).FirstOrDefault();


            if (obj.ParentRole.EqualsIgnoreCase("Admin"))
                obj.MobileNumber = "";


            return obj;
        }

        internal int SelectParentID(int UserID)
        {
            List<UserMaster> list = Select();
            return list.FirstOrDefault(user => user.UserID == UserID).ParentID;
        }

        internal UserMaster SelectAgentByAliceName(string AgentRefID)
        {
            List<UserMaster> list = Select();

            return list.FirstOrDefault(user => user.AliasName == AgentRefID);
        }

        internal ParentViewModel SelectAdmin()
        {
            List<UserMaster> list = Select();

            ParentViewModel obj = new ParentViewModel();
            obj = (from Parent in list
                   where Parent.UserID == Company.AdminUserID

                   select new ParentViewModel
                   {
                       FirmName = Parent.FirmName,
                       ParentID = Parent.UserID,
                       MobileNumber = "",
                   }).FirstOrDefault();

            return obj;
        }
        internal List<ParentViewModel> SelectDFOSOrSDFOSByParentID(int ParentID)
        {
            List<UserMaster> list = Select();
            List<ParentViewModel> obj = new List<ParentViewModel>();
            obj = (from Parent in list
                   where Parent.ParentID == ParentID
                   && (Parent.UserTypeID == (int)UserType.DFOS
                   || Parent.UserTypeID == (int)UserType.SDFOS)


                   select new ParentViewModel
                   {
                       FirmName = Parent.FirmName,
                       ParentID = Parent.UserID,
                       MobileNumber = Parent.MobileNumber,
                   }).ToList();

            return obj;
        }



        internal List<DownlineForDropDownResponse> SelectSupportUserForDropDown()
        {
            List<UserMaster> list = Select();

            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       where User.UserTypeID == (int)UserType.Support

                       && User.IsDelete == false
                       && User.IsActive == true

                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();
            return members;
        }

        internal FCMViewModel SelectFCMTokenByUserID(int UserID)
        {
            List<UserMaster> list = Select();
            FCMViewModel response = new FCMViewModel();
            response = (from user in list
                        where user.UserID == UserID
                        select new FCMViewModel
                        {
                            WebFCMToken = user.WebFCMToken,
                            AppFCMToken = user.AppFCMToken,
                        }
                      ).FirstOrDefault();
            return response;
        }

        internal async Task<List<DashboardUserViewModel>> SelectUserBalanceSummaryForAdminDashborad(string Date)
        {
            int CurrentdateInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            int selectdateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            List<UserMaster> userList = Select();

            List<WalletMaster> walletList = new WalletMasterDal().Select();
            int count = walletList.Count;
            List<DashboardUserViewModel> response = new List<DashboardUserViewModel>();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (CurrentdateInt == selectdateInt)
                {
                    var Mainlist = (from User in userList
                                    where User.UserTypeID != (int)UserType.Admin && User.UserTypeID != (int)UserType.Support
                                    && User.UserTypeID != (int)UserType.Developer
                                    join userWalletMaster in context.UserWalletMaster on User.UserID equals userWalletMaster.UserID
                                    join wallets in walletList on userWalletMaster.WalletID equals wallets.WalletID

                                    where wallets.IsActive == true
                                    select new
                                    {
                                        UserTypeID = User.UserTypeID,
                                        Balance = (float)System.Math.Round(userWalletMaster.Balance, 2),
                                        WalletID = userWalletMaster.WalletID,
                                        WalletName = wallets.Name,
                                    }).ToList();

                    var grouplist = (from User in Mainlist
                                     group User by new { User.UserTypeID, User.WalletName } into grp
                                     select new
                                     {
                                         UserTypeID = grp.Key.UserTypeID,
                                         WalletName = grp.Key.WalletName,
                                         Count = grp.Count() / count,
                                         WalletBalance = grp.Sum(X => X.Balance)
                                     }).ToList();

                    response = (from User in grouplist
                                join mainUser in listUserType on User.UserTypeID equals mainUser.UserTypeID
                                orderby User.UserTypeID ascending
                                select new DashboardUserViewModel
                                {
                                    UserTypeID = mainUser.UserTypeID,
                                    UserType = mainUser.UserType,
                                    Count = User.Count,
                                    WalletName = User.WalletName,
                                    WalletBalance = User.WalletBalance,
                                }).ToList();

                    response = response.OrderBy(t => t.UserTypeID).Where(t => t.Count > 0).Distinct().ToList();
                }
                else
                {
                    List<UserBalanceHistory> oldBalList = new UserBalanceHistoryDAL().SelectByDOCINT(selectdateInt);
                    var Mainlist = (from User in userList
                                    join userWalletMaster in oldBalList on User.UserID equals userWalletMaster.UserID
                                    where User.UserTypeID != (int)UserType.Admin && User.UserTypeID != (int)UserType.Support
                                    && User.UserTypeID != (int)UserType.Developer

                                    select new
                                    {
                                        UserTypeID = User.UserTypeID,
                                        Balance = userWalletMaster.Balance,
                                        WalletID = userWalletMaster.WalletID
                                    }).ToList();

                    var grouplist = (from User in Mainlist
                                     group User by new { User.UserTypeID } into grp
                                     select new
                                     {
                                         UserTypeID = grp.Key.UserTypeID,
                                         Count = grp.Count() / count,
                                         Recharge = grp.Where(q => q.WalletID == (int)WalletCode.Recharge).Sum(X => X.Balance),
                                         DMT = grp.Where(q => q.WalletID == (int)WalletCode.DMT).Sum(X => X.Balance),
                                         AEPS = grp.Where(q => q.WalletID == (int)WalletCode.AEPS).Sum(X => X.Balance),
                                         TotalBalance = grp.Sum(X => X.Balance)
                                     }).ToList();

                    response = (from User in grouplist
                                join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                                orderby User.UserTypeID ascending
                                select new DashboardUserViewModel
                                {
                                    UserType = usertype.UserType,
                                    Count = User.Count,
                                    Recharge = User.Recharge,
                                    DMT = User.DMT,
                                    AEPS = User.AEPS,
                                    TotalBalance = User.TotalBalance
                                }).ToList();
                }

                await Task.Delay(1);

                return response;
            }
        }

        internal async Task<TargetViewModel> SelectUserTarget(int UserID)
        {
            List<UserMaster> userList = Select();

            var Mainlist = (from User in userList
                            where User.ParentID == UserID
                            select new
                            {
                                TargetAmount = User.TargetAmount,
                                ParentID = User.ParentID
                            }).ToList();

            var targetlist = (from User in Mainlist
                              group User by new { User.ParentID } into grp
                              select new
                              {
                                  TargetAmount = grp.Sum(x => x.TargetAmount),
                              }).FirstOrDefault();

            TargetViewModel response = new TargetViewModel();
            response.TotalTarget = targetlist == null ? 0 : targetlist.TargetAmount;
            await Task.Delay(1);
            return response;
        }

        internal string SelectAPIToken(int UserID)
        {
            return Select().FirstOrDefault(U => U.UserID == UserID).APIToken;
        }

        internal bool SelectIsChannel(int reatilerID)
        {
            bool IsChannel = false;
            try
            {
                int parentID = Select().FirstOrDefault(U => U.UserID == reatilerID).ParentID;

                UserMaster item = Select().FirstOrDefault(U => U.UserID == parentID);

                int SuperparentID = 0;

                if (item != null)
                {
                    SuperparentID = item.ParentID;
                }

                IsChannel = Select().FirstOrDefault(U => U.UserID == reatilerID).IsChannel;
                if (!IsChannel)
                {
                    UserMaster um = Select().FirstOrDefault(U => U.UserID == parentID);
                    if (um == null)
                    {
                        return IsChannel;
                    }
                    IsChannel = um.IsChannel;
                }
                if (!IsChannel)
                {
                    UserMaster um = Select().FirstOrDefault(U => U.UserID == SuperparentID);
                    if (um == null)
                    {
                        return IsChannel;
                    }

                    IsChannel = um.IsChannel;
                }
            }
            catch (Exception ex)
            {
            }
            return IsChannel;
        }

        internal bool CheckIsChannelUser(int UplineUserID)
        {
            bool IsChannel = false;
            return IsChannel = Select().FirstOrDefault(U => U.UserID == UplineUserID).IsChannel;
        }

        internal List<DownlineForDropDownResponse> SelectChannelUserForDropDown(int userID)
        {
            List<UserMaster> list = Select();

            int UserTypeID = SelectUserTypeID(userID);

            List<DownlineForDropDownResponse> members = null;

            if (UserTypeID == (int)UserType.Admin)
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.IsDelete == false
                           && User.IsActive == true
                           && User.IsChannel == true
                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
                return members;
            }

            if (UserTypeID == (int)UserType.SuperDistributer || UserTypeID == (int)UserType.Distributer)
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.ParentID == userID
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }

            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.UserID == userID
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }
            return members;
        }

        private decimal getCurrentBalance(int WalletID)
        {
            decimal Total = 0m;
            List<UserMaster> userList = Select().Where(User => User.UserTypeID != (int)UserType.Admin && User.UserTypeID != (int)UserType.Support).ToList();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var Mainlist = (from User in userList
                                join userWalletMaster in context.UserWalletMaster on User.UserID equals userWalletMaster.UserID
                                select new
                                {
                                    Balance = userWalletMaster.Balance,
                                    WalletID = userWalletMaster.WalletID
                                }).ToList();

                if (WalletID != 0)
                {
                    Mainlist = (from item in Mainlist
                                where item.WalletID == WalletID
                                select item).ToList();
                }

                Total = Mainlist.Sum(i => i.Balance);
            }
            return Total;
        }

        internal async Task<decimal> SelectUserCurrentBalance(string Date, int WalletID)
        {
            int CurrentdateInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            int selectdateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            int AddDayInt = Convert.ToInt32(Convert.ToDateTime(Date).AddDays(1).ToString(Messages.DOCIntegerFomrat));

            var Total = 0m;

            if (CurrentdateInt == selectdateInt)
            {
                Total = getCurrentBalance(WalletID);
            }
            else
            {
                List<UserMaster> userList = Select().Where(User => User.UserTypeID != (int)UserType.Admin && User.UserTypeID != (int)UserType.Support).ToList(); ;

                List<UserBalanceHistory> balList = new UserBalanceHistoryDAL().SelectByDOCINT(AddDayInt);

                var historylist = (from User in userList
                                   join userWalletMaster in balList on User.UserID equals userWalletMaster.UserID
                                   select new
                                   {
                                       Balance = userWalletMaster.Balance,
                                       WalletID = userWalletMaster.WalletID
                                   }).ToList();
                if (WalletID != 0)
                {
                    historylist = (from item in historylist
                                   where item.WalletID == WalletID
                                   select item).ToList();
                }
                Total = historylist.Sum(i => i.Balance);
            }

            await Task.Delay(1);
            return Total;
        }

        internal List<Headerlist> SelectTotalBalanceSummary(int SelectTypeID, int UserID)
        {
            List<UserMaster> userList = Select();
            List<Headerlist> headerlistdata = new List<Headerlist>();
            int UserTypeID = SelectUserTypeID(UserID);

            List<WalletMaster> walletList = new WalletMasterDal().Select();
            int count = walletList.Count;

            int LoginUsertypeID = SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var Mainlist = (from User in userList
                                where User.UserTypeID != (int)UserType.Admin
                                join userWalletMaster in context.UserWalletMaster on
                                User.UserID equals userWalletMaster.UserID
                                select new
                                {
                                    UserTypeID = User.UserTypeID,
                                    ParentID = User.ParentID,
                                    Balance = userWalletMaster.Balance,
                                    WalletID = userWalletMaster.WalletID
                                }).ToList();

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    Mainlist = (from item in Mainlist
                                where item.ParentID == UserID
                                select item).ToList();
                }

                if (SelectTypeID != 0)
                {
                    Mainlist = (from item in Mainlist
                                where item.UserTypeID == SelectTypeID
                                select item).ToList();
                }

                var TotalRechargeBalance = Mainlist.Where(q => q.WalletID == (int)WalletCode.Recharge).ToList().Sum(q => q.Balance);
                var TotalDMTBalance = Mainlist.Where(q => q.WalletID == (int)WalletCode.DMT).ToList().Sum(q => q.Balance);
                var TotalAEPSBalance = Mainlist.Where(q => q.WalletID == (int)WalletCode.AEPS).ToList().Sum(q => q.Balance);
                var TotalBalance = Mainlist.Sum(i => i.Balance);
                List<WalletMaster> wallets = new WalletMasterDal().Select();
                foreach (WalletMaster item in wallets)
                {
                    if (item.WalletID == (int)WalletCode.Recharge)
                    {
                        headerlistdata.Add(new Headerlist
                        {
                            name = item.ShortName,
                            amount = TotalRechargeBalance
                        });
                    }
                    if (item.WalletID == (int)WalletCode.DMT)
                    {
                        headerlistdata.Add(new Headerlist
                        {
                            name = item.ShortName,
                            amount = TotalDMTBalance
                        });
                    }
                    if (item.WalletID == (int)WalletCode.AEPS)
                    {
                        headerlistdata.Add(new Headerlist
                        {
                            name = item.ShortName,
                            amount = TotalAEPSBalance
                        });
                    }
                }

                headerlistdata.Add(new Headerlist
                {
                    name = "TotalBal",
                    amount = TotalBalance
                });

                return headerlistdata;
            }
        }

        internal List<SelectInActiveUserResponse> SelectInActiveUsers(SelectInActiveUsersRequest mainRequest)
        {
            List<SelectInActiveUserResponse> userList = new List<SelectInActiveUserResponse>();
            List<int> ActiveUsers = new List<int>();

            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            ActiveUsers = context.TransactionMaster
                .OrderBy(t => t.UserID)
                .Where(t => t.DOC > mainRequest.FromDate &&
                       t.UserID != Company.AdminUserID).Select(m => m.UserID).Distinct().ToList();

            List<int> AllUsers = new List<int>();
            int count = 0;
            List<UserMaster> allUsers = Select();

            if (mainRequest.UserTypeID == -99 && mainRequest.UserID != 0)
            {
                AllUsers = allUsers.OrderBy(u => u.UserID).Where(t => t.UserTypeID != (int)UserType.Admin && t.UserID == mainRequest.UserID).Select(m => m.UserID).Distinct().ToList();
                count = AllUsers.Count;
                AllUsers = AllUsers.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }
            else if (mainRequest.UserTypeID == -99 && mainRequest.UserID == 0)
            {
                AllUsers = allUsers.OrderBy(u => u.UserID).Where(t => t.UserTypeID != (int)UserType.Admin).Select(m => m.UserID).Distinct().ToList();
                count = AllUsers.Count;
                AllUsers = AllUsers.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }
            else if (mainRequest.UserTypeID != -99 && mainRequest.UserID != 0)
            {
                AllUsers = allUsers.OrderBy(u => u.UserID).Where(t => t.UserTypeID != (int)UserType.Admin && t.UserTypeID == mainRequest.UserTypeID && t.UserID == mainRequest.UserID).Select(m => m.UserID).Distinct().ToList();
                count = AllUsers.Count;
                AllUsers = AllUsers.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }
            else
            {
                AllUsers = allUsers.OrderBy(u => u.UserID).Where(t => t.UserTypeID != (int)UserType.Admin && t.UserTypeID == mainRequest.UserTypeID).Select(m => m.UserID).Distinct().ToList();
                count = AllUsers.Count;
                AllUsers = AllUsers.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            IEnumerable<int> InActiveUsers = AllUsers.Except(ActiveUsers);
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            foreach (int UserID in InActiveUsers)
            {
                SelectInActiveUserResponse user = (from child in allUsers
                                                   join parent in allUsers on child.ParentID equals parent.UserID
                                                   join usertype in listUserType on child.UserTypeID equals usertype.UserTypeID
                                                   where child.UserID == UserID
                                                   select new SelectInActiveUserResponse
                                                   {
                                                       FirmName = child.FirmName.ConcatNameMobileAliasName(child.MobileNumber, child.AliasName),
                                                       UserType = usertype.UserType,
                                                       ParentName = parent.FirmName.ConcatNameMobileAliasName(parent.MobileNumber, parent.AliasName)
                                                   }).FirstOrDefault();

                user.LastTransactionDate = new TransactionDAL().SelectLastTransactionDateByID(UserID);
                user.Balance = new UserWalletMasterDal().SelectBalanceInStringByUserID(UserID);
                user.Count = count;
                userList.Add(user);
            }
            return userList;
        }

        internal async Task<decimal> SelectCurrentBalanceByUserID(int UserID, string Date)
        {
            int CurrentdateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            int selectdateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            int AddDayInt = Convert.ToInt32(Convert.ToDateTime(Date).AddDays(1).ToString(Messages.DOCIntegerFomrat));

            List<UserMaster> userList = Select();
            List<WalletMaster> walletList = new WalletMasterDal().Select();
            var Total = 0m;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (CurrentdateInt == selectdateInt)
                {
                    var Mainlist = (from userWalletMaster in context.UserWalletMaster
                                    where userWalletMaster.UserID == UserID
                                    select new
                                    {
                                        Balance = userWalletMaster.Balance,
                                        WalletID = userWalletMaster.WalletID
                                    }).ToList();

                    Total = Mainlist.Sum(i => i.Balance);
                }
                else
                {
                    List<UserBalanceHistory> oldBalList = new UserBalanceHistoryDAL().SelectByDOCINT(AddDayInt);
                    var Mainlist = (from userWalletMaster in oldBalList
                                    where userWalletMaster.UserID == UserID
                                    select new
                                    {
                                        Balance = userWalletMaster.Balance,
                                        WalletID = userWalletMaster.WalletID
                                    }).ToList();

                    Total = Mainlist.Sum(i => i.Balance);
                }
            }
            return Total;
        }

        internal async Task<List<DashboardUserViewModel>> SelectUserBalanceSummaryForDistributorDashborad(int UserID)
        {
            List<UserMaster> userList = Select().Where(User => User.UserTypeID != (int)UserType.Admin
                                && User.UserTypeID != (int)UserType.Support
                                && User.ParentID == UserID).ToList();

            List<WalletMaster> walletList = new WalletMasterDal().Select();
            int count = walletList.Count;
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                var Mainlist = (from User in userList
                                join userWalletMaster in context.UserWalletMaster on User.UserID equals userWalletMaster.UserID
                                select new
                                {
                                    UserTypeID = User.UserTypeID,
                                    Balance = userWalletMaster.Balance,
                                    WalletID = userWalletMaster.WalletID
                                }).ToList();

                var grouplist = (from User in Mainlist
                                 group User by new { User.UserTypeID } into grp
                                 select new
                                 {
                                     UserTypeID = grp.Key.UserTypeID,
                                     Count = grp.Count() / count,
                                     Recharge = grp.Where(q => q.WalletID == (int)WalletCode.Recharge).Sum(X => X.Balance),
                                     DMT = grp.Where(q => q.WalletID == (int)WalletCode.DMT).Sum(X => X.Balance),
                                     AEPS = grp.Where(q => q.WalletID == (int)WalletCode.AEPS).Sum(X => X.Balance),
                                     TotalBalance = grp.Sum(X => X.Balance)
                                 }).ToList();

                var response = (from User in grouplist
                                join mainUser in listUserType on User.UserTypeID equals mainUser.UserTypeID
                                select new DashboardUserViewModel
                                {
                                    UserType = mainUser.UserType,
                                    Count = User.Count,
                                    Recharge = User.Recharge,
                                    DMT = User.DMT,
                                    AEPS = User.AEPS,
                                    TotalBalance = User.TotalBalance
                                }).ToList();

                await Task.Delay(1);

                return response;
            }
        }

        internal UserMaster SelectByVirtualAccountNumber(string VirtualAccountNumber)
        {
            List<UserMaster> list = Select();

            return list.FirstOrDefault(U => U.VirtualAccountNumber == VirtualAccountNumber);
        }

        internal List<SelectDownlineReportResponse> SelectDownlineReport(SelectDownlineReportRequest mainRequest)
        {
            List<UserMaster> list = Select();
            List<WalletMaster> walletList = new WalletMasterDal().Select();
            List<SelectDownlineReportResponse> resposne = new List<SelectDownlineReportResponse>();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                resposne = (from user in list
                            join collection in context.CashCollectionMaster on user.UserID equals collection.UserID
                            join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                            where user.ParentID == mainRequest.UserID
                            orderby collection.Outstanding descending

                            select new SelectDownlineReportResponse
                            {
                                UserID = user.UserID,
                                FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                Role = usertype.UserType,
                                Outstanding = collection.Outstanding
                            }
                            ).ToList();

                List<UserWalletMaster> userWalletMasterList = context.UserWalletMaster.ToList();

                foreach (SelectDownlineReportResponse userRes in resposne)
                {
                    userRes.Wallets = (from walletMaster in walletList
                                       join wallet in userWalletMasterList on walletMaster.WalletID equals (wallet.WalletID)
                                       where wallet.UserID == userRes.UserID
                                       select new UserWallet
                                       {
                                           Name = walletMaster.Name,
                                           Balance = wallet.Balance,
                                           ShortName = walletMaster.ShortName,
                                           WalletID = walletMaster.WalletID
                                       }).ToList();
                }
            }
            return resposne;
        }

        internal List<DownlineForDropDownResponse> SelectRetailerAndAPIUserForDropDown(int UserID, int UserTypeID)
        {
            List<UserMaster> list = Select();
            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       where User.UserTypeID == UserTypeID
                       && User.IsDelete == false
                       && User.IsActive == true
                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();

            return members;
        }

        internal List<UserBackup> SelectUserBackUp()
        {
            List<UserMaster> list = Select();
            List<UserBackup> Users = new List<UserBackup>();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            Users = (from User in list
                     join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                     join Parent in list on User.ParentID equals Parent.UserID
                     where User.UserTypeID != (int)UserType.Admin
                     select new UserBackup
                     {
                         Role = usertype.UserType,
                         MobileNumber = User.MobileNumber,
                         UserID = User.UserID,
                         FirmName = User.FirmName,
                         ParentFirmName = Parent.FirmName,
                         ParentMobileNumber = Parent.MobileNumber
                     }).ToList();

            if (Users == null)
                return Users;

            return Users;
        }

        internal List<UserBackup> SelectUserWallet(UserBackup user)
        {
            List<UserBackup> Users = new List<UserBackup>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                List<WalletMaster> walletList = new WalletMasterDal().Select();

                UserWallet[] wallets = (from walletMaster in walletList
                                        join wallet in context.UserWalletMaster
                                        on walletMaster.WalletID equals (wallet.WalletID)
                                        where wallet.UserID == user.UserID
                                        select new UserWallet
                                        {
                                            Name = walletMaster.Name,
                                            Balance = wallet?.Balance ?? 0,
                                            ShortName = walletMaster?.ShortName ?? ""
                                        }).ToArray();
            }
            return Users;
        }

        internal List<SelectDatewiseResponse> SelectDatewise(SelectDatewiseRequest mainRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<UserMaster> list = Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            return (from user in list
                    where user.DOCInt >= From && user.DOCInt <= To && user.IsDelete == false
                    join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                    join ul in list on user.JoiningByUserID equals ul.UserID
                    join usertypeUL in listUserType on ul.UserTypeID equals usertypeUL.UserTypeID
                    orderby user.DOC descending
                    select new SelectDatewiseResponse
                    {
                        UserID = user.UserID,
                        Role = usertype.UserType,
                        RegistrationType = user.RegistrationType,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        JoiningBy = ul.FirmName + "  -  " + ul.MobileNumber + "  -  " + usertypeUL.UserType,
                        DOC = user.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        IsKYCApproved = user.IsKYCApproved,
                        IsKYC = user.IseKYC,

                    }).ToList();
        }

        internal SelectUserCommissionByUserIDResponse SelectUserCommissionByUserID(int userID)
        {
            List<UserMaster> userList = Select();

            SelectUserCommissionByUserIDResponse response = null;
            response = (from user in userList
                        where user.UserID == userID
                        select new SelectUserCommissionByUserIDResponse
                        {
                            Comm = user.Comm,
                            CommissionCategory = user.CommissionCategory,
                            CommType = CommissionType.Flexi,
                        }).FirstOrDefault();

            return response;
        }

        internal List<UserEmailIDViewModel> SelectAllEmailIDs()
        {
            List<UserEmailIDViewModel> response = null;

            List<UserMaster> userList = Select();

            response = (from user in userList
                        where user.IsDelete == false
                        orderby user.DOC descending
                        select new UserEmailIDViewModel
                        {
                            EmailID = user.EmailID,
                            UserID = user.UserID
                        }).ToList();

            return response;
        }

        internal string SelectResponseTypeByUserID(int UserID)
        {
            UserMaster um = Select().FirstOrDefault(U => U.UserID == UserID &&
            (U.UserTypeID == (int)UserType.APIUser || U.UserTypeID == (int)UserType.Retailer));
            if (um == null)
            {
                return "";
            }

            if (um.ResponseType == null || um.ResponseType.Length <= 2)
            {
                return "";
            }
            return um.ResponseType;
        }

        private List<UserMaster> SelectDeleted()
        {
            using var context = new DatabaseContext();
            return context.UserMaster.Where(U => U.IsDelete == true).ToList();
        }

        internal UserMaster SelectDeletedUsersByID(int UserID)
        {
            List<UserMaster> list = SelectDeleted();

            return list.FirstOrDefault(U => U.UserID == UserID);
        }

        internal List<DeletedUsersResponse> SelectAllDeleteUsers(SelectDeletedUsersRequest mainRequest, int UserID)
        {
            List<UserMaster> list = SelectDeleted();
            if (mainRequest.UserTypeID > 0)
            {
                list = (from item in list
                        where item.UserTypeID == mainRequest.UserTypeID
                        select item).ToList();
            }
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            List<DeletedUsersResponse> members = null;

            members = (from User in list
                       join usertype in listUserType on User.UserTypeID equals usertype.UserTypeID
                       select new DeletedUsersResponse
                       {
                           UserID = User.UserID,
                           ParentID = User.ParentID,
                           UserTypeID = User.UserTypeID,
                           UserType = usertype.UserType,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           MobileNumber = User.MobileNumber,
                       }).ToList();
            if (UserID != Company.AdminUserID)
            {
                members = (from item in members
                           where item.ParentID == UserID
                           select item).ToList();
            }

            return members;
        }

        public int SelectUserTypeIDForDeletedUsers(int UserID)
        {
            List<UserMaster> list = SelectDeleted();
            return list.FirstOrDefault(U => U.UserID == UserID).UserTypeID;
        }

        internal List<DownlineForDropDownResponse> SelectSDAndDistForDropDown()
        {
            List<UserMaster> list = Select();
            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       where (User.UserTypeID == (int)UserType.Distributer || User.UserTypeID == (int)UserType.SuperDistributer)

                       && User.IsActive == true
                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();

            return members;
        }

        internal List<DownlineForDropDownResponse> SelectByUserTypeID(int UserTypeID, int UserID)
        {
            List<UserMaster> list = Select();
            List<DownlineForDropDownResponse> members = null;

            members = (from User in list
                       where User.UserTypeID == UserTypeID
                        && User.IsActive == true
                        && User.UserID != UserID
                       select new DownlineForDropDownResponse
                       {
                           UserID = User.UserID,
                           FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                       }).ToList();

            return members;
        }

        internal List<RoleWiseUserViewModel> SelectByRole(int UserTypeID)
        {
            List<UserMaster> list = Select();
            List<RoleWiseUserViewModel> ReturnResponse = new List<RoleWiseUserViewModel>();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            ReturnResponse = (from user in list
                              join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                              join parent in list on user.ParentID equals parent.UserID
                              where user.UserTypeID == UserTypeID
                                                          && user.IsActive == true
                                                          && user.UserID != 1
                              select new RoleWiseUserViewModel
                              {
                                  FirmName = user.FirmName.ConcatNameMobileAliasNameRole(user.MobileNumber, user.AliasName, usertype.UserType),
                                  ParentName = parent.FirmName.ConcatNameMobileAliasName(parent.MobileNumber, parent.AliasName),

                                  UserType = usertype.UserType,
                                  UserID = user.UserID,
                                  MobileNumber = user.MobileNumber,
                              }
                          ).ToList();

            return ReturnResponse;
        }

        internal List<RoleWiseUserViewModel> SelectByRoleUserBulk(UserTypeSelectRequest selectByTypeRequest)
        {
            List<UserMaster> list = Select();

            List<RoleWiseUserViewModel> ReturnResponse = new List<RoleWiseUserViewModel>();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            ReturnResponse = (from user in list
                              join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                              join parent in list on user.ParentID equals parent.UserID

                              where user.UserTypeID == selectByTypeRequest.UserTypeID
                                                          && user.IsActive == true
                                                          && user.UserID != 1
                              select new RoleWiseUserViewModel
                              {
                                  FirmName = user.FirmName.ConcatNameMobileAliasNameRole(user.MobileNumber, user.AliasName, usertype.UserType),
                                  ParentName = parent.FirmName.ConcatNameMobileAliasName(parent.MobileNumber, parent.AliasName),

                                  UserType = usertype.UserType,
                                  UserID = user.UserID,
                                  MobileNumber = user.MobileNumber,
                              }
                          ).ToList();

            return ReturnResponse;
        }

        internal int SelectLastUserID()
        {
            int UserID = 0;
            List<UserMaster> list = Select();
            UserID = (from user in list
                      orderby user.UserID descending
                      select user.UserID).FirstOrDefault();

            return (UserID + 1);
        }


        internal async Task<bool> SelectActiveUserByMobileNumber(string mobileNumber)
        {
            UserMaster um = Select().FirstOrDefault(U => U.MobileNumber == mobileNumber);
            if (um == null)
            {
                return true;
            }

            return um.IsActive;
        }

        internal List<UserTypeWithCount> SelectUserTypeWithCount()
        {
            List<UserMaster> userList = Select().Where(User => User.UserTypeID != (int)UserType.Admin
            && User.UserTypeID != (int)UserType.Support
            && User.UserTypeID != (int)UserType.Developer).ToList(); ;

            List<UserTypeWithCount> ReturnResponse = new List<UserTypeWithCount>();

            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            var Mainlist = (from User in userList
                            select new
                            {
                                UserTypeID = User.UserTypeID,
                            }).ToList();

            var grouplist = (from User in Mainlist
                             group User by new { User.UserTypeID } into grp
                             select new
                             {
                                 UserTypeID = grp.Key.UserTypeID,
                                 Count = grp.Count(),
                             }).ToList();

            var grouplist1 = (from list in grouplist
                              group list by new { list.UserTypeID, list.Count } into grp1
                              select new
                              {
                                  UserTypeID = grp1.Key.UserTypeID,
                                  Count = grp1.Count(),
                                  TotalCount = grp1.Sum(X => X.Count)
                              }).ToList();

            ReturnResponse = (from User in grouplist
                              join mainUser in listUserType on User.UserTypeID equals mainUser.UserTypeID
                              orderby User.UserTypeID ascending
                              select new UserTypeWithCount
                              {
                                  UserTypeID = mainUser.UserTypeID,
                                  //TotalCount = User.TotalCount,
                                  UserType = mainUser.UserType,
                                  Count = User.Count,
                              }).ToList();

            ReturnResponse = ReturnResponse.OrderBy(t => t.UserTypeID).Where(t => t.Count > 0).Distinct().ToList();

            return ReturnResponse;
        }

        internal List<WalletView> SelectWalletBalanceUserTypewise(int UserTypeID, string Date)
        {
            int CurrentdateInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            int selectdateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            List<UserMaster> userList = Select().Where(User => User.UserTypeID == UserTypeID).ToList();

            List<WalletMaster> walletList = new WalletMasterDal().Select().Where(wallets => wallets.IsActive == true).ToList();
            int count = walletList.Count;
            List<WalletView> response = new List<WalletView>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                if (CurrentdateInt == selectdateInt)
                {
                    var Mainlist = (from User in userList
                                    join userWalletMaster in context.UserWalletMaster on User.UserID equals userWalletMaster.UserID
                                    join wallets in walletList on userWalletMaster.WalletID equals wallets.WalletID

                                    select new
                                    {
                                        Balance = (float)System.Math.Round(userWalletMaster.Balance, 2),
                                        WalletName = wallets.Name,
                                    }).ToList();

                    response = (from User in Mainlist
                                group User by new { User.WalletName } into grp
                                select new WalletView
                                {
                                    WalletName = grp.Key.WalletName,
                                    WalletBalance = grp.Sum(X => X.Balance)
                                }).ToList();
                }
                else
                {
                    List<UserBalanceHistory> balList = new UserBalanceHistoryDAL().SelectByDOCINT(selectdateInt);

                    var Mainlist = (from User in userList
                                    join userWalletMaster in balList on User.UserID equals userWalletMaster.UserID
                                    join wallets in walletList on userWalletMaster.WalletID equals wallets.WalletID

                                    select new
                                    {
                                        Balance = (float)System.Math.Round(userWalletMaster.Balance, 2),
                                        WalletName = wallets.Name,
                                    }).ToList();
                    response = (from User in Mainlist
                                group User by new { User.WalletName } into grp
                                select new WalletView
                                {
                                    WalletName = grp.Key.WalletName,
                                    WalletBalance = grp.Sum(X => X.Balance)
                                }).ToList();
                }

                return response;
            }
        }

        internal List<UserTypeWithCount> SelectUserTypeWithCountForDist(int UserID)
        {
            List<UserMaster> userList = Select();

            List<UserTypeWithCount> ReturnResponse = new List<UserTypeWithCount>();

            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            var Mainlist = (from User in userList
                            where User.UserTypeID != (int)UserType.Admin && User.UserTypeID != (int)UserType.Support
                             && User.UserTypeID != (int)UserType.Developer
                             && User.ParentID == UserID
                            select new
                            {
                                UserTypeID = User.UserTypeID,
                            }).ToList();

            var grouplist = (from User in Mainlist
                             group User by new { User.UserTypeID } into grp
                             select new
                             {
                                 UserTypeID = grp.Key.UserTypeID,
                                 Count = grp.Count(),
                             }).ToList();

            var grouplist1 = (from list in grouplist
                              group list by new { list.UserTypeID, list.Count } into grp1
                              select new
                              {
                                  UserTypeID = grp1.Key.UserTypeID,
                                  Count = grp1.Count(),
                                  TotalCount = grp1.Sum(X => X.Count)
                              }).ToList();

            ReturnResponse = (from User in grouplist
                              join mainUser in listUserType on User.UserTypeID equals mainUser.UserTypeID
                              orderby User.UserTypeID ascending
                              select new UserTypeWithCount
                              {
                                  UserTypeID = mainUser.UserTypeID,
                                  //TotalCount = User.TotalCount,
                                  UserType = mainUser.UserType,
                                  Count = User.Count,
                              }).ToList();

            return ReturnResponse;
        }

        internal List<WalletView> SelectWalletBalanceUserTypewiseForDist(int UserTypeID, int UserID)
        {
            List<UserMaster> userList = Select();

            List<WalletMaster> walletList = new WalletMasterDal().Select();
            int count = walletList.Count;
            List<WalletView> response = new List<WalletView>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                var Mainlist = (from User in userList
                                where User.UserTypeID == UserTypeID

                                join userWalletMaster in context.UserWalletMaster on
                                User.UserID equals userWalletMaster.UserID

                                join wallets in walletList on
                            userWalletMaster.WalletID equals wallets.WalletID
                                where wallets.IsActive == true && User.ParentID == UserID
                                select new
                                {
                                    Balance = (float)System.Math.Round(userWalletMaster.Balance, 2),
                                    WalletName = wallets.Name,
                                }).ToList();

                response = (from User in Mainlist
                            group User by new { User.WalletName } into grp
                            select new WalletView
                            {
                                WalletName = grp.Key.WalletName,
                                WalletBalance = grp.Sum(X => X.Balance)
                            }).ToList();

                return response;
            }
        }

        internal List<UserOnbordingListResponse> SelectUserOnbordingList(SelectUserOnbordingListRequest mainRequest, int userID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<UserMaster> userList = Select();

            List<UserOnbordingListResponse> response = new List<UserOnbordingListResponse>();

            var Mainlist = (from User in userList
                            where User.IsActive == true &&
                            (mainRequest.UserID == 0 ? 2 > 1 : User.UserID == mainRequest.UserID)
                            select new
                            {
                                UserID = User.UserID,
                                ICICIMerchantID = User.ICICI_Static_GUID,
                                FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                                PaysprintMerchantID = User.PaysprintAEPSAgentID,
                                UpdatedDOC = User.UpdatedDOC
                            }).ToList();

            if (mainRequest.ProviderID == Company.ICICIProviderID)
            {
                response = (from User in Mainlist
                            select new UserOnbordingListResponse
                            {
                                UserID = User.UserID,
                                FirmName = User.FirmName,
                                MerchantCode = User.ICICIMerchantID,
                                DOC = User.UpdatedDOC.LongFormatDate(),
                            }).ToList();

                return response;
            }

            if (mainRequest.ProviderID == Company.PaysprintProviderID)
            {
                List<PaysprintOnboardedUser> psList = new PaysprintOnboardedUserDAL().Select();

                response = (from User in Mainlist
                            join onborder in psList on User.UserID equals onborder.UserID
                            where onborder.DOCInt >= FromDate
                            && onborder.DOCInt <= ToDate
                            orderby onborder.DOC descending
                            select new UserOnbordingListResponse
                            {
                                UserID = User.UserID,
                                FirmName = User.FirmName,
                                MerchantCode = User.PaysprintMerchantID,
                                DOC = onborder.DOC.LongFormatDate(),
                            }).DistinctBy(x => x.UserID).ToList();

                return response;
            }

            return response;
        }
        internal List<UserOnbordingListResponse> SelectUserOnbordingStatusCheckList(SelectUserOnbordingListRequest mainRequest)
        {
            List<UserMaster> userList = Select();

            List<UserOnbordingListResponse> response = new List<UserOnbordingListResponse>();

            var Mainlist = (from User in userList
                            where User.IsActive == true &&
                             User.UserTypeID == (int)UserType.Retailer &&
                            (mainRequest.UserID == 0 ? 2 > 1 : User.UserID == mainRequest.UserID)
                            select new
                            {
                                UserID = User.UserID,
                                ICICIMerchantID = User.ICICI_Static_GUID,
                                FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                                PaysprintMerchantID = User.PaysprintAEPSAgentID,
                                MobileNumber = User.MobileNumber,
                            }).ToList();

            if (mainRequest.ProviderID == Company.ICICIProviderID)
            {
                response = (from User in Mainlist
                            select new UserOnbordingListResponse
                            {
                                UserID = User.UserID,
                                FirmName = User.FirmName,
                                MerchantCode = User.ICICIMerchantID,
                                MobileNumber = User.MobileNumber
                            }).ToList();

                return response;
            }

            if (mainRequest.ProviderID == Company.PaysprintProviderID)
            {
                response = (from User in Mainlist
                            select new UserOnbordingListResponse
                            {
                                UserID = User.UserID,
                                FirmName = User.FirmName,
                                MerchantCode = User.PaysprintMerchantID,
                                MobileNumber = User.MobileNumber
                            }).DistinctBy(x => x.UserID).ToList();

                return response;
            }

            return response;
        }

        internal List<DownlineForDropDownResponse> SelectUserForDropDownForCashCollection(int userID)
        {
            List<UserMaster> list = Select();

            int UserTypeID = SelectUserTypeID(userID);

            List<DownlineForDropDownResponse> members = null;

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
                return members;
            }

            if (UserTypeID == (int)UserType.SuperDistributer || UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SDFOS)
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.ParentID == userID
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }

            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                members = (from User in list
                           where User.UserTypeID != (int)UserType.Admin
                           && User.UserID == userID
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }
            return members;
        }

        internal UserMaster SelectByReferralCode(string ReferralCode)
        {
            return Select().FirstOrDefault(U => U.ReferralCode == ReferralCode);
        }

        internal List<DownlineForDropDownResponse> SelectOnlyParentUserForDropdown(int userID)
        {
            List<UserMaster> list = Select();
            int UserTypeID = SelectUserTypeID(userID);

            List<DownlineForDropDownResponse> members = null;
            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                members = (from User in list
                           where (User.UserTypeID == (int)UserType.Admin
                           || User.UserTypeID == (int)UserType.SuperDistributer
                           || User.UserTypeID == (int)UserType.Distributer)
                           && User.IsDelete == false
                           && User.IsActive == true

                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
                return members;
            }

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                members = (from User in list
                           where User.ParentID == userID
                           && User.UserTypeID == (int)UserType.Distributer
                           && User.IsDelete == false
                           && User.IsActive == true
                           select new DownlineForDropDownResponse
                           {
                               UserID = User.UserID,
                               FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                           }).ToList();
            }

            return members;
        }

        internal List<eKYCViewModel> SelecteKYCUserDetailsByUserID(int userID)
        {
            List<UserMaster> list = Select();

            return (from User in list
                    where User.UserID == userID
                    && User.IsDelete == false
                    && User.IseKYC == true

                    select new eKYCViewModel
                    {
                        PersonName = User.PersonName,
                        Address = User.Address,
                        CityName = User.CityName,
                        Pincode = User.Pincode,
                        ProfileImagePath = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                    }).ToList();
        }

        internal bool IsAdminByMobileNumber(string MobileNumber)
        {
            if (MobileNumber == null || MobileNumber.Trim().Length <= 8)
                return false;

            if (Company.AdminMobileNumber == MobileNumber)
            {
                return true;
            }

            if (Company.AdminMobileNumber.Length >= 8 &&
                Company.AdminMobileNumber != MobileNumber)
            {
                return false;
            }

            UserMaster item = Select().FirstOrDefault(U => U.MobileNumber == MobileNumber);
            if (item == null)
                return false;
            if (item.UserID == Company.AdminUserID)
            {
                Company.AdminMobileNumber = item.MobileNumber;
                return true;
            }
            return false;
        }

        internal UserMaster SelectParentByUserID(int UserID)
        {
            List<UserMaster> list = Select();
            UserMaster obj = new UserMaster();

            obj = (from Child in list
                   join Parent in list on Child.ParentID equals Parent.UserID
                   where Child.UserID == UserID
                   select Parent).FirstOrDefault();

            return obj;
        }

        public List<UserMaster> SelectCommissionHoldedUSer()
        {
            List<UserMaster> list = Select();
            return list.Where(U => U.IsDelete == false && U.IsActive == true && U.IsCommissionHold == true).ToList();
        }

        internal bool IsDuplicatePan(string PANNo, int UserID)
        {
            List<UserMaster> list = Select();
            UserMaster obj = (from item in list
                              where item.PANNo.EqualsIgnoreCase(PANNo)
                              && item.UserID != UserID
                              select item).FirstOrDefault();
            if (obj == null)
                return true;
            return false;
        }
        internal bool IsDublicatePanFindWithUserType(string PANNo, int UserTypeID, int UserID)
        {
            List<UserMaster> list = Select();
            UserMaster obj = (from item in list
                              where item.PANNo.EqualsIgnoreCase(PANNo)
                               && item.UserTypeID == UserTypeID
                              && item.UserID != UserID
                              select item).FirstOrDefault();
            if (obj == null)
                return true;
            return false;
        }

        internal bool IsDuplicateAadhar(string AadharNo, int UserID)
        {
            List<UserMaster> list = Select();
            UserMaster obj = (from item in list
                              where item.AadharNo.EqualsIgnoreCase(AadharNo)
                               && item.UserID != UserID
                              select item).FirstOrDefault();
            if (obj == null)
                return true;
            return false;
        }
        internal bool IsDublicateAadharFindWithUserType(string AadharNo, int UserTypeID, int UserID)
        {
            List<UserMaster> list = Select();
            UserMaster obj = (from item in list
                              where item.AadharNo.EqualsIgnoreCase(AadharNo)
                               && item.UserTypeID == UserTypeID && item.UserID != UserID
                              select item).FirstOrDefault();
            if (obj == null)
                return true;
            return false;
        }

        internal bool IsDuplicateGST(string GSTNo, int UserID)
        {
            List<UserMaster> list = Select();
            UserMaster obj = (from item in list
                              where item.GSTNo.EqualsIgnoreCase(GSTNo)
                               && item.UserID != UserID
                              select item).FirstOrDefault();
            if (obj == null)
                return true;
            return false;
        }

        internal bool IsValidforCollection(string customer_code, string bene_account_no)
        {
            List<UserMaster> list = Select();
            UserMaster obj = (from item in list
                              where item.AliasName == customer_code
                                                && item.VirtualAccountNumber == bene_account_no
                              select item).FirstOrDefault();
            if (obj == null)
                return false;
            return true;
        }

        internal int SelectLastUserID(int UserTypeID)
        {
            int UserID = 0;
            List<UserMaster> list = Select();
            UserID = (from user in list
                      where user.UserTypeID == UserTypeID
                      orderby user.UserID descending
                      select user.UserID).FirstOrDefault();

            return (UserID + 1);
        }

        internal string SelectByRefenceID(string ReferenceID)
        {
            List<UserMaster> userMasters= Select();
            var ReferenceDetails = userMasters.FirstOrDefault(emp => emp.AliasName == ReferenceID);
            string Name = ReferenceDetails?.PersonName.ConcatNameMobileAliasName(ReferenceDetails?.MobileNumber, ReferenceDetails?.AliasName);
            return Name;
           
        }
    }
}