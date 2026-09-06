using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.CashCollection;
using Project.Response.CashCollection;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class CashCollectionBal
    {
        public void InsertCashCollectionMaster(int UserID)
        {
            CashCollectionMaster ObjModel = new CashCollectionMaster
            {
                UserID = UserID,
                Outstanding = 0,
                MaxOutstanding = 999999
            };

            new CashCollectionMasterDal().Insert(ObjModel);
        }

        //to add entry in transaction  details
        internal int ProcessCashCollection(int senderID, int ReceiverID, int WalletID, decimal? cashCollectionAmount, decimal Amount, string CashCollectionRemark, string TransferType, string PayMode, string UTRNumber, string ToDate)
        {
            int Result = -1;

            string cashCollection = new UserBal().IsCashCollection();

             if (cashCollection.EqualsIgnoreCase("false"))
            {
                return 0;
            }

            if (TransferType == FundTransferType.ReverseRequest || TransferType == FundTransferType.Reverse)
            {
                Result = CashCallectionReverse(senderID, ReceiverID, WalletID, cashCollectionAmount ?? 0, CashCollectionRemark, PayMode, UTRNumber, ToDate);
            }
            else
            {
                Result = CashCallectionAdd(senderID, ReceiverID, WalletID, Amount, cashCollectionAmount ?? 0, CashCollectionRemark, PayMode, ToDate);
            }
            if (Result <= 0)
                return Result;

            return Result;
        }

        private int CashCallectionAdd(int senderID, int ReceiverID, int WalletID, decimal Amount, decimal cashCollectionAmount, string CashCollectionRemark, string PayMode, string ToDate)
        {
            decimal OS_OpeningBalance = 0;
            decimal OS_ClosingBalance = 0;
            int Result = -1;

            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(ReceiverID);
            if (ObjModel == null)
            {
                new CashCollectionBal().InsertCashCollectionMaster(ReceiverID);
                ObjModel = new CashCollectionMasterDal().SelectByUserID(ReceiverID);
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(ObjModel.UserID);

            OS_OpeningBalance = ObjModel.Outstanding;
            OS_ClosingBalance = OS_OpeningBalance + Amount;
            ObjModel.Outstanding = OS_ClosingBalance;
            try
            {
                Result = new CashCollectionMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string ReceiverDescription = $"Outstanding increased, by: {Amount} and new outstanding is: {Math.Round(ObjModel.Outstanding, 2)}";
            //Collection Details entry

            int referenceID = new CashCollectionDetailBal().Insert(ReceiverID, senderID, OS_OpeningBalance, Amount, OS_ClosingBalance, ReceiverDescription, PayMode,
                AmountCreditDebitType.Credit, "", WalletID, "", CollectionType.CASH_COLLECTION_INCREASED, ToDate);
            // entry for outstanding increase
            Result = TransactionDetailsInsertForCashCollection(ReceiverID, Amount, user.MobileNumber, senderID, OS_OpeningBalance,
                OS_ClosingBalance, WalletID, ReceiverDescription, referenceID, AmountCreditDebitType.Credit, DOCType.CASH_COLLECTION, PayMode);

            if (cashCollectionAmount > 0)
                CashCallectionReverse(senderID, ReceiverID, WalletID, cashCollectionAmount, CashCollectionRemark, PayMode, "", ToDate);

            return Result;
        }

        private int CashCallectionReverse(int senderID, int ReceiverID, int WalletID, decimal cashCollectionAmount, string CashCollectionRemark, string PayMode, string UTRNumber, string ToDate)
        {
            decimal OS_OpeningBalance = 0;
            decimal OS_ClosingBalance = 0;
            int Result = -1;

            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(ReceiverID);

            UserMaster user = new UserDAL().SelectByIDFromCache(ObjModel.UserID);

            OS_OpeningBalance = ObjModel.Outstanding;
            OS_ClosingBalance = OS_OpeningBalance - cashCollectionAmount;
            ObjModel.Outstanding = OS_ClosingBalance;

            //if (ObjModel.Outstanding < 0)
            //    return Result;

            try
            {
                Result = new CashCollectionMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string ReceiverDescription = $"Outstanding Decreased, by: {cashCollectionAmount} and new outstanding is: {Math.Round(ObjModel.Outstanding, 2)}";

            if (CashCollectionRemark.Length > 2)
                ReceiverDescription = $"{CashCollectionRemark} {ReceiverDescription}";
            //Collection Details entry

            int referenceID = new CashCollectionDetailBal().Insert(ReceiverID, senderID, OS_OpeningBalance, cashCollectionAmount, OS_ClosingBalance, ReceiverDescription, PayMode,
               "", AmountCreditDebitType.Debit, WalletID, UTRNumber, CollectionType.CASH_COLLECTION_DECREASED, ToDate);

            Result = TransactionDetailsInsertForCashCollection(senderID, cashCollectionAmount, user.MobileNumber, ReceiverID, OS_OpeningBalance,
                OS_ClosingBalance, WalletID, ReceiverDescription, referenceID, AmountCreditDebitType.Debit, DOCType.CASH_COLLECTION, PayMode);

            Task.Run(() =>
            {
                new FCMSelection().SendCashCollectionMessage(senderID, Convert.ToDecimal(cashCollectionAmount), user.FirmName, ObjModel.Outstanding, Message.CashCollectionMessage.FCM_Message_Cash_Collection);
                new MessageSelection().SendCashCollectionMessage(senderID, ReceiverID, cashCollectionAmount, OS_OpeningBalance, OS_ClosingBalance, referenceID.ToString());
            });
            return Result;
        }

        private int TransactionDetailsInsertForCashCollection(int AssignedUserID, decimal amount, string consumerNumber,
            int UserID, decimal openingBalance, decimal closingBalance, int walletID, string description,
            int referenceID, string CreditDebitType, string DOCType, string PayMode)
        {
            int Result = -1;
            TransactionDetails transactionDetails = new TransactionDetails
            {
                UserID = UserID,
                TransactionAmount = amount,
                ConsumerNumber = consumerNumber,
                AmountCreditDebit = CreditDebitType,
                CommissionCreditDebit = CommissionCreditDebitType.None,
                DOCType = DOCType,
                AssignedUserID = AssignedUserID,
                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                ServiceID = (int)ServiceCode.FundTransfer,
                OperatorID = (int)OperatorCode.FundTransfer,
                WalletID = walletID,
                TransactionDescription = description,
                ReferenceID = referenceID,
                TDS = 0,
                GST = 0,
                DiscountAmount = 0,
                PayMode = PayMode
            };

            try
            {
                Result = new TransactionDetailsDAL().Insert(transactionDetails);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return Result;
        }

        internal ReportWrapper SelectByUserIDReport(SelectByUserIDRequest mainRequest, int CurrentUserID, out string failureMessage)
        {
            ReportWrapper response = new ReportWrapper();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            Requests.TransactionDetails.ReportRequest txReportRequest = new Requests.TransactionDetails.ReportRequest
            {
                UserID = mainRequest.UserID,
                DOCType = "CASH COLLECTION",
            };

            List<Response.TransactionDetails.ViewModel> views = new List<Response.TransactionDetails.ViewModel>();

            views = new TransactionDetailsDAL().SelectByCashCallection(txReportRequest, CurrentUserID);

            List<Report> report = new List<Report>();

            foreach (Response.TransactionDetails.ViewModel view in views)
            {
                Report row = new Report
                {
                    Amount = (float)view.Amount,
                    OpeningBalance = (float)view.OpeningBalance,
                    ClosingBalance = (float)view.ClosingBalance,
                    FirmName = view.FirmName,
                    Date = view.DOC,
                    Remark = view.TransactionDescription,
                    AssignedUserName = view.AssignedUserName,
                    PayMode = view.PayMode
                };

                report.Add(row);
            }

            response.Count = report.Count();
            response.data = report.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();

            failureMessage = StatusType.Ok;
            return response;
        }

        public bool IsDuplicateRemarkForCashCollection(int ReceiverID, string Remark, out string failureMessage)
        {
            failureMessage = StatusType.Ok;

            if (Remark == null || Remark == "" || Remark.Length <= 0)
            {
                return false;
            }
            FundTransaction transaction = null;
            List<string> whiteList = new List<string>(new string[] { "No", "Yes" });

            string DuplicateFundReamrks = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DuplicateFundReamrks),
                                                                                            Message.FundMessage.DefaultDuplicateFundReamrks,
                                                                                            whiteList);

            if (!DuplicateFundReamrks.EqualsIgnoreCase(Message.FundMessage.DefaultDuplicateFundReamrks))
            {
                return false;
            }

            try
            {
                transaction = new FundTransactionDal().SelectTransactionByReceiverIDAndCashCollectionRemark(ReceiverID, Remark);
            }
            catch (Exception)
            {
                return false;
            }

            if (transaction == null)
                return false;

            failureMessage = "Duplicate cash collection remark";
            return true;
        }

        internal string Add(AddRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = FunctionalValidationProcessing.IsValidMPin(keyDataModel.UserID, mainRequest.Pin);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int ReceiverID = mainRequest.ReceiverID;
            int SenderID = keyDataModel.UserID;
            int Result = -1;

            try
            {
                string cashCollection = new UserBal().IsCashCollection();

                if (cashCollection.EqualsIgnoreCase("false"))
                    return failureMessage = Message.CashCollectionMessage.NotAllowedCashCollection;

                if (!IsNegativeOutstanding(mainRequest))
                    return failureMessage = Message.CashCollectionMessage.InValidOutstanding;

                if (IsDuplicateRemarkForCashCollection(mainRequest.ReceiverID, mainRequest.Remark, out failureMessage))
                    return failureMessage;

                Result = ProcessCashCollection(SenderID, ReceiverID, 1, mainRequest.Amount, 0, mainRequest.Remark, FundTransferType.Reverse, mainRequest.PayMode, mainRequest.UTRNumber, mainRequest.ToDate);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0)
                return failureMessage = Message.CashCollectionMessage.Insertfailure;

            failureMessage = StatusType.Ok;

            new FCMSelection().SendAPPBarRefreshFCM(SenderID);
            new FCMSelection().SendAPPBarRefreshFCM(ReceiverID);

            return Message.CashCollectionMessage.InsertSuccess;
        }

        private bool IsNegativeOutstanding(AddRequest mainRequest)
        {
            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(mainRequest.ReceiverID);
            decimal OS_ClosingBalance = ObjModel.Outstanding - mainRequest.Amount;
            if (OS_ClosingBalance < 0)
                return false;
            return true;
        }

        internal string Reverse(ReverseRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int ReceiverID = keyDataModel.UserID;
            int SenderID = mainRequest.ReceiverID;
            int Result = -1;

            try
            {
                Result = ProcessReverseCashCollection(SenderID, ReceiverID, 1, mainRequest.Amount, mainRequest.Remark, mainRequest.PayMode);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0)
                return failureMessage = Message.CashCollectionMessage.ReverseFailure;

            failureMessage = StatusType.Ok;

            return Message.CashCollectionMessage.ReverseSuccess;
        }

        internal int ProcessReverseCashCollection(int senderID, int ReceiverID, int WalletID, decimal cashCollectionAmount, string CashCollectionRemark, string PayMode)
        {
            int Result = -1;
            if (cashCollectionAmount <= 0)
                return Result;

            int referenceID = 0;

            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(senderID);

            decimal OS_OpeningBalance = 0;
            decimal OS_ClosingBalance = 0;
            OS_OpeningBalance = ObjModel.Outstanding;
            OS_ClosingBalance = OS_OpeningBalance + cashCollectionAmount;

            ObjModel.Outstanding = OS_ClosingBalance;

            //if (ObjModel.Outstanding < 0)
            //    return Result;
            try
            {
                Result = new CashCollectionMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0)
                return Result;

            UserMaster user = new UserDAL().SelectByIDFromCache(ObjModel.UserID);

            // Collection Details entry
            referenceID = new CashCollectionDetailBal().Insert(senderID, ReceiverID, OS_OpeningBalance, cashCollectionAmount, OS_ClosingBalance, CashCollectionRemark, PayMode,
             AmountCreditDebitType.Credit, "", WalletID, "", CollectionType.CASH_COLLECTION_REVERSE, "");

            //Entry for outstanding decrease
            Result = TransactionDetailsInsertForCashCollection(ReceiverID, cashCollectionAmount,
                user.MobileNumber, senderID, OS_OpeningBalance, OS_ClosingBalance, WalletID,
                CashCollectionRemark, referenceID, AmountCreditDebitType.Credit, DOCType.CASH_COLLECTION_REVERSE, PayMode);

            try
            {
                new FCMSelection().SendCashCollectionMessage(senderID, Convert.ToDecimal(cashCollectionAmount), "", ObjModel.Outstanding, Message.CashCollectionMessage.FCM_Message_Cash_Collection_Reverse);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Result;
        }

        internal ReportWrapper SelectReport(ReportRequest mainRequest, int CurrentUserID, out string failureMessage)
        {
            ReportWrapper response = new ReportWrapper();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            List<Response.TransactionDetails.ViewModel> views = new List<Response.TransactionDetails.ViewModel>();

            views = new TransactionDetailsDAL().SelectByCashCallectionFromdateTodate(mainRequest, CurrentUserID);

            List<Report> report = new List<Report>();

            foreach (Response.TransactionDetails.ViewModel view in views)
            {
                Report row = new Report
                {
                    Amount = (float)view.Amount,
                    OpeningBalance = (float)view.OpeningBalance,
                    ClosingBalance = (float)view.ClosingBalance,
                    FirmName = view.FirmName,
                    Date = view.DOC,
                    Remark = view.TransactionDescription,
                    AssignedUserName = view.AssignedUserName,
                    Type = view.DOCType,
                    AmountCreditDebit = view?.AmountCreditDebit,
                    PayMode = view?.PayMode
                };
                report.Add(row);
            }
            if (report.Count() == 0)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(report, parallelOptions, res =>
            {
                if (res.AmountCreditDebit.EqualsIgnoreCase("CREDIT"))
                    res.Credit = res.AmountCreditDebit;
                if (res.AmountCreditDebit.EqualsIgnoreCase("DEBIT"))
                    res.Debit = res.AmountCreditDebit;
            });

            response.IsExcelDownload = mainRequest.IsExcelDownload;
            response.data = report;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string SendCollectionMessage(SelectByUserIDRequest updateRequest, int UserID)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (UserID != Company.AdminUserID)
                return Message.UserMessage.InvalidUser;

            UserMaster user = null;
            user = new UserDAL().SelectByIDFromCache(updateRequest.UserID);
            if (user == null)
                return Message.UserMessage.InvalidUser;
            CashCollectionMaster outstanding = new CashCollectionMaster();
            try
            {
                outstanding = new CashCollectionMasterDal().SelectByUserID(updateRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            new MessageSelection().SendCollectionMessage(user, outstanding.Outstanding);

            return StatusType.Ok;
        }

        internal SelectByUserIDResponse SelectByUserID(int userID)
        {
            SelectByUserIDResponse result = new SelectByUserIDResponse();
            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(userID);
            if (ObjModel == null)
            {
                return result;
            }

            List<Project.Response.Wallet.DetailViewModel> wallets = new UserWalletMasterDal().SelectByUserID(userID);

            string bbal = "";

            foreach (Project.Response.Wallet.DetailViewModel item in wallets)
            {
                bbal += $"{item.ShortName} : {(int)item.Balance}, ";
            }

            result.BalanceDetail = bbal;
            result.Outstanding = ObjModel.Outstanding;
            result.MaxOutStanding = ObjModel.MaxOutstanding;

            return result;
        }

        public void SendBulkOutstandingMessage()
        {
            string OutstandingRemainder = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(
                Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.OutstandingRemainder), "No");

            if (!OutstandingRemainder.EqualsIgnoreCase("Yes"))
                return;

            List<UserMaster> userList = new UserDAL().Select()
                .Where(T => T.UserTypeID == (int)UserType.Retailer ||
                       T.UserTypeID == (int)UserType.APIUser ||
                       T.UserTypeID == (int)UserType.Distributer ||
                       T.UserTypeID == (int)UserType.SuperDistributer).ToList();

            List<CashCollectionMaster> CollectionList = new CashCollectionMasterDal().Select();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(userList.AsEnumerable(), parallelOptions, user =>
            {
                CashCollectionMaster ObjModel = CollectionList.FirstOrDefault(u => u.UserID == user.UserID);
                if (ObjModel == null)
                    return;
                new MessageSelection().SendCollectionMessage(user, ObjModel.Outstanding);
            });
        }

        public void SendBulkMorningMessage()
        {
            List<UserMaster> userList = new UserDAL().Select()
                    .Where(T => T.UserTypeID == (int)UserType.Retailer ||
                           T.UserTypeID == (int)UserType.APIUser ||
                           T.UserTypeID == (int)UserType.Distributer ||
                           T.UserTypeID == (int)UserType.SuperDistributer).ToList();

            string OnlyActiveUserToMorningMessage = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(
                Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.OnlyActiveUserToMorningMessage), "Yes");
           
            List<FundTransaction> Activelist = new TransactionDAL().IsLast5DaysActiveUser();
            string cashCollection = new FundBal().IsCashCollection();
            List<CashCollectionMaster> CollectionList = new CashCollectionMasterDal().Select();
            List<Response.Wallet.UserBalanceViewModel> userBalances = new UserWalletMasterDal().SelectUserWalletBalance();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(userList.AsEnumerable(), parallelOptions, user =>
            {
                if (OnlyActiveUserToMorningMessage.EqualsIgnoreCase("Yes"))
                {
                    FundTransaction usermodel = Activelist.FirstOrDefault(u => u.ReceiverID == user.UserID);
                    if (usermodel == null)
                        return;
                }
                decimal Outstanding = 0;
                if (cashCollection.EqualsIgnoreCase("true"))
                {
                    CashCollectionMaster ObjModel = CollectionList.FirstOrDefault(u => u.UserID == user.UserID);
                    if (ObjModel != null)
                        Outstanding = ObjModel.Outstanding;
                }
                Response.Wallet.UserBalanceViewModel balance = userBalances.FirstOrDefault(u => u.UserID == user.UserID);
                new MessageSelection().SendMorningMessage(user, Outstanding, balance.RechargeBalance, balance.DMTBalance);
            });
        }
       internal (OutstandingWrapper,string) OutstandingReport(SelectByUserIDRequest mainRequest)
        {
            OutstandingWrapper wrapper = new OutstandingWrapper();
            List<OutstandingViewModel> list = new CashCollectionMasterDal().OutstandingReport(mainRequest);
            string FailureMessage = "";
            if (list.Count < 0)
            { 
                FailureMessage = Message.DaybookReport.NotFoundReport;
                return (wrapper, FailureMessage);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.SearchText).ToList();
            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.data = list;
            FailureMessage = StaticModel.StatusType.Ok;
            return (wrapper, FailureMessage);
        }
    }
}