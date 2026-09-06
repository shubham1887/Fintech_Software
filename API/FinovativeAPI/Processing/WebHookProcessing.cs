using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Requests.WebHookSetting;
using Project.Response.WebHookSetting;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class WebHookProcessing
    {
        public void FundTransferWebHook(int SenderID, int ReceiverID, decimal Amount, int WalletID, string UTRNumber, float OldOutstanding, string FundTransactionID, int ServiceID, string TransactionDescription)
        {
            Response.WebHookSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new WebHookSettingBal().SelectByType(WebHookTypes.FundTransfer);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel == null) return;

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserWalletMaster SenderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(SenderID, WalletID);

            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);

            viewModel.URL = viewModel.URL.Replace("%20", " ");

            if (!viewModel.IsActive)
            {
                return;
            }

            ReplacementParam param = new ReplacementParam();
            param.BaseURL = viewModel.URL;
            param.MethodType = viewModel.MethodType;
            param.ReceiverMobileNumber = Receiver.MobileNumber;
            param.SenderMobileNumber = Sender.MobileNumber;
            param.Wallet = Wallet.ShortName;
            param.Amount = Amount.TwoDigitDecimal();
            param.UTRNumber = UTRNumber;
            param.Remark = TransactionDescription;

            string URLReplace = URLReplaceWithparameter(param);

            string HttpResponse = "";
            if (viewModel.MethodType.EqualsIgnoreCase("GET"))
            {
                HttpResponse = HttpProcessing.CallGetMethod(URLReplace, 10);
            }
            else
            {
                HttpResponse = HttpProcessing.CallPostMethod(URLReplace, viewModel.HeaderJson);
            }

            Task.Run(() =>
            {
                //new Logger().Info("FundTransferWebHook", "FundTransferWebHookForSender", URLReplace + "::" + HttpResponse);

                WebHookLogRequest mainRequest = new WebHookLogRequest
                {
                    Type = WebHookTypes.FundTransfer,
                    RequestURL = URLReplace,
                    Response = HttpResponse
                };

                new WebHookLogBal().Insert(mainRequest);
            });
        }

        public string URLReplaceWithparameter(ReplacementParam param)
        {
            string str;
            try
            {
                str = param.BaseURL;

                str = getURLEncode(param.MethodType, str, "[SenderMobileNumber]", param.SenderMobileNumber);
                str = getURLEncode(param.MethodType, str, "[ReceiverMobileNumber]", param.ReceiverMobileNumber);
                str = getURLEncode(param.MethodType, str, "[Wallet]", param.Wallet);
                str = getURLEncode(param.MethodType, str, "[Amount]", param.Amount);
                str = getURLEncode(param.MethodType, str, "[Remark]", param.Remark);
                str = getURLEncode(param.MethodType, str, "[UTRNumber]", param.UTRNumber);

                str = getURLEncode(param.MethodType, str, "[FirmName]", param.FirmName);
                str = getURLEncode(param.MethodType, str, "[PersonName]", param.PersonName);
                str = getURLEncode(param.MethodType, str, "[MobileNumber]", param.MobileNumber);
                str = getURLEncode(param.MethodType, str, "[EmailId]", param.EmailId);
                str = getURLEncode(param.MethodType, str, "[State]", param.State);
                str = getURLEncode(param.MethodType, str, "[Address]", param.Address);
                str = getURLEncode(param.MethodType, str, "[Pincode]", param.Pincode);
                str = getURLEncode(param.MethodType, str, "[GSTNo]", param.GSTNo);
                str = getURLEncode(param.MethodType, str, "[PANNo]", param.PANNo);
                str = getURLEncode(param.MethodType, str, "[AadharNo]", param.AadharNo);
                str = getURLEncode(param.MethodType, str, "[WhatsAppNumber]", param.WhatsAppNumber);
                str = getURLEncode(param.MethodType, str, "[ParentNo]", param.ParentNo);

                str = getURLEncode(param.MethodType, str, "[SystemName]", Company.ClientName);
            }
            catch (Exception ex)
            {
                str = "";
                new Logger().Write(ex);
            }

            return str;
        }

        private string getURLEncode(string methodType, string mainStr, string replacewith, string replaceWord)
        {
            if (methodType.EqualsIgnoreCase(MethodType.Get))
            {
                mainStr = mainStr.Replace(replacewith, System.Web.HttpUtility.UrlEncode(replaceWord));
            }
            else
            {
                mainStr = mainStr.Replace(replacewith, replaceWord);
            }
            return mainStr;
        }

        public void SignupWebHook(int userID)
        {
            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user == null) return;

            Response.WebHookSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new WebHookSettingBal().SelectByType(WebHookTypes.Signup);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null) return;

            viewModel.URL = viewModel.URL.Replace("%20", " ");

            if (!viewModel.IsActive)
            {
                return;
            }

            string Parent = "";
            UserMaster userparent = new UserMaster();
            try
            {
                userparent = new UserDAL().SelectByIDFromCache(user.ParentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (userparent == null)
                Parent = "";

            Parent = userparent.MobileNumber;

            ReplacementParam param = new ReplacementParam();
            param.BaseURL = viewModel.URL;
            param.MethodType = viewModel.MethodType;
            param.FirmName = user.FirmName;
            param.MobileNumber = user.MobileNumber;
            param.WhatsAppNumber = user.WhatsAppNumber;
            param.PersonName = user.PersonName;
            param.Address = user.Address;
            param.Pincode = user.Pincode;
            param.GSTNo = user.GSTNo;
            param.PANNo = user.PANNo;
            param.AadharNo = user.AadharNo;
            param.EmailId = user.EmailID;
            param.ParentNo = Parent;

            string URLReplace = URLReplaceWithparameter(param);

            string HttpResponse = "";
            if (viewModel.MethodType.EqualsIgnoreCase("GET"))
            {
                HttpResponse = HttpProcessing.CallGetMethod(URLReplace, 10);
            }
            else
            {
                HttpResponse = HttpProcessing.CallPostMethod(URLReplace, viewModel.HeaderJson);
            }

            Task.Run(() =>
            {
                // new Logger().Info("WebHookProcessing", "SignupWebHook", URLReplace + "::" + HttpResponse);

                WebHookLogRequest mainRequest = new WebHookLogRequest
                {
                    Type = WebHookTypes.Signup,
                    RequestURL = URLReplace,
                    Response = HttpResponse
                };

                new WebHookLogBal().Insert(mainRequest);
            });
        }
    }
}