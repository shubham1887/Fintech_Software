using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.RetailerToAllFundTransfer;
using Project.Response.RetailerToAllFundTransfer;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class RetailerToAllFundTransferBal
    {
        internal string Transfer(TransferRequest mainRequest, int CurrentUserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            int senderWalletID = (new SettingMasterBal().getW2OWithSenderWallet().EqualsIgnoreCase("Yes") && mainRequest.SenderWalletID > 0) ? mainRequest.SenderWalletID : (int)WalletCode.AEPS;

            if (CurrentUserID == mainRequest.ReceiverID)
                return "Self transfer not allowed";

            if (mainRequest.ReceiverID == Company.AdminUserID)
                return "Could not transfer to admin";

            WalletMaster wallet = new WalletBal().SelectByID(mainRequest.WalletID);
            if (!wallet.IsWalletTransferEnabled)
                return "Could not transfer to this wallet";

            UserWalletMaster SenderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(CurrentUserID, senderWalletID);

            if (mainRequest.Amount > SenderWallet.Balance)
            {
                failureMessage = "Dear customer you don't have  enough balance to transfer";
                return failureMessage;
            }
            // deduct balance in AEPS Wallet
            decimal SenderOpeningBalance = SenderWallet.Balance;
            decimal SenderClosingBalance = SenderOpeningBalance - mainRequest.Amount;
            SenderWallet.Balance = SenderClosingBalance;

            UserWalletMaster ReceiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(mainRequest.ReceiverID, mainRequest.WalletID);
            decimal ReceiverOpeningBalance = ReceiverWallet.Balance;
            decimal ReceiverClosingBalance = ReceiverOpeningBalance + mainRequest.Amount;
            ReceiverWallet.Balance = ReceiverClosingBalance;
            string Remark = $"Fund Received by R2W";
            FundTransaction ObjModel = new FundTransaction
            {
                Amount = mainRequest.Amount,
                WalletID = mainRequest.WalletID,
                ReceiverID = mainRequest.ReceiverID,
                SenderID = CurrentUserID,
                RequestToID = CurrentUserID,
                SenderOpeningBalance = SenderOpeningBalance,
                SenderClosingBalance = SenderClosingBalance,
                ReceiverClosingBalance = ReceiverClosingBalance,
                ReceiverOpeningBalance = ReceiverOpeningBalance,
                AdminRemark = mainRequest.Remark,
                Remark = Remark,
                PayMode = "CASH",
                Type = FundTransferType.ThroughWalletToOther,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = "",
                Through = FundTransferType.ThroughDirectTransfer,
            };

            int Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;

            MemberDetailResponse receiver = null;
            try
            {
                receiver = new UserDAL().SelectDetailByUserID(ObjModel.ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            List<WalletMaster> wallets = new WalletMasterDal().Select();
            string fromWalletName = wallets.FirstOrDefault(w => w.WalletID == senderWalletID).Name;
            string toWalletName = wallets.FirstOrDefault(w => w.WalletID == mainRequest.WalletID).Name;

            string TransactionDescription = $"Fund transfered from Wallet {fromWalletName} , To: {receiver.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";
            try
            {
                Result = new AccountProcessing().DebitBalance(
                   UserWallet: SenderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: receiver.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundTrnasfer,
                   AssignedUserID: ObjModel.ReceiverID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: ObjModel.FundTransactionID,
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
            MemberDetailResponse sender = null;
            try
            {
                sender = new UserDAL().SelectDetailByUserID(ObjModel.SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            TransactionDescription = $"Fund received wallet {toWalletName} , From: {sender.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                ReceiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: ReceiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: sender.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundReceive,
                   AssignedUserID: ObjModel.SenderID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: ObjModel.FundTransactionID,
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
                    new FCMSelection().SendFundTransferMessage(ObjModel.Amount, sender.FirmName, receiver.UserID, FundMessage.FCM_Message_Fund_Transfer);
                    new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);
                    new MessageSelection().SendFundTransferMessage(sender.UserID, mainRequest.ReceiverID, ObjModel.Amount, mainRequest.WalletID, ObjModel.UTRNumber, 0, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        internal ViewModel GenerateOTP(OTPRequest otpRequest, int UserID, out string failureMessage)
        {
            ViewModel response = new ViewModel();

            failureMessage = otpRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            failureMessage = Message.UserMessage.UserNotFound;
            UserMaster userMaster = null;
           
            string walletToOtherOTPFlag = new SettingMasterBal().getWalletToOther();
            try
            {
                userMaster = new UserDAL().SelectUserByMobileNumberSynchronization(otpRequest.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (!walletToOtherOTPFlag.EqualsIgnoreCase("Yes"))
            {
                failureMessage = StatusType.Ok;
                response.UserID = userMaster.UserID;
                response.FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName);
                response.Message = Messages.NoOTPRequired;
                response.IsOtpRequired = false;
                return response;
            }

            if (userMaster == null) return response;
            if (UserID == userMaster.UserID || userMaster.UserID == Company.AdminUserID) return response;
            new MessageSelection().SendFundTransferOTP(userMaster);

            response.UserID = userMaster.UserID;
            response.FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName);
            response.Message = Messages.OTPSend;
            response.IsOtpRequired = true;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal ViewModel VerifyFoundTransferOTP(OTPRequest otpRequest, out string failureMessage)
        {
            ViewModel response = new ViewModel();
            failureMessage = UserMessage.UserNotFound;
            UserMaster userMaster = null;
            try
            {
                userMaster = new UserDAL().SelectUserByMobileNumberSynchronization(otpRequest.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (userMaster == null) return response;
            bool IsValidOTP = new OTPMasterBal().IsValid(userMaster.UserID, otpRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.FundTransferOTP));
            if (!IsValidOTP)
            {
                failureMessage = UserMessage.InvalidOTP;
                return response;
            }
            try
            {
                response.UserID = userMaster.UserID;
                response.FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            response.Message = UserMessage.OTPverifiedsuccess;
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}