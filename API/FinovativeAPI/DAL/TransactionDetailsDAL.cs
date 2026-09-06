using Project.Model;
using Project.Processing;
using Project.Requests.TransactionDetails;
using Project.Response.Fund;
using Project.Response.TransactionDetails;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class TransactionDetailsDAL
    {
        internal List<ViewModel> SelectForFilter(ReportRequest txReportRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            List<ViewModel> list = new List<ViewModel>();

            List<TransactionDetails> transDetailsList = new List<TransactionDetails>();
            List<Archive_TransactionDetails> transDetailsListArchive = new List<Archive_TransactionDetails>();

            List<TransactionMaster> transactionMasterlist = new List<TransactionMaster>();
            List<Archive_TransactionMaster> transactionMasterlistArchive = new List<Archive_TransactionMaster>();
            List<FundTransaction> fundTransactionlist = new List<FundTransaction>();
            //string AccountStatmentWithCashCollection = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AccountStatmentWithCashCollection), "No");


            List<UserMaster> usersList = new UserDAL().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().Select();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    transDetailsListArchive = (from tranDetails in context.Archive_TransactionDetails
                                               where tranDetails.DOCInt >= From
                                               && tranDetails.DOCInt <= To
                                               && (txReportRequest.WalletID == 0 ? 2 > 1 : tranDetails.WalletID == txReportRequest.WalletID)
                                               && (!txReportRequest.Type.EqualsIgnoreCase("All Type") ? tranDetails.DOCType == txReportRequest.Type : 2 > 1)
                                               //&& (AccountStatmentWithCashCollection.EqualsIgnoreCase("Yes") ? 2 > 1 : tranDetails.DOCType != DOCType.CASH_COLLECTION )
                                               && tranDetails.DOCType != DOCType.CASH_COLLECTION
                                                  && (
                                       txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? tranDetails.ConsumerNumber == txReportRequest.CriteriaText :
                                       txReportRequest.Criteria.EqualsIgnoreCase("ReferenceID") ? tranDetails.ReferenceID ==int.Parse(txReportRequest.CriteriaText) : 2 > 1)

                                               orderby tranDetails.TransactionDetailID descending
                                               select tranDetails).ToList();
                    transDetailsList = SelectByArchive(transDetailsListArchive);

                    transactionMasterlistArchive = (from tranMaster in context.Archive_TransactionMaster
                                                    where tranMaster.DOCInt >= From
                                                    && tranMaster.DOCInt <= To
                                                    && (txReportRequest.WalletID == 0 ? 2 > 1 : tranMaster.WalletID == txReportRequest.WalletID)
                                                    orderby tranMaster.TransactionMasterID descending
                                                    select tranMaster).ToList();
                    transactionMasterlist = new TransactionReportDAL().SelectByArchive(transactionMasterlistArchive);
                }
                else
                {
                    transDetailsList = (from tranDetails in context.TransactionDetails
                                        where tranDetails.DOCInt >= From
                                        && tranDetails.DOCInt <= To
                                        && (txReportRequest.WalletID == 0 ? 2 > 1 : tranDetails.WalletID == txReportRequest.WalletID)
                                        && (!txReportRequest.Type.EqualsIgnoreCase("All Type") ? tranDetails.DOCType == txReportRequest.Type : 2 > 1)
                                        //&& (AccountStatmentWithCashCollection.EqualsIgnoreCase("Yes") ? 2 > 1 : tranDetails.DOCType != DOCType.CASH_COLLECTION)
                                        && tranDetails.DOCType != DOCType.CASH_COLLECTION
                                               && (
                                       txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? tranDetails.ConsumerNumber == txReportRequest.CriteriaText :
                                       txReportRequest.Criteria.EqualsIgnoreCase("ReferenceID") ? tranDetails.ReferenceID == int.Parse(txReportRequest.CriteriaText) : 2 > 1)

                                        orderby tranDetails.TransactionDetailID descending
                                        select tranDetails).ToList();

                    transactionMasterlist = (from tranMaster in context.TransactionMaster
                                             where tranMaster.DOCInt >= From
                                             && tranMaster.DOCInt <= To
                                             && (txReportRequest.WalletID == 0 ? 2 > 1 : tranMaster.WalletID == txReportRequest.WalletID)
                                          
                                             orderby tranMaster.TransactionMasterID descending
                                             select tranMaster).ToList();
                }

                fundTransactionlist = (from tranMaster in context.FundTransaction
                                       where tranMaster.DOCInt >= From
                                       && tranMaster.DOCInt <= To
                                       && (txReportRequest.WalletID == 0 ? 2 > 1 : tranMaster.WalletID == txReportRequest.WalletID)
                                       orderby tranMaster.FundTransactionID descending
                                       select tranMaster).ToList();
            }

            list = (from transactionDetails in transDetailsList
                    join userMaster in usersList on transactionDetails.UserID equals userMaster.UserID
                    join assignuser in usersList on transactionDetails.AssignedUserID equals assignuser.UserID
                    join operatorMaster in opList
                    on transactionDetails.OperatorID equals operatorMaster.OperatorID
                    join traMaster in transactionMasterlist on transactionDetails.ReferenceID equals traMaster.TransactionMasterID
                   into traMasterdetails
                    from LtraM in traMasterdetails.DefaultIfEmpty()
                    join fund in fundTransactionlist on transactionDetails.ReferenceID equals fund.FundTransactionID
                    into funddetails
                    from Lfund in funddetails.DefaultIfEmpty()

                    select new ViewModel
                    {
                        TransactionDetailID = transactionDetails.TransactionDetailID,
                        UserID = transactionDetails.UserID,
                        FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName),
                        AliasName = userMaster.AliasName,
                        PersonName = userMaster.PersonName,
                        AssignedUserName = assignuser.FirmName.ConcatNameMobileAliasName(assignuser.MobileNumber, assignuser.AliasName),
                        ReferenceID = transactionDetails.ReferenceID,
                        ReferenceNo = LtraM?.ReferenceNo ?? Lfund?.UTRNumber ?? "",
                        RechargeDate = LtraM?.DOC.ToString() ?? "",
                        OperatorName = operatorMaster.OperatorName,
                        DOCType = transactionDetails.DOCType,
                        ConsumerNumber = transactionDetails.ConsumerNumber,
                        DOC = transactionDetails.DOC.IndiaDateFormat(),

                        OpeningBalance = transactionDetails.OpeningBalance,
                        Amount = transactionDetails.TransactionAmount,
                        DiscountAmount = transactionDetails.DiscountAmount,
                        WalletID = transactionDetails.WalletID,
                        GST = transactionDetails.GST,
                        TDS = transactionDetails.TDS,
                        ClosingBalance = transactionDetails.ClosingBalance,
                        AmountCreditDebit = transactionDetails.AmountCreditDebit,
                        CommissionCreditDebit = transactionDetails.CommissionCreditDebit,
                        TransactionDescription = transactionDetails.TransactionDescription,
                        OriginalDOC = transactionDetails.DOC,
                        Charge = LtraM?.Charge ?? 0,
                        NetCommission = LtraM?.NetCommission ?? 0,

                    }).ToList();
            if (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID"))
            {
                list = list.Where(txn => txn.ReferenceNo == txReportRequest.CriteriaText).ToList();
            }

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (txReportRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == txReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == userID
                            select item).ToList();
                }
            }
            if (Util.isRoleSuperDistOrDist(UserTypeID) || Util.isDFOSOrSDFOS(UserTypeID))
            {
                if (txReportRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == txReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == userID
                            select item).ToList();
                }
            }
            if (Util.isRoleRetailerOrClientOrAPIUser(UserTypeID))
            {
                list = (from item in list
                        where item.UserID == userID
                        select item).ToList();
            }

            return list;
        }

        private List<TransactionDetails> SelectByArchive(List<Archive_TransactionDetails> transDetailsListArchive)
        {
            List<TransactionDetails> tranList = transDetailsListArchive.Select(row => new TransactionDetails
            {
                TransactionDetailID = row.TransactionDetailID,
                UserID = row.UserID,
                ReferenceID = row.ReferenceID,
                AssignedUserID = row.AssignedUserID,
                OperatorID = row.OperatorID,
                ServiceID = row.ServiceID,
                WalletID = row.WalletID,
                DOCType = row.DOCType,
                OpeningBalance = row.OpeningBalance,
                ClosingBalance = row.ClosingBalance,
                TransactionAmount = row.TransactionAmount,
                DiscountAmount = row.DiscountAmount,
                GST = row.GST,
                TDS = row.TDS,
                AmountCreditDebit = row.AmountCreditDebit,
                CommissionCreditDebit = row.CommissionCreditDebit,
                TransactionDescription = row.TransactionDescription,
                ConsumerNumber = row.ConsumerNumber,
                PayMode = row.PayMode,
                DOC = row.DOC,
                UpdatedDOC = row.UpdatedDOC,
                DOCInt = row.DOCInt
            }).ToList();

            return tranList;
        }

        internal List<ViewModel> SelectByCashCallectionFromdateTodate(Requests.CashCollection.ReportRequest txReportRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from transactionDetails in context.TransactionDetails
                        join userMaster in context.UserMaster on
                        transactionDetails.UserID equals userMaster.UserID
                        join Assigneduser in context.UserMaster on
                       transactionDetails.AssignedUserID equals Assigneduser.UserID
                        where transactionDetails.DOCInt >= From
                        && transactionDetails.DOCInt <= To
                       && (transactionDetails.DOCType == DOCType.CASH_COLLECTION ||
                        transactionDetails.DOCType == DOCType.CASH_COLLECTION_REVERSE)

                        orderby transactionDetails.TransactionDetailID descending

                        select new ViewModel
                        {
                            TransactionDetailID = transactionDetails.TransactionDetailID,
                            UserID = transactionDetails.UserID,
                            FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName),
                            DOCType = transactionDetails.DOCType,
                            DOC = transactionDetails.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            OpeningBalance = transactionDetails.OpeningBalance,
                            Amount = transactionDetails.TransactionAmount,
                            ClosingBalance = transactionDetails.ClosingBalance,
                            TransactionDescription = transactionDetails.TransactionDescription,
                            AssignedUserID = transactionDetails.AssignedUserID,
                            AssignedUserName = Assigneduser.FirmName.ConcatNameMobileAliasName(Assigneduser.MobileNumber, Assigneduser.AliasName),
                            AmountCreditDebit = transactionDetails.AmountCreditDebit,
                            PayMode = transactionDetails.PayMode ?? ""
                        }).ToList();
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            int ParentID = new UserDAL().SelectParentID(userID);
            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (txReportRequest.UserID != 0)
                {
                    list = (from item in list
                            where (item.AssignedUserID == txReportRequest.UserID ||
                           item.UserID == txReportRequest.UserID)
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where (item.AssignedUserID == userID ||
                           item.UserID == userID)
                            select item).ToList();
                }
            }
            if (UserTypeID == (int)UserType.SuperDistributer || UserTypeID == (int)UserType.Distributer)
            {
                if (txReportRequest.UserID != 0)
                {
                    if (txReportRequest.UserID == userID)
                    {
                        list = (from item in list
                                where (item.AssignedUserID == userID && item.UserID == ParentID) ||
                                (item.UserID == userID && item.AssignedUserID == ParentID)
                                select item).ToList();
                    }
                    else
                    {
                        list = (from item in list
                                where (item.UserID == txReportRequest.UserID
                                || item.AssignedUserID == txReportRequest.UserID)
                                select item).ToList();
                    }
                }
                else
                {
                    list = (from item in list
                            where (item.AssignedUserID == userID ||
                            item.UserID == userID)
                             && item.AssignedUserID != ParentID
                            select item).ToList();
                }
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                list = (from item in list
                        where (item.UserID == userID ||
                        item.AssignedUserID == userID)
                        select item).ToList();
            }
            if (!txReportRequest.Type.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.TransactionDescription.Contains(txReportRequest.Type)
                        select item).ToList();
            }
            if (!txReportRequest.PayMode.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.PayMode == txReportRequest.PayMode
                        select item).ToList();
            }

            return list;
        }

        internal List<ViewModel> SelectByCashCallection(ReportRequest txReportRequest, int userID)
        {
            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from transactionDetails in context.TransactionDetails
                        join userMaster in context.UserMaster on
                        transactionDetails.UserID equals userMaster.UserID
                        join Assigneduser in context.UserMaster on
                       transactionDetails.AssignedUserID equals Assigneduser.UserID
                        where transactionDetails.DOCType == DOCType.CASH_COLLECTION ||
                         transactionDetails.DOCType == DOCType.CASH_COLLECTION_REVERSE
                        //&& transactionDetails.UserID == txReportRequest.UserID
                        && transactionDetails.AssignedUserID == userID
                        orderby transactionDetails.TransactionDetailID descending

                        select new ViewModel
                        {
                            TransactionDetailID = transactionDetails.TransactionDetailID,
                            UserID = transactionDetails.UserID,
                            FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName),
                            DOCType = transactionDetails.DOCType,
                            DOC = transactionDetails.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            OpeningBalance = transactionDetails.OpeningBalance,
                            Amount = transactionDetails.TransactionAmount,
                            ClosingBalance = transactionDetails.ClosingBalance,
                            TransactionDescription = transactionDetails.TransactionDescription,
                            AssignedUserID = transactionDetails.AssignedUserID,
                            AssignedUserName = Assigneduser.FirmName.ConcatNameMobileAliasName(Assigneduser.MobileNumber, Assigneduser.AliasName),
                            PayMode = transactionDetails.PayMode
                        }).ToList();

                if (txReportRequest.UserID != Company.AdminUserID)
                {
                    list = (from item in list
                            where item.UserID == txReportRequest.UserID
                            select item).ToList();
                }
            }

            return list;
        }

        internal int Insert(TransactionDetails details)
        {
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.TransactionDetails.Add(details);
                context.SaveChanges();
                Result = details.TransactionDetailID;
            }
            return Result;
        }

        internal TransactionDetails SelectTransactionForDuplicateCheckCore(TransactionDetails obj)
        {
            //check duplicate in txn details userid, operatorid, walletid, TransactionAmount, DOCType, ReferenceID in last
            DateTime currentDateTime = DateTime.Now.AddMinutes(-1);
            using var context = new DatabaseContext();
            return context.TransactionDetails.FirstOrDefault(f => f.UserID == obj.UserID && f.OperatorID == obj.OperatorID &&
                           f.DOCType == obj.DOCType && f.ReferenceID == obj.ReferenceID &&
                           f.WalletID == obj.WalletID && f.TransactionAmount == obj.TransactionAmount && f.DOC > currentDateTime);
        }

        internal ClosingBalanceView IsAmountValid(int UserID, int WalletID)
        {
            ClosingBalanceView clbal = new ClosingBalanceView();
            TransactionDetails ObjModel = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                ObjModel = context.TransactionDetails.OrderByDescending(t => t.DOC)
                    .FirstOrDefault(t => t.UserID == UserID && t.WalletID == WalletID && 
                                    t.DOCType != DOCType.CASH_COLLECTION && 
                                    t.DOCType != DOCType.CASH_COLLECTION_REVERSE);
            }

            if (ObjModel == null)
                return clbal;

            clbal.ClosingBalance = ObjModel.ClosingBalance;
            return clbal;
        }

        internal async Task<List<AdminAccountingViewModel>> SelectAdminAccounting(AdminAccountingRequest reportRequest)
        {
            int currentDateInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            int DayaddTo = Convert.ToInt32((Convert.ToDateTime(reportRequest.FromDate).AddDays(1)).ToString(Messages.DOCIntegerFomrat));

            int From = Convert.ToInt32(Convert.ToDateTime(reportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            List<AdminAccountingViewModel> listresponse = new List<AdminAccountingViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var transactionMaster = (from tran in context.TransactionMaster
                                         where tran.DOCInt == From
                                          && (tran.ServiceID == (int)ServiceCode.AEPS
                                          || tran.ServiceID == (int)ServiceCode.MATM)
                                          && tran.Status == StatusType.Success
                                         select new
                                         {
                                             DOCInt = tran.DOCInt,
                                             DebitAmount = tran.DebitAmount,
                                             DistributerCommission = tran.DistributerCommission,
                                             SuperDistributerCommission = tran.SuperDistributerCommission
                                         }).ToList();

                var AEPES = (from txn in transactionMaster
                             group txn by new { txn.DOCInt } into grp
                             select new
                             {
                                 DebitAmount = grp.Sum(X => X.DebitAmount),
                                 DistributerCommission = grp.Where(x => x.DistributerCommission >= 0).Sum(X => X.DistributerCommission),
                                 SuperDistributerCommission = grp.Where(x => x.DistributerCommission >= 0).Sum(X => X.SuperDistributerCommission),
                             }).FirstOrDefault();

                var transactionDetails = (from details in context.TransactionDetails
                                          where details.DOCInt == From
                                           && details.UserID == Company.AdminUserID
                                           && (reportRequest.WalletID == 0 ? 2 > 1 : details.WalletID == reportRequest.WalletID)
                                          select new
                                          {
                                              DOCInt = details.DOCInt,
                                              DiscountAmount = details.DiscountAmount,
                                              CommissionCreditDebit = details.CommissionCreditDebit,
                                              WalletID = details.WalletID,
                                          }).ToList();

                var Commission = (from txn in transactionDetails
                                  group txn by new { txn.DOCInt } into grp
                                  select new
                                  {
                                      CommissionCredit = grp.Where(x => x.CommissionCreditDebit == CommissionCreditDebitType.Credit).Sum(X => X.DiscountAmount),
                                      CommissionDebit = grp.Where(x => x.CommissionCreditDebit == CommissionCreditDebitType.Debit).Sum(X => X.DiscountAmount),
                                  }).FirstOrDefault();

                FundTransferViewModel fund = null;
                decimal ReverseBal = 0;
                decimal virtualBal = 0;
                decimal opBal = 0;
                fund = new FundTransactionDal().SelectFundTransferByUserIDAndWalletID(From, Company.AdminUserID, reportRequest.WalletID);
                ReverseBal = new FundTransactionDal().SelectFundReverseByUserIDAndWalletID(From, Company.AdminUserID, reportRequest.WalletID);
                virtualBal = new VirtualBalanceDal().SelectVirtualBalanceByWalletID(From, From, reportRequest.WalletID);
                opBal = new UserBalanceHistoryDAL().SelectOpeningBalanceByWalletID(Company.AdminUserID, From, reportRequest.WalletID);

                decimal ClosingBalance = 0;
                if (currentDateInt == From)
                {
                    if (reportRequest.WalletID != 0)
                    {
                        ClosingBalance = new UserWalletMasterDal().SelectSumByUserIDAndWalletID(Company.AdminUserID, reportRequest.WalletID);
                    }
                    else
                    {
                        ClosingBalance = new UserWalletMasterDal().SelectSumByUserID(Company.AdminUserID);
                    }
                }
                else
                {
                    if (reportRequest.WalletID != 0)
                    {
                        ClosingBalance = new UserBalanceHistoryDAL().SelectClosingBalanceByWalletID(Company.AdminUserID, DayaddTo, reportRequest.WalletID);
                    }
                    else
                    {
                        ClosingBalance = new UserBalanceHistoryDAL().SelectAllClosingBalance(Company.AdminUserID, DayaddTo);
                    }
                }
                AdminAccountingViewModel response = new AdminAccountingViewModel();
                response.OpeningBalance = opBal;
                response.Purchase = virtualBal;
                response.Transfer = fund?.Transfer ?? 0;
                response.PGTransfer = fund?.PGTransfer ?? 0;
                response.AEPSTransfer = AEPES?.DebitAmount ?? 0;
                response.Reverse = ReverseBal;
                response.CommissionCredit = Commission?.CommissionCredit ?? 0;
                response.CommissionDebit = Commission?.CommissionDebit ?? 0;
                decimal TotalDebitAmount = 0;
                decimal TotalCreditAmount = (response.OpeningBalance + response.Purchase + response.Reverse + response.CommissionCredit);
                if (reportRequest.WalletID == (int)WalletCode.AEPS || reportRequest.WalletID == 0)
                {
                    TotalDebitAmount = (response.Transfer + response.PGTransfer + response.CommissionDebit + response.AEPSTransfer);
                }
                else
                {
                    TotalDebitAmount = (response.Transfer + response.PGTransfer + response.CommissionDebit);
                }
                response.ClosingBalance = ClosingBalance;
                response.ExpectedClosingBalance = TotalCreditAmount - TotalDebitAmount;
                response.Difference = response.ClosingBalance - response.ExpectedClosingBalance;

                listresponse.Add(response);
            }
            return listresponse;
        }

        internal void Delete(int DeleteDOCInt, int fromDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.TransactionDetails.Where(U => U.DOCInt >= fromDOCInt && U.DOCInt <= DeleteDOCInt);
                context.TransactionDetails.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}