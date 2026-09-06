using Microsoft.EntityFrameworkCore;
using Project.Model;
using Project.Processing;
using Project.Requests.Fund;
using Project.Response.Fund;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class FundTransactionDal
    {
        public int Insert(FundTransaction ObjModel)
        {
            using var context = new DatabaseContext();
            context.FundTransaction.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.FundTransactionID;
        }

        internal int Update(FundTransaction objModel)
        {
            using var context = new DatabaseContext();
            context.FundTransaction.Update(objModel);
            context.SaveChanges();
            return objModel.FundTransactionID;
        }

        internal FundTransaction SelectByID(int FundTransactionID)
        {
            using var context = new DatabaseContext();
            return context.FundTransaction.FirstOrDefault(W => W.FundTransactionID == FundTransactionID);
        }

        internal List<RequestViewModel> SelectByRequestToID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            if (selectRequest.Type.EqualsIgnoreCase(FundTransferType.Received))
                list = SelectByDownline(From, To, UserID);
            else
                list = SelectByOwn(From, To, UserID);

            if (!selectRequest.Status.EqualsIgnoreCase("All"))
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();

            return list;
        }

        private List<RequestViewModel> SelectByDownline(int fromDate, int toDate, int userID)
        {
            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.RequestToID == userID
                        && fundTransaction.DOCInt >= fromDate
                        && fundTransaction.DOCInt <= toDate
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            AdminRemark = fundTransaction.AdminRemark,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            BankName = fundTransaction.BankName,
                            ReceiptPath = fundTransaction.ReceiptPath.ConcatWebAPIURL(),
                            ReceiverID = fundTransaction.ReceiverID,
                            UTRNumber = fundTransaction.UTRNumber,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            TransactionDOC = fundTransaction.TransactionDOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = (fundTransaction.Status == StatusType.Process) ? "" : fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                        }).ToList();
            }
            return list;
        }

        internal FundTransaction SelectTransactionByReceiverIDAndRemark(int UserID, string Remark)
        {
            using var context = new DatabaseContext();
            return context.FundTransaction.FirstOrDefault(f => f.ReceiverID == UserID && f.Remark == Remark);
        }

        internal FundTransaction SelectTransactionByReceiverIDAndCashCollectionRemark(int UserID, string Remark)
        {
            using var context = new DatabaseContext();
            return context.FundTransaction.FirstOrDefault(f => f.ReceiverID == UserID && f.CashCollectionRemark == Remark);
        }

        internal FundTransaction SelectTransactionByReceiverIDAmountAndForDuplicate(int UserID, decimal Amount, DateTime date)
        {
            using var context = new DatabaseContext();
            return context.FundTransaction.OrderByDescending(f => f.FundTransactionID).FirstOrDefault(f => f.ReceiverID == UserID && f.Amount == Amount && f.DOC > date);
        }

        internal FundTransaction SelectTransactionUTRNumberForDuplicate(int UserID, string UTRNumber)
        {
            using var context = new DatabaseContext();
            return context.FundTransaction.OrderByDescending(f => f.FundTransactionID).FirstOrDefault(f => f.ReceiverID == UserID && f.UTRNumber == UTRNumber);
        }
        internal FundTransaction SelectReferralTransactionByReceiverIDAmountAndForDuplicate(int UserID, decimal Amount, string UTRNumber)
        {
            using var context = new DatabaseContext();
            return context.FundTransaction.OrderByDescending(f => f.FundTransactionID).FirstOrDefault(f => f.ReceiverID == UserID && f.Amount == Amount && f.UTRNumber == UTRNumber);
        }
        internal bool CheckDuplicateTransaction(int UserID, decimal Amount, DateTime CurrentDateTime)
        {
            int yesterdayInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-1)).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                bool hasDuplicateTransaction = context.FundTransaction
                    .Where(t => t.DOCInt >= yesterdayInt && t.ReceiverID == UserID)
                    .Any(t => t.Amount == Amount &&
                              t.UpdatedDOC > CurrentDateTime &&
                              t.Status == StatusType.Success);

                return hasDuplicateTransaction;
            }
        }

        internal FundTransaction SelectTransactionForDuplicateCheckCore(FundTransaction obj, DateTime date)
        {
            //int UserID,int WalletID, decimal Amount, string paymode, string type, string status
            using var context = new DatabaseContext();
            return context.FundTransaction.FirstOrDefault(f => f.ReceiverID == obj.ReceiverID &&
                           f.WalletID == obj.WalletID && f.Amount == obj.Amount &&
                           f.PayMode == obj.PayMode && f.Type == obj.Type &&
                           f.Through == obj.Through &&
                           f.Status == obj.Status && f.DOC > date);
        }

        internal FundTransaction SelectTransactionForDuplicateCheckForPGTransaction(FundTransaction obj)
        {
            using var context = new DatabaseContext();
            return context.FundTransaction.FirstOrDefault(f => f.ReceiverID == obj.ReceiverID &&
                           f.WalletID == obj.WalletID && f.Through == obj.Through &&
                           f.UTRNumber == obj.UTRNumber);

            //&& f.AdminRemark == obj.AdminRemark
            //&& f.Amount == obj.Amount : no need to check amount, bcz amount is ++ or -- if admin set comm or surcharge
            //f.Type == obj.Type &&
        }

        internal async Task<int> SelectFundRequestCount(int UserID)
        {
            int FundRequestCount = 0;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                FundRequestCount = await context.FundTransaction.Where(f => f.RequestToID == UserID && f.Status == StatusType.Process &&
                                                                       f.Type == FundTransferType.TransferRequest).CountAsync();
            }
            return FundRequestCount;
        }

        internal async Task<int> SelectFundReverseRequestCount(int UserID)
        {
            int FundReverseRequestCount = 0;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                FundReverseRequestCount = await context.FundTransaction.Where(f => f.RequestToID == UserID && f.Status == StatusType.Process &&
                                                                       f.Type == FundTransferType.ReverseRequest).CountAsync();
            }
            return FundReverseRequestCount;
        }

        private List<RequestViewModel> SelectByOwn(int fromDate, int toDate, int userID)
        {
            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.ReceiverID == userID
                        && fundTransaction.DOCInt >= fromDate
                        && fundTransaction.DOCInt <= toDate
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.RequestTo.FirmName.ConcatNameMobileAliasName(fundTransaction.RequestTo.MobileNumber, fundTransaction.RequestTo.AliasName),
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            AdminRemark = fundTransaction.AdminRemark,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            ReceiptPath = fundTransaction.ReceiptPath.ConcatWebAPIURL(),
                            BankName = fundTransaction.BankName,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            TransactionDOC = fundTransaction.TransactionDOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = (fundTransaction.Status == StatusType.Process) ? "" : fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            UTRNumber = fundTransaction.UTRNumber,
                        }).ToList();
            }
            return list;
        }

        internal decimal SelectReversalBlockedAmountByWalletID(int walletID, int UserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                var FundReverse = (from fund in context.FundTransaction
                                   where fund.SenderID == UserID
                                   && fund.WalletID == walletID
                                    && fund.Type == FundTransferType.ReverseRequest
                                    && fund.Status == StatusType.Process
                                   group fund by 1 into grp
                                   select new
                                   {
                                       Reverse = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                decimal ReverseBal = FundReverse?.Reverse ?? 0;
                return ReverseBal;
            }
        }


        public bool ReferralTransferLimit(int ReceiverID)
        {
            List<FundTransaction> list = new List<FundTransaction>();
            int today = int.Parse(DateTime.Today.ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.ReceiverID == ReceiverID
                             && fundTransaction.PayMode == FundTransferType.RefrralTransfer
                             && fundTransaction.DOCInt == today
                        select fundTransaction).ToList();


                if (list.Count <= 3)
                    return true;
                return false;

            }
        }

        internal List<RequestViewModel> SelectBySenderID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));


            List<RequestViewModel> list = new List<RequestViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<UserMaster> userList = new UserDAL().Select();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        join user in context.UserMaster on fundTransaction.RequestAcceptedUserID equals user.UserID
                        into userleft
                        from LFUser in userleft.DefaultIfEmpty()

                        join state in context.StateMaster on fundTransaction.Receiver.StateID equals state.ID
                        into stateDetails
                        from Leftstate in stateDetails.DefaultIfEmpty()

                        where fundTransaction.SenderID == UserID
                             && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        && (fundTransaction.Through == FundTransferType.ThroughDirectTransfer
                        || fundTransaction.Through == FundTransferType.ThroughRequestTransfer)
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                         && (!selectRequest.PayMode.EqualsIgnoreCase("All") ? fundTransaction.PayMode == selectRequest.PayMode : 2 > 1)
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            TxnID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Type = fundTransaction.Type,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Through = fundTransaction.Through,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            TransactionDOC = fundTransaction.TransactionDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                            SenderClosingBalance = fundTransaction.SenderClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                            IsDelete = fundTransaction.IsDelete,
                            RequestAcceptedUser = LFUser.FirmName.ConcatNameMobileAliasName(LFUser.MobileNumber ?? "", LFUser.AliasName ?? "") ?? "",
                            State = Leftstate.Name ?? ""
                        }).ToList();



            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.ReceiverID == selectRequest.UserID
                        select item).ToList();
            }
            if (Company.BackDateFundTransfer.EqualsIgnoreCase("Yes"))
            {
                list = (from item in list
                        where item.IsDelete == false
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectDownlineFundTransferReportForAdmin(SelectRequest selectRequest, List<Response.User.DownlineForDropDownResponse> senderuserlist, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            List<FundTransaction> tranList = new List<FundTransaction>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.DOCInt >= From &&
                        fundTransaction.DOCInt <= To &&
                        (fundTransaction.Through == FundTransferType.ThroughDirectTransfer ||
                        fundTransaction.Through == FundTransferType.ThroughRequestTransfer) &&
                        (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1) &&
                        (!selectRequest.PayMode.EqualsIgnoreCase("ALL") ? fundTransaction.PayMode == selectRequest.PayMode : 2 > 1)
                        orderby fundTransaction.DOC descending
                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            TxnID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Type = fundTransaction.Type,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Through = fundTransaction.Through,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                            SenderClosingBalance = fundTransaction.SenderClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber
                        }).ToList();
            }

            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }
            list = (from fundTransaction in list
                    join sender in senderuserlist on fundTransaction.SenderID equals sender.UserID
                    select new RequestViewModel
                    {
                        SenderID = fundTransaction.SenderID,
                        ReceiverID = fundTransaction.ReceiverID,
                        FundTransactionID = fundTransaction.FundTransactionID,
                        TxnID = fundTransaction.TxnID,
                        Receiver = fundTransaction.Receiver,
                        Sender = fundTransaction.Sender,
                        ReceiverName = fundTransaction.ReceiverName,
                        ReceiverFirmName = fundTransaction.ReceiverFirmName,
                        ReceiverMobile = fundTransaction.ReceiverMobile,
                        SenderName = fundTransaction.SenderName,
                        SenderFirmName = fundTransaction.SenderFirmName,
                        SenderMobile = fundTransaction.SenderMobile,
                        Type = fundTransaction.Type,
                        Amount = fundTransaction.Amount,
                        OriginalAmount = fundTransaction.OriginalAmount,
                        Wallet = fundTransaction.Wallet,
                        Status = fundTransaction.Status,
                        Remark = fundTransaction.Remark,
                        SenderRemark = fundTransaction.AdminRemark,
                        Through = fundTransaction.Through,
                        PayMode = fundTransaction.PayMode,
                        DOCType = fundTransaction.DOCType,
                        DOC = fundTransaction.DOC,
                        BankName = fundTransaction.BankName,
                        ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                        ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                        SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                        SenderClosingBalance = fundTransaction.SenderClosingBalance,
                        UTRNumber = fundTransaction.UTRNumber
                    }).ToList();

            return list;
        }

        internal RequestHeaderModel RequestHeaderData(List<RequestViewModel> returnList)
        {
            return (from item in returnList
                    group item by 1 into grp
                    select new RequestHeaderModel
                    {
                        TotalSuccess = grp.Where(t => t.Status == StatusType.Success).Sum(x => x.Amount),
                        TotalFailure = grp.Where(t => t.Status == StatusType.Failure).Sum(x => x.Amount),
                        TotalProcess = grp.Where(t => t.Status == StatusType.Process).Sum(x => x.Amount),
                    }).FirstOrDefault();
        }

        internal List<RequestViewModel> SelectPGTransferBySenderID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<RequestViewModel> list = new List<RequestViewModel>();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction

                        join state in context.StateMaster on fundTransaction.Sender.StateID equals state.ID
                        into stateDetails
                        from state in stateDetails.DefaultIfEmpty()

                        where fundTransaction.SenderID == UserID
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.Through == FundTransferType.PGTransfer
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Type = fundTransaction.Type,
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Through = fundTransaction.Through,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                            SenderClosingBalance = fundTransaction.SenderClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                            State = state.Name,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.ReceiverID == selectRequest.UserID
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectECollectionTransferBySenderID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<RequestViewModel> list = new List<RequestViewModel>();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.SenderID == UserID
                        && fundTransaction.Through == FundTransferType.ECollection
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Type = fundTransaction.Type,
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Through = fundTransaction.Through,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.ReceiverID == selectRequest.UserID
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectByReceiverID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        join user in context.UserMaster on fundTransaction.RequestAcceptedUserID equals user.UserID
                        into UserLF
                        from UL in UserLF.DefaultIfEmpty()
                        where fundTransaction.ReceiverID == UserID
                          && (fundTransaction.Through == FundTransferType.ThroughDirectTransfer
                        || fundTransaction.Through == FundTransferType.ThroughRequestTransfer)
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            TxnID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Through = fundTransaction.Through,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Type = fundTransaction.Type,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                            IsDelete = fundTransaction.IsDelete,
                            RequestAcceptedUser = UL.FirmName.ConcatNameMobileAliasName(UL.MobileNumber ?? "", UL.AliasName ?? "") ?? "",
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }
            if (Company.BackDateFundTransfer.EqualsIgnoreCase("Yes"))
            {
                list = (from item in list
                        where item.IsDelete == false
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectByTotalTransferReport(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        join user in context.UserMaster on fundTransaction.RequestAcceptedUserID equals user.UserID
                        into userleft
                        from LFUser in userleft.DefaultIfEmpty()
                        where fundTransaction.SenderID == UserID
                             && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To

                        && (fundTransaction.Through == FundTransferType.ThroughDirectTransfer
                        || fundTransaction.Through == FundTransferType.ThroughRequestTransfer)

                         && (!selectRequest.PayMode.EqualsIgnoreCase(StatusType.ALL) ? fundTransaction.PayMode == selectRequest.PayMode : 2 > 1)

                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            ReceiverID = fundTransaction.ReceiverID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            Through = fundTransaction.Through,
                            Type = fundTransaction.Type,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            WalletID = fundTransaction.WalletID,
                            Wallet = selectRequest.WalletID == 0 ? StaticModel.StatusType.ALL : fundTransaction.WalletMaster.Name ?? "",
                            Status = selectRequest.Status.EqualsIgnoreCase(StatusType.ALL) ? StatusType.Success : fundTransaction.Status

                        }).ToList();
            }
            if (selectRequest.WalletID > 0)
            {
                list = (from item in list
                        where item.WalletID == selectRequest.WalletID
                        select item).ToList();
            }

            if (!selectRequest.Status.EqualsIgnoreCase(StatusType.ALL))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.ReceiverID == selectRequest.UserID
                        select item).ToList();
            }

            list = list
                .GroupBy(fundTransaction => fundTransaction.ReceiverID)
                .Select(group => new RequestViewModel
                {
                    ReceiverID = group.Key,
                    Amount = group.Sum(fundTransaction => fundTransaction.Amount),
                    OriginalAmount = group.Sum(fundTransaction => fundTransaction.OriginalAmount),
                    Receiver = group.First().Receiver,
                    Sender = group.First().Sender,
                    Through = group.First().Through,
                    Type = group.First().Type,
                    Wallet = group.First().Wallet,
                    Status = group.First().Status,
                }).ToList();


            return list;

        }

        internal List<RequestViewModel> SelectByPGReceiverID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.ReceiverID == UserID
                          && fundTransaction.Through == FundTransferType.PGTransfer
                           && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            TxnID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),

                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            Through = fundTransaction.Through,
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Type = fundTransaction.Type,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectByECollectionReceiverID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.ReceiverID == UserID
                          && fundTransaction.Through == FundTransferType.ECollection
                           && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            TxnID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),

                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            Through = fundTransaction.Through,
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Type = fundTransaction.Type,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectReverseCreditByReceiverID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.ReceiverID == UserID
                          && (fundTransaction.Through == FundTransferType.ThroughReverseRequestTransfer
                        || fundTransaction.Through == FundTransferType.ThroughDirectReverse)
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Through = fundTransaction.Through,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Type = fundTransaction.Type,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.SenderID == selectRequest.UserID
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectDownlineFundReverseReportForAdmin(SelectRequest selectRequest, List<Response.User.DownlineForDropDownResponse> receiveruserlist, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where (fundTransaction.Through == FundTransferType.ThroughReverseRequestTransfer
                        || fundTransaction.Through == FundTransferType.ThroughDirectReverse)
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Through = fundTransaction.Through,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Type = fundTransaction.Type,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            UTRNumber = fundTransaction.UTRNumber,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            //if (selectRequest.UserID != 0)
            //{
            //    list = (from item in list
            //            where item.SenderID == selectRequest.UserID
            //            select item).ToList();
            //}
            list = (from fundTransaction in list
                    join receiver in receiveruserlist on fundTransaction.ReceiverID equals receiver.UserID
                    select new RequestViewModel
                    {
                        SenderID = fundTransaction.SenderID,
                        ReceiverID = fundTransaction.ReceiverID,
                        FundTransactionID = fundTransaction.FundTransactionID,
                        TxnID = fundTransaction.TxnID,
                        Receiver = fundTransaction.Receiver,
                        Sender = fundTransaction.Sender,
                        ReceiverName = fundTransaction.ReceiverName,
                        ReceiverFirmName = fundTransaction.ReceiverFirmName,
                        ReceiverMobile = fundTransaction.ReceiverMobile,
                        SenderName = fundTransaction.SenderName,
                        SenderFirmName = fundTransaction.SenderFirmName,
                        SenderMobile = fundTransaction.SenderMobile,
                        Type = fundTransaction.Type,
                        Amount = fundTransaction.Amount,
                        OriginalAmount = fundTransaction.OriginalAmount,
                        Wallet = fundTransaction.Wallet,
                        Status = fundTransaction.Status,
                        Remark = fundTransaction.Remark,
                        SenderRemark = fundTransaction.AdminRemark,
                        Through = fundTransaction.Through,
                        PayMode = fundTransaction.PayMode,
                        DOCType = fundTransaction.DOCType,
                        DOC = fundTransaction.DOC,
                        BankName = fundTransaction.BankName,
                        ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                        ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                        SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                        SenderClosingBalance = fundTransaction.SenderClosingBalance,
                        UTRNumber = fundTransaction.UTRNumber,
                    }).ToList();

            return list;
        }

        internal List<RequestViewModel> SelectWallettransfer(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction

                        join state in context.StateMaster on fundTransaction.Sender.StateID equals state.ID into stateGroup
                        from state in stateGroup.DefaultIfEmpty()

                        where // fundTransaction.ReceiverID == UserID &&

                          (fundTransaction.Through == FundTransferType.ThroughWalletTransfer
                       )
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Through = fundTransaction.Through,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Type = fundTransaction.Type,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                            SenderClosingBalance = fundTransaction.SenderClosingBalance,
                            UTRNumber = fundTransaction.UTRNumber,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                            State = state.Name,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.SenderID == selectRequest.UserID
                        select item).ToList();
            }
            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                list = (from item in list
                        where item.ReceiverID == UserID
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectReverseDebitBySenderID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.SenderID == UserID
                       && (fundTransaction.Through == FundTransferType.ThroughReverseRequestTransfer
                        || fundTransaction.Through == FundTransferType.ThroughDirectReverse)
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Type = fundTransaction.Type,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Through = fundTransaction.Through,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                            SenderClosingBalance = fundTransaction.SenderClosingBalance,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectWallettoOther(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where //fundTransaction.SenderID == UserID &&
                        (fundTransaction.Type == FundTransferType.ThroughWalletToOther)
                         //       (fundTransaction.Through == FundTransferType.ThroughWalletToOther
                         //     || fundTransaction.Through == FundTransferType.ThroughDirectTransfer)
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Type = fundTransaction.Type,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Through = fundTransaction.Through,
                            PayMode = fundTransaction.PayMode,
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                            SenderClosingBalance = fundTransaction.SenderClosingBalance,
                            OrignalFirmName = fundTransaction.Receiver.FirmName,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectBySenderIDAndReceiverID(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where (fundTransaction.SenderID == UserID
                        || fundTransaction.ReceiverID == UserID)
                         && (selectRequest.WalletID != 0 ? fundTransaction.WalletID == selectRequest.WalletID : 2 > 1)
                        && fundTransaction.DOCInt >= From
                        && fundTransaction.DOCInt <= To
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            SenderID = fundTransaction.SenderID,
                            ReceiverID = fundTransaction.ReceiverID,
                            TxnID = fundTransaction.FundTransactionID,
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ReceiverName = fundTransaction.Receiver.PersonName,
                            ReceiverFirmName = fundTransaction.Receiver.FirmName,
                            ReceiverMobile = fundTransaction.Receiver.MobileNumber,
                            SenderName = fundTransaction.Sender.PersonName,
                            SenderFirmName = fundTransaction.Sender.FirmName,
                            SenderMobile = fundTransaction.Sender.MobileNumber,
                            Through = fundTransaction.Through,
                            Amount = fundTransaction.Amount,
                            OriginalAmount = fundTransaction.OriginalAmount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            SenderRemark = fundTransaction.AdminRemark,
                            Type = fundTransaction.Type,
                            // PayMode = fundTransaction.PayMode,
                            PayMode = (fundTransaction.Through.EqualsIgnoreCase(FundTransferType.ThroughWalletTransfer) ? "" : fundTransaction.PayMode),
                            DOCType = fundTransaction.Type,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = fundTransaction.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            BankName = fundTransaction.BankName,
                            ReceiverOpeningBalance = fundTransaction.ReceiverOpeningBalance,
                            ReceiverClosingBalance = fundTransaction.ReceiverClosingBalance,
                            SenderOpeningBalance = fundTransaction.SenderOpeningBalance,
                            SenderClosingBalance = fundTransaction.SenderClosingBalance,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }
            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.SenderID == selectRequest.UserID
                        || item.ReceiverID == selectRequest.UserID
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectLast10Request(int UserID)
        {
            List<RequestViewModel> list = new List<RequestViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.ReceiverID == UserID

                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),

                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,

                            PayMode = fundTransaction.PayMode,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                        }).Take(10).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectLast10ByReceiverID(int SenderID, int ReceiverID, int WalletID)
        {
            List<RequestViewModel> list = new List<RequestViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.SenderID == SenderID &&
                        fundTransaction.ReceiverID == ReceiverID &&
                        fundTransaction.WalletID == WalletID

                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),

                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,

                            PayMode = fundTransaction.PayMode,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                        }).Take(10).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectLast10BySenderID(int SenderID)
        {
            List<RequestViewModel> list = new List<RequestViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.SenderID == SenderID
                        && fundTransaction.Through == FundTransferType.ThroughDirectTransfer
                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.AdminRemark,
                            PayMode = fundTransaction.PayMode,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                        }).Take(10).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectLast10ForReverseTransaction(int SenderID, int WalletID, int UserID)
        {
            List<RequestViewModel> list = new List<RequestViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        where fundTransaction.SenderID == SenderID &&
                        fundTransaction.WalletID == WalletID && (
                        fundTransaction.Through == FundTransferType.ThroughDirectReverse ||
                        fundTransaction.Through == FundTransferType.ReverseRequest)

                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),

                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,

                            PayMode = fundTransaction.PayMode,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                        }).Take(10).ToList();
            }

            return list;
        }

        internal RequestViewModel SelectDetailByID(int RequestToID, int FundTransactionID)
        {
            int WalletID;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                WalletID = context.FundTransaction.FirstOrDefault(o => o.RequestToID == RequestToID && o.FundTransactionID == FundTransactionID).WalletID;
            }

            decimal SenderBalance = new UserWalletMasterDal().SelectSumByUserIDAndWalletID(RequestToID, WalletID);
            RequestViewModel list = new RequestViewModel();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from fundTransaction in context.FundTransaction
                        join user in context.UserMaster on fundTransaction.ReceiverID equals user.UserID
                        join parent in context.UserMaster on user.ParentID equals parent.UserID
                        where fundTransaction.RequestToID == RequestToID &&
                        fundTransaction.FundTransactionID == FundTransactionID &&
                        fundTransaction.Status == StatusType.Process

                        orderby fundTransaction.DOC descending

                        select new RequestViewModel
                        {
                            FundTransactionID = fundTransaction.FundTransactionID,
                            Receiver = fundTransaction.Receiver.FirmName.ConcatNameMobileAliasName(fundTransaction.Receiver.MobileNumber, fundTransaction.Receiver.AliasName),
                            Sender = fundTransaction.Sender.FirmName.ConcatNameMobileAliasName(fundTransaction.Sender.MobileNumber, fundTransaction.Sender.AliasName),
                            ParentName = parent.FirmName.ConcatNameMobileAliasName(parent.MobileNumber, parent.AliasName),
                            SenderBalance = SenderBalance,
                            Amount = fundTransaction.Amount,
                            Wallet = fundTransaction.WalletMaster.Name,
                            Status = fundTransaction.Status,
                            Remark = fundTransaction.Remark,
                            UTRNumber = fundTransaction.UTRNumber,
                            PayMode = fundTransaction.PayMode,
                            DOC = fundTransaction.DOC.ToString(Messages.DOCDisplayFormat),
                        }).FirstOrDefault();
            }

            return list;
        }

        internal async Task<DashboardFundViewModel> SelectTransferForDashboard(int UserID, string Date)
        {
            DateTime selectedDate = Convert.ToDateTime(Date);
            int dateInt = Convert.ToInt32(selectedDate.ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var fundSelectedDate = (from fund in context.FundTransaction
                                        where fund.DOCInt == dateInt && fund.Status == StatusType.Success
                                        || (fund.Through == FundTransferType.ThroughRequestTransfer && fund.UpdatedDOC.Date == selectedDate.Date)
                                        select fund).ToList();

                var mainListTransfer = (from fund in fundSelectedDate
                                        where fund.SenderID == UserID
                                             && (fund.Through == FundTransferType.ThroughRequestTransfer || fund.Through == FundTransferType.ThroughDirectTransfer)
                                        select new
                                        {
                                            Amount = fund.Amount,
                                            DOCInt = fund.DOCInt,
                                            UpdatedDOCInt = Convert.ToInt32(Convert.ToDateTime(fund.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                                        }).ToList();

                var mainListPGTransfer = (from fund in fundSelectedDate
                                          where fund.SenderID == UserID
                                          && fund.Through == FundTransferType.PGTransfer
                                          select new
                                          {
                                              Amount = fund.Amount,
                                              DOCInt = fund.DOCInt,
                                          }).ToList();

                var mainListECollection = (from fund in fundSelectedDate
                                           where fund.SenderID == UserID
                                           && fund.Through == FundTransferType.ECollection
                                           select new
                                           {
                                               Amount = fund.Amount,
                                               DOCInt = fund.DOCInt
                                           }).ToList();

                var mainListReverse = (from fund in fundSelectedDate
                                       where fund.ReceiverID == UserID
                                       && (fund.Through == FundTransferType.ThroughDirectReverse
                                      || fund.Through == FundTransferType.ThroughReverseRequestTransfer)

                                       select new
                                       {
                                           Amount = fund.Amount,
                                           DOCInt = fund.DOCInt
                                       }).ToList();

                var tranferList = (from fund in mainListTransfer
                                   where fund.DOCInt == dateInt
                                   || fund.UpdatedDOCInt == dateInt
                                   group fund by new { fund.UpdatedDOCInt } into grp
                                   select new
                                   {
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                var PGtranferList = (from fund in mainListPGTransfer
                                     group fund by new { fund.DOCInt } into grp
                                     select new
                                     {
                                         Count = grp.Count(),
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();

                var eCollectionList = (from fund in mainListECollection
                                       group fund by new { fund.DOCInt } into grp
                                       select new
                                       {
                                           Count = grp.Count(),
                                           Amount = grp.Sum(x => x.Amount),
                                       }).FirstOrDefault();

                var reverseList = (from fund in mainListReverse
                                   group fund by new { fund.DOCInt } into grp
                                   select new
                                   {
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                DashboardFundViewModel response = new DashboardFundViewModel();

                response.TransferAmount = tranferList?.Amount ?? 0;
                response.TransferCount = tranferList?.Count ?? 0;
                response.PGTransferAmount = PGtranferList?.Amount ?? 0;
                response.ECollectionAmount = eCollectionList?.Amount ?? 0;

                response.ReverseAmount = reverseList?.Amount ?? 0;
                await Task.Delay(1);
                return response;
            }
        }

        internal async Task<DashboardFundViewModel> SelectFundForRetailer(int UserID, string Date)
        {
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {

                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainListSelectedDate = (from fund in context.FundTransaction
                                            where fund.DOCInt == dateInt
                                            select fund).ToList();

                var mainListReceived = (from fund in mainListSelectedDate
                                        where fund.ReceiverID == UserID
                                         && (fund.Through == FundTransferType.ThroughRequestTransfer
                                         || fund.Through == FundTransferType.ThroughDirectTransfer)

                                        select new
                                        {
                                            Amount = fund.Amount,
                                            DOCInt = fund.DOCInt
                                        }).ToList();
                var mainListPGReceived = (from fund in mainListSelectedDate
                                          where fund.ReceiverID == UserID
                                          && (fund.Through == FundTransferType.PGTransfer
                                          || fund.Through == FundTransferType.ECollection)

                                          select new
                                          {
                                              Amount = fund.Amount,
                                              DOCInt = fund.DOCInt
                                          }).ToList();

                var mainListReverse = (from fund in mainListSelectedDate
                                       where fund.SenderID == UserID
                                       && (fund.Through == FundTransferType.ThroughDirectReverse
                                       || fund.Through == FundTransferType.ThroughReverseRequestTransfer)
                                       select new
                                       {
                                           Amount = fund.Amount,
                                           DOCInt = fund.DOCInt
                                       }).ToList();

                var tranferList = (from fund in mainListReceived
                                   group fund by new { fund.DOCInt } into grp
                                   select new
                                   {
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();
                var PGtranferList = (from fund in mainListPGReceived

                                     group fund by new { fund.DOCInt } into grp
                                     select new
                                     {
                                         Count = grp.Count(),
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();

                var reverseList = (from fund in mainListReverse

                                   group fund by new { fund.DOCInt } into grp
                                   select new
                                   {
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                DashboardFundViewModel response = new DashboardFundViewModel();

                response.PurchaseAmount = tranferList?.Amount ?? 0;
                response.PurchaseCount = tranferList?.Count ?? 0;
                response.ReverseAmount = reverseList?.Amount ?? 0;
                response.PGPurchaseAmount = PGtranferList?.Amount ?? 0;
                await Task.Delay(1);

                return response;
            }
        }

        internal async Task<DashboardFundViewModel> SelectTransferForDistributor(int UserID, string Date)
        {
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainListTransfer = (from fund in context.FundTransaction
                                        where fund.SenderID == UserID
                                         && fund.DOCInt == dateInt
                                        && (fund.Through == FundTransferType.ThroughRequestTransfer
                                       || fund.Through == FundTransferType.ThroughDirectTransfer)

                                        select new
                                        {
                                            Amount = fund.Amount,
                                            DOCInt = fund.DOCInt
                                        }).ToList();

                var mainListReceived = (from fund in context.FundTransaction
                                        where fund.ReceiverID == UserID
                                         && fund.DOCInt == dateInt
                                        && (fund.Through == FundTransferType.ThroughRequestTransfer
                                       || fund.Through == FundTransferType.ThroughDirectTransfer
                                      || fund.Through == FundTransferType.PGTransfer)

                                        select new
                                        {
                                            Amount = fund.Amount,
                                            DOCInt = fund.DOCInt
                                        }).ToList();

                var mainListDeducted = (from fund in context.FundTransaction
                                        where fund.ReceiverID == UserID
                                        && (fund.Through == FundTransferType.ThroughDirectReverse
                                       || fund.Through == FundTransferType.ThroughReverseRequestTransfer)
                                        && fund.DOCInt == dateInt
                                        select new
                                        {
                                            Amount = fund.Amount,
                                            DOCInt = fund.DOCInt
                                        }).ToList();

                var mainListReverse = (from fund in context.FundTransaction
                                       where fund.SenderID == UserID

                                       && (fund.Through == FundTransferType.ThroughDirectReverse
                                      || fund.Through == FundTransferType.ThroughReverseRequestTransfer)
                                       && fund.DOCInt == dateInt
                                       select new
                                       {
                                           Amount = fund.Amount,
                                           DOCInt = fund.DOCInt
                                       }).ToList();

                var tranferList = (from fund in mainListTransfer

                                   group fund by new { fund.DOCInt } into grp
                                   select new
                                   {
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                var receivedList = (from fund in mainListReceived

                                    group fund by new { fund.DOCInt } into grp
                                    select new
                                    {
                                        Count = grp.Count(),
                                        Amount = grp.Sum(x => x.Amount),
                                    }).FirstOrDefault();

                var DeductedList = (from fund in mainListDeducted

                                    group fund by new { fund.DOCInt } into grp
                                    select new
                                    {
                                        Count = grp.Count(),
                                        Amount = grp.Sum(x => x.Amount),
                                    }).FirstOrDefault();

                var reverseList = (from fund in mainListReverse

                                   group fund by new { fund.DOCInt } into grp
                                   select new
                                   {
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                DashboardFundViewModel response = new DashboardFundViewModel();

                response.TransferAmount = tranferList?.Amount ?? 0;
                response.TransferCount = tranferList?.Count ?? 0;
                response.PurchaseAmount = receivedList?.Amount ?? 0;
                response.PurchaseCount = receivedList?.Count ?? 0;

                response.DownlineReverseAmount = DeductedList?.Amount ?? 0;

                response.ReverseAmount = reverseList?.Amount ?? 0;

                await Task.Delay(1);

                return response;
            }
        }

        internal string SelectReferenceNoByFundTransactionID(int FundTransactionID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.FundTransaction.FirstOrDefault(o => o.FundTransactionID == FundTransactionID).AdminRemark;
            }
        }

        internal string SelectRemarkByFundTransactionID(int FundTransactionID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.FundTransaction.FirstOrDefault(o => o.FundTransactionID == FundTransactionID).Remark;
            }
        }

        internal FundTransferViewModel SelectFundTransferByUserIDAndWalletID(int From, int UserID, int WalletID)
        {
            FundTransferViewModel response = null;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var MainFund = (from fund in context.FundTransaction
                                where fund.DOCInt == From
                                && fund.SenderID == UserID
                                && (WalletID != 0 ? fund.WalletID == WalletID : 2 > 1)
                                select new
                                {
                                    Amount = fund.Amount,
                                    DOCInt = fund.DOCInt,
                                    Through = fund.Through,
                                    WalletID = fund.WalletID
                                }).ToList();

                response = (from fund in MainFund
                            group fund by new { fund.DOCInt } into grp
                            select new FundTransferViewModel
                            {
                                Transfer = grp.Where(x => x.Through == FundTransferType.ThroughRequestTransfer
                                  || x.Through == FundTransferType.ThroughDirectTransfer).Sum(x => x.Amount),
                                PGTransfer = grp.Where(x => x.Through == FundTransferType.ECollection
                                   || x.Through == FundTransferType.PGTransfer).Sum(x => x.Amount)
                            }).FirstOrDefault();

                return response;
            }
        }

        internal decimal SelectWalletTransferWalletID(int From, int WalletID)
        {
            decimal WalletTransfer = 0;
            if (WalletID == (int)WalletCode.AEPS)
                WalletID = 0;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var Transfer = (from fund in context.FundTransaction
                                where fund.DOCInt == From
                                && (fund.Through == FundTransferType.ThroughWalletTransfer)
                                && (WalletID != 0 ? fund.WalletID == WalletID : 2 > 1)
                                group fund by 1 into grp
                                select new
                                {
                                    WalletTransfer = grp.Sum(x => x.Amount),
                                }).FirstOrDefault();

                WalletTransfer = Transfer?.WalletTransfer ?? 0;
                return WalletTransfer;
            }
        }

        internal decimal SelectFundReverseByUserIDAndWalletID(int From, int UserID, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var FundReverse = (from fund in context.FundTransaction
                                   where fund.DOCInt == From
                                   && fund.ReceiverID == UserID
                                   && fund.Through == FundTransferType.ThroughDirectReverse
                                   && (WalletID != 0 ? fund.WalletID == WalletID : 2 > 1)
                                   group fund by 1 into grp
                                   select new
                                   {
                                       Reverse = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                decimal ReverseBal = FundReverse?.Reverse ?? 0;
                return ReverseBal;
            }
        }

        internal HeaderViewModel GetHeaderSummaryData(List<RequestViewModel> returnList, int UserTypeID, int UserID)
        {
            HeaderViewModel response = new HeaderViewModel();

            var tranferList = (from fund in returnList
                               where (fund.Through == FundTransferType.ThroughRequestTransfer
                                   || fund.Through == FundTransferType.ThroughDirectTransfer)
                                   && (UserTypeID != (int)UserType.Admin ? fund.SenderID == UserID : 2 > 1)
                               group fund by new { fund.Status } into grp
                               select new
                               {
                                   Count = grp.Count(),
                                   Amount = grp.Sum(x => x.Amount),
                               }).FirstOrDefault();

            var PGtranferList = (from fund in returnList
                                 where fund.Through == FundTransferType.PGTransfer
                                 group fund by new { fund.Status } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();
            var ECtranferList = (from fund in returnList
                                 where fund.Through == FundTransferType.ECollection
                                 group fund by new { fund.Status } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

            var receivedList = (from fund in returnList
                                where (fund.Through == FundTransferType.ThroughRequestTransfer
                                  || fund.Through == FundTransferType.ThroughDirectTransfer)
                                  && (UserTypeID != (int)UserType.Admin ? fund.ReceiverID == UserID : 2 > 1)
                                group fund by new { fund.Status } into grp
                                select new
                                {
                                    Count = grp.Count(),
                                    Amount = grp.Sum(x => x.Amount),
                                }).FirstOrDefault();
            var PGreceivedList = (from fund in returnList
                                  where fund.Through == FundTransferType.PGTransfer
                                    && (UserTypeID != (int)UserType.Admin ? fund.ReceiverID == UserID : 2 > 1)
                                  group fund by new { fund.Status } into grp
                                  select new
                                  {
                                      Count = grp.Count(),
                                      Amount = grp.Sum(x => x.Amount),
                                  }).FirstOrDefault();
            var ECreceivedList = (from fund in returnList
                                  where fund.Through == FundTransferType.ECollection
                                    && (UserTypeID != (int)UserType.Admin ? fund.ReceiverID == UserID : 2 > 1)
                                  group fund by new { fund.Status } into grp
                                  select new
                                  {
                                      Count = grp.Count(),
                                      Amount = grp.Sum(x => x.Amount),
                                  }).FirstOrDefault();

            var DeductedList = (from fund in returnList
                                where (fund.Through == FundTransferType.ThroughDirectReverse
                                   || fund.Through == FundTransferType.ThroughReverseRequestTransfer)
                                     && (UserTypeID != (int)UserType.Admin ? fund.ReceiverID == UserID : 2 > 1)
                                group fund by new { fund.Status } into grp
                                select new
                                {
                                    Count = grp.Count(),
                                    Amount = grp.Sum(x => x.Amount),
                                }).FirstOrDefault();

            var reverseList = (from fund in returnList
                               where (fund.Through == FundTransferType.ThroughDirectReverse
                                  || fund.Through == FundTransferType.ThroughReverseRequestTransfer)
                                   && (UserTypeID != (int)UserType.Admin ? fund.SenderID == UserID : 2 > 1)
                               group fund by new { fund.Status } into grp
                               select new
                               {
                                   Count = grp.Count(),
                                   Amount = grp.Sum(x => x.Amount),
                               }).FirstOrDefault();

            response.TransferAmount = tranferList?.Amount ?? 0;
            response.TransferCount = tranferList?.Count ?? 0;

            response.PGTransferAmount = PGtranferList?.Amount ?? 0;
            response.PGTransferCount = PGtranferList?.Count ?? 0;

            response.ECTransferAmount = ECtranferList?.Amount ?? 0;
            response.ECTransferCount = ECtranferList?.Count ?? 0;

            response.ReceiveAmount = receivedList?.Amount ?? 0;
            response.ReceiveCount = receivedList?.Count ?? 0;
            response.PGReceiveAmount = PGreceivedList?.Amount ?? 0;
            response.PGReceiveCount = PGreceivedList?.Count ?? 0;
            response.ECReceiveAmount = ECreceivedList?.Amount ?? 0;
            response.ECReceiveCount = ECreceivedList?.Count ?? 0;

            response.DownlineReverseAmount = DeductedList?.Amount ?? 0;
            response.DownlineReverseCount = DeductedList?.Count ?? 0;

            response.ReverseAmount = reverseList?.Amount ?? 0;
            response.ReverseCount = reverseList?.Count ?? 0;

            return response;
        }

        internal DashboardFundViewModel SelectFundUserwiseHeader(Requests.TransactionReport.UserSaleSelectRequest SelectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(SelectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(SelectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainListTransfer = (from fund in context.FundTransaction
                                        where fund.SenderID == SelectRequest.UserID
                                         && fund.DOCInt >= FromDate
                                         && fund.DOCInt <= ToDate
                                        && (fund.Through == FundTransferType.ThroughRequestTransfer
                                       || fund.Through == FundTransferType.ThroughDirectTransfer)
                                        select new
                                        {
                                            Amount = fund.Amount,
                                            DOCInt = fund.DOCInt
                                        }).ToList();

                var mainListReceived = (from fund in context.FundTransaction
                                        where fund.ReceiverID == SelectRequest.UserID
                                         && fund.DOCInt == FromDate
                                        && (fund.Through == FundTransferType.ThroughRequestTransfer
                                       || fund.Through == FundTransferType.ThroughDirectTransfer)

                                        select new
                                        {
                                            Amount = fund.Amount,
                                            DOCInt = fund.DOCInt
                                        }).ToList();

                var tranferList = (from fund in mainListTransfer

                                   group fund by new { fund.DOCInt } into grp
                                   select new
                                   {
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                var receivedList = (from fund in mainListReceived

                                    group fund by new { fund.DOCInt } into grp
                                    select new
                                    {
                                        Count = grp.Count(),
                                        Amount = grp.Sum(x => x.Amount),
                                    }).FirstOrDefault();

                DashboardFundViewModel response = new DashboardFundViewModel();

                response.TransferAmount = tranferList?.Amount ?? 0;
                response.TransferCount = tranferList?.Count ?? 0;
                response.PurchaseAmount = receivedList?.Amount ?? 0;
                response.PurchaseCount = receivedList?.Count ?? 0;

                return response;
            }
        }
    }
}