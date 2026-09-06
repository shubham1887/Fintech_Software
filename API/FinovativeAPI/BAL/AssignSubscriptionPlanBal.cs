using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ServiceSubscription;
using Project.Response.ServiceSubscription;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class AssignSubscriptionPlanBal
    {
        public Response.ServiceAndUserwiseSubscription.InsertResponse Insert(int UserID, AssignPlanInsertRequest insertRequest)
        {
            if (UserID != Company.AdminUserID)
                insertRequest.UserID = UserID;

            Response.ServiceAndUserwiseSubscription.InsertResponse response = new();
            response.Status = StatusType.Failure;
            response.Message = insertRequest.IsValidModalState();

            if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
                return response;

            if (!new AssignSubscriptionChargeDal().CheckDuplicate(insertRequest.UserID, insertRequest.ServiceSubscriptionCreatePlanID))
            {
                response.Message = Messages.AlreadyExits;
                return response;
            }

            Project.Response.ServiceSubscription.AssignPlanViewModel model = new ServiceSubscriptionCreatePlanMasterDal().SelectByplanDetailID(insertRequest.ServiceSubscriptionCreatePlanID);

            Project.Response.ServiceSubscription.ViewModel viewModel = new ServiceSubscriptionCreatePlanMasterDal().SelectByplanID(insertRequest.ServiceSubscriptionCreatePlanID);

            UserSubscriptionPlanMapping ServiceUserCharge = new UserSubscriptionPlanMapping
            {
                PlanName = viewModel.PlanName,
                Amount = insertRequest.PlanAmount,              
                OpeningBalance = 0,              
                ClosingBalance = 0,              
                Status = "Success",              
                Remark = "Subscription Plan Amount is Zero",              
                UserID = insertRequest.UserID,
                ServiceSubscriptionCreatePlanMasterID = insertRequest.ServiceSubscriptionCreatePlanID
            };

            response.Services = new ServiceSubscriptionPlanDetailDal().SelectServiceList(insertRequest.ServiceSubscriptionCreatePlanID);

            int Result = 0;
            if (insertRequest.PlanAmount <= 0)
            {
               Result = new AssignSubscriptionChargeDal().Insert(ServiceUserCharge);
        
                new UserBal().AddServiceMapping(insertRequest.UserID, response.Services);

                response.Status = StatusType.Ok;
                response.Message = Message.ServiceAndUserwiseSubscriptionMessage.InsertSuccess;
                response.Amount = model.Amount;
                response.TotalAmount = model.TotalAmount;
                response.GST = model.GST;
                response.PlanName = model.PlanName;
                return response;
            }
            if(insertRequest.PlanAmount > 0)
            {             
                response.Message = SubscriptionChargeCutting(insertRequest.PlanAmount, insertRequest.UserID, insertRequest.ServiceSubscriptionCreatePlanID);

                if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
                {
                    response.Status = Messages.InsertFailure;
                    response.Message = response.Message;
                    return response;
                }
                    
                new UserBal().AddServiceMapping(insertRequest.UserID, response.Services);
   
                response.Status = StatusType.Ok;
                response.Message = Message.ServiceAndUserwiseSubscriptionMessage.InsertSuccess;
                response.Amount = model.Amount;
                response.TotalAmount = model.TotalAmount;
                response.GST = model.GST;
                response.PlanName = model.PlanName;
                return response;
            }

            response.Status = Messages.InsertFailure;
            response.Message = Messages.InsertFailure;
            return response;
        }

        public string SubscriptionChargeCutting(decimal Amount, int UserID, int ServiceSubscriptionCreatePlanID)
        {
            int ReceiverID = Company.AdminUserID;
            if (Amount <= 0M) return "";

            UserMaster receiver = null;
            try
            {
                receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (receiver == null || receiver.ParentID <= 0) return UserMessage.NotFoundOrActive;

            UserMaster sender = new UserMaster();
            try
            {
                sender = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (sender == null) return UserMessage.NotFoundOrActive;

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(sender.UserID, (int)WalletCode.Recharge);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            if ((senderWallet.Balance) < Amount)
            {
                return $"{Message.FundMessage.LowBalance}";
            }
           
            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, (int)WalletCode.Recharge);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;

            string Remark = $"Service Subscription Charge, To: {receiver.FirmName}[{receiver.MobileNumber}] Amount: {Amount.TwoDigitDecimal()} Charge by {sender.FirmName}";

            decimal SenderClosingBalance = senderWallet.Balance - Amount;
            decimal ReceiverClosingBalance = receiverWallet.Balance + Amount;

            Project.Response.ServiceSubscription.ViewModel viewModel = new ServiceSubscriptionCreatePlanMasterDal().SelectByplanID(ServiceSubscriptionCreatePlanID);

            int AssignSubscriptionChargeID = InsertServiceSubscription(Amount, UserID, senderWallet.Balance, SenderClosingBalance, Remark, viewModel.PlanName, ServiceSubscriptionCreatePlanID);

            if (AssignSubscriptionChargeID <= 0) return StatusType.Failure;

            string TransactionDescription = $"Fund transfered, To: {receiver.FirmName} Amount: {Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = SenderClosingBalance;

                int Result = new AccountProcessing().DebitBalance(
                     UserWallet: senderWallet,
                     Amount: Amount,
                     AmountWithCommission: Amount,
                     ConsumerNumber: receiver.MobileNumber,
                     CommissionCreditDebit: CommissionCreditDebitType.None,
                     AssignedUserID: ReceiverID,
                     DOCType: DOCType.ServiceSubscriptionCharge,
                     ServiceID: 0,
                     OperatorID: 0,
                     TransactionDescription: TransactionDescription,
                     ReferenceID: AssignSubscriptionChargeID,
                     TDS: 0,
                     GST: 0,
                     DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            TransactionDescription = $"Fund received, From: {sender.FirmName} Amount: {Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ReceiverClosingBalance;

                int Result = new AccountProcessing().CreditBalance(
                       UserWallet: receiverWallet,
                       Amount: Amount,
                       AmountWithCommission: Amount,
                       ConsumerNumber: sender.MobileNumber,
                       CommissionCreditDebit: CommissionCreditDebitType.None,
                       DOCType: DOCType.ServiceSubscriptionCharge,
                       AssignedUserID: UserID,
                       ServiceID: 0,
                       OperatorID: 0,
                       TransactionDescription: TransactionDescription,
                       ReferenceID: AssignSubscriptionChargeID,
                       TDS: 0,
                       GST: 0,
                       DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                Task.Run(() =>
                {
                    new FCMSelection().SendFundTransferMessage(Amount, sender.FirmName, receiver.UserID, ServiceAndUserwiseSubscriptionMessage.FCM_Message_Transfer);

                    new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);

                    new MessageSelection().SendSubscriptionMessage(sender.UserID, ReceiverID, Amount, (int)WalletCode.Recharge, AssignSubscriptionChargeID.ToString());
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        private int InsertServiceSubscription(decimal Amount, int UserID, decimal OpeningBal, decimal ClosingBal, string Remark,string PlanName, int ServiceSubscriptionCreatePlanID)
        {
            int Result = 0;

            UserSubscriptionPlanMapping charge = new();
            charge.UserID = UserID;        
            charge.Amount = Amount;
            charge.OpeningBalance = OpeningBal;
            charge.ClosingBalance = ClosingBal;               
            charge.PlanName = PlanName;                 
            charge.Remark = Remark;                 
            charge.Status = "SUCCESS";                 
            charge.ServiceSubscriptionCreatePlanMasterID = ServiceSubscriptionCreatePlanID;                 
            Result = new AssignSubscriptionChargeDal().Insert(charge);

            return Result;
        }

        internal string SelectByUserID(int UserID)
        {
            string failureMessage;
            AssignPlanViewModel response = null;
            try
            {
                response = new AssignSubscriptionChargeDal().SelectByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response != null)
            {
                failureMessage = Messages.AlreadyExits;
                return failureMessage;
            }
      
            return StatusType.Ok;
        }
    }
}