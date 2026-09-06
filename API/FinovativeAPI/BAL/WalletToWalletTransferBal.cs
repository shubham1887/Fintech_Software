using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.WalletToWalletTransfer;
using Project.Response.User;
using Project.Response.Wallet;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class WalletToWalletTransferBal
    {
        internal (string, WallettoOtherTransferResponse) Transfer(AEPSToOtherRequest mainRequest, int CurrentUserID)
        {
            WallettoOtherTransferResponse response = new WallettoOtherTransferResponse();
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return (failureMessage,response);

            int fromWalletID = (int)WalletCode.AEPS;

            if (fromWalletID == mainRequest.WalletID)
                return ("Could not transfer to same wallet",response);

            WalletMaster wallet = new WalletBal().SelectByID(mainRequest.WalletID);
            if (!wallet.IsWalletTransferEnabled)
                return ("Could not transfer to this wallet",response);

            UserWalletMaster fromWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(CurrentUserID, fromWalletID);

            if (mainRequest.Amount > fromWallet.Balance)
            {
                failureMessage = "Dear customer you don't have  enough balance to transfer";
                return (failureMessage, response);
            }

            decimal cappingAmount = new UserBalanceCappingBal().SelectByWalletIDAndUserID(CurrentUserID, fromWalletID);
            if ((fromWallet.Balance - cappingAmount) < mainRequest.Amount)
            {
                failureMessage = $"{Message.FundMessage.LowBalance} and minimum wallet balance is {cappingAmount.TwoDigitDecimal()}, for {mainRequest.Amount.TwoDigitDecimal()}";
                return (failureMessage, response);
            }

            // deduct balance in AEPS Wallet
            decimal fromWalletOpeningBalance = fromWallet.Balance;
            decimal fromWalletClosingBalance = fromWalletOpeningBalance - mainRequest.Amount;
            fromWallet.Balance = fromWalletClosingBalance;

            UserWalletMaster toWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(CurrentUserID, mainRequest.WalletID);
            decimal toWalletOpeningBalance = toWallet.Balance;
            decimal toWalletClosingBalance = toWalletOpeningBalance + mainRequest.Amount;
            toWallet.Balance = toWalletClosingBalance;

            FundTransaction ObjModel = new FundTransaction
            {
                Amount = mainRequest.Amount,
                WalletID = (int)WalletCode.AEPS,
                ReceiverID = CurrentUserID,
                SenderID = CurrentUserID,
                RequestToID = CurrentUserID,
                SenderOpeningBalance = fromWalletOpeningBalance,
                SenderClosingBalance = fromWalletClosingBalance,
                ReceiverClosingBalance = toWalletClosingBalance,
                ReceiverOpeningBalance = toWalletOpeningBalance,
                AdminRemark = mainRequest.Remark,
                Remark = mainRequest.Remark,
                PayMode = "CASH",
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = "",
                Through = FundTransferType.ThroughWalletReverse,
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
                return (Message.FundMessage.Failure, response);

            ObjModel.FundTransactionID = Result;

            MemberDetailResponse member = null;
            try
            {
                member = new UserDAL().SelectDetailByUserID(ObjModel.ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            List<WalletMaster> wallets = new WalletMasterDal().Select();
            string fromWalletName = wallets.FirstOrDefault(w => w.WalletID == fromWalletID).Name;
            string toWalletName = wallets.FirstOrDefault(w => w.WalletID == mainRequest.WalletID).Name;

            string TransactionDescription = $"Fund transfered, from: {fromWalletName} wallet to {toWalletName} wallet, Amount: {ObjModel.Amount}";

            try
            {
                Result = new AccountProcessing().DebitBalance(
                   UserWallet: fromWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundTrnasfer,
                   AssignedUserID: CurrentUserID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                    return (Message.FundMessage.Failure, response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                member = new UserDAL().SelectDetailByUserID(ObjModel.SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            ObjModel = new FundTransaction
            {
                Amount = mainRequest.Amount,
                WalletID = mainRequest.WalletID,
                ReceiverID = CurrentUserID,
                SenderID = CurrentUserID,
                RequestToID = CurrentUserID,
                SenderOpeningBalance = fromWalletOpeningBalance,
                SenderClosingBalance = fromWalletClosingBalance,
                ReceiverClosingBalance = toWalletClosingBalance,
                ReceiverOpeningBalance = toWalletOpeningBalance,
                AdminRemark = mainRequest.Remark,
                Remark = mainRequest.Remark,
                PayMode = "CASH",
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = "",
                Through = FundTransferType.ThroughWalletTransfer,
            };

            Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return (Message.FundMessage.Failure, response);

            ObjModel.FundTransactionID = Result;

            TransactionDescription = $"Fund received, from: {fromWalletName} wallet to {toWalletName} wallet, Amount: {ObjModel.Amount}";

            try
            {
                Result = new AccountProcessing().CreditBalance(
                   UserWallet: toWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
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
                    return (Message.FundMessage.Failure, response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            new MessageSelection().SendWalletTranferMessage(ObjModel.SenderID, ObjModel.ReceiverID, ObjModel.Amount, ObjModel.WalletID, fromWalletName, toWalletName);
            response.AepsWalletBal = fromWallet.Balance.ToString();
            response.mainWalletBal = toWallet.Balance.ToString();
            response.ReferenceID = ObjModel.FundTransactionID;
            response.Remark = mainRequest.Remark;
            response.Amount = mainRequest.Amount;
            response.DateTime = DateTime.Now.Long24HrDisplayFormat();
            
            return (StatusType.Ok, response);
        }
    }
}
