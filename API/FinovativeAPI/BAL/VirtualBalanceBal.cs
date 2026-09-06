using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.VirtualBalance;
using Project.Response.VirtualBalance;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class VirtualBalanceBal
    {
        internal AddResponse Add(AddRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            AddResponse response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //Check request is from Admin only
            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

            if (user == null)
                return response;

            if (user.UserTypeID != (int)UserType.Admin)
                return response;

            Task.Run(() => new MessageSelection().SendVirtulBalanceOTP(user));

            failureMessage = StatusType.Ok;
            response = new AddResponse()
            {
                IsOTPSent = true,
                Amount = mainRequest.Amount,
                WalletID = mainRequest.WalletID,
            };
            return response;
        }

        internal AddResponse AddWithOTP(AddWithOTPRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            AddResponse response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //Check request is from Admin
            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

            if (user == null)
                return response;

            if (user.UserTypeID != (int)UserType.Admin)
                return response;

            bool IsValidOTP = new OTPMasterBal().IsValid(user.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.VirtualBalance));

            failureMessage = Messages.InvalidOTP;
            response = new AddResponse()
            {
                WalletID = mainRequest.WalletID,
                Amount = mainRequest.Amount,
            };

            if (!IsValidOTP)
                return response;

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(keyDataModel.UserID, mainRequest.WalletID);

            failureMessage = Message.UserMessage.InvalidUser;
            if (userWallet == null)
                return response;

            decimal OpeningBalance = userWallet.Balance;
            decimal ClosingBalance = OpeningBalance + mainRequest.Amount;

            VirtualBalance ObjModel = new VirtualBalance
            {
                Amount = mainRequest.Amount,
                OpeningBalance = userWallet.Balance,
                ClosingBalance = ClosingBalance,
                DOCType = DOCType.VirtualBalance,
                WalletID = mainRequest.WalletID,
            };

            int Result = 0;
            try
            {
                Result = new VirtualBalanceDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= -1)
                return response;

            int ReferenceID = Result;

            string description = $"Virtual Balance, Rs. {mainRequest.Amount} added";
            try
            {
                Result = ProcessVirtualBalanceTransaction(user, mainRequest.Amount, mainRequest.WalletID, ReferenceID, description);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            response.Message = description;
            failureMessage = StatusType.Ok;

            return response;
        }

        internal List<ViewModel> SelectLast10(KeyDataModel keyDataModel, out string failureMessage)
        {
            List<ViewModel> response = null;
            failureMessage = Messages.DataNotFound;

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //Check request is from Admin
            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

            if (user == null)
                return response;

            if (user.UserTypeID != (int)UserType.Admin)
                return response;

            try
            {
                response = new VirtualBalanceDal().SelectLast10();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            return response;
        }

        private int ProcessVirtualBalanceTransaction(UserMaster user, decimal amount, int WalletID, int ReferenceID, string description)
        {
            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(user.UserID, WalletID);

            int Result = new AccountProcessing().CreditBalance(
                 UserWallet: userWallet,
                 AmountWithCommission: amount,
                 Amount: amount,
                 ConsumerNumber: user.MobileNumber,
                 CommissionCreditDebit: CommissionCreditDebitType.None,
                 DOCType: DOCType.FundReceive,
                 AssignedUserID: Company.AdminUserID,
                 ServiceID: (int)ServiceCode.FundTransfer,
                 OperatorID: (int)OperatorCode.FundTransfer,
                 TransactionDescription: description,
                 ReferenceID: ReferenceID,
                 TDS: 0,
                 GST: 0,
                 DiscountAmount: 0);

            return Result;
        }

        internal ReportWrapper SelectForFilter(SelectRequest selectRequest, int UserID, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            List<ViewModel> response = null;
            failureMessage = Messages.DataNotFound;

            if (UserID != Company.AdminUserID)
                return wrapper;

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                response = new VirtualBalanceDal().SelectForFilter(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }
            failureMessage = Messages.DataNotFound;

            if (response == null || response.Count <= 0) return wrapper;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;

            if (!selectRequest.IsExcelDownload)
            {
                response = response.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }

            wrapper.list = response;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
    }
}