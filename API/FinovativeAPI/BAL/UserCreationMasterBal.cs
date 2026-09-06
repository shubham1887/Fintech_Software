using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserCreationMaster;
using Project.Response.UserCreationMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class UserCreationMasterBal
    {
        internal string Insert(InsertRequest insertRequest, int UserID)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            int ServiceID = (int)ServiceCode.UserCreation;
            int OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID((int)ServiceCode.UserCreation);

            if (OperatorID <= 0)
                return UserCreationMessage.InvalidOperator;

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);
            if (ServiceID != DBServiceID)
                return UserCreationMessage.InvalidServiceCode;

            UserCreationWallet userCreationWallet;
            UserWalletMaster userWallet;
            decimal DebitAmount = 0;
            try
            {
                userCreationWallet = new UserCreationWalletDal().SelectUserCreationWalletByUserID(UserID);
                if (userCreationWallet == null)
                    return UserCreationMessage.UserCreationWalletNotFound;

                DebitAmount = new UserCreationSettingDal().SelectCalculatedAmountByUserID(UserID, insertRequest.PurchaseCount);

                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, insertRequest.WalletID);

                if (userWallet == null)
                    return UserCreationMessage.InvalidWallet;

                failureMessage = new AccountProcessing().BalanceValidationBeforeTransaction(userWallet, DebitAmount, 0);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return TransactionMessage.Failure;
            }
            UserCreationMaster ObjModel = new UserCreationMaster
            {
                UserID = UserID,
                ServiceID = (int)ServiceCode.UserCreation,
                WalletID = insertRequest.WalletID,
                OpeningIdCount = userCreationWallet.IdCount,
                PurchaseCount = insertRequest.PurchaseCount,
                ClosingIdCount = userCreationWallet.IdCount + insertRequest.PurchaseCount,
                OpeningBalance = userWallet.Balance,
                Amount = DebitAmount,
                ClosingBalance = userWallet.Balance - DebitAmount,
                Type = IDPurchase.Purchase,
            };

            int rowsAfftected = new UserCreationMasterDal().Insert(ObjModel);
            if (rowsAfftected <= 0)
            {
                return TransactionMessage.Failure;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            string TransactionDescription = $"{DOCType.UserCreation}, to: purchase id Count{insertRequest.PurchaseCount} Amount: {DebitAmount.TwoDigitDecimal()}";

            try
            {
                int Result = 0;

                Result = new AccountProcessing().DebitBalance(
                UserWallet: userWallet,
                AmountWithCommission: DebitAmount,
                Amount: DebitAmount,
                ConsumerNumber: user.MobileNumber,
                CommissionCreditDebit: CommissionCreditDebitType.None,
                DOCType: DOCType.UserCreation,
                AssignedUserID: UserID,
                ServiceID: ServiceID,
                OperatorID: OperatorID,
                TransactionDescription: TransactionDescription,
                ReferenceID: rowsAfftected,
                TDS: 0,
                GST: 0,
                DiscountAmount: 0);

                if (Result <= 0)
                    return TransactionMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return StatusType.Ok; ;
            }
            new UserCreationWalletBal().Update(UserID, insertRequest.PurchaseCount, CommissionCreditDebitType.Credit);

            return StatusType.Ok;
        }

        internal List<ViewModel> Last10Report(int UserID)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            try
            {
                viewModel = new UserCreationMasterDal().Last10Report(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return viewModel;
        }

        internal string UpdateTransaction(int UserID, int CreatedUserID, int PurchaseCount, string Type, string CreditDebitType)
        {
            UserCreationWallet userCreationWallet;
            UserWalletMaster userWallet;

            try
            {
                userCreationWallet = new UserCreationWalletDal().SelectUserCreationWalletByUserID(UserID);
                if (userCreationWallet == null)
                {
                    new UserCreationWalletBal().Insert(UserID);
                    userCreationWallet = new UserCreationWalletDal().SelectUserCreationWalletByUserID(UserID);
                }

                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, (int)WalletCode.Recharge);

                if (userWallet == null)
                    return UserCreationMessage.InvalidWallet;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return TransactionMessage.Failure;
            }
            UserCreationMaster ObjModel = new UserCreationMaster
            {
                UserID = UserID,
                CreatedUserID = CreatedUserID,
                ServiceID = (int)ServiceCode.UserCreation,
                WalletID = (int)WalletCode.Recharge,
                OpeningIdCount = userCreationWallet.IdCount,
                PurchaseCount = PurchaseCount,
                ClosingIdCount = userCreationWallet.IdCount - PurchaseCount,
                OpeningBalance = userWallet.Balance,
                Amount = 0,
                ClosingBalance = userWallet.Balance,
                Type = Type,
            };
            int rowsAfftected = new UserCreationMasterDal().Insert(ObjModel);
            if (rowsAfftected <= 0)
            {
                return TransactionMessage.Failure;
            }

            new UserCreationWalletBal().Update(UserID, PurchaseCount, CreditDebitType);

            return StatusType.Ok;
        }

        internal string FreeTransfer(TransferRequest request)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int ServiceID = (int)ServiceCode.UserCreation;
            int OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID((int)ServiceCode.UserCreation);

            if (OperatorID <= 0)
                return UserCreationMessage.InvalidOperator;

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);
            if (ServiceID != DBServiceID)
                return UserCreationMessage.InvalidServiceCode;

            UserCreationWallet userCreationWallet;

            try
            {
                userCreationWallet = new UserCreationWalletDal().SelectUserCreationWalletByUserID(request.UserID);
                if (userCreationWallet == null)
                {
                    new UserCreationWalletBal().Insert(request.UserID);
                    userCreationWallet = new UserCreationWalletDal().SelectUserCreationWalletByUserID(request.UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return TransactionMessage.Failure;
            }
            UserCreationMaster ObjModel = new UserCreationMaster
            {
                UserID = request.UserID,
                ServiceID = (int)ServiceCode.UserCreation,
                WalletID = (int)WalletCode.Recharge,
                OpeningIdCount = userCreationWallet?.IdCount ?? 0,
                PurchaseCount = request.PurchaseCount,
                ClosingIdCount = userCreationWallet.IdCount + request.PurchaseCount,
                OpeningBalance = 0,
                Amount = 0,
                ClosingBalance = 0,
                Type = IDPurchase.Free,
            };

            int rowsAfftected = new UserCreationMasterDal().Insert(ObjModel);
            if (rowsAfftected <= 0)
            {
                return TransactionMessage.Failure;
            }

            new UserCreationWalletBal().Update(request.UserID, request.PurchaseCount, IDPurchase.Credit);

            return StatusType.Ok;
        }

        internal UserFilterWrapper SelectForFilter(SelectRequest mainRequest, out string failureMessage)
        {
            failureMessage = "";
            List<ViewModel> list = new List<ViewModel>();
            UserFilterWrapper wrapper = new UserFilterWrapper();

            try
            {
                list = new UserCreationMasterDal().SelectForFilter(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = UserMessage.NotFoundOrActive;

            if (list.Count <= 0) return wrapper;

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

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
    }
}