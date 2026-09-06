using Project.Model;
using Project.Processing;
using Project.Requests.TransactionReport;
using Project.Response.Fund;
using Project.Response.TransactionReport;
using Project.Response.UserAccounting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserAccountingDAL
    {
        internal List<UserwiseAccountingViewModel> UserwiseAccounting(UserwiseAccountingRequest selectRequest)
        {
            int Date = Convert.ToInt32(Convert.ToDateTime(selectRequest.Date).ToString(Messages.DOCIntegerFomrat));
            int AddDay = Convert.ToInt32(Convert.ToDateTime(selectRequest.Date).AddDays(1).ToString(Messages.DOCIntegerFomrat));
            int PastAddDay = Convert.ToInt32(Convert.ToDateTime(selectRequest.Date).AddDays(-365).ToString(Messages.DOCIntegerFomrat));

            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            List<UserMaster> usersList = new UserDAL().Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            if (!selectRequest.IsExcelDownload)
            {
                usersList = (from user in usersList
                             join role in listUserType on user.UserTypeID equals role.UserTypeID
                             where (user.UserTypeID == (int)UserType.Retailer ||
                                user.UserTypeID == (int)UserType.APIUser ||
                                user.UserTypeID == (int)UserType.Client)
                             && (selectRequest.UserID == 0 ? 2 > 1 : user.UserID == selectRequest.UserID)
                             orderby user.UserID ascending
                             select user).Skip(selectRequest.SkipCount).Take(selectRequest.TakeCount).ToList();
            }
            else
            {
                usersList = (from user in usersList
                             select user).ToList();
            }

            var FundSenderReceiverList = (from fund in context.FundTransaction
                                          where fund.DOCInt == Date &&
                                                fund.WalletID == selectRequest.WalletID
                                          select fund).ToList();

            List<FundReceiverViewModel> mainFundReceiverList = (from fund in FundSenderReceiverList
                                                                join user in usersList on fund.ReceiverID equals user.UserID
                                                                select new FundReceiverViewModel
                                                                {
                                                                    DOCInt = fund.DOCInt,
                                                                    Amount = fund.Amount,
                                                                    SenderID = fund.SenderID,
                                                                    WalletID = fund.WalletID,
                                                                    ReceiverID = fund.ReceiverID,
                                                                    ParentID = user.ParentID,
                                                                    UserTypeID = user.UserTypeID,
                                                                    Through = fund.Through,
                                                                }).ToList();

            var ReceiverFundList = (from fund in mainFundReceiverList
                                    group fund by new { fund.ReceiverID } into grp
                                    select new
                                    {
                                        ReceiverID = grp.Key.ReceiverID,
                                        ReceiverAmount = grp.Where(q => q.Through == FundTransferType.ThroughDirectTransfer
                                              || q.Through == FundTransferType.ThroughRequestTransfer
                                              || q.Through == FundTransferType.PGTransfer).Sum(X => X?.Amount ?? 0),
                                    }).ToList();

            List<FundReceiverViewModel> mainFundReverseList = (from fund in FundSenderReceiverList
                                                               join user in usersList on fund.SenderID equals user.UserID
                                                               select new FundReceiverViewModel
                                                               {
                                                                   DOCInt = fund.DOCInt,
                                                                   Amount = fund.Amount,
                                                                   SenderID = fund.SenderID,
                                                                   WalletID = fund.WalletID,
                                                                   ReceiverID = fund.ReceiverID,
                                                                   ParentID = user.ParentID,
                                                                   UserTypeID = user.UserTypeID,
                                                                   Through = fund.Through,
                                                               }).ToList();

            var ReverseFundList = (from fund in mainFundReceiverList
                                   group fund by new { fund.SenderID } into grp
                                   select new
                                   {
                                       SenderID = grp.Key.SenderID,
                                       ReverseAmount = grp.Where(q => q.Through == FundTransferType.ThroughDirectReverse
                                     || q.Through == FundTransferType.ThroughReverseRequestTransfer).Sum(X => X.Amount),
                                   }).ToList();

            var TransList = (from txn in context.TransactionMaster
                             where txn.DOCInt == Date &&
                             txn.WalletID == selectRequest.WalletID
                             select txn).ToList();

            var AllmainList = (from txn in TransList
                               join user in usersList on txn.UserID equals user.UserID
                               where txn.WalletID == selectRequest.WalletID
                               select new
                               {
                                   WalletID = txn.WalletID,
                                   UserID = txn.UserID,
                                   UserTypeID = user.UserTypeID,
                                   Status = txn.Status,
                                   Amount = txn.Amount,
                                   RetailerCommission = txn.RetailerCommission,
                               }).ToList();

            var mainList = (from txn in AllmainList
                            where txn.Status == StatusType.Success
                            select txn).ToList();

            var Grouplist = (from txn in mainList
                             group txn by new { txn.WalletID, txn.UserID } into grp
                             select new
                             {
                                 WalletID = grp.Key.WalletID,
                                 UserID = grp.Key.UserID,
                                 Sale = grp.Sum(X => X.Amount),
                                 RetailerCommission = grp.Where(q => q.RetailerCommission > 0).Sum(X => X.RetailerCommission),
                                 Surcharge = grp.Where(q => q.RetailerCommission < 0).Sum(X => X.RetailerCommission),
                             }).ToList();

            var OldTransFailList = (from txn in context.TransactionMaster
                                    where txn.DOCInt != Date
                                    && txn.DOCInt >= PastAddDay
                                    && txn.DOCInt < Date
                                    && txn.Status == StatusType.Failure
                                    && txn.WalletID == selectRequest.WalletID
                                    select txn).ToList();

            var OldData = (from txn in OldTransFailList
                           join user in usersList on txn.UserID equals user.UserID

                           select new
                           {
                               Status = txn.Status,
                               WalletID = txn.WalletID,
                               UserID = txn.UserID,
                               Amount = txn.Amount,
                               DebitAmount = txn.DebitAmount,
                               ConsumerNumber = txn.ConsumerNumber,
                               DOCInt = txn.DOCInt,
                               UpdatedDOCInt = Convert.ToInt32(Convert.ToDateTime(txn.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                           }).ToList();

            var GroupOldData = (from txn in OldData
                                where txn.UpdatedDOCInt == Date
                                group txn by new { txn.WalletID, txn.UserID } into grp
                                select new
                                {
                                    WalletID = grp.Key.WalletID,
                                    UserID = grp.Key.UserID,
                                    OldFailAmount = grp.Sum(X => X.Amount),
                                    OldFailDebitAmount = grp.Sum((X) => X.DebitAmount),
                                }).ToList();

            var OldTransFailListThatDate = (from txn in context.TransactionMaster
                                            where txn.DOCInt >= Date &&
                                            txn.DOCInt <= Date + 4 &&
                                            txn.Status == StatusType.Failure &&
                                            txn.WalletID == selectRequest.WalletID
                                            select txn).ToList();

            var OldDataThatDate = (from txn in OldTransFailListThatDate
                                   join user in usersList on txn.UserID equals user.UserID
                                   select new
                                   {
                                       Status = txn.Status,
                                       WalletID = txn.WalletID,
                                       UserID = txn.UserID,
                                       Amount = txn.Amount,
                                       DebitAmount = txn.DebitAmount,
                                       ConsumerNumber = txn.ConsumerNumber,
                                       DOCInt = txn.DOCInt,
                                       UpdatedDOCInt = Convert.ToInt32(Convert.ToDateTime(txn.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                                   }).ToList();

            var GroupOldDataThatDate = (from txn in OldDataThatDate
                                        where txn.DOCInt == Date && txn.UpdatedDOCInt != Date
                                        group txn by new { txn.WalletID, txn.UserID } into grp
                                        select new
                                        {
                                            WalletID = grp.Key.WalletID,
                                            UserID = grp.Key.UserID,
                                            OldFailAmount = grp.Sum(X => X.Amount),
                                            OldFailDebitAmount = grp.Sum((X) => X.DebitAmount),
                                        }).ToList();
            var ManualUpdate = (from manualupdate in context.TransactionManualUpdate
                                where manualupdate.DOCInt == Date && manualupdate.TransactionMaster.WalletID == selectRequest.WalletID
                                select new
                                {
                                    TransactionID = manualupdate.TransactionMasterID,
                                    UserID = manualupdate.TransactionMaster.UserID,
                                    WalletID = manualupdate.TransactionMaster.WalletID,
                                }).ToList();

            var ManualUpdateCountData = ManualUpdate
                .GroupBy(mu => mu.UserID)
                .Select(group => new
                {
                    UserID = group.Key,
                    TransactionCount = group.Count(),
                }).ToList();


            List<WalletMaster> walletslist = new WalletMasterDal().Select();
            List<UserwiseAccountingViewModel> response = (
                from user in usersList
                join wallet in walletslist on selectRequest.WalletID equals wallet.WalletID
                join role in listUserType on user.UserTypeID equals role.UserTypeID
                where user.UserTypeID == (int)UserType.Retailer ||
                user.UserTypeID == (int)UserType.APIUser ||
                user.UserTypeID == (int)UserType.Client
                let openingBal = new UserBalanceHistoryDAL().SelectOpeningBalance(user.UserID, wallet.WalletID, Date)
                let closingBalances = new UserWalletMasterDal().SelectClosingBalance(user.UserID, selectRequest.WalletID, Date, AddDay)
                let sale = Grouplist.FirstOrDefault(txn => txn.UserID == user.UserID)?.Sale ?? 0
                let retailerCommission = Grouplist.FirstOrDefault(txn => txn.UserID == user.UserID)?.RetailerCommission ?? 0
                let surcharge = Math.Abs(Grouplist.FirstOrDefault(txn => txn.UserID == user.UserID)?.Surcharge ?? 0)
                let ReverseAmount = ReverseFundList.FirstOrDefault(r => r.SenderID == user.UserID)?.ReverseAmount ?? 0
                let purchase = ReceiverFundList.FirstOrDefault(r => r.ReceiverID == user.UserID)?.ReceiverAmount ?? 0
                let TotalOldFailDebitAmount = GroupOldData.FirstOrDefault(c => c.UserID == user.UserID)?.OldFailDebitAmount ?? 0
                let TotalOldAmountAdjustment = GroupOldData.FirstOrDefault(c => c.UserID == user.UserID)?.OldFailAmount ?? 0
                let TotalOldAmountAdjustmentExceptSelectedDate = GroupOldDataThatDate.FirstOrDefault(c => c.UserID == user.UserID)?.OldFailAmount ?? 0
                let TotalDebitAmountAdjustmentExceptSelectedDate = GroupOldDataThatDate.FirstOrDefault(c => c.UserID == user.UserID)?.OldFailDebitAmount ?? 0
                let ManualUpdateCount = ManualUpdateCountData.FirstOrDefault(c => c.UserID == user.UserID)?.TransactionCount ?? 0
                // let totalManualToday = manualpass - manualfailed
                let CommsionAmount = retailerCommission - surcharge
                let TotalCrAmt = openingBal + purchase + TotalOldFailDebitAmount - TotalDebitAmountAdjustmentExceptSelectedDate
                let TotalDrAmt = (sale - retailerCommission - ReverseAmount) + surcharge
                let ExpClosingBal = openingBal + purchase + TotalOldFailDebitAmount - TotalDebitAmountAdjustmentExceptSelectedDate - (sale - retailerCommission - ReverseAmount) + surcharge
                let Difference = (openingBal + purchase + TotalOldFailDebitAmount - TotalDebitAmountAdjustmentExceptSelectedDate - (sale - retailerCommission - ReverseAmount) + surcharge) - closingBalances
                select new UserwiseAccountingViewModel
                {
                    FirmName = user.FirmName.ConcatNameAliasName(user.AliasName),
                    UserMobile = user.MobileNumber,
                    WalletName = wallet.Name,
                    OpeningBal = openingBal,
                    ClosingBalance = closingBalances,
                    Sale = sale,
                    Role = role.UserType,
                    Surcharge = surcharge,
                    Purchase = purchase,
                    TotalOldAmountAdjustment = TotalOldFailDebitAmount,
                    TotalNewAmountAdjustment = TotalDebitAmountAdjustmentExceptSelectedDate,
                    UserID = user.UserID,
                    CommsionAmount = CommsionAmount,
                    TotalCrAmt = TotalCrAmt,
                    TotalDrAmt = TotalDrAmt,
                    ExpClosingBal = ExpClosingBal,
                    Difference = Difference,
                    ManualUpdateCount = ManualUpdateCount
                }).ToList();

            return response;

        }

        internal List<OldAmountAdjustmentViewModel> SelectForFilterByOldAmountAdjustment(UserWiseAccountingCompRequest mainRequest, int UserID)
        {
            int Date = Convert.ToInt32(Convert.ToDateTime(mainRequest.Date).ToString(Messages.DOCIntegerFomrat));
            int AddDay = Convert.ToInt32(Convert.ToDateTime(mainRequest.Date).AddDays(1).ToString(Messages.DOCIntegerFomrat));
            int PastAddDay = Convert.ToInt32(Convert.ToDateTime(mainRequest.Date).AddDays(-30).ToString(Messages.DOCIntegerFomrat));


            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            List<UserMaster> usersList = new UserDAL().Select();
            List<OldAmountAdjustmentViewModel> response = new List<OldAmountAdjustmentViewModel>();
            var oldTransFailList = context.TransactionMaster
                                  .Where(txn => txn.DOCInt != Date &&
                                                 txn.DOCInt >= PastAddDay &&
                                                 txn.DOCInt < Date &&
                                                 txn.Status == StatusType.Failure)
                                  .ToList();

            var OldDataThatDate = (from txn in oldTransFailList
                                   join user in usersList on txn.UserID equals user.UserID
                                   join provider in context.ProviderMaster on
                                   txn.ProviderID equals provider.ProviderID into g
                                   from provider in g.DefaultIfEmpty()
                                   join Opreator in context.OperatorMaster on
                                   txn.OperatorID equals Opreator.OperatorID into op
                                   from opreator in op.DefaultIfEmpty()
                                   let updatedDocInt = Convert.ToInt32(Convert.ToDateTime(txn.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                                   where updatedDocInt == Date && txn.WalletID == mainRequest.WalletID && (mainRequest.UserID == 0 ? 2 > 1 : txn.UserID == mainRequest.UserID)
                                   select new OldAmountAdjustmentViewModel
                                   {
                                       FirmName = user.FirmName.ConcatNameAliasName(user.AliasName),
                                       DOC = txn.DOC.LongFormatDate(),
                                       UpdatedDOC = txn.UpdatedDOC.LongFormatDate(),
                                       ProviderName = provider.ProviderName ?? "",
                                       Operator = opreator.OperatorName ?? "",
                                       ConsumerNumber = txn.ConsumerNumber,
                                       Amount = txn.Amount,
                                       Commission = txn.RetailerCommission,
                                       DebitAmount = txn.DebitAmount,
                                       Status = txn.Status,
                                       Description = txn.Description
                                   }).ToList();



            response.AddRange(OldDataThatDate);
            return response;
        }
        internal List<NewAmountAdjustmentViewModel> SelectForFilterByNewAmountAdjustment(UserWiseAccountingCompRequest mainRequest, int UserID)
        {
            int Date = Convert.ToInt32(Convert.ToDateTime(mainRequest.Date).ToString(Messages.DOCIntegerFomrat));
            int AddDay = Convert.ToInt32(Convert.ToDateTime(mainRequest.Date).AddDays(1).ToString(Messages.DOCIntegerFomrat));
            int PastAddDay = Convert.ToInt32(Convert.ToDateTime(mainRequest.Date).AddDays(-10).ToString(Messages.DOCIntegerFomrat));


            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            List<UserMaster> usersList = new UserDAL().Select();
            List<NewAmountAdjustmentViewModel> response = new List<NewAmountAdjustmentViewModel>();
            var oldTransFailList = context.TransactionMaster
                                  .Where(txn => txn.DOCInt >= Date &&
                                            txn.DOCInt <= Date + 4 &&
                                            txn.Status == StatusType.Failure)
                                  .ToList();
            var OldDatasFailListThatDate = (from txn in oldTransFailList
                                            join user in usersList on txn.UserID equals user.UserID
                                            join provider in context.ProviderMaster on
                                            txn.ProviderID equals provider.ProviderID into g
                                            from provider in g.DefaultIfEmpty()
                                            join Opreator in context.OperatorMaster on
                                            txn.OperatorID equals Opreator.OperatorID into op
                                            from opreator in op.DefaultIfEmpty()
                                            let updatedDocInt = Convert.ToInt32(Convert.ToDateTime(txn.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                                            where updatedDocInt != Date && txn.DOCInt == Date && txn.WalletID == mainRequest.WalletID && (mainRequest.UserID == 0 ? 2 > 1 : txn.UserID == mainRequest.UserID)
                                            select new NewAmountAdjustmentViewModel
                                            {
                                                FirmName = user.FirmName.ConcatNameAliasName(user.AliasName),
                                                DOC = txn.DOC.LongFormatDate(),
                                                UpdatedDOC = txn.UpdatedDOC.LongFormatDate(),
                                                ProviderName = provider.ProviderName,
                                                Operator = opreator.OperatorName,
                                                ConsumerNumber = txn.ConsumerNumber,
                                                Amount = txn.Amount,
                                                Commission = txn.RetailerCommission,
                                                DebitAmount = txn.DebitAmount,
                                                Status = txn.Status,
                                                Description = txn.Description
                                            }).ToList();

            response.AddRange(OldDatasFailListThatDate);

            return response;
        }
        internal List<ManualSuccessFailViewModel> SelectForFilterByManualUpdate(UserWiseAccountingCompRequest mainRequest)
        {
            int Date = Convert.ToInt32(Convert.ToDateTime(mainRequest.Date).ToString(Messages.DOCIntegerFomrat));
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            List<UserMaster> usersList = new UserDAL().Select();
            List<ManualSuccessFailViewModel> response = new List<ManualSuccessFailViewModel>();
            response = (from manualupdate in context.TransactionManualUpdate
                        where manualupdate.DOCInt == Date
                        && manualupdate.TransactionMaster.WalletID == mainRequest.WalletID
                        && (mainRequest.UserID == 0 ? 2 > 1 : manualupdate.TransactionMaster.UserID == mainRequest.UserID)

                        select new ManualSuccessFailViewModel
                        {
                            TransactionMasterID = manualupdate.TransactionMasterID,

                            FirmName = manualupdate.UserMaster.FirmName.ConcatNameMobileAliasName(manualupdate.UserMaster.MobileNumber, manualupdate.UserMaster.AliasName),
                            ConsumerNumber = manualupdate.TransactionMaster.ConsumerNumber,
                            OperatorName = manualupdate.TransactionMaster.OperatorMaster.OperatorName ?? "",

                            Amount = manualupdate.TransactionMaster.Amount,
                            OldStatus = manualupdate.OldStatus,
                            NewStatus = manualupdate.NewStatus,
                            ReferenceNo = manualupdate.ProviderrefNo,
                            DOC = manualupdate.DOC.LongFormatDate(),
                            UpdatedDOC = manualupdate.DOC.LongFormatDate(),
                        }).ToList();

            return response;
        }
    }
}

