using Project.DAL;
using Project.Model;
using Project.Requests.Transaction;
using Project.Response.FCMResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class FCMSelection
    {
        public void SendTransactionMessage(int UserID, RechargeRequest mainrequest, string Status)
        {
            ///BackgroundJob.Enqueue(() =>
            try
            {
                SendTransactionMessageCore(UserID, mainrequest, Status);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void SendTransactionMessageCore(int UserID, RechargeRequest mainrequest, string Status)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(UserID);

            Response.Operator.ViewModel OperatorModel = new OperatorMasterDAL().SelectDetailsByOperatorID(mainrequest.OperatorID);

            string message = Message.TransactionMessage.FCM_Message_Mobile_Transaction_Pending;
            message = message.Replace("[ConsumerNumber]", mainrequest.ConsumerNumber);
            message = message.Replace("[Amount]", mainrequest.Amount.TwoDigitDecimal());
            message = message.Replace("[Operator]", OperatorModel.OperatorName);
            message = message.Replace("[Status]", Status);
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, Status);
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, status: Status, defaultPlatform: PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            SendAPPBarRefreshFCM(UserID);
        }

        public void SendDMTTransactionMessage(int UserID, MoneyTransferRequest mainRequest, string Status)
        {
            //BackgroundJob.Enqueue(() =>
            SendDMTTransactionMessageCore(UserID, mainRequest, Status);
        }

        private void SendDMTTransactionMessageCore(int UserID, MoneyTransferRequest mainRequest, string Status)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(UserID);

            string message = Message.TransactionMessage.FCM_Message_DMT_Transaction_Pending;
            message = message.Replace("[AccountNumber]", mainRequest.AccountNo);
            message = message.Replace("[Amount]", mainRequest.Amount.TwoDigitDecimal());
            message = message.Replace("[Status]", Status);
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, Status);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, Status);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            SendAPPBarRefreshFCM(UserID);
        }

        public void SendComplaintMessageAdmin(string ConsumerNumber, string FirmName, string message)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(Company.AdminUserID);

            message = message.Replace("[ConsumerNumber]", ConsumerNumber);
            message = message.Replace("[FirmName]", FirmName);

            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, "Complaint", "DEFAULT", 10000, PlatformCode.Web);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, "Complaint", "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            SendAPPBarRefreshFCM(Company.AdminUserID);
        }

        public void SendComplaintMessage(string ConsumerNumber, string Amount, string FirmName, int UserID, string message)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(UserID);
            message = message.Replace("[ConsumerNumber]", ConsumerNumber);
            message = message.Replace(" [Amount]", Amount);
            message = message.Replace("[FirmName]", FirmName);

            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, "Complaint", "DEFAULT", 10000, PlatformCode.Web);
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, "Complaint", "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            SendAPPBarRefreshFCM(UserID);
        }

        public void SendFundTransferMessage(decimal Amount, string SenderFirmName, int ReceiverID, string message)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(ReceiverID);
            if (currentUserFCM == null)
            {
                return;
            }

            //string message = Message.FundMessage.FCM_Message_Fund_Transfer;

            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
            message = message.Replace("[FirmName]", SenderFirmName);

            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, "Fund", "DEFAULT", 10000, PlatformCode.Web);
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, "Fund", "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            SendAPPBarRefreshFCM(ReceiverID);
        }

        public void SendKYCRequest()
        {
            //FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(Company.AdminUserID);
            SendAPPBarRefreshFCM(Company.AdminUserID);
        }

        public void SendFundRequestMessage(int ToUserID, decimal Amount, string FirmName)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(ToUserID);

            string message = Message.FundMessage.FCM_Message_Fund_Request;

            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
            message = message.Replace("[FirmName]", FirmName);

            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, "", "DEFAULT", 10000, PlatformCode.Web);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, "", "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            SendAPPBarRefreshFCM(ToUserID);
        }

        public void SendCashCollectionMessage(int UserID, decimal Amount, string FirmName, decimal Outstanding, string message)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(UserID);
            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
            message = message.Replace("[FirmName]", FirmName);
            message = message.Replace("[Outstanding]", Math.Round(Outstanding, 2).ToString());

            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            SendAPPBarRefreshFCM(UserID);
        }

        internal void SendAPPBarRefreshFCM(int UserID)
        {
            FCMViewModel user = new UserDAL().SelectFCMTokenByUserID(UserID);
            new FCMProcessing().SendDataMessage(user?.AppFCMToken, Message.FundMessage.RefreshBalanceFlag, Message.FundMessage.RefreshBalanceFlag, "DEFAULT", 10000, PlatformCode.App);

            new FCMProcessing().SendDataMessage(user?.WebFCMToken, Message.FundMessage.RefreshBalanceFlag, Message.FundMessage.RefreshBalanceFlag, "DEFAULT", 10000, PlatformCode.Web);
        }

        internal void SendAPPBarRefreshFCMInBackground(int UserID)
        {
            Task.Run(() =>
            {
                SendAPPBarRefreshFCM(UserID);
            });
        }

        internal void SendOperatorRefresh()
        {
            List<UserMaster> usersList = new UserDAL().Select();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(usersList.AsEnumerable(), parallelOptions, user =>
            //foreach (UserMaster user in usersList)
            {
                new FCMProcessing().SendDataMessage(user?.AppFCMToken, Message.FundMessage.RefreshOperatorFlag, Message.FundMessage.RefreshOperatorFlag, "DEFAULT", 10000, PlatformCode.App);
            });
        }

        public void SendFundReverseMessage(decimal Amount, string FirmName)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(Company.AdminUserID);

            string message = Message.FundMessage.FCM_Message_Fund_Reverse;

            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
            message = message.Replace("[FirmName]", FirmName);

            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, "Fund Reverse", "DEFAULT", 10000, PlatformCode.Web);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, "Fund Reverse", "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            SendAPPBarRefreshFCM(Company.AdminUserID);
        }

        public void SendSignUpMessageToAdmin(string MobileNumber, string FirmName)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(Company.AdminUserID);

            string message = Message.UserMessage.NewUsergeneration;

            message = message.Replace("[FirmName]", FirmName);
            message = message.Replace("[MobileNumber]", MobileNumber);

            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, "New user generated", "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, "New user generated", "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            SendAPPBarRefreshFCM(Company.AdminUserID);
        }

        public void SendKYCNotification(bool IsApproved, Model.UserMaster user, string message)
        {
            string status = IsApproved ? "accepted" : "rejected";

            message = message.Replace("[FirmName]", user.FirmName);
            message = message.Replace("[Status]", status);
            message = message.Replace("[SystemName]", Company.ClientName);

            try
            {
                new FCMProcessing().SendNoticationMessage(user.WebFCMToken, message, "KYC " + status, "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                new FCMProcessing().SendNoticationMessage(user.AppFCMToken, message, "KYC " + status, "DEFAULT", 10000, PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public void SendServiceSubscriptionDuePaymentMessage(int UserID, Response.ServiceAndUserwiseSubscription.UserSubscriptionChargeViewModel model)
        { 
            try
            {
                  SendServiceSubscriptionDuePaymentMessageCore(UserID, model);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void SendServiceSubscriptionDuePaymentMessageCore(int UserID, Response.ServiceAndUserwiseSubscription.UserSubscriptionChargeViewModel model)
        {
            FCMViewModel currentUserFCM = new UserDAL().SelectFCMTokenByUserID(UserID);

            string message = Message.ServiceAndUserwiseSubscriptionMessage.FCM_Payment_Due_Message;
            message = message.Replace("[ServiceName]", model.ServiceName);
            message = message.Replace("[Amount]", model.Charge.TwoDigitDecimal());
            message = message.Replace("[Days]", model.DueDays.ToString());
            message = message.Replace("[LastPaymentDate]", model.PaymentEndDate);
            try
            {
                new FCMProcessing().SendNoticationMessage(currentUserFCM.WebFCMToken, message, StatusType.Process);
                new FCMProcessing().SendNoticationMessage(currentUserFCM.AppFCMToken, message, status: StatusType.Process, defaultPlatform: PlatformCode.App);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            SendAPPBarRefreshFCM(UserID);
        }
        
    }
}