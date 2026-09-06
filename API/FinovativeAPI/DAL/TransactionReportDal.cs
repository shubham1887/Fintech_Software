using FinovativeAPI.Response.Dashboard;
using FinovativeAPI.StaticModel;
using Microsoft.EntityFrameworkCore;
using Projecdt.Response.TransactionReport;
using Project.BAL;
using Project.Model;
using Project.Processing;
using Project.Requests.DayBookReport;
using Project.Requests.Tax;
using Project.Requests.Transaction;
using Project.Requests.TransactionReport;
using Project.Response.DayBookReport;
using Project.Response.Fund;
using Project.Response.Redeemcommission;
using Project.Response.Tax;
using Project.Response.TransactionMaster;
using Project.Response.TransactionReport;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class TransactionReportDAL
    {
        public TransactionReportDAL()
        {
        }

        internal List<AEPSViewModel> SelectForMicroATMReport(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<AEPSViewModel> list = new List<AEPSViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.MATM).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.MATM
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.MATM
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new AEPSViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.DOCFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Remark = t.Description,
                        RetailerComm = t.RetailerCommission,
                        DistributerComm = t.DistributerCommission,
                        SuperDistributerComm = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        RequestID = t.ProviderRequestID,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        UserID = t.UserID,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                        CustomerNumber = t.Bill_CustomerMessageMobileNo,
                        NetCommission = t.NetCommission
                    }).ToList();

            return list;
        }

        internal Response.TransactionMaster.HeaderViewModel CMSHeaderSummary(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.CMS
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    list = SelectByArchive(tranListArchive);
                }
                else
                {
                    list = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.CMS
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                            && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            orderby Tran.TransactionMasterID descending
                            select Tran).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal int SelectForCMSCount(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.CMS
                                                && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                                && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       select Tran).ToList();
                    return tranListArchive.Count();
                }
                else
                {
                    tranList = (from Tran in context.TransactionMaster
                                where Tran.DOCInt >= FromDate
                                && Tran.DOCInt <= ToDate
                                && Tran.ServiceID == (int)ServiceCode.CMS
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                select Tran).ToList();
                    return tranList.Count();
                }
            }
        }

        internal List<AEPSViewModel> SelectForCMSReport(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<AEPSViewModel> list = new List<AEPSViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.CMS).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.CMS
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.CMS
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new AEPSViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Remark = t.Description,
                        RetailerComm = t.RetailerCommission,
                        DistributerComm = t.DistributerCommission,
                        SuperDistributerComm = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        RequestID = t.ProviderRequestID,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        UserID = t.UserID,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                    }).ToList();

            return list;
        }

        internal List<AEPSViewModel> SelectForCMSReportExcelDownload(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<AEPSViewModel> list = new List<AEPSViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.CMS).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.CMS
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.CMS
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).ToList();
                }
            }
            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new AEPSViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Remark = t.Description,
                        RetailerComm = t.RetailerCommission,
                        DistributerComm = t.DistributerCommission,
                        SuperDistributerComm = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = (t.Status == StatusType.Failure) ? t.Description : t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        RequestID = t.ProviderRequestID,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                    }).ToList();

            return list;
        }

        internal List<AEPSViewModel> SelectForPOSReport(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<AEPSViewModel> list = new List<AEPSViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.POS).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.POS
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.POS
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }

                list = (from t in transactionMaster
                        join User in usersList on t.UserID equals User.UserID
                        join provider in providerList on t.ProviderID equals provider.ProviderID
                        join op in operatorList on t.OperatorID equals op.OperatorID
                        select new AEPSViewModel
                        {
                            FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                            DOC = t.DOC.LongFormatDate(),
                            UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                            DistributerUserID = t.DistributerUserID,
                            SuperDistributerUserID = t.SuperDistributerUserID,
                            OperatorName = op.OperatorName,
                            OpeningBalance = t.OpeningBalance,
                            Amount = t.Amount,
                            DebitAmount = t.DebitAmount,
                            ClosingBalance = t.ClosingBalance,
                            ProviderName = provider.ProviderName,
                            Status = t.Status,
                            Remark = t.Description,
                            RetailerComm = t.RetailerCommission,
                            DistributerComm = t.DistributerCommission,
                            SuperDistributerComm = t.SuperDistributerCommission,
                            IPAddress = t.IPAddress,
                            ComplaintStatus = t.ComplaintStatus,
                            Through = t.Through,
                            TransactionMasterID = t.TransactionMasterID,
                            ReferenceNo = t.ReferenceNo,
                            ConsumerNumber = t.ConsumerNumber,
                            RequestID = t.ProviderRequestID,
                            Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                            UserID = t.UserID,
                            GST = t.RetailerGST,
                            TDS = t.RetailerTDS,
                            DistributerGST = t.DistributerGST,
                            DistributerTDS = t.DistributerTDS,
                            SuperDistributerGST = t.SuperDistributerGST,
                            SuperDistributerTDS = t.SuperDistributerTDS,
                        }).ToList();
            }

            return list;
        }

        internal int SelectForMicroATMCount(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.MATM
                                                && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                                && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       select Tran).ToList();
                    return tranListArchive.Count();
                }
                else
                {
                    tranList = (from Tran in context.TransactionMaster
                                where Tran.DOCInt >= FromDate
                                && Tran.DOCInt <= ToDate
                                && Tran.ServiceID == (int)ServiceCode.MATM
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                select Tran).ToList();
                    return tranList.Count();
                }
            }
        }

        internal int SelectForPOSCount(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                   && Tran.DOCInt <= ToDate
                                   && Tran.ServiceID == (int)ServiceCode.POS
                                            && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                            GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       select Tran).ToList();
                    return tranListArchive.Count();
                }
                else
                {
                    tranList = (from Tran in context.TransactionMaster
                                where Tran.DOCInt >= FromDate
                                && Tran.DOCInt <= ToDate
                                && Tran.ServiceID == (int)ServiceCode.POS
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                select Tran).ToList();
                    return tranList.Count();
                }
            }
        }

        internal Response.TransactionMaster.HeaderViewModel CashDepositHeaderSummary(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.CashDeposit
                        && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal Response.TransactionMaster.HeaderViewModel WalleTopUpHeaderSummary(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.WalletTopUp
                        && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForPanCardExcelDownload(PanCardReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.PanCard).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.PanCard
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }
            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = (t.Status == StatusType.Failure) ? t.Description : t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        ProviderRequestID = t.ProviderRequestID,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                    }).ToList();

            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForPanCardReport(PanCardReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.PanCard).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.PanCard
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        ProviderRequestID = t.ProviderRequestID,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                    }).ToList();

            return list;
        }

        internal int SelectForPanCardCount(PanCardReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.PanCard
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
            }

            return tranList.Count();
        }

        internal Response.TransactionMaster.HeaderViewModel PanCardHeaderSummary(PanCardReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.PanCard
                        && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal int SelectForWalleTopUpCount(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.WalletTopUp
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
            }

            return tranList.Count();
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForWalleTopUpReport(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.WalletTopUp).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.WalletTopUp
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        ProviderRequestID = t.ProviderRequestID,
                        BeneName = t.DMT_BeneficaryNameByBank.EqualsIgnoreCase("") ? t.DMT_BeneficiaryName : t.DMT_BeneficaryNameByBank
                    }).ToList();

            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForWalleTopUpExcelDownload(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.WalletTopUp).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.WalletTopUp
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }
            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = (t.Status == StatusType.Failure) ? t.Description : t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        ProviderRequestID = t.ProviderRequestID,
                        BeneName = t.DMT_BeneficaryNameByBank.EqualsIgnoreCase("") ? t.DMT_BeneficiaryName : t.DMT_BeneficaryNameByBank
                    }).ToList();

            return list;
        }

        internal int SelectForCashDepositCount(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.CashDeposit
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
            }

            return tranList.Count();
        }

        internal List<CashDepositViewModel> SelectForCashDepositReport(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.CashDeposit).ToList();

            List<CashDepositViewModel> list = new List<CashDepositViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.CashDeposit
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new CashDepositViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Remark = t.Description,
                        RetailerComm = t.RetailerCommission,
                        DistributerComm = t.DistributerCommission,
                        SuperDistributerComm = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        BeneficaryName = t.DMT_BeneficaryNameByBank,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        RequestID = t.ProviderRequestID
                    }).ToList();
            return list;
        }

        internal List<CashDepositViewModel> SelectForCashDepositReportExcelDownload(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<CashDepositViewModel> list = new List<CashDepositViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.CashDeposit).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.CashDeposit
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }
            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new CashDepositViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Remark = t.Description,
                        RetailerComm = t.RetailerCommission,
                        DistributerComm = t.DistributerCommission,
                        SuperDistributerComm = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = (t.Status == StatusType.Failure) ? t.Description : t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        RequestID = t.ProviderRequestID
                    }).ToList();

            return list;
        }

        internal int SelectForPayoutCount(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.Payout
                                                && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                                && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       select Tran).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from Tran in context.TransactionMaster
                                where Tran.DOCInt >= FromDate
                                && Tran.DOCInt <= ToDate
                                && Tran.ServiceID == (int)ServiceCode.Payout
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                select Tran).ToList();
                }

                if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    tranList = (from item in tranList
                                where fAmount <= item.Amount && item.Amount <= tAmount
                                select item).ToList();
                }
            }

            return tranList.Count();
        }

        internal int SelectForVendorPayCount(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.VendorPay
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();

                if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    tranList = (from item in tranList
                                where fAmount <= item.Amount && item.Amount <= tAmount
                                select item).ToList();
                }
            }

            return tranList.Count();
        }

        internal Response.TransactionMaster.HeaderViewModel PayoutHeaderSummary(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.Payout
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    list = SelectByArchive(tranListArchive);
                }
                else
                {
                    list = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.Payout
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                            && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            orderby Tran.TransactionMasterID descending
                            select Tran).ToList();
                }
            }
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal Response.TransactionMaster.HeaderViewModel VendorPayHeaderSummary(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.VendorPay
                        && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal Response.TransactionMaster.HeaderViewModel UPITransferHeaderSummary(UPITransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.UPITransfer
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    list = SelectByArchive(tranListArchive);
                }
                else
                {
                    list = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.UPITransfer
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                            && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            orderby Tran.TransactionMasterID descending
                            select Tran).ToList();
                }
            }
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;
            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForPayoutReport(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.Payout).ToList();

            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.Payout
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.Payout
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.DOCFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        IFSC = t.DMT_IFSC,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                        Remark = t.Bill_Optional6,
                        Charge = t.Charge,
                        NetCommission = t.NetCommission
                    }).ToList();

            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForVendorPayReport(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.VendorPay).ToList();

            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.VendorPay
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        IFSC = t.DMT_IFSC,
                        Remark = t.Bill_Optional6

                    }).ToList();

            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForPayoutReportExcelDownload(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.Payout).ToList();
            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.Payout
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.Payout
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.DOCFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                        Charge = t.Charge,
                        NetCommission = t.NetCommission
                    }).ToList();
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (txReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                if (txReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                list = (from item in list
                        where fAmount <= item.Amount && item.Amount <= tAmount
                        select item).ToList();
            }
            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForVendorPayReportExcelDownload(PayoutReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.VendorPay).ToList();
            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.VendorPay
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName
                    }).ToList();
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (txReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                if (txReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                list = (from item in list
                        where fAmount <= item.Amount && item.Amount <= tAmount
                        select item).ToList();
            }
            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForUPITransferReport(UPITransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.UPITransfer).ToList();

            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.UPITransfer
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.UPITransfer
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                    }).ToList();
            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForUPITransferReportExcelDownload(UPITransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.UPITransfer).ToList();
            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.UPITransfer
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.UPITransfer
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                        Latitude = t.SimCardDetails,
                        Longitude = t.Location
                    }).ToList();
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }
            return list;
        }

        internal List<AmountWiseInsightsViewModel> AmountWiseInsightsSelect(AmountWiseInsightsSelectRequest selectRequest)
        {
            List<AmountWiseInsightsViewModel> list = new List<AmountWiseInsightsViewModel>();
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.Mobile
                            && Tran.Status != StatusType.Failure
                            select Tran).ToList();
            }

            if (selectRequest.ProviderID > 0)
            {
                tranList = (from item in tranList
                            where item.ProviderID == selectRequest.ProviderID
                            select item).ToList();
            }

            if (selectRequest.OperatorID > 0)
            {
                tranList = (from item in tranList
                            where item.OperatorID == selectRequest.OperatorID
                            select item).ToList();
            }

            var Success = (from item in tranList
                           where item.Status == StatusType.Success
                           group item by new { item.Status, item.Amount } into grp
                           select new
                           {
                               Amount = grp.Key.Amount,
                               SuccessAmount = grp.Sum(X => X.Amount),
                               ROfferCommission = grp.Sum(X => X.ROfferCommission),
                               SuccessCount = grp.Count(),
                           }).ToList();

            var total = (from item in tranList
                         group item by new { item.Status, item.Amount } into grp
                         select new
                         {
                             Amount = grp.Key.Amount,
                             TotalAmount = grp.Sum(X => X.Amount),
                             TotalCount = grp.Count(),
                         }).ToList();

            var finallist = (from main in Success
                             join t in total on main.Amount equals t.Amount
                             select new
                             {
                                 Amount = (main?.Amount ?? 0),
                                 TotalAmount = t?.TotalAmount ?? 0,
                                 TotalCount = t?.TotalCount ?? 0,
                                 SuccessAmount = main?.SuccessAmount ?? 0,
                                 SuccessCount = main?.SuccessCount ?? 0,
                                 ROffer = (main?.ROfferCommission ?? 0),
                             }).ToList();

            decimal SumOFTotalAmount = finallist.Select(c => c.TotalAmount).Sum();
            decimal SumOfRoFfer = finallist.Select(c => c.ROffer).Sum();

            var orderlist = (from main in finallist

                             select new
                             {
                                 Amount = main.Amount,
                                 AmountPercentage = new Util().PercentageForAmountWiseInsigths(main.Amount, main.TotalAmount, SumOFTotalAmount),
                                 TotalAmount = main.TotalAmount,
                                 TotalCount = main.TotalCount,
                                 SuccessAmount = main.SuccessAmount,
                                 SuccessCount = main.SuccessCount,
                                 ROffer = main.ROffer,
                                 ROfferPercentage = new Util().PercentageForAmountWiseInsigths(main.ROffer, main.ROffer, SumOfRoFfer),
                             }).OrderByDescending(x => Convert.ToDecimal(x.AmountPercentage)).ToList();

            list = (from main in orderlist
                    select new AmountWiseInsightsViewModel
                    {
                        Amount = main.Amount,
                        AmountPercentage = main.AmountPercentage.ToString() + " %",
                        TotalAmount = main.TotalAmount,
                        TotalCount = main.TotalCount,
                        SuccessAmount = main.SuccessAmount,
                        SuccessCount = main.SuccessCount,
                        ROffer = main.ROffer,
                        ROfferPercentage = main.ROfferPercentage.ToString() + " %",
                    }).ToList();

            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForOTTTransferReport(OTTTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.OTT).ToList();

            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.OTT
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        TDS = t.RetailerTDS,
                        GST = t.RetailerGST
                    }).ToList();
            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForOTTTransferReportExcelDownload(OTTTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.OTT).ToList();
            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.OTT
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        TDS = t.RetailerTDS,
                        GST = t.RetailerGST
                    }).ToList();
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }
            return list;
        }

        internal int SelectForOTTTransferCount(OTTTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.OTT
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
                if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
                {
                    if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                    {
                        decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                        decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                        tranList = (from item in tranList
                                    where fAmount <= item.Amount && item.Amount <= tAmount
                                    select item).ToList();
                    }
                }
            }

            return tranList.Count();
        }

        internal Response.TransactionMaster.HeaderViewModel OTTTransferHeaderSummary(OTTTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.OTT
                        && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;
            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForBusBookingTransferReportExcelDownload(BusBookingTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.BusBooking).ToList();
            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.BusBooking
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        TDS = t.RetailerTDS,
                        GST = t.RetailerGST
                    }).ToList();
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }
            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForBusBookingTransferReport(BusBookingTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.BusBooking).ToList();

            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.BusBooking
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName,
                        TDS = t.RetailerTDS,
                        GST = t.RetailerGST
                    }).ToList();
            return list;
        }

        internal int SelectForBusBookingTransferCount(BusBookingTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.BusBooking
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
                if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
                {
                    if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                    {
                        decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                        decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                        tranList = (from item in tranList
                                    where fAmount <= item.Amount && item.Amount <= tAmount
                                    select item).ToList();
                    }
                }
            }

            return tranList.Count();
        }

        internal Response.TransactionMaster.HeaderViewModel BusBookingTransferHeaderSummary(BusBookingTransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.BusBooking
                        && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }
            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                {
                    decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                    decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                    list = (from item in list
                            where fAmount <= item.Amount && item.Amount <= tAmount
                            select item).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;
            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal int SelectForUPITransferCount(UPITransferReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.UPITransfer
                                                && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                                && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       select Tran).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from Tran in context.TransactionMaster
                                where Tran.DOCInt >= FromDate
                                && Tran.DOCInt <= ToDate
                                && Tran.ServiceID == (int)ServiceCode.UPITransfer
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                select Tran).ToList();
                }
                if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
                {
                    if (!txReportRequest.CriteriaText.EqualsIgnoreCase("") && !txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                    {
                        decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                        decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                        tranList = (from item in tranList
                                    where fAmount <= item.Amount && item.Amount <= tAmount
                                    select item).ToList();
                    }
                }
            }

            return tranList.Count();
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForAccountOpeningReport(AccontOpeningRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.AccountOpening).ToList();

            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.AccountOpening
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName
                    }).ToList();
            return list;
        }

        internal List<Project.Response.TransactionMaster.ViewModel> SelectForAccountOpeningReportExcelDownload(AccontOpeningRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.AccountOpening).ToList();
            List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.AccountOpening
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new Project.Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ProviderRequestID = t.ProviderRequestID,
                        PaymentType = t.DMT_PaymentType,
                        BeneName = t.DMT_BeneficiaryName
                    }).ToList();
            return list;
        }

        internal int SelectForAccountOpeningCount(AccontOpeningRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.AccountOpening
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
            }

            return tranList.Count();
        }

        internal List<MATMExcelViewModel> SelectForMicroATMReportExcelDownload(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<MATMExcelViewModel> list = new List<MATMExcelViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.MATM).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.MATM
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.MATM
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).ToList();
                }
            }
            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new MATMExcelViewModel
                    {
                        TransactionDatetime = t.DOC.DOCFormatDate(),
                        EntityID = User.AliasName,
                        EntityName = User.FirmName,
                        Channel = t.Through,
                        ProductName = "MicroATM",
                        ServiceName = op.OperatorName,
                        TransactionID = t.ProviderRequestID,
                        RRN = t.ReferenceNo,
                        TransactionAmount = t.Amount,
                        Commission = t.RetailerCommission,
                        TDS = t.RetailerTDS,
                        NetCommission = t.NetCommission,
                        Charge = t.Charge,
                        GST = t.RetailerGST,
                        SettlementAmount = t.Amount + t.NetCommission,
                        OpeningBalance = t.OpeningBalance,
                        ClosingBalance = t.ClosingBalance,
                        DScommission = t.DistributerCommission,
                        MDScommission = t.SuperDistributerCommission,
                        TxnStatus = t.Status,
                        CustomerMobileNumber = t.DMT_SenderNumber,
                        CardNumber = t.ConsumerNumber,
                        //BankName = t.DMT_Bank,
                        DSID = t.DistributerUserID,
                        MDSID = t.SuperDistributerUserID,
                        Latitude = t.Location,
                        Longitude = t.SimCardDetails,
                        PartnerName = provider.ProviderName,
                        PartnerRefNo = t.ProviderRefNo,
                        Remark = t.Description
                    }).ToList();

            return list;
        }

        internal List<AEPSViewModel> SelectForPOSReportExcelDownload(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<AEPSViewModel> list = new List<AEPSViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.POS).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.POS
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.POS
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).ToList();
                }
                list = (from t in transactionMaster
                        join User in usersList on t.UserID equals User.UserID
                        join provider in providerList on t.ProviderID equals provider.ProviderID
                        join op in operatorList on t.OperatorID equals op.OperatorID
                        select new AEPSViewModel
                        {
                            FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                            DOC = t.DOC.LongFormatDate(),
                            UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                            UserID = t.UserID,
                            DistributerUserID = t.DistributerUserID,
                            SuperDistributerUserID = t.SuperDistributerUserID,
                            OperatorName = op.OperatorName,
                            OpeningBalance = t.OpeningBalance,
                            Amount = t.Amount,
                            DebitAmount = t.DebitAmount,
                            ClosingBalance = t.ClosingBalance,
                            ProviderName = provider.ProviderName,
                            Status = t.Status,
                            Remark = t.Description,
                            RetailerComm = t.RetailerCommission,
                            DistributerComm = t.DistributerCommission,
                            SuperDistributerComm = t.SuperDistributerCommission,
                            IPAddress = t.IPAddress,
                            ComplaintStatus = t.ComplaintStatus,
                            Through = t.Through,
                            TransactionMasterID = t.TransactionMasterID,
                            ReferenceNo = (t.Status == StatusType.Failure) ? t.Description : t.ReferenceNo,
                            ConsumerNumber = t.ConsumerNumber,
                            RequestID = t.ProviderRequestID,

                            GST = t.RetailerGST,
                            TDS = t.RetailerTDS,
                            DistributerGST = t.DistributerGST,
                            DistributerTDS = t.DistributerTDS,
                            SuperDistributerGST = t.SuperDistributerGST,
                            SuperDistributerTDS = t.SuperDistributerTDS,
                        }).ToList();
            }

            return list;
        }

        internal int SelectForAEPSCount(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.AEPS
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                      && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.OperatorID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
            }

            return tranList.Count();
        }

        internal List<AEPSViewModel> SelectForAEPSReport(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.AEPS).ToList();

            List<AEPSViewModel> list = new List<AEPSViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.AEPS
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.AEPS
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new AEPSViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        AgentID = User.AliasName,
                        Name = User.FirmName,
                        ConsumerNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DOC = t.DOC.DOCFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        ProviderName = provider.ProviderName,
                        Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        Status = t.Status,
                        Remark = t.Description,
                        RetailerComm = t.RetailerCommission,
                        DistributerComm = t.DistributerCommission,
                        SuperDistributerComm = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        RequestID = t.ProviderRequestID,
                        AdminProfit = t.AdminProfit,
                        ProviderCommission = t.ProviderCommission,
                        SenderNumber = t.DMT_SenderNumber,
                        GST = t.RetailerGST,
                        TDS = t.RetailerTDS,
                        DistributerGST = t.DistributerGST,
                        DistributerTDS = t.DistributerTDS,
                        SuperDistributerGST = t.SuperDistributerGST,
                        SuperDistributerTDS = t.SuperDistributerTDS,
                        Latitude = t.Location,
                        Longitude = t.SimCardDetails,
                        DeviceType = t.Bill_Optional3,
                        NetCommission = t.NetCommission,
                    }).ToList();
            return list;
        }
        internal List<AEPSExcelViewModel> SelectForAEPSExcelDownloadReport(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.AEPS).ToList();

            List<AEPSExcelViewModel> list = new List<AEPSExcelViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.AEPS
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    transactionMaster = SelectByArchive(tranListArchive);
                }
                else
                {
                    transactionMaster = (from Tran in context.TransactionMaster
                                         where Tran.DOCInt >= FromDate
                                         && Tran.DOCInt <= ToDate
                                         && Tran.ServiceID == (int)ServiceCode.AEPS
                                         && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                         && (txReportRequest.OperatorID == 0 ? 2 > 1 : Tran.OperatorID == txReportRequest.OperatorID)
                                         && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                         && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                         && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                         GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                         && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                         GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                         && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                         orderby Tran.TransactionMasterID descending
                                         select Tran).ToList();
                }
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    select new AEPSExcelViewModel
                    {
                        TransactionDatetime = t.DOC.DOCFormatDate(),
                        EntityID = User.AliasName,
                        EntityName = User.FirmName,
                        Channel = t.Through,
                        ProductName = "AEPS",
                        ServiceName = op.OperatorName,
                        TransactionID = t.ProviderRequestID,
                        RRN = t.ReferenceNo,
                        TransactionAmount = t.Amount,
                        Commission = t.RetailerCommission,
                        TDS = t.RetailerTDS,
                        NetCommission = t.NetCommission,
                        Charge = t.Charge,
                        GST = t.RetailerGST,
                        SettlementAmount = t.Amount + t.NetCommission,
                        OpeningBalance = t.OpeningBalance,
                        ClosingBalance = t.ClosingBalance,
                        DScommission = t.DistributerCommission,
                        MDScommission = t.SuperDistributerCommission,
                        TxnStatus = t.Status,
                        CustomerMobileNumber = t.DMT_SenderNumber,
                        AadhaarNumber = t.ConsumerNumber,
                        BankName = t.DMT_Bank,
                        DSID = t.DistributerUserID,
                        MDSID = t.SuperDistributerUserID,
                        Latitude = t.Location,
                        Longitude = t.SimCardDetails,
                        PartnerName = provider.ProviderName,
                        BiometricDevice = t.Bill_Optional3,
                        STAN = t.Bill_Optional1,
                        Remark = t.Description,

                    }).ToList();
            return list;
        }

        internal Response.TransactionMaster.HeaderViewModel AEPSHeaderSummary(List<AEPSViewModel> list, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerComm),
                               DistCommi = grp.Sum(X => X.DistributerComm),
                               SuperCommi = grp.Sum(X => X.SuperDistributerComm),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;
            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal Response.TransactionMaster.HeaderViewModel MicroATMHeaderSummary(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<TransactionMaster> list = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.MATM
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                            && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    list = SelectByArchive(tranListArchive);
                }
                else
                {
                    list = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.MATM
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                            && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            orderby Tran.TransactionMasterID descending
                            select Tran).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal Response.TransactionMaster.HeaderViewModel POSHeaderSummary(AEPSReport txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from Tran in context.Archive_TransactionMaster
                                       where Tran.DOCInt >= FromDate
                                       && Tran.DOCInt <= ToDate
                                       && Tran.ServiceID == (int)ServiceCode.POS
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                       && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                       orderby Tran.TransactionMasterID descending
                                       select Tran).ToList();
                    list = SelectByArchive(tranListArchive);
                }
                else
                {
                    list = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.POS
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                            && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            orderby Tran.TransactionMasterID descending
                            select Tran).ToList();
                }
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal Response.TransactionMaster.HeaderViewModel CreditCardHeaderSummary(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.CreditCardPayment
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal List<Response.DayBookReport.ViewModel> SelectForDayBook(Requests.DayBookReport.SelectRequest mainRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.DayBookReport.ViewModel> report = new List<Response.DayBookReport.ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster

                                where txn.DOCInt >= FromDate && txn.DOCInt <= ToDate

                                select new
                                {
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    OperatorID = txn.OperatorID,
                                    OperatorName = txn.OperatorMaster.OperatorName,
                                    Amount = txn.Amount,
                                    Status = txn.Status,
                                    RetailerCommission = txn.RetailerCommission,
                                    CashBack = txn.CashBack,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission
                                }).ToList();

                if (mainList == null)
                    return report;

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    mainList = (from item in mainList
                                where item.SuperDistributerUserID == UserID
                                select item).ToList();
                }
                if (UserTypeID == (int)UserType.Distributer)
                {
                    mainList = (from item in mainList
                                where item.DistributerUserID == UserID
                                select item).ToList();
                }
                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Client)
                {
                    mainList = (from item in mainList
                                where item.UserID == UserID
                                select item).ToList();
                }

                if (mainRequest.UserTypeID == 0 || mainRequest.UserTypeID == (int)UserType.Retailer ||
                    mainRequest.UserTypeID == (int)UserType.APIUser || mainRequest.UserTypeID == (int)UserType.Client)
                {
                    if (mainRequest.UserID != 0)
                    {
                        mainList = (from item in mainList
                                    where item.UserID == mainRequest.UserID
                                    select item).ToList();
                    }
                }

                if (mainRequest.UserTypeID == (int)UserType.Distributer)
                {
                    if (mainRequest.UserID != 0)
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID > 1
                                    select item).ToList();
                    }
                }
                if (mainRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    if (mainRequest.UserID != 0)
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID > 1
                                    select item).ToList();
                    }
                }

                var totalList = (from txn in mainList
                                 group txn by new { txn.OperatorName, txn.OperatorID } into grp
                                 select new
                                 {
                                     OperatorID = grp.Key.OperatorID,
                                     OperatorName = grp.Key.OperatorName,
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).ToList();

                var successList = (from txn in mainList
                                   where txn.Status == StatusType.Success
                                   group txn by new { txn.OperatorName, txn.OperatorID } into grp
                                   select new
                                   {
                                       OperatorID = grp.Key.OperatorID,
                                       OperatorName = grp.Key.OperatorName,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                       RetailerCommission = grp.Where(q => q.RetailerCommission > 0).Sum(x => x.RetailerCommission) + grp.Sum(x => x.CashBack),
                                       DistributerCommission = grp.Where(q => q.DistributerCommission > 0).Sum(x => x.DistributerCommission),
                                       SuperDistributerCommission = grp.Where(q => q.SuperDistributerCommission > 0).Sum(x => x.SuperDistributerCommission),
                                       Surcharge = grp.Where(q => q.RetailerCommission < 0).Sum(x => Math.Abs(x.RetailerCommission)),
                                       DistSurcharge = grp.Where(q => q.DistributerCommission < 0).Sum(x => Math.Abs(x.DistributerCommission)),
                                       SuperDistSurcharge = grp.Where(q => q.SuperDistributerCommission < 0).Sum(x => Math.Abs(x.SuperDistributerCommission)),
                                   }).ToList();

                var failList = (from txn in mainList
                                where txn.Status == StatusType.Failure
                                group txn by new { txn.OperatorName, txn.OperatorID } into grp
                                select new
                                {
                                    OperatorID = grp.Key.OperatorID,
                                    OperatorName = grp.Key.OperatorName,
                                    Count = grp.Count(),
                                    Amount = grp.Sum(x => x.Amount),
                                }).ToList();

                var pendingList = (from txn in mainList
                                   where txn.Status == StatusType.Process
                                   group txn by new { txn.OperatorName, txn.OperatorID } into grp
                                   select new
                                   {
                                       OperatorID = grp.Key.OperatorID,
                                       OperatorName = grp.Key.OperatorName,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).ToList();

                report = (from total in totalList
                          join success in successList on total.OperatorID equals success.OperatorID into successDetails
                          from successVal in successDetails.DefaultIfEmpty()
                          join fail in failList on total.OperatorID equals fail.OperatorID into failDetails
                          from failVal in failDetails.DefaultIfEmpty()

                          join pending in pendingList on total.OperatorID equals pending.OperatorID into pendingDetails
                          from pendingVal in pendingDetails.DefaultIfEmpty()

                          select new Response.DayBookReport.ViewModel
                          {
                              OperatorID = total.OperatorID,
                              Operator = total.OperatorName,
                              TotalCount = total.Count,
                              TotalAmount = total.Amount,

                              SuccessCount = successVal?.Count ?? 0,
                              SuccessAmount = successVal?.Amount ?? 0,
                              RetailerCommission = successVal?.RetailerCommission ?? 0,
                              DistributerCommission = successVal?.DistributerCommission ?? 0,
                              SuperDistributerCommission = successVal?.SuperDistributerCommission ?? 0,
                              Surcharge = successVal?.Surcharge ?? 0,
                              FailedCount = failVal?.Count ?? 0,
                              FailedAmount = failVal?.Amount ?? 0,

                              PendingCount = pendingVal?.Count ?? 0,
                              PendingAmount = pendingVal?.Amount ?? 0,
                          }).ToList();
            }

            return report;
        }

        internal List<Response.DayBookReport.ViewModel> SelectByProviderID(Requests.DayBookReport.SelectRequest mainRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();

            List<Response.DayBookReport.ViewModel> report = new List<Response.DayBookReport.ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.ProviderID == mainRequest.ProviderID
                               && txn.DOCInt >= FromDate && txn.DOCInt <= ToDate
                               && (mainRequest.UserID == 0 ? 2 > 1 : txn.UserID == mainRequest.UserID)

                                select new
                                {
                                    UserID = txn.UserID,
                                    OperatorID = txn.OperatorID,
                                    ServiceName = txn.ServiceMaster.Name,
                                    OperatorName = txn.OperatorMaster.OperatorName,
                                    Amount = txn.Amount,
                                    Status = txn.Status,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    AdminCommission = txn.AdminCommission,
                                }).ToList();

                if (mainList == null)
                    return report;

                var totalList = (from txn in mainList
                                 group txn by new { txn.OperatorName, txn.OperatorID, txn.ServiceName } into grp
                                 select new
                                 {
                                     OperatorID = grp.Key.OperatorID,
                                     OperatorName = grp.Key.OperatorName,
                                     ServiceName = grp.Key.ServiceName,
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).ToList();

                var successList = (from txn in mainList
                                   where txn.Status == StatusType.Success
                                   group txn by new { txn.OperatorName, txn.OperatorID } into grp
                                   select new
                                   {
                                       OperatorID = grp.Key.OperatorID,
                                       OperatorName = grp.Key.OperatorName,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                       RetailerCommission = grp.Where(q => q.RetailerCommission > 0).Sum(x => x.RetailerCommission),
                                       DistributerCommission = grp.Where(q => q.DistributerCommission > 0).Sum(x => x.DistributerCommission),
                                       SuperDistributerCommission = grp.Where(q => q.SuperDistributerCommission > 0).Sum(x => x.SuperDistributerCommission),
                                       AdminCommission = grp.Sum(x => x.AdminCommission),
                                   }).ToList();

                var failList = (from txn in mainList
                                where txn.Status == StatusType.Failure
                                group txn by new { txn.OperatorName, txn.OperatorID } into grp
                                select new
                                {
                                    OperatorID = grp.Key.OperatorID,
                                    OperatorName = grp.Key.OperatorName,
                                    Count = grp.Count(),
                                    Amount = grp.Sum(x => x.Amount),
                                }).ToList();

                var pendingList = (from txn in mainList
                                   where txn.Status == StatusType.Process
                                   group txn by new { txn.OperatorName, txn.OperatorID } into grp
                                   select new
                                   {
                                       OperatorID = grp.Key.OperatorID,
                                       OperatorName = grp.Key.OperatorName,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).ToList();

                report = (from total in totalList
                          join success in successList on total.OperatorID equals success.OperatorID into successDetails
                          from successVal in successDetails.DefaultIfEmpty()
                          join fail in failList on total.OperatorID equals fail.OperatorID into failDetails
                          from failVal in failDetails.DefaultIfEmpty()
                          join pending in pendingList on total.OperatorID equals pending.OperatorID into pendingDetails
                          from pendingVal in pendingDetails.DefaultIfEmpty()

                          select new Response.DayBookReport.ViewModel
                          {
                              ServiceName = total.ServiceName,
                              OperatorID = total.OperatorID,
                              Operator = total.OperatorName,
                              TotalCount = total.Count,
                              TotalAmount = total.Amount,
                              SuccessCount = successVal?.Count ?? 0,
                              SuccessAmount = successVal?.Amount ?? 0,
                              RetailerCommission = successVal?.RetailerCommission ?? 0,
                              DistributerCommission = successVal?.DistributerCommission ?? 0,
                              SuperDistributerCommission = successVal?.SuperDistributerCommission ?? 0,
                              AdminCommission = successVal?.AdminCommission ?? 0,
                              FailedCount = failVal?.Count ?? 0,
                              FailedAmount = failVal?.Amount ?? 0,
                              PendingCount = pendingVal?.Count ?? 0,
                              PendingAmount = pendingVal?.Amount ?? 0,
                          }).ToList();
            }

            return report;
        }

        private List<object> FilterUserWiseDayBookForUserType(List<object> mainList, int UserID)
        {
            return mainList;
        }

        internal List<ProviderDayBookViewModel> ProviderDaybook(ProviderDayBookRequest mainRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ProviderDayBookViewModel> report = new List<ProviderDayBookViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                join provider in context.ProviderMaster on
                                txn.ProviderID equals provider.ProviderID
                                where txn.DOCInt >= FromDate && txn.DOCInt <= ToDate
                                orderby txn.ServiceID descending
                                select new
                                {
                                    ProviderName = provider.ProviderName,
                                    ProviderID = txn.ProviderID,
                                    OperatorID = txn.OperatorID,
                                    OperatorName = txn.OperatorMaster.OperatorName,
                                    Amount = txn.Amount,
                                    AdminCommission = txn.AdminCommission,
                                    Status = txn.Status,
                                    ProviderCommission = txn.ProviderCommission,
                                    RetailerCommission = txn.RetailerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                }).ToList();

                if (mainList == null)
                    return report;
                if (mainRequest.ProviderID != 0)
                {
                    mainList = mainList.Where(q => q.ProviderID == mainRequest.ProviderID).ToList();
                }
                if (mainRequest.OperatorID != 0)
                {
                    mainList = mainList.Where(q => q.OperatorID == mainRequest.OperatorID).ToList();
                }

                var totalList = (from txn in mainList
                                 group txn by new { txn.OperatorName, txn.OperatorID, txn.ProviderID, txn.ProviderName } into grp
                                 select new
                                 {
                                     ProviderName = grp.Key.ProviderName,
                                     ProviderID = grp.Key.ProviderID,
                                     OperatorID = grp.Key.OperatorID,
                                     OperatorName = grp.Key.OperatorName,
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).ToList();

                var successList = (from txn in mainList
                                   where txn.Status == StatusType.Success
                                   group txn by new { txn.OperatorID, txn.ProviderID } into grp
                                   select new
                                   {
                                       OperatorID = grp.Key.OperatorID,
                                       ProviderID = grp.Key.ProviderID,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                       AdminCommission = grp.Sum(x => x.AdminCommission),
                                       ProviderCommission = grp.Sum(x => x.ProviderCommission),
                                       RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                       DistributerCommission = grp.Sum(x => x.DistributerCommission),
                                       SuperDistributerCommission = grp.Sum(x => x.SuperDistributerCommission),
                                       TotalCommission = grp.Sum(x => x.RetailerCommission + x.DistributerCommission + x.SuperDistributerCommission)
                                   }).ToList();

                var failList = (from txn in mainList
                                where txn.Status == StatusType.Failure
                                group txn by new { txn.OperatorID, txn.ProviderID } into grp
                                select new
                                {
                                    OperatorID = grp.Key.OperatorID,
                                    ProviderID = grp.Key.ProviderID,
                                    Count = grp.Count(),
                                    Amount = grp.Sum(x => x.Amount),
                                }).ToList();

                var pendingList = (from txn in mainList
                                   where txn.Status == StatusType.Process
                                   group txn by new { txn.OperatorID, txn.ProviderID } into grp
                                   select new
                                   {
                                       OperatorID = grp.Key.OperatorID,
                                       ProviderID = grp.Key.ProviderID,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                   }).ToList();

                report = (from total in totalList
                          join success in successList on new { X1 = total.OperatorID, X2 = total.ProviderID } equals new { X1 = success.OperatorID, X2 = success.ProviderID }
                          into successDetails
                          from successVal in successDetails.DefaultIfEmpty()
                          join fail in failList on new { X1 = total.OperatorID, X2 = total.ProviderID } equals new { X1 = fail.OperatorID, X2 = fail.ProviderID }
                          into failDetails
                          from failVal in failDetails.DefaultIfEmpty()
                          join pending in pendingList on new { X1 = total.OperatorID, X2 = total.ProviderID } equals new { X1 = pending.OperatorID, X2 = pending.ProviderID }
                          into pendingDetails
                          from pendingVal in pendingDetails.DefaultIfEmpty()
                          orderby total.ProviderName, total.Amount descending
                          select new ProviderDayBookViewModel
                          {
                              ProviderName = total.ProviderName,
                              ProviderID = total.ProviderID,
                              OperatorID = total.OperatorID,
                              Operator = total.OperatorName,
                              TotalHits = total.Count,
                              TotalAmount = total.Amount,

                              SuccessHits = successVal?.Count ?? 0,
                              SuccessAmount = successVal?.Amount ?? 0,

                              ProviderCommission = successVal?.ProviderCommission ?? 0,
                              AdminCommission = successVal?.AdminCommission ?? 0,

                              RetailerCommission = successVal?.RetailerCommission ?? 0,
                              DistributerCommission = successVal?.DistributerCommission ?? 0,

                              SuperDistributerCommission = successVal?.SuperDistributerCommission ?? 0,

                              FailedHits = failVal?.Count ?? 0,
                              FailedAmount = failVal?.Amount ?? 0,
                              PendingHits = pendingVal?.Count ?? 0,
                              PendingAmount = pendingVal?.Amount ?? 0,
                              Profit = (successVal?.AdminCommission ?? 0) - (successVal?.TotalCommission ?? 0)
                          }).ToList();
            }

            return report;
        }

        internal int GetCreditCardTransactionCount(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<TransactionMaster> tranList = new List<TransactionMaster>();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from TransactionMaster in context.TransactionMaster
                            where TransactionMaster.DOCInt >= FromDate
                            && TransactionMaster.DOCInt <= ToDate
                            && TransactionMaster.ServiceID == (int)ServiceCode.CreditCardPayment
                            && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                            && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                            && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                            && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                            && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                            orderby TransactionMaster.TransactionMasterID descending
                            select TransactionMaster).ToList();

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        tranList = (from item in tranList
                                    where item.SuperDistributerUserID == UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.Distributer)
                    {
                        tranList = (from item in tranList
                                    where item.DistributerUserID == UserID
                                    select item).ToList();
                    }
                }
                if (Util.isRoleAdminOrSupport(UserTypeID) && txReportRequest.UserID != 0)
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(txReportRequest.UserID);
                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        tranList = (from item in tranList
                                    where item.SuperDistributerUserID == txReportRequest.UserID
                                    select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        tranList = (from item in tranList
                                    where item.DistributerUserID == txReportRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        tranList = (from item in tranList
                                    where item.UserID == txReportRequest.UserID
                                    select item).ToList();
                    }
                }
                return tranList.Count();
            }
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForCreditCardFilter(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.CreditCardPayment).ToList();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from TransactionMaster in context.TransactionMaster
                            where TransactionMaster.DOCInt >= FromDate
                            && TransactionMaster.DOCInt <= ToDate
                             && TransactionMaster.ServiceID == (int)ServiceCode.CreditCardPayment
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                            && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                            && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                            && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                            && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? TransactionMaster.ReferenceNo == txReportRequest.CriteriaText :
                            txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? TransactionMaster.ConsumerNumber == txReportRequest.CriteriaText :
                            txReportRequest.Criteria.EqualsIgnoreCase("APIUserRequestID") ? TransactionMaster.APIUserRequestID == txReportRequest.CriteriaText : 2 > 1)

                            orderby TransactionMaster.TransactionMasterID descending
                            select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID

                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.ViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Logo = operators?.OperatorImagePath.ConcatWebAPIURL(),

                        SenderNo = TransactionMaster.DMT_SenderNumber,

                        SenderName = TransactionMaster.DMT_SenderName,

                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",
                        ReferenceNo = (TransactionMaster.Status == StatusType.Failure) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                        PaymentType = TransactionMaster.DMT_PaymentType,
                        Through = TransactionMaster.Through,

                        DOC = TransactionMaster.DOC.LongFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName ?? "",

                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        GST = TransactionMaster.AdminGST,
                        TDS = TransactionMaster.RetailerTDS,
                    }).ToList();

            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForCreditCardFilterAndExcelDownload(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.CreditCardPayment).ToList();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from TransactionMaster in context.TransactionMaster
                            where TransactionMaster.DOCInt >= FromDate
                            && TransactionMaster.DOCInt <= ToDate
                             && TransactionMaster.ServiceID == (int)ServiceCode.CreditCardPayment
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                            && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                            && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                            && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                            && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                            && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                            GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                            && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                            GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                            && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? TransactionMaster.ReferenceNo == txReportRequest.CriteriaText :
                            txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? TransactionMaster.ConsumerNumber == txReportRequest.CriteriaText :
                            txReportRequest.Criteria.EqualsIgnoreCase("APIUserRequestID") ? TransactionMaster.APIUserRequestID == txReportRequest.CriteriaText : 2 > 1)

                            orderby TransactionMaster.TransactionMasterID descending
                            select TransactionMaster).ToList();
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID

                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.ViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Logo = operators?.OperatorImagePath.ConcatWebAPIURL(),

                        SenderNo = TransactionMaster.DMT_SenderNumber,

                        SenderName = TransactionMaster.DMT_SenderName,

                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",
                        ReferenceNo = (TransactionMaster.Status == StatusType.Failure) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                        PaymentType = TransactionMaster.DMT_PaymentType,
                        Through = TransactionMaster.Through,

                        DOC = TransactionMaster.DOC.LongFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName ?? "",

                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        GST = TransactionMaster.AdminGST,
                        TDS = TransactionMaster.RetailerTDS,
                    }).ToList();

            return list;
        }

        internal List<MobileDashboardViewModel> SelectForRetailerDashborad(Requests.TransactionReport.SelectByDateRequest mainRequest, int UserID)
        {
            List<MobileDashboardViewModel> report = new List<MobileDashboardViewModel>();

            int[] ids = new int[] { (int)ServiceCode.Mobile, (int)ServiceCode.Dth, (int)ServiceCode.PostPaid };

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            int dateInt = Int32.Parse(mainRequest.DateModified.ToString(Messages.DOCIntegerFomrat));

            var mainList = new[] { new { UserID = 0, Amount = 0.0M, Status = "-", RetailerCommission = 0.0M } }.ToList();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (UserTypeID == (int)UserType.Admin ||
                    UserTypeID == (int)UserType.Distributer ||
                    UserTypeID == (int)UserType.SuperDistributer)
                {
                    List<DownlineForDropDownResponse> userList = new UserDAL().SelectDownlineForDropDown(UserID);
                    List<int> userIDs = userList.Select(s => s.UserID).ToList();

                    mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt == dateInt
                                && ids.Contains(txn.ServiceID)
                                && userIDs.Contains(txn.UserID)
                                select new
                                {
                                    UserID = txn.UserID,
                                    Amount = txn.Amount,
                                    Status = txn.Status,
                                    RetailerCommission = txn.DistributerCommission
                                }).ToList();
                }
                else
                {
                    mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt == dateInt
                                && txn.UserID == UserID
                                && ids.Contains(txn.ServiceID)
                                select new
                                {
                                    UserID = txn.UserID,
                                    Amount = txn.Amount,
                                    Status = txn.Status,
                                    RetailerCommission = txn.RetailerCommission
                                }).ToList();
                }

                if (mainList == null || mainList.Count <= 0)
                {
                    return report;
                }

                var totalList = (from txn in mainList
                                 group txn by new { txn.UserID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                     RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                 }).ToList();

                var successList = (from txn in mainList
                                   where txn.Status == StatusType.Success
                                   group txn by new { txn.UserID } into grp
                                   select new
                                   {
                                       UserID = grp.Key.UserID,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                       RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                   }).ToList();

                var failList = (from txn in mainList
                                where txn.Status == StatusType.Failure
                                group txn by new { txn.UserID } into grp
                                select new
                                {
                                    UserID = grp.Key.UserID,
                                    Count = grp.Count(),
                                    Amount = grp.Sum(x => x.Amount),
                                    RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                }).ToList();

                var pendingList = (from txn in mainList
                                   where txn.Status == StatusType.Process
                                   group txn by new { txn.UserID } into grp
                                   select new
                                   {
                                       UserID = grp.Key.UserID,
                                       Count = grp.Count(),
                                       Amount = grp.Sum(x => x.Amount),
                                       RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                   }).ToList();

                report = (from total in totalList
                          join success in successList on total.UserID equals success.UserID into successDetails
                          from successVal in successDetails.DefaultIfEmpty()
                          join fail in failList on total.UserID equals fail.UserID into failDetails
                          from failVal in failDetails.DefaultIfEmpty()

                          join pending in pendingList on total.UserID equals pending.UserID into pendingDetails
                          from pendingVal in pendingDetails.DefaultIfEmpty()

                          select new MobileDashboardViewModel
                          {
                              TotalCount = total.Count,
                              TotalAmount = total.Amount,

                              SuccessCount = successVal?.Count ?? 0,
                              SuccessAmount = successVal?.Amount ?? 0,
                              RetailerCommission = successVal?.RetailerCommission ?? 0,

                              FailedCount = failVal?.Count ?? 0,
                              FailedAmount = failVal?.Amount ?? 0,

                              PendingCount = pendingVal?.Count ?? 0,
                              PendingAmount = pendingVal?.Amount ?? 0,
                          }).ToList();
            }

            return report;
        }

        internal List<TDSReportViewModel> SelectTDSByUserID(TDSReportRequest selectRequest, int UserID)
        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                selectRequest.UserTypeID = UserTypeID;
                selectRequest.UserID = UserID;
            }

            if (selectRequest.UserTypeID == 0 && selectRequest.UserID == 0)
            {
                List<TDSReportViewModel> res = TDSOnlyUserTypeIDAndUserID(selectRequest, UserID);
                return res;
            }
            else if (selectRequest.UserTypeID != 0 && selectRequest.UserID == 0)
            {
                List<TDSReportViewModel> res = TDSOnlyUserTypeID(selectRequest);
                return res;
            }

            else
            {
                List<UserMaster> usersList = new UserDAL().Select();
                // take skip and take users if data for skip and take count

                usersList = (from user in usersList
                             where user.UserTypeID == selectRequest.UserTypeID
                             select user).ToList();
                using (var context = new DatabaseContext())
                {
                    List<TransactionInfo> mainList;
                    List<TransactionInfo> pgList;
                    context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                    if (selectRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                    {

                        mainList = (from txn in context.Archive_TransactionMaster
                                    where txn.DOCInt >= From && txn.DOCInt <= To
                                        && txn.Status == StatusType.Success
                                    select new TransactionInfo
                                    {
                                        ID = txn.TransactionMasterID,
                                        UserID = txn.UserID,
                                        DistributerUserID = txn.DistributerUserID,
                                        SuperDistributerUserID = txn.SuperDistributerUserID,
                                        Status = txn.Status,
                                        Amount = txn.Amount,
                                        ServiceID = txn.ServiceID,
                                        RetailerCommission = txn.RetailerCommission,
                                        CashBack = txn.CashBack,
                                        DistributerCommission = txn.DistributerCommission,
                                        SuperDistributerCommission = txn.SuperDistributerCommission,
                                        RetailerTDS = txn.RetailerTDS,
                                        DistributerTDS = txn.DistributerTDS,
                                        SuperDistributerTDS = txn.SuperDistributerTDS
                                    }).ToList();
                    }
                    else
                    {
                        mainList = (from txn in context.TransactionMaster
                                    where txn.DOCInt >= From && txn.DOCInt <= To
                                     && txn.Status == StatusType.Success
                                    select new TransactionInfo
                                    {
                                        ID = txn.TransactionMasterID,
                                        UserID = txn.UserID,
                                        DistributerUserID = txn.DistributerUserID,
                                        SuperDistributerUserID = txn.SuperDistributerUserID,
                                        Status = txn.Status,
                                        Amount = txn.Amount,
                                        ServiceID = txn.ServiceID,
                                        RetailerCommission = txn.RetailerCommission,
                                        CashBack = txn.CashBack,
                                        DistributerCommission = txn.DistributerCommission,
                                        SuperDistributerCommission = txn.SuperDistributerCommission,
                                        RetailerTDS = txn.RetailerTDS,
                                        DistributerTDS = txn.DistributerTDS,
                                        SuperDistributerTDS = txn.SuperDistributerTDS
                                    }).ToList();

                    }

                    pgList = (from txn in context.PaymentGateway
                              where txn.DOCInt >= From && txn.DOCInt <= To
                              && txn.Status == StatusType.Success
                              select new TransactionInfo
                              {
                                  ID = txn.PaymentGatewayID,
                                  UserID = txn.UserID,
                                  DistributerUserID = txn.DistributerUserID,
                                  SuperDistributerUserID = txn.SuperDistributerUserID,
                                  Status = txn.Status,
                                  Amount = txn.Amount,
                                  ServiceID = (int)ServiceCode.PaymentGateway,
                                  RetailerCommission = txn.Commission,
                                  CashBack = 0M,
                                  DistributerCommission = txn.DistributerCommission,
                                  SuperDistributerCommission = txn.SuperDistributerCommission,
                                  RetailerTDS = txn.RetailerTDS,
                                  DistributerTDS = txn.DistributerTDS,
                                  SuperDistributerTDS = txn.SuperDistributerTDS
                              }).ToList();


                    var listFinal = mainList.Union(pgList).ToList();



                    List<UserTypeMaster> userTypelist = new UserTypeDAL().Select();

                    if (selectRequest.UserTypeID == (int)UserType.Distributer)
                    {
                        var grouplist = (from txn in listFinal
                                         where txn.DistributerTDS > 0
                                         group txn by new { txn.DistributerUserID } into grp
                                         select new
                                         {
                                             UserID = grp.Key.DistributerUserID,
                                             SucessCount = grp.Count(),
                                             SucessAMT = grp.Sum(X => X.Amount),
                                             Commission = grp.Sum(X => X.DistributerCommission),
                                             RetailerTDS = grp.Sum(X => X.RetailerTDS),
                                             DistributerTDS = grp.Sum(X => X.DistributerTDS),
                                             SuperDistributerTDS = grp.Sum(X => X.SuperDistributerTDS)
                                         }).ToList();

                        var response = (from User in usersList
                                        join txn in grouplist on User.UserID equals txn.UserID into lefttxn
                                        from LT in lefttxn.DefaultIfEmpty()
                                        join role in userTypelist on User.UserTypeID equals role.UserTypeID
                                        where User.UserID == selectRequest.UserID
                                        orderby LT?.SucessAMT descending
                                        select new TDSReportViewModel
                                        {
                                            UserName = User.PersonName,
                                            MobileNumber = User.MobileNumber,
                                            PanNumber = User.PANNo,
                                            AadharNo = User.AadharNo,
                                            FirmName = User.FirmName,
                                            UserType = role.UserType,
                                            SuccessAmt = LT?.SucessAMT ?? 0,
                                            SuccessCount = LT?.SucessCount ?? 0,
                                            Commission = LT?.Commission ?? 0,
                                            TDS = LT?.DistributerTDS ?? 0,
                                        }).ToList();
                        return response;
                    }
                    else if (selectRequest.UserTypeID == (int)UserType.SuperDistributer)
                    {
                        var grouplist = (from txn in listFinal
                                         where txn.SuperDistributerTDS > 0
                                         group txn by new { txn.SuperDistributerTDS } into grp
                                         select new
                                         {
                                             UserID = grp.Key.SuperDistributerTDS,
                                             SucessCount = grp.Count(),
                                             SucessAMT = grp.Sum(X => X.Amount),
                                             Commission = grp.Sum(X => X.SuperDistributerCommission),
                                             RetailerTDS = grp.Sum(X => X.RetailerTDS),
                                             DistributerTDS = grp.Sum(X => X.DistributerTDS),
                                             SuperDistributerTDS = grp.Sum(X => X.SuperDistributerTDS)
                                         }).ToList();

                        var response = (from User in usersList
                                        join txn in grouplist on User.UserID equals txn.UserID into lefttxn
                                        from LT in lefttxn.DefaultIfEmpty()
                                        join role in userTypelist on User.UserTypeID equals role.UserTypeID
                                        where User.UserID == selectRequest.UserID
                                        orderby LT?.SucessAMT descending
                                        select new TDSReportViewModel
                                        {
                                            UserName = User.PersonName,
                                            MobileNumber = User.MobileNumber,
                                            PanNumber = User.PANNo,
                                            AadharNo = User.AadharNo,
                                            FirmName = User.FirmName,
                                            UserType = role.UserType,
                                            SuccessAmt = LT?.SucessAMT ?? 0,
                                            SuccessCount = LT?.SucessCount ?? 0,
                                            Commission = LT?.Commission ?? 0,
                                            TDS = LT?.SuperDistributerTDS ?? 0,
                                        }).ToList();
                        return response;
                    }
                    else
                    {
                        var grouplist = (from txn in listFinal
                                         where txn.RetailerTDS > 0
                                         group txn by new { txn.UserID } into grp
                                         select new
                                         {
                                             UserID = grp.Key.UserID,
                                             SucessCount = grp.Count(),
                                             SucessAMT = grp.Sum(X => X.Amount),
                                             Commission = grp.Where(X => X.RetailerCommission > 0).Sum(X => X.RetailerCommission),
                                             CashBack = grp.Sum(X => X.CashBack),
                                             RetailerTDS = grp.Sum(X => X.RetailerTDS),
                                             DistributerTDS = grp.Sum(X => X.DistributerTDS),
                                             SuperDistributerTDS = grp.Sum(X => X.SuperDistributerTDS)
                                         }).ToList();

                        var response = (from User in usersList
                                        join txn in grouplist on User.UserID equals txn.UserID into lefttxn
                                        from LT in lefttxn.DefaultIfEmpty()
                                        join role in userTypelist on User.UserTypeID equals role.UserTypeID
                                        where User.UserID == selectRequest.UserID
                                        orderby LT?.SucessAMT descending
                                        select new TDSReportViewModel
                                        {
                                            UserName = User.PersonName,
                                            MobileNumber = User.MobileNumber,
                                            PanNumber = User.PANNo,
                                            AadharNo = User.AadharNo,
                                            FirmName = User.FirmName,
                                            UserType = role.UserType,
                                            SuccessAmt = LT?.SucessAMT ?? 0,
                                            SuccessCount = LT?.SucessCount ?? 0,
                                            Commission = (LT?.Commission ?? 0) + (LT?.CashBack ?? 0),
                                            TDS = LT?.RetailerTDS ?? 0,
                                        }).ToList();
                        return response;
                    }
                }
            }
        }

        private List<TDSReportViewModel> TDSOnlyUserTypeIDAndUserID(TDSReportRequest selectRequest, int UserID)
        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                selectRequest.UserTypeID = UserTypeID;
                selectRequest.UserID = UserID;
            }

            List<UserMaster> usersList = new UserDAL().Select();

            // take skip and take users if data for skip and take count


            usersList = (from user in usersList
                         where user.UserTypeID == (int)UserType.Retailer
                              || user.UserTypeID == (int)UserType.Distributer
                            || user.UserTypeID == (int)UserType.SuperDistributer
                         select user).ToList();


            using (var context = new DatabaseContext())
            {

                List<TransactionInfo> mainList;
                List<TransactionInfo> pgList;
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;


                if (selectRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    mainList = (from txn in context.Archive_TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                 && txn.Status == StatusType.Success
                                select new TransactionInfo
                                {
                                    ID = txn.TransactionMasterID,
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    RetailerCommission = txn.RetailerCommission,
                                    RetailerTDS = txn.RetailerTDS,
                                    DistributerTDS = txn.DistributerTDS,
                                    SuperDistributerTDS = txn.SuperDistributerTDS
                                }).ToList();
                }
                else
                {
                    mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                 && txn.Status == StatusType.Success
                                select new TransactionInfo
                                {
                                    ID = txn.TransactionMasterID,
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    RetailerCommission = txn.RetailerCommission,
                                    RetailerTDS = txn.RetailerTDS,
                                    DistributerTDS = txn.DistributerTDS,
                                    SuperDistributerTDS = txn.SuperDistributerTDS
                                }).ToList();

                }

                pgList = (from txn in context.PaymentGateway
                          where txn.DOCInt >= From && txn.DOCInt <= To
                          && txn.Status == StatusType.Success
                          select new TransactionInfo
                          {
                              ID = txn.PaymentGatewayID,
                              UserID = txn.UserID,
                              DistributerUserID = txn.DistributerUserID,
                              SuperDistributerUserID = txn.SuperDistributerUserID,
                              Status = txn.Status,
                              Amount = txn.Amount,
                              ServiceID = (int)ServiceCode.PaymentGateway,
                              RetailerCommission = txn.Commission,
                              RetailerTDS = txn.RetailerTDS,
                              DistributerTDS = txn.DistributerTDS,
                              SuperDistributerTDS = txn.SuperDistributerTDS
                          }).ToList();

                var listFinal = mainList.Union(pgList).ToList();

                List<UserTypeMaster> userTypelist = new UserTypeDAL().Select();

                var grouplist = (from txn in listFinal
                                 where txn.RetailerTDS > 0
                                 group txn by new { txn.UserID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     SucessCount = grp.Count(),
                                     SucessAMT = grp.Sum(X => X.Amount),
                                     Commission = grp.Sum(X => X.RetailerCommission),
                                     RetailerTDS = grp.Sum(X => X.RetailerTDS),
                                     DistributerTDS = grp.Sum(X => X.DistributerTDS),
                                     SuperDistributerTDS = grp.Sum(X => X.SuperDistributerTDS)
                                 }).ToList();

                var response = (from User in usersList
                                join txn in grouplist on User.UserID equals txn.UserID into lefttxn
                                from LT in lefttxn.DefaultIfEmpty()
                                join role in userTypelist on User.UserTypeID equals role.UserTypeID
                                orderby LT?.SucessAMT descending
                                select new TDSReportViewModel
                                {
                                    UserName = User.PersonName,
                                    MobileNumber = User.MobileNumber,
                                    PanNumber = User.PANNo,
                                    AadharNo = User.AadharNo,
                                    FirmName = User.FirmName,
                                    UserType = role.UserType,
                                    SuccessAmt = LT?.SucessAMT ?? 0,
                                    SuccessCount = LT?.SucessCount ?? 0,
                                    Commission = LT?.Commission ?? 0,
                                    TDS = LT?.RetailerTDS ?? 0,
                                }).ToList();
                return response;
            }
        }



        private List<TDSReportViewModel> TDSOnlyUserTypeID(TDSReportRequest selectRequest)
        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<UserMaster> usersList = new UserDAL().Select();
            // take skip and take users if data for skip and take count

            usersList = (from user in usersList
                         where user.UserTypeID == selectRequest.UserTypeID
                         select user).ToList();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                List<TransactionInfo> mainList;
                List<TransactionInfo> pgList;

                if (selectRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {

                    mainList = (from txn in context.Archive_TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                && txn.Status == StatusType.Success

                                select new TransactionInfo
                                {
                                    ID = txn.TransactionMasterID,
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    RetailerCommission = txn.RetailerCommission,
                                    CashBack = txn.CashBack,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    RetailerTDS = txn.RetailerTDS,
                                    DistributerTDS = txn.DistributerTDS,
                                    SuperDistributerTDS = txn.SuperDistributerTDS
                                }).ToList();
                }
                else
                {

                    mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                 && txn.Status == StatusType.Success

                                select new TransactionInfo
                                {
                                    ID = txn.TransactionMasterID,
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    RetailerCommission = txn.RetailerCommission,
                                    CashBack = txn.CashBack,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    RetailerTDS = txn.RetailerTDS,
                                    DistributerTDS = txn.DistributerTDS,
                                    SuperDistributerTDS = txn.SuperDistributerTDS
                                }).ToList();
                }

                pgList = (from txn in context.PaymentGateway
                          where txn.DOCInt >= From && txn.DOCInt <= To
                          && txn.Status == StatusType.Success
                          select new TransactionInfo
                          {
                              ID = txn.PaymentGatewayID,
                              UserID = txn.UserID,
                              DistributerUserID = txn.DistributerUserID,
                              SuperDistributerUserID = txn.SuperDistributerUserID,
                              Status = txn.Status,
                              Amount = txn.Amount,
                              ServiceID = (int)ServiceCode.PaymentGateway,
                              RetailerCommission = txn.Commission,
                              CashBack = 0M,
                              DistributerCommission = txn.DistributerCommission,
                              SuperDistributerCommission = txn.SuperDistributerCommission,
                              RetailerTDS = txn.RetailerTDS,
                              DistributerTDS = txn.DistributerTDS,
                              SuperDistributerTDS = txn.SuperDistributerTDS
                          }).ToList();

                var listFinal = mainList.Union(pgList).ToList();
                List<UserTypeMaster> userTypelist = new UserTypeDAL().Select();

                if (selectRequest.UserTypeID == (int)UserType.Distributer)
                {
                    var grouplist = (from txn in listFinal
                                     where txn.DistributerTDS > 0
                                     group txn by new { txn.DistributerUserID } into grp
                                     select new
                                     {
                                         UserID = grp.Key.DistributerUserID,
                                         SucessCount = grp.Count(),
                                         SucessAMT = grp.Sum(X => X.Amount),
                                         Commission = grp.Sum(X => X.DistributerCommission),
                                         RetailerTDS = grp.Sum(X => X.RetailerTDS),
                                         DistributerTDS = grp.Sum(X => X.DistributerTDS),
                                         SuperDistributerTDS = grp.Sum(X => X.SuperDistributerTDS)
                                     }).ToList();

                    var response = (from User in usersList
                                    join txn in grouplist on User.UserID equals txn.UserID into lefttxn
                                    from LT in lefttxn.DefaultIfEmpty()
                                    join role in userTypelist on User.UserTypeID equals role.UserTypeID
                                    where User.UserTypeID == (int)UserType.Retailer || User.UserTypeID == (int)UserType.APIUser ||
                                          User.UserTypeID == (int)UserType.Client || User.UserTypeID == (int)UserType.Distributer
                                          || User.UserTypeID == (int)UserType.SuperDistributer
                                    orderby LT?.SucessAMT descending
                                    select new TDSReportViewModel
                                    {
                                        UserName = User.PersonName,
                                        MobileNumber = User.MobileNumber,
                                        PanNumber = User.PANNo,
                                        AadharNo = User.AadharNo,
                                        FirmName = User.FirmName,
                                        UserType = role.UserType,
                                        SuccessAmt = LT?.SucessAMT ?? 0,
                                        SuccessCount = LT?.SucessCount ?? 0,
                                        Commission = LT?.Commission ?? 0,
                                        TDS = LT?.DistributerTDS ?? 0,
                                    }).ToList();
                    return response;
                }
                else if (selectRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    var grouplist = (from txn in listFinal
                                     where txn.SuperDistributerTDS > 0
                                     group txn by new { txn.SuperDistributerUserID } into grp
                                     select new
                                     {
                                         UserID = grp.Key.SuperDistributerUserID,
                                         SucessCount = grp.Count(),
                                         SucessAMT = grp.Sum(X => X.Amount),
                                         Commission = grp.Sum(X => X.SuperDistributerCommission),
                                         RetailerTDS = grp.Sum(X => X.RetailerTDS),
                                         DistributerTDS = grp.Sum(X => X.DistributerTDS),
                                         SuperDistributerTDS = grp.Sum(X => X.SuperDistributerTDS)
                                     }).ToList();

                    var response = (from User in usersList
                                    join txn in grouplist on User.UserID equals txn.UserID into lefttxn
                                    from LT in lefttxn.DefaultIfEmpty()
                                    join role in userTypelist on User.UserTypeID equals role.UserTypeID
                                    where User.UserTypeID == (int)UserType.Retailer || User.UserTypeID == (int)UserType.APIUser ||
                                          User.UserTypeID == (int)UserType.Client || User.UserTypeID == (int)UserType.Distributer
                                          || User.UserTypeID == (int)UserType.SuperDistributer
                                    orderby LT?.SucessAMT descending
                                    select new TDSReportViewModel
                                    {
                                        UserName = User.PersonName,
                                        MobileNumber = User.MobileNumber,
                                        PanNumber = User.PANNo,
                                        AadharNo = User.AadharNo,
                                        FirmName = User.FirmName,
                                        UserType = role.UserType,
                                        SuccessAmt = LT?.SucessAMT ?? 0,
                                        SuccessCount = LT?.SucessCount ?? 0,
                                        Commission = LT?.Commission ?? 0,
                                        TDS = LT?.SuperDistributerTDS ?? 0,
                                    }).ToList();
                    return response;
                }
                else
                {
                    var grouplist = (from txn in listFinal
                                     where txn.RetailerTDS > 0
                                     group txn by new { txn.UserID } into grp
                                     select new
                                     {
                                         UserID = grp.Key.UserID,
                                         SucessCount = grp.Count(),
                                         SucessAMT = grp.Sum(X => X.Amount),
                                         Commission = grp.Sum(X => X.RetailerCommission),
                                         CashBack = grp.Sum(X => X.CashBack),
                                         RetailerTDS = grp.Sum(X => X.RetailerTDS),
                                         DistributerTDS = grp.Sum(X => X.DistributerTDS),
                                         SuperDistributerTDS = grp.Sum(X => X.SuperDistributerTDS)
                                     }).ToList();

                    var response = (from User in usersList
                                    join txn in grouplist on User.UserID equals txn.UserID into lefttxn
                                    from LT in lefttxn.DefaultIfEmpty()
                                    join role in userTypelist on User.UserTypeID equals role.UserTypeID
                                    where User.UserTypeID == (int)UserType.Retailer || User.UserTypeID == (int)UserType.APIUser ||
                                          User.UserTypeID == (int)UserType.Client || User.UserTypeID == (int)UserType.Distributer
                                          || User.UserTypeID == (int)UserType.SuperDistributer
                                    orderby LT?.SucessAMT descending
                                    select new TDSReportViewModel
                                    {
                                        UserName = User.PersonName,
                                        MobileNumber = User.MobileNumber,
                                        PanNumber = User.PANNo,
                                        AadharNo = User.AadharNo,
                                        FirmName = User.FirmName,
                                        UserType = role.UserType,
                                        SuccessAmt = LT?.SucessAMT ?? 0,
                                        SuccessCount = LT?.SucessCount ?? 0,
                                        Commission = (LT?.Commission ?? 0) + (LT?.CashBack ?? 0),
                                        TDS = LT?.RetailerTDS ?? 0,
                                    }).ToList();
                    return response;
                }
            }
        }

        internal List<SuccessPayoutTransactionModel> SelectSuccessPayoutTransaction(int userID)
        {
            DateTime FromDate = DateTime.Now.AddDays(-30);
            DateTime ToDate = DateTime.Now;

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(ToDate.ToString(Messages.DOCIntegerFomrat));

            List<SuccessPayoutTransactionModel> list = new List<SuccessPayoutTransactionModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from txn in context.TransactionMaster
                        where txn.Status == StatusType.Success
                        && txn.ServiceID == (int)ServiceCode.Payout
                        && txn.DOCInt >= From && txn.DOCInt <= To
                        && txn.UserID == userID
                        orderby txn.DOC descending
                        select new SuccessPayoutTransactionModel
                        {
                            PaymentType = txn.DMT_PaymentType,
                            AccountNo = txn.ConsumerNumber,
                            IFSC = txn.DMT_IFSC,
                            BankName = txn.DMT_Bank,
                            BeneficiaryName = txn.DMT_BeneficiaryName
                        }).Distinct().Take(10).ToList();
            }
            return list;
        }

        internal List<SuccessUPITransferTransactionModel> SelectSuccessUPITransaction(int userID)
        {
            DateTime FromDate = DateTime.Now.AddDays(-30);
            DateTime ToDate = DateTime.Now;

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(ToDate.ToString(Messages.DOCIntegerFomrat));

            List<SuccessUPITransferTransactionModel> list = new List<SuccessUPITransferTransactionModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from txn in context.TransactionMaster
                        where txn.Status == StatusType.Success
                        && txn.ServiceID == (int)ServiceCode.UPITransfer
                        && txn.DOCInt >= From && txn.DOCInt <= To
                        && txn.UserID == userID
                        orderby txn.DOC descending
                        select new SuccessUPITransferTransactionModel
                        {
                            PaymentType = txn.DMT_PaymentType,
                            AccountNo = txn.ConsumerNumber,
                            IFSC = txn.DMT_IFSC,
                            BankName = txn.DMT_Bank,
                            BeneficiaryName = txn.DMT_BeneficiaryName,
                            OperatorID = txn.OperatorID
                        }).Distinct().Take(10).ToList();
            }
            return list;
        }

        internal List<GSTDetailReportViewModel> SelectGSTDetailReportForAdmin(GSTDetailReportRequest selectRequest, int UserID)
        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                 && txn.Status == StatusType.Success
                                select new
                                {
                                    ID = txn.TransactionMasterID,
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    AdminGST = txn.AdminGST,
                                    Commission = txn.RetailerCommission
                                }).ToList();

                var pgList = (from txn in context.PaymentGateway
                              where txn.DOCInt >= From && txn.DOCInt <= To
                              && txn.Status == StatusType.Success
                              select new
                              {
                                  ID = txn.PaymentGatewayID,
                                  UserID = txn.UserID,
                                  DistributerUserID = txn.DistributerUserID,
                                  SuperDistributerUserID = txn.SuperDistributerUserID,
                                  Status = txn.Status,
                                  Amount = txn.Amount,
                                  ServiceID = (int)ServiceCode.PaymentGateway,
                                  AdminGST = txn.AdminGST,
                                  Commission = txn.Commission
                              }).ToList();

                var listFinal = mainList.Union(pgList).ToList();

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    if (UserTypeID == (int)UserType.Distributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.DistributerUserID == UserID
                                     select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.SuperDistributerUserID == UserID
                                     select item).ToList();
                    }
                    else
                    {
                        listFinal = (from item in listFinal
                                     where item.UserID == UserID
                                     select item).ToList();
                    }
                }

                if (UserTypeID == (int)UserType.Admin && selectRequest.UserID != 0)
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);

                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.SuperDistributerUserID == selectRequest.UserID
                                     select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.DistributerUserID == selectRequest.UserID
                                     select item).ToList();
                    }
                    else
                    {
                        listFinal = (from item in listFinal
                                     where item.UserID == selectRequest.UserID
                                     select item).ToList();
                    }
                }

                var grouplist = (from txn in listFinal
                                 group txn by new { txn.UserID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     SucessCount = grp.Count(),
                                     SucessAMT = grp.Sum(X => X.Amount),
                                     AdminGST = grp.Sum(X => X.AdminGST),
                                     Commission = grp.Sum(X => X.Commission),
                                 }).ToList();

                List<UserMaster> Userlist = new UserDAL().Select();
                List<StateMaster> Statelist = new StateDAL().Select();
                List<UserTypeMaster> userTypelist = new UserTypeDAL().Select();
                var response = (from txn in grouplist
                                join user in Userlist on txn.UserID equals user.UserID
                                join state in Statelist on user.StateID equals state.ID
                                join usertype in userTypelist on user.UserTypeID equals usertype.UserTypeID
                                orderby txn?.SucessAMT descending
                                select new GSTDetailReportViewModel
                                {
                                    UserName = user.PersonName,
                                    MobileNumber = user.MobileNumber,
                                    GSTNumber = user.GSTNo,
                                    FirmName = user.FirmName,
                                    UserType = usertype.UserType,
                                    StateName = state.Name,
                                    SuccessAmt = txn?.SucessAMT ?? 0,
                                    SuccessCount = txn?.SucessCount ?? 0,
                                    GST = txn?.AdminGST ?? 0,
                                    Commission = txn?.Commission ?? 0,
                                }).ToList();

                return response;
            }
        }

        internal List<IncomeReportViewModel> IncomeReport(IncomeReportRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                 && txn.Status == StatusType.Success
                                select new
                                {
                                    ID = txn.TransactionMasterID,
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    AdminCommission = txn.AdminCommission

                                }).ToList();

                var pgList = (from txn in context.PaymentGateway
                              where txn.DOCInt >= From && txn.DOCInt <= To
                              && txn.Status == StatusType.Success
                              select new
                              {
                                  ID = txn.PaymentGatewayID,
                                  UserID = txn.UserID,
                                  DistributerUserID = txn.DistributerUserID,
                                  SuperDistributerUserID = txn.SuperDistributerUserID,
                                  Status = txn.Status,
                                  Amount = txn.Amount,
                                  ServiceID = (int)ServiceCode.PaymentGateway,
                                  RetailerCommission = txn.Commission,
                                  DistributerCommission = txn.DistributerCommission,
                                  SuperDistributerCommission = txn.SuperDistributerCommission,
                                  AdminCommission = txn.Commission,

                              }).ToList();

                var listFinal = mainList.Union(pgList).ToList();

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    if (UserTypeID == (int)UserType.Distributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.DistributerUserID == UserID
                                     select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.SuperDistributerUserID == UserID
                                     select item).ToList();
                    }
                    else
                    {
                        listFinal = (from item in listFinal
                                     where item.UserID == UserID
                                     select item).ToList();
                    }
                }

                if (UserTypeID == (int)UserType.Admin && selectRequest.UserID != 0)
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);

                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.SuperDistributerUserID == selectRequest.UserID
                                     select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        listFinal = (from item in listFinal
                                     where item.DistributerUserID == selectRequest.UserID
                                     select item).ToList();
                    }
                    else
                    {
                        listFinal = (from item in listFinal
                                     where item.UserID == selectRequest.UserID
                                     select item).ToList();
                    }
                }

                var grouplist = (from txn in listFinal
                                 group txn by new { txn.ServiceID } into grp
                                 select new
                                 {
                                     ServiceID = grp.Key.ServiceID,
                                     SucessCount = grp.Count(),
                                     SucessAMT = grp.Sum(X => X.Amount),
                                     RCommission = grp.Sum(X => X.RetailerCommission),
                                     DCommission = grp.Sum(X => X.DistributerCommission),
                                     SCommission = grp.Sum(X => X.SuperDistributerCommission),
                                     AdminCommission = grp.Sum(X => X.AdminCommission)

                                 }).ToList();

                List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();
                var response = (from Service in Servicelist
                                join txn in grouplist on Service.ServiceID equals txn.ServiceID into groupdetails
                                from gpL in groupdetails.DefaultIfEmpty()
                                orderby gpL?.SucessAMT descending
                                select new IncomeReportViewModel
                                {
                                    ServiceName = Service.Name,
                                    SuccessAmt = gpL?.SucessAMT ?? 0,
                                    SuccessCount = gpL?.SucessCount ?? 0,
                                    RCommission = gpL?.RCommission ?? 0,
                                    DCommission = gpL?.DCommission ?? 0,
                                    SCommission = gpL?.SCommission ?? 0,
                                    AdminCommission = gpL?.AdminCommission ?? 0,
                                    NetProfit = (gpL?.AdminCommission ?? 0) - ((gpL?.RCommission ?? 0) + (gpL?.DCommission ?? 0) + (gpL?.SCommission ?? 0))
                                }).ToList();

                return response;
            }
        }

        private DashboardTodaySummaryCommissionSurcharge getTodaySummaryTotalCommissionSurcharge(List<DashboardDatewiseData> mainList)
        {
            var TotalCommissionSurchargeList = (from txn in mainList
                                                where txn.Status == StatusType.Success
                                                group txn by new { txn.DOCInt } into grp
                                                select new DashboardTodaySummaryCommissionSurcharge
                                                {
                                                    SuperDistributerCommission = grp.Where(t => t.SuperDistributerCommission > 0).Sum(x => x.SuperDistributerCommission),
                                                    DistributerCommission = grp.Where(t => t.DistributerCommission > 0).Sum(x => x.DistributerCommission),
                                                    RetailerCommission = grp.Where(t => t.RetailerCommission > 0).Sum(x => x.RetailerCommission),

                                                    SuperDistributerSurcharge = grp.Where(t => t.SuperDistributerCommission <= 0).Sum(x => Math.Abs(x.SuperDistributerCommission)),
                                                    DistributerSurcharge = grp.Where(t => t.DistributerCommission <= 0).Sum(x => Math.Abs(x.DistributerCommission)),
                                                    RetailerSurcharge = grp.Where(t => t.RetailerCommission <= 0).Sum(x => Math.Abs(x.RetailerCommission)),

                                                    RetailerTDS = grp.Sum(x => Math.Abs(x.RetailerTDS)),
                                                    DistributerTDS = grp.Sum(x => Math.Abs(x.DistributerTDS)),
                                                    SuperDistributerTDS = grp.Sum(x => Math.Abs(x.SuperDistributerTDS)),
                                                }).FirstOrDefault();

            return TotalCommissionSurchargeList;
        }

        private DashboardTodaySummaryCommissionSurcharge getAdminCommissionWithUser(List<DashboardDatewiseData> mainList)
        {
            var CommissionWithSurchargeListWithAdmin = (from txn in mainList
                                                        where txn.Status == StatusType.Success

                                                        group txn by new { txn.DOCInt } into grp
                                                        select new DashboardTodaySummaryCommissionSurcharge
                                                        {
                                                            AdminCommission = grp.Where(t => t.AdminCommission > 0).Sum(x => x.AdminCommission),
                                                            AdminSurcharge = grp.Where(t => t.AdminCommission <= 0).Sum(x => x.AdminCommission),

                                                            SuperDistributerCommission = grp.Where(t => t.RetailerCommission > 0).Sum(x => x.SuperDistributerCommission),
                                                            DistributerCommission = grp.Where(t => t.RetailerCommission > 0).Sum(x => x.DistributerCommission),
                                                            RetailerCommission = grp.Where(t => t.RetailerCommission > 0).Sum(x => x.RetailerCommission),

                                                            SuperDistributerSurcharge = grp.Where(t => t.RetailerCommission <= 0).Sum(x => x.SuperDistributerCommission),
                                                            DistributerSurcharge = grp.Where(t => t.RetailerCommission <= 0).Sum(x => x.DistributerCommission),
                                                            RetailerSurcharge = grp.Where(t => t.RetailerCommission <= 0).Sum(x => x.RetailerCommission),
                                                        }).FirstOrDefault();

            return CommissionWithSurchargeListWithAdmin;
        }

        private RofferViewModel getROfferCommissionWithPercentage(List<DashboardDatewiseData> mainList)
        {
            RofferViewModel response = new RofferViewModel();
            var maindata = (from txn in mainList
                            where txn.Status == StatusType.Success
                            && txn.ROfferCommission > 0
                            group txn by new { txn.DOCInt } into grp
                            select new
                            {
                                ROfferCommission = grp.Sum(x => x.ROfferCommission),
                                TotalTransactionAmount = grp.Sum(x => x.Amount),
                            }).FirstOrDefault();
            if (maindata == null)
                return response;
            decimal PercentageAmount = (maindata.ROfferCommission / maindata.TotalTransactionAmount) * 100;
            response.RofferComm = maindata.ROfferCommission;
            response.RofferTotal = maindata.TotalTransactionAmount;
            response.RofferPer = PercentageAmount.TwoDigitDecimal();
            return response;
        }

        internal async Task<DashboardViewModel> SelectForAdminDashborad(string Date, int WalletID)
        {
            DateTime datetime = Convert.ToDateTime(Date);
            DateTime datetimePast10Days = Convert.ToDateTime(Date).AddDays(-10);
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            int dateIntPastDays = Convert.ToInt32(Convert.ToDateTime(datetimePast10Days).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt == dateInt
                                select new DashboardDatewiseData
                                {
                                    TransactionMasterID = txn.TransactionMasterID,
                                    WalletID = txn.WalletID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    DebitAmount = txn.DebitAmount,
                                    ServiceID = txn.ServiceID,
                                    DOCInt = txn.DOCInt,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    RetailerTDS = txn.RetailerTDS,
                                    DistributerTDS = txn.DistributerTDS,
                                    SuperDistributerTDS = txn.SuperDistributerTDS,
                                    AdminCommission = txn.AdminCommission,
                                    ROfferCommission = txn.ROfferCommission
                                }).ToList();
                if (WalletID != 0)
                {
                    mainList = (from item in mainList
                                where item.WalletID == WalletID
                                select item).ToList();
                }

                //"yyyyMMdd"
                var oldData = (from txn in context.TransactionMaster
                               where txn.DOCInt >= dateIntPastDays && txn.DOCInt != dateInt && txn.UpdatedDOC.Date == datetime.Date &&
                               (txn.Status == StatusType.Failure || txn.Status == StatusType.Success)
                               select new
                               {
                                   Status = txn.Status,
                                   WalletID = txn.WalletID,
                                   Amount = txn.DebitAmount,
                                   ConsumerNumber = txn.ConsumerNumber,
                                   DOCInt = txn.DOCInt,
                                   UpdatedDOCInt = Convert.ToInt32(Convert.ToDateTime(txn.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                               }).ToList();
                if (WalletID != 0)
                {
                    oldData = (from item in oldData
                               where item.WalletID == WalletID
                               select item).ToList();
                }

                decimal? pastFailAmount = (from txn in oldData
                                           where txn.UpdatedDOCInt == dateInt && txn.Status == StatusType.Failure
                                           select txn).Sum(o => o.Amount);

                decimal? pastSuccessAmount = (from txn in oldData
                                              where txn.UpdatedDOCInt == dateInt && txn.Status == StatusType.Success
                                              select txn).Sum(o => o.Amount);

                var totalList = (from txn in mainList
                                 group txn by new { txn.DOCInt } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

                var TotalSuccessList = (from txn in mainList
                                        where txn.Status == StatusType.Success
                                        group txn by new { txn.DOCInt } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                var CommissionWithSurchargeListWithAdmin = getAdminCommissionWithUser(mainList);
                var totCommssionSurchageTDS = getTodaySummaryTotalCommissionSurcharge(mainList);
                RofferViewModel rofferViewModel = getROfferCommissionWithPercentage(mainList);

                var TotalFailList = (from txn in mainList
                                     where txn.Status == StatusType.Failure
                                     && txn.ServiceID != (int)ServiceCode.AEPS
                                     && txn.ServiceID != (int)ServiceCode.MATM
                                     group txn by new { txn.DOCInt } into grp
                                     select new
                                     {
                                         Count = grp.Count(),
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();

                var TotalPendingList = (from txn in mainList
                                        where txn.Status == StatusType.Process
                                        && txn.ServiceID != (int)ServiceCode.AEPS
                                         && txn.ServiceID != (int)ServiceCode.MATM
                                        group txn by new { txn.DOCInt } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                var PrepaidSucess = (from txn in mainList
                                     where txn.ServiceID == (int)ServiceCode.Mobile
                                     && txn.Status == StatusType.Success
                                     group txn by new { txn.DOCInt } into grp
                                     select new
                                     {
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();

                var PrepaidFail = (from txn in mainList
                                   where txn.ServiceID == (int)ServiceCode.Mobile
                                   && txn.Status == StatusType.Failure
                                   group txn by new { txn.DOCInt } into grp
                                   select new
                                   {
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                var DthSucess = (from txn in mainList
                                 where txn.ServiceID == (int)ServiceCode.Dth
                                 && txn.Status == StatusType.Success
                                 group txn by new { txn.DOCInt } into grp
                                 select new
                                 {
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

                var DthFail = (from txn in mainList
                               where txn.ServiceID == (int)ServiceCode.Dth
                               && txn.Status == StatusType.Failure
                               group txn by new { txn.DOCInt } into grp
                               select new
                               {
                                   Amount = grp.Sum(x => x.Amount),
                               }).FirstOrDefault();

                var PostPaidSucess = (from txn in mainList
                                      where txn.ServiceID == (int)ServiceCode.PostPaid
                                      && txn.Status == StatusType.Success
                                      group txn by new { txn.DOCInt } into grp
                                      select new
                                      {
                                          Amount = grp.Sum(x => x.Amount),
                                      }).FirstOrDefault();

                var PostPaidFail = (from txn in mainList
                                    where txn.ServiceID == (int)ServiceCode.PostPaid
                                    && txn.Status == StatusType.Failure
                                    group txn by new { txn.DOCInt } into grp
                                    select new
                                    {
                                        Amount = grp.Sum(x => x.Amount),
                                    }).FirstOrDefault();
                var DMTSucess = (from txn in mainList
                                 where txn.ServiceID == (int)ServiceCode.MoneyTransfer
                                 && txn.ServiceID == (int)ServiceCode.VendorPay
                                 && txn.Status == StatusType.Success
                                 group txn by new { txn.DOCInt } into grp
                                 select new
                                 {
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

                var DMTFail = (from txn in mainList
                               where txn.ServiceID == (int)ServiceCode.MoneyTransfer
                                && txn.ServiceID == (int)ServiceCode.VendorPay
                               && txn.Status == StatusType.Failure
                               group txn by new { txn.DOCInt } into grp
                               select new
                               {
                                   Amount = grp.Sum(x => x.Amount),
                               }).FirstOrDefault();
                var AEPSSuccess = (from txn in mainList
                                   where txn.ServiceID == (int)ServiceCode.AEPS
                                   && txn.Status == StatusType.Success
                                   group txn by new { txn.DOCInt } into grp
                                   select new
                                   {
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                var AEPSFail = (from txn in mainList
                                where txn.ServiceID == (int)ServiceCode.AEPS
                                && txn.Status == StatusType.Failure
                                group txn by new { txn.DOCInt } into grp
                                select new
                                {
                                    Amount = grp.Sum(x => x.Amount),
                                }).FirstOrDefault();

                var MATMSuccess = (from txn in mainList
                                   where txn.ServiceID == (int)ServiceCode.MATM
                                   && txn.Status == StatusType.Success
                                   group txn by new { txn.DOCInt } into grp
                                   select new
                                   {
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();
                var POSSuccess = (from txn in mainList
                                  where txn.ServiceID == (int)ServiceCode.POS
                                  && txn.Status == StatusType.Success
                                  group txn by new { txn.DOCInt } into grp
                                  select new
                                  {
                                      Amount = grp.Sum(x => x.Amount),
                                  }).FirstOrDefault();

                var CMSSuccess = (from txn in mainList
                                  where txn.ServiceID == (int)ServiceCode.CMS
                                  && txn.Status == StatusType.Success
                                  group txn by new { txn.DOCInt } into grp
                                  select new
                                  {
                                      Amount = grp.Sum(x => x.Amount),
                                  }).FirstOrDefault();

                decimal PGCommission = new PaymentGatewayDAL().SelectPGCommission(dateInt);

                DashboardViewModel report = new DashboardViewModel();

                report.TotalCount = totalList?.Count ?? 0;
                report.TotalAmount = totalList?.Amount ?? 0;

                report.PastFail = pastFailAmount ?? 0;
                report.PastSuccess = pastSuccessAmount ?? 0;

                report.TotalSuccessCount = TotalSuccessList?.Count ?? 0;
                report.TotalSuccessAmount = TotalSuccessList?.Amount ?? 0;

                report.TotalAdminCommission = (CommissionWithSurchargeListWithAdmin?.AdminCommission ?? 0);
                report.TotalAdminSurcharge = CommissionWithSurchargeListWithAdmin?.AdminSurcharge ?? 0;

                report.TotalFailedCount = TotalFailList?.Count ?? 0;
                report.TotalFailedAmount = TotalFailList?.Amount ?? 0;
                report.TotalProcessAmount = TotalPendingList?.Amount ?? 0;

                report.PrePaidSuccess = PrepaidSucess?.Amount ?? 0;
                report.PrePaidFail = PrepaidFail?.Amount ?? 0;

                report.PostPaidSuccess = PostPaidSucess?.Amount ?? 0;
                report.PostPaidFail = PostPaidFail?.Amount ?? 0;

                report.DthSuccess = DthSucess?.Amount ?? 0;
                report.DthFail = DthFail?.Amount ?? 0;

                report.DMTSuccess = DMTSucess?.Amount ?? 0;
                report.DMTFail = DMTFail?.Amount ?? 0;

                report.AEPSSuccess = AEPSSuccess?.Amount ?? 0;
                report.AEPSFail = AEPSFail?.Amount ?? 0;
                report.MATMSuccess = MATMSuccess?.Amount ?? 0;
                report.POSSuccess = POSSuccess?.Amount ?? 0;
                report.CMSSuccess = CMSSuccess?.Amount ?? 0;

                report.RofferComm = rofferViewModel?.RofferComm ?? 0;
                report.RofferTotal = rofferViewModel?.RofferTotal ?? 0;
                report.RofferPer = rofferViewModel?.RofferPer ?? "0";

                //beow 2 line for today summary only
                report.TotalCommission = (totCommssionSurchageTDS?.RetailerCommission ?? 0) + (totCommssionSurchageTDS?.DistributerCommission ?? 0) + (totCommssionSurchageTDS?.SuperDistributerCommission ?? 0) + PGCommission;
                report.Surcharge = (totCommssionSurchageTDS?.RetailerSurcharge ?? 0) + (totCommssionSurchageTDS?.DistributerSurcharge ?? 0) + (totCommssionSurchageTDS?.SuperDistributerSurcharge ?? 0);
                report.TotalTDS = (totCommssionSurchageTDS?.RetailerTDS ?? 0) + (totCommssionSurchageTDS?.DistributerTDS ?? 0) + (totCommssionSurchageTDS?.SuperDistributerTDS ?? 0);

                var ProfitCommission = (CommissionWithSurchargeListWithAdmin?.RetailerCommission ?? 0) + (CommissionWithSurchargeListWithAdmin?.DistributerCommission ?? 0) + (CommissionWithSurchargeListWithAdmin?.SuperDistributerCommission ?? 0) + PGCommission;
                var ProfitSurcharge = (CommissionWithSurchargeListWithAdmin?.RetailerSurcharge ?? 0) + (CommissionWithSurchargeListWithAdmin?.DistributerSurcharge ?? 0) + (CommissionWithSurchargeListWithAdmin?.SuperDistributerSurcharge ?? 0);

                report.AdminCommissionProfit = report.TotalAdminCommission - ProfitCommission;
                report.AdminSurchargeProfit = report.TotalAdminSurcharge - ProfitSurcharge;

                await Task.Delay(1);

                return report;
            }
        }

        internal List<SaleViewModel> ServicewiseSale(Requests.TransactionReport.SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                select new
                                {
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    AdminCommission = txn.AdminCommission,
                                }).ToList();

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    if (UserTypeID == (int)UserType.Distributer)
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID == UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID == UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainList = (from item in mainList
                                    where item.UserID == UserID
                                    select item).ToList();
                    }
                }

                if (UserTypeID == (int)UserType.Admin && selectRequest.UserID != 0)
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);

                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID == selectRequest.UserID
                                    select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID == selectRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainList = (from item in mainList
                                    where item.UserID == selectRequest.UserID
                                    select item).ToList();
                    }
                }

                var grouplist = (from txn in mainList
                                 group txn by new { txn.ServiceID } into grp
                                 select new
                                 {
                                     ServiceID = grp.Key.ServiceID,
                                     SucessCount = grp.Where(q => q.Status == StatusType.Success).Count(),
                                     FailCount = grp.Where(q => q.Status == StatusType.Failure).Count(),
                                     PendingCount = grp.Where(q => q.Status == StatusType.Process).Count(),
                                     SucessAMT = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.Amount),
                                     FailAMT = grp.Where(q => q.Status == StatusType.Failure).Sum(X => X.Amount),
                                     PendingAMT = grp.Where(q => q.Status == StatusType.Process).Sum(X => X.Amount),
                                     RCommission = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.RetailerCommission),
                                     DCommission = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.DistributerCommission),
                                     SCommission = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.SuperDistributerCommission),
                                     ACommission = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.AdminCommission)
                                 }).ToList();

                List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();
                var response = (from Service in Servicelist
                                join txn in grouplist on Service.ServiceID equals txn.ServiceID into groupdetails
                                from gpL in groupdetails.DefaultIfEmpty()
                                orderby gpL?.SucessAMT descending
                                select new SaleViewModel
                                {
                                    ServiceName = Service.Name,
                                    SuccessAmt = gpL?.SucessAMT ?? 0,
                                    FailAmt = gpL?.FailAMT ?? 0,
                                    PendingAmount = gpL?.PendingAMT ?? 0,
                                    SuccessCount = gpL?.SucessCount ?? 0,
                                    FailCount = gpL?.FailCount ?? 0,
                                    PendingCount = gpL?.PendingCount ?? 0,
                                    RCommission = gpL?.RCommission ?? 0,
                                    DCommission = gpL?.DCommission ?? 0,
                                    SCommission = gpL?.SCommission ?? 0,
                                    ACommission = gpL?.ACommission ?? 0,
                                }).ToList();

                return response;
            }
        }

        internal List<ProviderSaleViewModel> ProviderSale(Requests.TransactionReport.ProviderSaleRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<ProviderSaleViewModel> responce = null;
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var txnList = (from txn in context.TransactionMaster
                               where txn.DOCInt >= From
                               && txn.DOCInt <= To
                               && txn.Status == StatusType.Success
                               && (selectRequest.UserID == 0 ? 2 > 1 : txn.UserID == selectRequest.UserID)
                               orderby txn.TransactionMasterID
                               select txn).ToList();

                var mainList = (from txn in txnList
                                join provider in providerList on
                                 txn.ProviderID equals provider.ProviderID
                                select new
                                {
                                    ProviderID = txn.ProviderID,
                                    ServiceID = txn.ServiceID,
                                    ProviderName = provider.ProviderName,
                                    ProviderOpeningBalance = txn.ProviderOpeningBalance,
                                    ProviderClosingBalance = txn.ProviderClosingBalance,
                                    AdminCommission = txn.AdminCommission,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    DOCInt = txn.DOCInt
                                }).ToList();

                if (mainList == null)
                    return responce;
                if (selectRequest.ProviderID != 0)
                {
                    mainList = (from item in mainList
                                where item.ProviderID == selectRequest.ProviderID
                                select item).ToList();
                }
                if (selectRequest.ServiceID != 0)
                {
                    mainList = (from item in mainList
                                where item.ServiceID == selectRequest.ServiceID
                                select item).ToList();
                }
                var grouplist = (from txn in mainList
                                 group txn by new { txn.ProviderID, txn.ProviderName } into grp
                                 select new
                                 {
                                     ProviderID = grp.Key.ProviderID,
                                     ProviderName = grp.Key.ProviderName,
                                     ProviderOpeningBalance = grp.FirstOrDefault().ProviderClosingBalance,
                                     ProviderClosingBalance = grp.LastOrDefault().ProviderClosingBalance,
                                     SucessAMT = grp.Sum(X => X.Amount),
                                     AdminCommission = grp.Sum(X => X.AdminCommission),
                                     RCommission = grp.Sum(X => X.RetailerCommission),
                                     DCommission = grp.Sum(X => X.DistributerCommission),
                                     SCommission = grp.Sum(X => X.SuperDistributerCommission),
                                 }).ToList();

                List<Response.ProviderPurchase.PurchaseViewModel> Purchaselist = new ProviderPurchaseDal().SelectPurchase(selectRequest);
                var response = (from txn in grouplist
                                join purchase in Purchaselist on txn.ProviderID equals purchase.ProviderID
                                into leftPurchase
                                from LP in leftPurchase.DefaultIfEmpty()
                                select new ProviderSaleViewModel
                                {
                                    ProvderName = txn.ProviderName,
                                    ProviderID = txn.ProviderID,
                                    Purchase = (LP?.Purchase ?? 0),
                                    Sale = (txn?.SucessAMT ?? 0),
                                    RetailerCommission = txn?.RCommission ?? 0,
                                    DistributerCommission = txn?.DCommission ?? 0,
                                    SuperDistributerCommission = txn?.SCommission ?? 0,
                                    OpeningBalance = txn?.ProviderOpeningBalance ?? 0,
                                    AdminCommission = txn.AdminCommission,
                                    ClosingBalance = txn?.ProviderClosingBalance ?? 0,
                                    Difference = ((txn?.ProviderOpeningBalance ?? 0) + (LP?.Purchase ?? 0)) - (txn?.SucessAMT ?? 0),
                                    Profit = txn.AdminCommission - (txn?.RCommission ?? 0) - (txn?.DCommission ?? 0) - (txn?.SCommission ?? 0)
                                }).ToList();

                return response;
            }
        }

        internal List<LapuViewModel> LapuwiseReport(LapuReportRequest lapuReportRequest)
        {
            int[] ids = new int[] { (int)ServiceCode.Mobile, (int)ServiceCode.Dth, (int)ServiceCode.PostPaid };
            int From = Convert.ToInt32(Convert.ToDateTime(lapuReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(lapuReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<LapuViewModel> list = new List<LapuViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from txn in context.TransactionMaster
                        join service in context.ProviderMaster on
                         txn.ProviderID equals service.ProviderID
                        where txn.DOCInt >= From
                      && txn.DOCInt <= To
                      && ids.Contains(txn.ServiceID)
                      /*&& txn.LapuNo != Messages.MatcherParamNotFound
                      && txn.LapuNo.Length >= 10*/

                        orderby txn.DOC descending
                        select new LapuViewModel
                        {
                            ProviderID = txn.ProviderID,
                            FirmName = txn.UserMaster.FirmName.ConcatNameMobileAliasName(txn.UserMaster.MobileNumber, txn.UserMaster.AliasName),
                            //LapuNumber = txn.LapuNo,
                            ServiceID = txn.ServiceID,
                            OperatorID = txn.OperatorID,
                            OperatorName = txn.OperatorMaster.OperatorName,
                            ConsumerNumber = txn.ConsumerNumber,
                            Amount = txn.Amount,
                            Status = txn.Status,
                            ClosingBalance = txn.ProviderClosingBalance,
                            Commission = txn.ProviderCommission,
                            ReferenceNo = txn.ReferenceNo,
                            ProviderName = service.ProviderName,
                            DOC = txn.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();
                if (lapuReportRequest.ServiceID != 0)
                {
                    list = (from item in list
                            where item.ServiceID == lapuReportRequest.ServiceID
                            select item).ToList();
                }

                if (lapuReportRequest.OperatorID != 0)
                {
                    list = (from item in list
                            where item.OperatorID == lapuReportRequest.OperatorID
                            select item).ToList();
                }

                if (lapuReportRequest.ProviderID != 0)
                {
                    list = (from item in list
                            where item.ProviderID == lapuReportRequest.ProviderID
                            select item).ToList();
                }

                return list;
            }
        }

        internal Response.TransactionMaster.HeaderViewModel GetHeaderSummaryData(LapuReportRequest lapuReportRequest)
        {
            List<LapuViewModel> list = LapuwiseReport(lapuReportRequest);
            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            var Comm = (from item in list
                        where item.Status == StatusType.Success
                        group item by new { item.Status } into grp
                        select new
                        {
                            CommissionAmount = grp.Sum(X => X.Commission),
                        }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            headerlist.Commission = Comm?.CommissionAmount ?? 0;

            return headerlist;
        }

        internal List<ProviderFailureViewModel> ProviderFailure(ProviderFailureRequest providerFailureRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(providerFailureRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(providerFailureRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ProviderFailureViewModel> list = new List<ProviderFailureViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var MainList = (from txn in context.TransactionMaster
                                join provider in context.ProviderMaster on
                                txn.ProviderID equals provider.ProviderID
                                where txn.DOCInt >= From
                                && txn.DOCInt <= To
                                && txn.Status == StatusType.Failure
                                select new
                                {
                                    ProviderID = txn.ProviderID,
                                    ProviderName = provider.ProviderName,
                                    FailuerMessage = txn.Description,
                                    Amount = txn.Amount,
                                    DOC = txn.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                }).ToList();
                if (providerFailureRequest.ProviderID != 0)
                {
                    MainList = (from item in MainList
                                where item.ProviderID == providerFailureRequest.ProviderID
                                select item).ToList();
                }

                var Totallist = (from item in MainList
                                 group item by new { item.ProviderID, item.ProviderName, item.FailuerMessage } into grp
                                 select new
                                 {
                                     ProviderID = grp.Key.ProviderID,
                                     ProviderName = grp.Key.ProviderName,
                                     FailuerMessage = grp.Key.FailuerMessage,
                                     Amount = grp.Sum(x => x.Amount),
                                     Count = grp.Count()
                                 }).ToList();

                list = (from item in Totallist
                        select new ProviderFailureViewModel
                        {
                            ProviderID = item.ProviderID,
                            ProviderName = item.ProviderName,
                            FailuerMessage = item.FailuerMessage,
                            Amount = item.Amount,
                            Count = item.Count,
                        }).ToList();

                return list;
            }
        }

        internal List<GSTSummaryViewModel> GSTsummary(GSTSummaryRequest selectRequest)

        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<GSTSummaryViewModel> list = new List<GSTSummaryViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var MainList = (from item in context.TransactionMaster
                                where item.DOCInt >= From && item.DOCInt <= To && item.Status == StatusType.Success
                                group item by new { item.UserID } into grp
                                select new
                                {
                                    UserID = grp.Key.UserID,
                                    Sale = grp.Sum(x => x.Amount),
                                }).ToList();

                List<UserMaster> usersList = new UserDAL().Select();
                usersList = usersList.Where(u => u.IsInvoiceApplicable == true && u.GSTNo.Length > 3).ToList();
                List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
                List<StateMaster> statelist = new StateDAL().Select();

                InvoiceSetting settings = new InvoiceSettingDal().Select();
                list = (from main in MainList
                        join user in usersList on main.UserID equals user.UserID
                        join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                        join state in statelist on user.StateID equals state.ID

                        select new GSTSummaryViewModel
                        {
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            FirmName = user.FirmName,
                            UserType = usertype.UserType,
                            GSTNo = user.GSTNo,
                            InvoiceNo = new InvoiceProcessing().GetInvoiceNumber(settings.CompanyPrefix, main.UserID, FromDate.Year, FromDate.Month, "USER"),
                            InvoiceType = "R",
                            State = state.Name,
                            ReverseCharge = 0,
                            Rate = 18,
                            InvoiceValue = main.Sale,
                            TaxableValue = (main.Sale * 100 / 118),
                            IGST = (settings.State != state.Name) ? ((main.Sale * 100 / 118) * 9 / 100) : 0,
                            CGST = ((main.Sale * 100 / 118) * 9 / 100),
                            SGST = (settings.State == state.Name) ? ((main.Sale * 100 / 118) * 9 / 100) : 0,
                        }
                     ).ToList();
            }
            return list;
        }

        internal async Task<DashboardViewModel> SelectForRetailer(int UserID, string Date)
        {
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            int[] ids = new int[] { (int)ServiceCode.Mobile, (int)ServiceCode.Dth, (int)ServiceCode.PostPaid, (int)ServiceCode.AEPS };
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt == dateInt
                                && txn.UserID == UserID
                                select new
                                {
                                    TransactionMasterID = txn.TransactionMasterID,
                                    Status = txn.Status,
                                    WalletID = txn.WalletID,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    DOCInt = txn.DOCInt,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                }).ToList();

                //"yyyyMMdd"
                var oldData = (from txn in context.TransactionMaster
                               where txn.UserID == UserID
                               where
                               txn.UpdatedDOC.Date == DateTime.Now.Date &&
                               txn.DOCInt == dateInt
                               select new
                               {
                                   Status = txn.Status,
                                   WalletID = txn.WalletID,
                                   Amount = txn.DebitAmount,
                                   ConsumerNumber = txn.ConsumerNumber,
                                   DOCInt = txn.DOCInt,
                                   UpdatedDOCInt = Convert.ToInt32(Convert.ToDateTime(txn.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                               }).ToList();

                decimal? pastFailAmount = (from txn in oldData
                                           where txn.UpdatedDOCInt - txn.DOCInt >= 1 && txn.Status == StatusType.Failure
                                           select txn).Sum(o => o.Amount);

                decimal? pastSuccessAmount = (from txn in oldData
                                              where txn.UpdatedDOCInt - txn.DOCInt >= 1 && txn.Status == StatusType.Success
                                              select txn).Sum(o => o.Amount);

                var totalList = (from txn in mainList
                                 group txn by new { txn.DOCInt } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

                var TotalSuccessList = (from txn in mainList
                                        where txn.Status == StatusType.Success
                                        group txn by new { txn.DOCInt } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                var TotalFailList = (from txn in mainList
                                     where txn.Status == StatusType.Failure
                                     group txn by new { txn.DOCInt } into grp
                                     select new
                                     {
                                         Count = grp.Count(),
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();
                var TotalPendingList = (from txn in mainList
                                        where txn.Status == StatusType.Process
                                        group txn by new { txn.DOCInt } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                var AEPSSuccess = (from txn in mainList
                                   where txn.ServiceID == (int)ServiceCode.AEPS
                                   && txn.Status == StatusType.Success
                                   group txn by new { txn.DOCInt } into grp
                                   select new
                                   {
                                       Amount = grp.Sum(x => x.Amount),
                                   }).FirstOrDefault();

                var totalsurcharge = (from txn in mainList
                                      where txn.Status == StatusType.Success
                                        && txn.RetailerCommission <= 0
                                        && txn.DistributerCommission <= 0
                                        && txn.SuperDistributerCommission <= 0
                                      group txn by new { txn.DOCInt } into grp
                                      select new
                                      {
                                          RetailerCommission = grp.Sum(x => Math.Abs(x.RetailerCommission)),
                                          DistributerCommission = grp.Sum(x => Math.Abs(x.DistributerCommission)),
                                          SuperDistributerCommission = grp.Sum(x => Math.Abs(x.SuperDistributerCommission)),
                                      }).FirstOrDefault();

                DashboardViewModel report = new DashboardViewModel();

                report.TotalCount = totalList?.Count ?? 0;
                report.TotalAmount = totalList?.Amount ?? 0;

                report.PastFail = pastFailAmount ?? 0;
                report.PastSuccess = pastSuccessAmount ?? 0;

                report.TotalSuccessCount = TotalSuccessList?.Count ?? 0;
                report.TotalSuccessAmount = TotalSuccessList?.Amount ?? 0;
                report.AEPSSuccess = AEPSSuccess?.Amount ?? 0;

                report.TotalFailedCount = TotalFailList?.Count ?? 0;
                report.TotalFailedAmount = TotalFailList?.Amount ?? 0;

                report.TotalProcessAmount = TotalPendingList?.Amount ?? 0;
                var TotalRetailerCommission = mainList.Where(t => t.Status == StatusType.Success && t.RetailerCommission > 0).Sum(i => i.RetailerCommission);

                var TotalRetailersurcharge = mainList.Where(t => t.Status == StatusType.Success && t.RetailerCommission <= 0).Sum(x => Math.Abs(x.RetailerCommission));

                report.TotalCommission = TotalRetailerCommission;
                report.Surcharge = TotalRetailersurcharge;

                await Task.Delay(1);

                return report;
            }
        }

        internal async Task<DashboardViewModel> SelectForDistributorDashborad(int UserID, string Date)
        {
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            int[] ids = new int[] { (int)ServiceCode.Mobile, (int)ServiceCode.Dth, (int)ServiceCode.PostPaid, (int)ServiceCode.AEPS };

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt == dateInt
                                select new
                                {
                                    TransactionMasterID = txn.TransactionMasterID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                    DOCInt = txn.DOCInt,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID
                                }).ToList();

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    if (UserTypeID == (int)UserType.Distributer)
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID == UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID == UserID
                                    select item).ToList();
                    }
                }
                var TotalSuccessList = (from txn in mainList
                                        where txn.Status == StatusType.Success
                                        group txn by new { txn.DOCInt } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                var TotalFailList = (from txn in mainList
                                     where txn.Status == StatusType.Failure
                                     group txn by new { txn.DOCInt } into grp
                                     select new
                                     {
                                         Count = grp.Count(),
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();

                DashboardViewModel report = new DashboardViewModel();
                report.TotalSuccessCount = TotalSuccessList?.Count ?? 0;
                report.TotalSuccessAmount = TotalSuccessList?.Amount ?? 0;

                report.TotalFailedCount = TotalFailList?.Count ?? 0;
                report.TotalFailedAmount = TotalFailList?.Amount ?? 0;

                var Commission = 0m;
                var surcharge = 0m;
                var PGCommission = 0m;

                PGCommission = new PaymentGatewayDAL().SelectPGCommissionforUpline(UserTypeID, dateInt);
                if (UserTypeID == (int)UserType.Distributer)
                {
                    Commission = mainList.Where(t => t.Status == StatusType.Success && t.DistributerCommission > 0).Sum(i => i.DistributerCommission);
                    surcharge = mainList.Where(t => t.Status == StatusType.Success && t.DistributerCommission <= 0).Sum(x => Math.Abs(x.DistributerCommission));
                }
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    Commission = mainList.Where(t => t.Status == StatusType.Success && t.SuperDistributerCommission > 0).Sum(i => i.SuperDistributerCommission);
                    surcharge = mainList.Where(t => t.Status == StatusType.Success && t.SuperDistributerCommission <= 0).Sum(x => Math.Abs(x.SuperDistributerCommission));
                }
                report.TotalCommission = Commission + PGCommission;
                report.Surcharge = surcharge;

                await Task.Delay(1);

                return report;
            }
        }

        private bool isValidUserTypeForTransactionRepor(int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Admin ||
                UserTypeID == (int)UserType.SuperDistributer ||
                UserTypeID == (int)UserType.Distributer ||
                UserTypeID == (int)UserType.Retailer ||
                UserTypeID == (int)UserType.APIUser)
            {
                return true;
            }

            return false;
        }

        private List<FundSenderViewModel> FilterUserWiseFundSenderForUserType(List<FundSenderViewModel> mainList, int UserTypeID, int UserID)
        {
            if (UserID <= 0)
                return mainList;

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                mainList = (from item in mainList
                            where (item.SuperDistID == UserID || item.ParentID == UserID)
                            select item).ToList();

                return mainList;
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                mainList = (from item in mainList
                            where item.ParentID == UserID
                            select item).ToList();

                return mainList;
            }

            if (UserTypeID == 0 && UserID == 0)
            {
                mainList = (from item in mainList
                            select item).ToList();
                return mainList;
            }

            mainList = (from item in mainList
                        where item.SenderID == UserID
                        select item).ToList();

            return mainList;
        }

        private List<FundReceiverViewModel> FilterUserWiseFundreceiverForUserType(List<FundReceiverViewModel> mainList, int UserTypeID, int UserID)
        {
            if (UserID <= 0)
                return mainList;

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                mainList = (from item in mainList
                            where (item.SuperDistID == UserID || item.ParentID == UserID)
                            select item).ToList();

                return mainList;
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                mainList = (from item in mainList
                            where item.ParentID == UserID
                            select item).ToList();

                return mainList;
            }

            if (UserTypeID == 0 && UserID == 0)
            {
                mainList = (from item in mainList
                            select item).ToList();
                return mainList;
            }

            mainList = (from item in mainList
                        where item.ReceiverID == UserID
                        select item).ToList();

            return mainList;
        }

        internal List<UserSaleViewModel> UserwiseSaleAndExcelDownload(UserSaleSelectRequest selectRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int AddDayInt = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).AddDays(1).ToString(Messages.DOCIntegerFomrat));

            bool isValid = isValidUserTypeForTransactionRepor(UserID);
            if (isValid == false)
            {
                return null;
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                selectRequest.UserTypeID = UserTypeID;
                selectRequest.UserID = UserID;
            }

            if (selectRequest.UserTypeID == 0 && selectRequest.UserID == 0)
            {
                List<UserSaleViewModel> res = UserwiseSaleOnlyUserTypeIDAndUserID(selectRequest, FromDate, ToDate, AddDayInt);
                return res;
            }
            else if (selectRequest.UserTypeID != 0 && selectRequest.UserID == 0)
            {
                List<UserSaleViewModel> res = UserwiseSaleOnlyUserTypeID(selectRequest, FromDate, ToDate, AddDayInt);
                return res;
            }
            else
            {
                using var context = new DatabaseContext();

                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var TransList = (from txn in context.TransactionMaster
                                 where txn.DOCInt >= FromDate
                                && txn.DOCInt <= ToDate
                                 && txn.WalletID == selectRequest.WalletID
                                 select txn).ToList();

                List<UserWiseSaleViewModel> AllmainList = new List<UserWiseSaleViewModel>();
                if (selectRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    AllmainList = (from txn in TransList
                                   where txn.SuperDistributerUserID == selectRequest.UserID
                                   select new UserWiseSaleViewModel
                                   {
                                       WalletID = txn.WalletID,
                                       UserID = txn.SuperDistributerUserID,
                                       Status = txn.Status,
                                       Amount = txn.Amount,
                                       RetailerCommission = txn.RetailerCommission,
                                       DistributerCommission = txn.DistributerCommission,
                                       SuperDistributerCommission = txn.SuperDistributerCommission,
                                       SuperDistributerUserID = txn.SuperDistributerUserID,
                                       DistributerUserID = txn.DistributerUserID,
                                       Surcharge = txn.SuperDistributerCommission
                                   }).ToList();
                }
                else if (selectRequest.UserTypeID == (int)UserType.Distributer)
                {
                    AllmainList = (from txn in TransList
                                   where txn.DistributerUserID == selectRequest.UserID
                                   select new UserWiseSaleViewModel
                                   {
                                       WalletID = txn.WalletID,
                                       UserID = txn.DistributerUserID,
                                       Status = txn.Status,
                                       Amount = txn.Amount,
                                       RetailerCommission = txn.RetailerCommission,
                                       DistributerCommission = txn.DistributerCommission,
                                       SuperDistributerCommission = txn.SuperDistributerCommission,
                                       SuperDistributerUserID = txn.SuperDistributerUserID,
                                       DistributerUserID = txn.DistributerUserID,
                                       Surcharge = txn.DistributerCommission
                                   }).ToList();
                }
                else
                {
                    AllmainList = (from txn in TransList
                                   where txn.UserID == selectRequest.UserID
                                   select new UserWiseSaleViewModel
                                   {
                                       WalletID = txn.WalletID,
                                       UserID = txn.UserID,
                                       Status = txn.Status,
                                       Amount = txn.Amount,
                                       RetailerCommission = txn.RetailerCommission,
                                       DistributerCommission = txn.DistributerCommission,
                                       SuperDistributerCommission = txn.SuperDistributerCommission,
                                       SuperDistributerUserID = txn.SuperDistributerUserID,
                                       DistributerUserID = txn.DistributerUserID,
                                       Surcharge = txn.RetailerCommission
                                   }).ToList();
                }
                List<UserWiseSaleViewModel> mainList = (from txn in AllmainList
                                                        where txn.Status == StatusType.Success
                                                        select txn).ToList();

                var Grouplist = (from txn in mainList
                                 group txn by new { txn.WalletID, txn.UserID } into grp
                                 select new
                                 {
                                     WalletID = grp.Key.WalletID,
                                     UserID = grp.Key.UserID,
                                     Sale = grp.Sum(X => X.Amount),
                                     SuccessCount = grp.Count(),
                                     RetailerCommission = grp.Where(q => q.RetailerCommission > 0).Sum(X => X.RetailerCommission),
                                     DistributerCommission = grp.Where(q => q.DistributerCommission > 0).Sum(X => X.DistributerCommission),
                                     SuperDistributerCommission = grp.Where(q => q.DistributerCommission > 0).Sum(X => X.DistributerCommission),
                                     Surcharge = grp.Where(q => q.Surcharge < 0).Sum(X => X.Surcharge),
                                 }).ToList();

                List<UserWiseSaleViewModel> FailmainList = (from txn in AllmainList
                                                            where txn.Status == StatusType.Failure
                                                            select txn).ToList();
                var GroupFailList = (from txn in FailmainList
                                     group txn by new { txn.WalletID, txn.UserID } into grp
                                     select new
                                     {
                                         WalletID = grp.Key.WalletID,
                                         UserID = grp.Key.UserID,
                                         TotalFail = grp.Sum(X => X.Amount),
                                         FailCount = grp.Count(),
                                     }).ToList();
                List<UserMaster> usersList = new UserDAL().Select().Where(U => U.UserID == UserID).ToList();

                var FundSenderReceiverList = (from txn in context.FundTransaction
                                              where txn.DOCInt >= FromDate
                                               && txn.DOCInt <= ToDate
                                              && txn.WalletID == selectRequest.WalletID
                                              select txn).ToList();

                List<FundSenderViewModel> mainFundSenderList = (from fund in FundSenderReceiverList
                                                                join user in usersList on fund.SenderID equals user.UserID

                                                                select new FundSenderViewModel
                                                                {
                                                                    DOCInt = fund.DOCInt,
                                                                    Amount = fund.Amount,
                                                                    SenderID = fund.SenderID,
                                                                    WalletID = fund.WalletID,
                                                                    ParentID = user.ParentID,
                                                                    UserTypeID = user.UserTypeID,
                                                                    Through = fund.Through,
                                                                }).ToList();

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

                var SenderfundList = (from fund in mainFundSenderList

                                      group fund by new { fund.SenderID } into grp
                                      select new
                                      {
                                          SenderID = grp.Key.SenderID,
                                          TransferAmount = grp.Where(q => q.Through == FundTransferType.ThroughRequestTransfer
                                             || q.Through == FundTransferType.ThroughDirectTransfer
                                             || q.Through == FundTransferType.PGTransfer).Sum(X => X.Amount),

                                          ReverseAmountDr = grp.Where(q => q.SenderID == grp.Key.SenderID
                                             && (q.Through == FundTransferType.ThroughDirectReverse
                                         || q.Through == FundTransferType.ThroughReverseRequestTransfer)).Sum(X => X.Amount),
                                      }).ToList();

                var ReceiverFundList = (from fund in mainFundReceiverList
                                        group fund by new { fund.ReceiverID } into grp
                                        select new
                                        {
                                            ReceiverID = grp.Key.ReceiverID,
                                            ReceiverAmount = grp.Where(q => q.Through == FundTransferType.ThroughDirectTransfer
                                                  || q.Through == FundTransferType.ThroughRequestTransfer
                                                  || q.Through == FundTransferType.PGTransfer).Sum(X => X?.Amount ?? 0),
                                            ReverseAmountCr = grp.Where(q => q.Through == FundTransferType.ThroughDirectReverse
                                          || q.Through == FundTransferType.ThroughReverseRequestTransfer).Sum(X => X.Amount),
                                        }).ToList();

                List<WalletMaster> walletslist = new WalletMasterDal().Select();
                List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

                List<UserSaleViewModel> response = new List<UserSaleViewModel>();
                response = (from User in usersList
                            join txn in Grouplist on User.UserID equals txn.UserID into lefttxn
                            from LT in lefttxn.DefaultIfEmpty()
                            join txnfail in GroupFailList on User.UserID equals txnfail.UserID into lefttxnfail
                            from LTFail in lefttxnfail.DefaultIfEmpty()
                            join Sender in SenderfundList on User.UserID equals Sender.SenderID into leftSender
                            from LS in leftSender.DefaultIfEmpty()
                            join Receiver in ReceiverFundList on User.UserID equals Receiver.ReceiverID into leftReceiver
                            from LR in leftReceiver.DefaultIfEmpty()
                            join wallet in walletslist on selectRequest.WalletID equals wallet.WalletID
                            join role in listUserType on User.UserTypeID equals role.UserTypeID
                            where User.UserTypeID == (int)UserType.Retailer || User.UserTypeID == (int)UserType.APIUser ||
                            User.UserTypeID == (int)UserType.Client
                           || User.UserTypeID == (int)UserType.Distributer || User.UserTypeID == (int)UserType.SuperDistributer
                            select new UserSaleViewModel
                            {
                                FirmName = User.FirmName.ConcatNameAliasName(User.AliasName),
                                UserMobile = User.MobileNumber,
                                ParentID = User.ParentID,
                                WalletName = wallet.Name,
                                OpeningBal = 0,
                                Sale = LT?.Sale ?? 0,
                                SuccessCount = LT?.SuccessCount ?? 0,
                                FailCount = LTFail?.FailCount ?? 0,
                                RetailerCommission = LT?.RetailerCommission ?? 0,
                                DistributerCommission = LT?.DistributerCommission ?? 0,
                                SuperDistributerCommission = LT?.SuperDistributerCommission ?? 0,
                                Surcharge = Math.Abs(LT?.Surcharge ?? 0),
                                Role = role.UserType,
                                UserID = User.UserID,
                                TransferAmount = LS?.TransferAmount ?? 0,
                                ReverseAmountDr = LS?.ReverseAmountDr ?? 0,
                                ReverseAmountCr = LR?.ReverseAmountCr ?? 0,
                                Purchase = LR?.ReceiverAmount ?? 0,
                            }).ToList();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(response, parallelOptions, res =>
                {
                    res.OpeningBal = new UserBalanceHistoryDAL().SelectOpeningBalance(res.UserID, selectRequest.WalletID, FromDate);
                    res.ClosingBalance = new UserWalletMasterDal().SelectClosingBalance(res.UserID, selectRequest.WalletID, ToDate, AddDayInt);
                });
                return response;
            }
        }

        public List<UserMaster> SelectDownlinelist(List<UserMaster> usersList, int UserTypeID, int UserID)
        {
            if (UserTypeID == (int)UserType.Distributer)
            {
                usersList = usersList.Where(U => U.ParentID == UserID).ToList();
            }
            else if (UserTypeID == (int)UserType.SuperDistributer)
            {
                List<UserMaster> distrubutorList = usersList.Where(U => U.ParentID == UserID).ToList();
                foreach (UserMaster distItem in distrubutorList)
                {
                    usersList.Add(distItem);
                    List<UserMaster> list = usersList.Where(U => U.ParentID == distItem.UserID).ToList();
                    if (list != null && list.Count >= 1)
                    {
                        usersList.AddRange(list);
                    }
                }
            }
            else
            {
                usersList = usersList.Where(U => U.UserID == UserID).ToList();
            }
            return usersList;
        }

        internal List<UserSaleViewModel> UserwiseSale(UserSaleSelectRequest selectRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int AddDayInt = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).AddDays(1).ToString(Messages.DOCIntegerFomrat));

            bool isValid = isValidUserTypeForTransactionRepor(UserID);
            if (isValid == false)
            {
                return null;
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                selectRequest.UserTypeID = UserTypeID;
                selectRequest.UserID = UserID;
            }

            if (selectRequest.UserTypeID == 0 && selectRequest.UserID == 0)
            {
                List<UserSaleViewModel> res = UserwiseSaleOnlyUserTypeIDAndUserID(selectRequest, FromDate, ToDate, AddDayInt);
                return res;
            }
            else if (selectRequest.UserTypeID != 0 && selectRequest.UserID == 0)
            {
                List<UserSaleViewModel> res = UserwiseSaleOnlyUserTypeID(selectRequest, FromDate, ToDate, AddDayInt);
                return res;
            }
            else
            {
                using var context = new DatabaseContext();

                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var TransList = (from txn in context.TransactionMaster
                                 where txn.DOCInt >= FromDate
                                && txn.DOCInt <= ToDate
                                 && txn.WalletID == selectRequest.WalletID
                                 orderby txn.TransactionMasterID descending
                                 select txn).ToList();

                List<UserWiseSaleViewModel> AllmainList = new List<UserWiseSaleViewModel>();
                if (selectRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    AllmainList = (from txn in TransList
                                   where txn.SuperDistributerUserID == selectRequest.UserID
                                   select new UserWiseSaleViewModel
                                   {
                                       WalletID = txn.WalletID,
                                       UserID = txn.SuperDistributerUserID,
                                       Status = txn.Status,
                                       Amount = txn.Amount,
                                       RetailerCommission = txn.RetailerCommission,
                                       DistributerCommission = txn.DistributerCommission,
                                       SuperDistributerCommission = txn.SuperDistributerCommission,
                                       SuperDistributerUserID = txn.SuperDistributerUserID,
                                       DistributerUserID = txn.DistributerUserID,
                                       Surcharge = txn.SuperDistributerCommission
                                   }).ToList();
                }
                else if (selectRequest.UserTypeID == (int)UserType.Distributer)
                {
                    AllmainList = (from txn in TransList
                                   where txn.DistributerUserID == selectRequest.UserID
                                   select new UserWiseSaleViewModel
                                   {
                                       WalletID = txn.WalletID,
                                       UserID = txn.DistributerUserID,
                                       Status = txn.Status,
                                       Amount = txn.Amount,
                                       RetailerCommission = txn.RetailerCommission,
                                       DistributerCommission = txn.DistributerCommission,
                                       SuperDistributerCommission = txn.SuperDistributerCommission,
                                       SuperDistributerUserID = txn.SuperDistributerUserID,
                                       DistributerUserID = txn.DistributerUserID,
                                       Surcharge = txn.DistributerCommission
                                   }).ToList();
                }
                else
                {
                    AllmainList = (from txn in TransList
                                   where txn.UserID == selectRequest.UserID
                                   select new UserWiseSaleViewModel
                                   {
                                       WalletID = txn.WalletID,
                                       UserID = txn.UserID,
                                       Status = txn.Status,
                                       Amount = txn.Amount,
                                       RetailerCommission = txn.RetailerCommission,
                                       DistributerCommission = txn.DistributerCommission,
                                       SuperDistributerCommission = txn.SuperDistributerCommission,
                                       SuperDistributerUserID = txn.SuperDistributerUserID,
                                       DistributerUserID = txn.DistributerUserID,
                                       Surcharge = txn.RetailerCommission
                                   }).ToList();
                }

                List<UserWiseSaleViewModel> mainList = (from txn in AllmainList
                                                        where txn.Status == StatusType.Success
                                                        select txn).ToList();

                var Grouplist = (from txn in mainList
                                 group txn by new { txn.WalletID, txn.UserID } into grp
                                 select new
                                 {
                                     WalletID = grp.Key.WalletID,
                                     UserID = grp.Key.UserID,
                                     Sale = grp.Sum(X => X.Amount),
                                     SuccessCount = grp.Count(),
                                     RetailerCommission = grp.Where(q => q.RetailerCommission > 0).Sum(X => X.RetailerCommission),
                                     DistributerCommission = grp.Where(q => q.DistributerCommission > 0).Sum(X => X.DistributerCommission),
                                     SuperDistributerCommission = grp.Where(q => q.SuperDistributerCommission > 0).Sum(X => X.SuperDistributerCommission),
                                     Surcharge = grp.Where(q => q.Surcharge < 0).Sum(X => X.Surcharge),
                                 }).ToList();

                List<UserWiseSaleViewModel> FailmainList = (from txn in AllmainList
                                                            where txn.Status == StatusType.Failure
                                                            select txn).ToList();
                var GroupFailList = (from txn in FailmainList
                                     group txn by new { txn.WalletID, txn.UserID } into grp
                                     select new
                                     {
                                         WalletID = grp.Key.WalletID,
                                         UserID = grp.Key.UserID,
                                         TotalFail = grp.Sum(X => X.Amount),
                                         FailCount = grp.Count(),
                                     }).ToList();

                List<UserMaster> usersList = new UserDAL().Select().Where(U => U.UserID == selectRequest.UserID).ToList();

                var FundSenderReceiverList = (from txn in context.FundTransaction
                                              where txn.DOCInt >= FromDate
                                               && txn.DOCInt <= ToDate
                                              && txn.WalletID == selectRequest.WalletID
                                              select txn).ToList();

                List<FundSenderViewModel> mainFundSenderList = (from fund in FundSenderReceiverList
                                                                join user in usersList on fund.SenderID equals user.UserID

                                                                select new FundSenderViewModel
                                                                {
                                                                    DOCInt = fund.DOCInt,
                                                                    Amount = fund.Amount,
                                                                    SenderID = fund.SenderID,
                                                                    WalletID = fund.WalletID,
                                                                    ParentID = user.ParentID,
                                                                    UserTypeID = user.UserTypeID,
                                                                    Through = fund.Through,
                                                                }).ToList();

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

                var SenderfundList = (from fund in mainFundSenderList

                                      group fund by new { fund.SenderID } into grp
                                      select new
                                      {
                                          SenderID = grp.Key.SenderID,
                                          TransferAmount = grp.Where(q => q.Through == FundTransferType.ThroughRequestTransfer
                                             || q.Through == FundTransferType.ThroughDirectTransfer
                                             || q.Through == FundTransferType.PGTransfer).Sum(X => X.Amount),

                                          ReverseAmountDr = grp.Where(q => q.SenderID == grp.Key.SenderID
                                             && (q.Through == FundTransferType.ThroughDirectReverse
                                         || q.Through == FundTransferType.ThroughReverseRequestTransfer)).Sum(X => X.Amount),
                                      }).ToList();

                var ReceiverFundList = (from fund in mainFundReceiverList
                                        group fund by new { fund.ReceiverID } into grp
                                        select new
                                        {
                                            ReceiverID = grp.Key.ReceiverID,
                                            ReceiverAmount = grp.Where(q => q.Through == FundTransferType.ThroughDirectTransfer
                                                  || q.Through == FundTransferType.ThroughRequestTransfer
                                                  || q.Through == FundTransferType.PGTransfer).Sum(X => X?.Amount ?? 0),
                                            ReverseAmountCr = grp.Where(q => q.Through == FundTransferType.ThroughDirectReverse
                                          || q.Through == FundTransferType.ThroughReverseRequestTransfer).Sum(X => X.Amount),
                                        }).ToList();

                List<WalletMaster> walletslist = new WalletMasterDal().Select();
                List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

                List<UserSaleViewModel> response = new List<UserSaleViewModel>();
                response = (from User in usersList
                            join txn in Grouplist on User.UserID equals txn.UserID into lefttxn
                            from LT in lefttxn.DefaultIfEmpty()
                            join txnfail in GroupFailList on User.UserID equals txnfail.UserID into lefttxnfail
                            from LTFail in lefttxnfail.DefaultIfEmpty()
                            join Sender in SenderfundList on User.UserID equals Sender.SenderID into leftSender
                            from LS in leftSender.DefaultIfEmpty()
                            join Receiver in ReceiverFundList on User.UserID equals Receiver.ReceiverID into leftReceiver
                            from LR in leftReceiver.DefaultIfEmpty()
                            join wallet in walletslist on selectRequest.WalletID equals wallet.WalletID
                            join role in listUserType on User.UserTypeID equals role.UserTypeID
                            where User.UserTypeID == (int)UserType.Retailer || User.UserTypeID == (int)UserType.APIUser ||
                            User.UserTypeID == (int)UserType.Client
                           || User.UserTypeID == (int)UserType.Distributer || User.UserTypeID == (int)UserType.SuperDistributer
                            select new UserSaleViewModel
                            {
                                FirmName = User.FirmName.ConcatNameAliasName(User.AliasName),
                                UserMobile = User.MobileNumber,
                                ParentID = User.ParentID,
                                WalletName = wallet.Name,
                                OpeningBal = 0,
                                Sale = LT?.Sale ?? 0,
                                SuccessCount = LT?.SuccessCount ?? 0,
                                FailCount = LTFail?.FailCount ?? 0,
                                RetailerCommission = LT?.RetailerCommission ?? 0,
                                DistributerCommission = LT?.DistributerCommission ?? 0,
                                SuperDistributerCommission = LT?.SuperDistributerCommission ?? 0,
                                Surcharge = Math.Abs(LT?.Surcharge ?? 0),
                                Role = role.UserType,
                                UserID = User.UserID,
                                TransferAmount = LS?.TransferAmount ?? 0,
                                ReverseAmountDr = LS?.ReverseAmountDr ?? 0,
                                ReverseAmountCr = LR?.ReverseAmountCr ?? 0,
                                Purchase = LR?.ReceiverAmount ?? 0,
                            }).ToList();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(response, parallelOptions, res =>
                {
                    res.OpeningBal = new UserBalanceHistoryDAL().SelectOpeningBalance(res.UserID, selectRequest.WalletID, FromDate);
                    res.ClosingBalance = new UserWalletMasterDal().SelectClosingBalance(res.UserID, selectRequest.WalletID, ToDate, AddDayInt);
                });
                return response;
            }
        }

        private List<UserSaleViewModel> UserwiseSaleOnlyUserTypeID(UserSaleSelectRequest selectRequest, int FromDate, int ToDate, int AddDayInt)
        {
            using var context = new DatabaseContext();
            List<UserMaster> usersList = new UserDAL().Select();
            // take skip and take users if data for skip and take count
            if (!selectRequest.IsExcelDownload)
            {
                usersList = (from user in usersList
                             where user.UserTypeID == selectRequest.UserTypeID
                             orderby user.UserID
                             select user).Skip(selectRequest.SkipCount).Take(selectRequest.TakeCount).ToList();
            }
            else
            {
                usersList = (from user in usersList
                             where user.UserTypeID == selectRequest.UserTypeID
                             select user).ToList();
            }

            var TransList = (from txn in context.TransactionMaster
                             where txn.DOCInt >= FromDate
                                && txn.DOCInt <= ToDate
                                && txn.WalletID == selectRequest.WalletID
                             select txn).ToList();
            List<UserWiseSaleViewModel> AllmainList = new List<UserWiseSaleViewModel>();
            if (selectRequest.UserTypeID == (int)UserType.SuperDistributer)
            {
                AllmainList = (from txn in TransList
                               join user in usersList on
                               txn.SuperDistributerUserID equals user.UserID
                               select new UserWiseSaleViewModel
                               {
                                   WalletID = txn.WalletID,
                                   UserID = txn.SuperDistributerUserID,
                                   UserTypeID = user.UserTypeID,
                                   Status = txn.Status,
                                   Amount = txn.Amount,
                                   RetailerCommission = txn.RetailerCommission,
                                   DistributerCommission = txn.DistributerCommission,
                                   SuperDistributerCommission = txn.SuperDistributerCommission,
                                   SuperDistributerUserID = txn.SuperDistributerUserID,
                                   DistributerUserID = txn.DistributerUserID,
                                   Surcharge = txn.SuperDistributerCommission
                               }).ToList();
            }
            else if (selectRequest.UserTypeID == (int)UserType.Distributer)
            {
                AllmainList = (from txn in TransList
                               join user in usersList on
                               txn.DistributerUserID equals user.UserID
                               select new UserWiseSaleViewModel
                               {
                                   WalletID = txn.WalletID,
                                   UserID = txn.DistributerUserID,
                                   UserTypeID = user.UserTypeID,
                                   Status = txn.Status,
                                   Amount = txn.Amount,
                                   RetailerCommission = txn.RetailerCommission,
                                   DistributerCommission = txn.DistributerCommission,
                                   SuperDistributerCommission = txn.SuperDistributerCommission,
                                   SuperDistributerUserID = txn.SuperDistributerUserID,
                                   DistributerUserID = txn.DistributerUserID,
                                   Surcharge = txn.DistributerCommission
                               }).ToList();
            }
            else
            {
                AllmainList = (from txn in TransList
                               join user in usersList on txn.UserID equals user.UserID
                               select new UserWiseSaleViewModel
                               {
                                   WalletID = txn.WalletID,
                                   UserID = txn.UserID,
                                   UserTypeID = user.UserTypeID,
                                   Status = txn.Status,
                                   Amount = txn.Amount,
                                   RetailerCommission = txn.RetailerCommission,
                                   DistributerCommission = txn.DistributerCommission,
                                   SuperDistributerCommission = txn.SuperDistributerCommission,
                                   SuperDistributerUserID = txn.SuperDistributerUserID,
                                   DistributerUserID = txn.DistributerUserID,
                                   Surcharge = txn.RetailerCommission
                               }).ToList();
            }

            List<UserWiseSaleViewModel> mainList = (from txn in AllmainList
                                                    where txn.Status == StatusType.Success
                                                    select txn).ToList();

            var Grouplist = (from txn in mainList
                             group txn by new { txn.WalletID, txn.UserID } into grp
                             select new
                             {
                                 WalletID = grp.Key.WalletID,
                                 UserID = grp.Key.UserID,
                                 Sale = grp.Sum(X => X.Amount),
                                 SuccessCount = grp.Count(),
                                 RetailerCommission = grp.Where(q => q.RetailerCommission > 0).Sum(X => X.RetailerCommission),
                                 DistributerCommission = grp.Where(q => q.DistributerCommission > 0).Sum(X => X.DistributerCommission),
                                 SuperDistributerCommission = grp.Where(q => q.SuperDistributerCommission > 0).Sum(X => X.SuperDistributerCommission),
                                 Surcharge = grp.Where(q => q.Surcharge < 0).Sum(X => X.Surcharge),
                             }).ToList();

            List<UserWiseSaleViewModel> FailmainList = (from txn in AllmainList
                                                        where txn.Status == StatusType.Failure
                                                        select txn).ToList();
            var GroupFailList = (from txn in FailmainList
                                 group txn by new { txn.WalletID, txn.UserID } into grp
                                 select new
                                 {
                                     WalletID = grp.Key.WalletID,
                                     UserID = grp.Key.UserID,
                                     TotalFail = grp.Sum(X => X.Amount),
                                     FailCount = grp.Count(),
                                 }).ToList();

            var FundSenderReceiverList = (from txn in context.FundTransaction
                                          where txn.DOCInt >= FromDate
                                           && txn.DOCInt <= ToDate
                                          && txn.WalletID == selectRequest.WalletID
                                          select txn).ToList();

            List<FundSenderViewModel> mainFundSenderList = (from fund in FundSenderReceiverList
                                                            join user in usersList on fund.SenderID equals user.UserID

                                                            select new FundSenderViewModel
                                                            {
                                                                DOCInt = fund.DOCInt,
                                                                Amount = fund.Amount,
                                                                SenderID = fund.SenderID,
                                                                WalletID = fund.WalletID,
                                                                ParentID = user.ParentID,
                                                                UserTypeID = user.UserTypeID,
                                                                Through = fund.Through,
                                                            }).ToList();

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

            var SenderfundList = (from fund in mainFundSenderList

                                  group fund by new { fund.SenderID } into grp
                                  select new
                                  {
                                      SenderID = grp.Key.SenderID,
                                      TransferAmount = grp.Where(q => q.Through == FundTransferType.ThroughRequestTransfer
                                         || q.Through == FundTransferType.ThroughDirectTransfer
                                         || q.Through == FundTransferType.PGTransfer).Sum(X => X.Amount),

                                      ReverseAmountDr = grp.Where(q => q.SenderID == grp.Key.SenderID
                                         && (q.Through == FundTransferType.ThroughDirectReverse
                                     || q.Through == FundTransferType.ThroughReverseRequestTransfer)).Sum(X => X.Amount),
                                  }).ToList();

            var ReceiverFundList = (from fund in mainFundReceiverList
                                    group fund by new { fund.ReceiverID } into grp
                                    select new
                                    {
                                        ReceiverID = grp.Key.ReceiverID,
                                        ReceiverAmount = grp.Where(q => q.Through == FundTransferType.ThroughDirectTransfer
                                              || q.Through == FundTransferType.ThroughRequestTransfer
                                              || q.Through == FundTransferType.PGTransfer).Sum(X => X?.Amount ?? 0),
                                        ReverseAmountCr = grp.Where(q => q.Through == FundTransferType.ThroughDirectReverse
                                      || q.Through == FundTransferType.ThroughReverseRequestTransfer).Sum(X => X.Amount),
                                    }).ToList();

            List<WalletMaster> walletslist = new WalletMasterDal().Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            List<UserSaleViewModel> response = new List<UserSaleViewModel>();

            response = (from User in usersList
                        join txn in Grouplist on User.UserID equals txn.UserID into lefttxn
                        from LT in lefttxn.DefaultIfEmpty()
                        join txnfail in GroupFailList on User.UserID equals txnfail.UserID into lefttxnfail
                        from LTFail in lefttxnfail.DefaultIfEmpty()
                        join Sender in SenderfundList on User.UserID equals Sender.SenderID into leftSender
                        from LS in leftSender.DefaultIfEmpty()
                        join Receiver in ReceiverFundList on User.UserID equals Receiver.ReceiverID into leftReceiver
                        from LR in leftReceiver.DefaultIfEmpty()
                        join wallet in walletslist on selectRequest.WalletID equals wallet.WalletID
                        join role in listUserType on User.UserTypeID equals role.UserTypeID
                        where User.UserTypeID == (int)UserType.Retailer || User.UserTypeID == (int)UserType.APIUser ||
                              User.UserTypeID == (int)UserType.Client || User.UserTypeID == (int)UserType.Distributer
                              || User.UserTypeID == (int)UserType.SuperDistributer
                        select new UserSaleViewModel
                        {
                            FirmName = User.FirmName.ConcatNameAliasName(User.AliasName),
                            UserMobile = User.MobileNumber,
                            ParentID = User.ParentID,
                            WalletName = wallet.Name,
                            OpeningBal = 0,
                            Sale = LT?.Sale ?? 0,
                            SuccessCount = LT?.SuccessCount ?? 0,
                            FailCount = LTFail?.FailCount ?? 0,
                            RetailerCommission = LT?.RetailerCommission ?? 0,
                            DistributerCommission = LT?.DistributerCommission ?? 0,
                            SuperDistributerCommission = LT?.SuperDistributerCommission ?? 0,
                            Surcharge = Math.Abs(LT?.Surcharge ?? 0),
                            Role = role.UserType,
                            UserID = User.UserID,
                            TransferAmount = LS?.TransferAmount ?? 0,
                            ReverseAmountDr = LS?.ReverseAmountDr ?? 0,
                            ReverseAmountCr = LR?.ReverseAmountCr ?? 0,
                            Purchase = LR?.ReceiverAmount ?? 0,
                        }).ToList();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(response, parallelOptions, res =>
            {
                //foreach (UserSaleViewModel res in response)    {
                res.OpeningBal = new UserBalanceHistoryDAL().SelectOpeningBalance(res.UserID, selectRequest.WalletID, FromDate);

                res.ClosingBalance = new UserWalletMasterDal().SelectClosingBalance(res.UserID, selectRequest.WalletID, ToDate, AddDayInt);
            });
            return response;
        }

        private List<UserSaleViewModel> UserwiseSaleOnlyUserTypeIDAndUserID(UserSaleSelectRequest selectRequest, int FromDate, int ToDate, int AddDayInt)
        {
            using var context = new DatabaseContext();
            List<UserMaster> usersList = new UserDAL().Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            // take skip and take users if data for skip and take count
            if (!selectRequest.IsExcelDownload)
            {
                usersList = (from user in usersList
                             join role in listUserType on user.UserTypeID equals role.UserTypeID
                             where user.UserTypeID == (int)UserType.Retailer || user.UserTypeID == (int)UserType.APIUser || user.UserTypeID == (int)UserType.Client
                             orderby user.UserID ascending
                             select user).Skip(selectRequest.SkipCount).Take(selectRequest.TakeCount).ToList();
            }
            else
            {
                usersList = (from user in usersList
                             select user).ToList();
            }
            var TransList = (from txn in context.TransactionMaster
                             where txn.DOCInt >= FromDate
                                && txn.DOCInt <= ToDate
                             && txn.WalletID == selectRequest.WalletID
                             select txn).ToList();

            List<UserWiseSaleViewModel> AllmainList = (from txn in TransList
                                                       join user in usersList on txn.UserID equals user.UserID
                                                       where txn.WalletID == selectRequest.WalletID

                                                       select new UserWiseSaleViewModel
                                                       {
                                                           WalletID = txn.WalletID,
                                                           UserID = txn.UserID,
                                                           UserTypeID = user.UserTypeID,
                                                           Status = txn.Status,
                                                           Amount = txn.Amount,
                                                           RetailerCommission = txn.RetailerCommission,
                                                           DistributerCommission = txn.DistributerCommission,
                                                           SuperDistributerCommission = txn.SuperDistributerCommission,
                                                           SuperDistributerUserID = txn.SuperDistributerUserID,
                                                           DistributerUserID = txn.DistributerUserID
                                                       }).ToList();

            List<UserWiseSaleViewModel> mainList = (from txn in AllmainList
                                                    where txn.Status == StatusType.Success
                                                    select txn).ToList();

            var Grouplist = (from txn in mainList
                             group txn by new { txn.WalletID, txn.UserID } into grp
                             select new
                             {
                                 WalletID = grp.Key.WalletID,
                                 UserID = grp.Key.UserID,
                                 Sale = grp.Sum(X => X.Amount),
                                 SuccessCount = grp.Count(),
                                 RetailerCommission = grp.Where(q => q.RetailerCommission > 0).Sum(X => X.RetailerCommission),
                                 DistributerCommission = grp.Where(q => q.DistributerCommission > 0).Sum(X => X.DistributerCommission),
                                 SuperDistributerCommission = grp.Where(q => q.SuperDistributerCommission > 0).Sum(X => X.SuperDistributerCommission),
                                 Surcharge = grp.Where(q => q.RetailerCommission < 0).Sum(X => X.RetailerCommission),
                             }).ToList();

            List<UserWiseSaleViewModel> FailmainList = (from txn in AllmainList
                                                        where txn.Status == StatusType.Failure
                                                        select txn).ToList();
            var GroupFailList = (from txn in FailmainList
                                 group txn by new { txn.WalletID, txn.UserID } into grp
                                 select new
                                 {
                                     WalletID = grp.Key.WalletID,
                                     UserID = grp.Key.UserID,
                                     TotalFail = grp.Sum(X => X.Amount),
                                     FailCount = grp.Count(),
                                 }).ToList();

            var FundSenderReceiverList = (from fund in context.FundTransaction
                                          where fund.DOCInt >= FromDate
                                                && fund.DOCInt <= ToDate
                                          && fund.WalletID == selectRequest.WalletID
                                          select fund).ToList();

            List<FundSenderViewModel> mainFundSenderList = (from fund in FundSenderReceiverList
                                                            join user in usersList on fund.SenderID equals user.UserID
                                                            select new FundSenderViewModel
                                                            {
                                                                DOCInt = fund.DOCInt,
                                                                Amount = fund.Amount,
                                                                SenderID = fund.SenderID,
                                                                WalletID = fund.WalletID,
                                                                ParentID = user.ParentID,
                                                                UserTypeID = user.UserTypeID,
                                                                Through = fund.Through,
                                                            }).ToList();

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

            var SenderfundList = (from fund in mainFundSenderList

                                  group fund by new { fund.SenderID } into grp
                                  select new
                                  {
                                      SenderID = grp.Key.SenderID,
                                      TransferAmount = grp.Where(q => q.Through == FundTransferType.ThroughRequestTransfer
                                         || q.Through == FundTransferType.ThroughDirectTransfer
                                         || q.Through == FundTransferType.PGTransfer).Sum(X => X.Amount),

                                      ReverseAmountDr = grp.Where(q => q.SenderID == grp.Key.SenderID
                                         && (q.Through == FundTransferType.ThroughDirectReverse
                                     || q.Through == FundTransferType.ThroughReverseRequestTransfer)).Sum(X => X.Amount),
                                  }).ToList();

            var ReceiverFundList = (from fund in mainFundReceiverList
                                    group fund by new { fund.ReceiverID } into grp
                                    select new
                                    {
                                        ReceiverID = grp.Key.ReceiverID,
                                        ReceiverAmount = grp.Where(q => q.Through == FundTransferType.ThroughDirectTransfer
                                              || q.Through == FundTransferType.ThroughRequestTransfer
                                              || q.Through == FundTransferType.PGTransfer).Sum(X => X?.Amount ?? 0),
                                        ReverseAmountCr = grp.Where(q => q.Through == FundTransferType.ThroughDirectReverse
                                      || q.Through == FundTransferType.ThroughReverseRequestTransfer).Sum(X => X.Amount),
                                    }).ToList();

            List<WalletMaster> walletslist = new WalletMasterDal().Select();

            List<UserSaleViewModel> response = new List<UserSaleViewModel>();
            response = (from User in usersList
                        join txn in Grouplist on User.UserID equals txn.UserID into lefttxn
                        from LT in lefttxn.DefaultIfEmpty()
                        join txnfail in GroupFailList on User.UserID equals txnfail.UserID into lefttxnfail
                        from LTFail in lefttxnfail.DefaultIfEmpty()
                        join Sender in SenderfundList on User.UserID equals Sender.SenderID into leftSender
                        from LS in leftSender.DefaultIfEmpty()
                        join Receiver in ReceiverFundList on User.UserID equals Receiver.ReceiverID into leftReceiver
                        from LR in leftReceiver.DefaultIfEmpty()
                        join wallet in walletslist on selectRequest.WalletID equals wallet.WalletID
                        join role in listUserType on User.UserTypeID equals role.UserTypeID
                        where User.UserTypeID == (int)UserType.Retailer || User.UserTypeID == (int)UserType.APIUser || User.UserTypeID == (int)UserType.Client
                        || User.UserTypeID == (int)UserType.Distributer || User.UserTypeID == (int)UserType.SuperDistributer
                        select new UserSaleViewModel
                        {
                            FirmName = User.FirmName.ConcatNameAliasName(User.AliasName),
                            UserMobile = User.MobileNumber,
                            ParentID = User.ParentID,
                            WalletName = wallet.Name,
                            OpeningBal = 0,
                            Sale = LT?.Sale ?? 0,
                            SuccessCount = LT?.SuccessCount ?? 0,
                            FailCount = LTFail?.FailCount ?? 0,
                            RetailerCommission = LT?.RetailerCommission ?? 0,
                            DistributerCommission = LT?.DistributerCommission ?? 0,
                            SuperDistributerCommission = LT?.SuperDistributerCommission ?? 0,
                            Surcharge = Math.Abs(LT?.Surcharge ?? 0),
                            Role = role.UserType,
                            UserID = User.UserID,
                            TransferAmount = LS?.TransferAmount ?? 0,
                            ReverseAmountDr = LS?.ReverseAmountDr ?? 0,
                            ReverseAmountCr = LR?.ReverseAmountCr ?? 0,
                            Purchase = LR?.ReceiverAmount ?? 0,
                        }).ToList();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(response, parallelOptions, res =>
            {
                //   foreach (UserSaleViewModel res in response) {
                res.OpeningBal = new UserBalanceHistoryDAL().SelectOpeningBalance(res.UserID, selectRequest.WalletID, FromDate);

                res.ClosingBalance = new UserWalletMasterDal().SelectClosingBalance(res.UserID, selectRequest.WalletID, ToDate, AddDayInt);
            });
            return response;
        }

        internal List<TopPerformerViewModel> SelectTopPerformer(SelectTopPerformerRequest selectRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt >= FromDate && txn.DOCInt <= ToDate
                                && txn.Status == StatusType.Success
                                select new
                                {
                                    UserID = txn.UserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    ServiceID = txn.ServiceID,
                                    Amount = txn.Amount,
                                }).ToList();
                if (selectRequest.ServiceID != 0)
                {
                    mainList = (from item in mainList
                                where item.ServiceID == selectRequest.ServiceID
                                select item).ToList();
                }
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    mainList = (from item in mainList
                                where item.SuperDistributerUserID == UserID
                                select item).ToList();
                }
                if (UserTypeID == (int)UserType.Distributer)
                {
                    mainList = (from item in mainList
                                where item.DistributerUserID == UserID
                                select item).ToList();
                }
                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.Client)
                {
                    mainList = (from item in mainList
                                where item.UserID == UserID
                                select item).ToList();
                }

                var TotalList = (from txn in mainList
                                 group txn by new { txn.UserID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     // ServiceID = grp.Key.ServiceID,
                                     TotalSale = grp.Sum(x => x.Amount),
                                 }).ToList();

                List<TopPerformerViewModel> response = new List<TopPerformerViewModel>();

                response = (from item in TotalList
                            join user in context.UserMaster on item.UserID equals user.UserID
                            orderby item.TotalSale descending
                            select new TopPerformerViewModel
                            {
                                FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                EmailID = user.EmailID,
                                TotalSale = item.TotalSale
                            }).Take(selectRequest.TopPerformance).ToList();
                return response;
            }
        }

        internal List<OperatorSaleViewModel> OperatorSale(Requests.TransactionReport.OperatorSaleRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<OperatorSaleViewModel> responce = null;

            using (var context = new DatabaseContext())
            {
                var mainList = (from txn in context.TransactionMaster
                                join Operator in context.OperatorMaster on
                               txn.OperatorID equals Operator.OperatorID
                                join Service in context.ServiceMaster on
                         Operator.ServiceID equals Service.ServiceID
                                join User in context.UserMaster on
                               txn.UserID equals User.UserID
                                where txn.DOCInt >= From
                                && txn.DOCInt <= To
                                && txn.Status == StatusType.Success

                                select new
                                {
                                    UserID = txn.UserID,
                                    UserName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                                    OperatorID = txn.OperatorID,
                                    OperatorName = Operator.OperatorName,
                                    ServiceID = Service.ServiceID,
                                    ServiceName = Service.Name,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    DOCInt = txn.DOCInt,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    DistributerUserID = txn.DistributerUserID
                                }).ToList();

                if (mainList == null)
                    return responce;

                if (selectRequest.ServiceID != 0)
                {
                    mainList = (from item in mainList
                                where item.ServiceID == selectRequest.ServiceID
                                select item).ToList();
                }

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID == UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.Distributer)
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID == UserID
                                    select item).ToList();
                    }
                }
                if (Util.isRoleAdminOrSupport(UserTypeID) && selectRequest.UserID != 0)
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID == selectRequest.UserID
                                    select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID == selectRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainList = (from item in mainList
                                    where item.UserID == selectRequest.UserID
                                    select item).ToList();
                    }
                }

                if (selectRequest.UserID != 0)
                {
                    var response = (from txn in mainList
                                    orderby txn.ServiceName, txn.OperatorName ascending
                                    group txn by new { txn.ServiceName, txn.OperatorName, txn.UserName, txn.OperatorID } into grp

                                    select new OperatorSaleViewModel
                                    {
                                        UserName = grp.Key.UserName,
                                        ServiceName = grp.Key.ServiceName,
                                        OperatorName = grp.Key.OperatorName,
                                        OperatorID = grp.Key.OperatorID,
                                        SuccessCount = grp.Count(),
                                        SuccessAmount = grp.Sum(X => X.Amount),
                                        ReatilerComm = grp.Sum(X => X.RetailerCommission),
                                        DistributerComm = grp.Sum(X => X.DistributerCommission),
                                        SuperDistributerComm = grp.Sum(X => X.SuperDistributerCommission),
                                        RetailerCommPercentage = "",
                                        DistributerCommPercentage = "",
                                        SuperDistributerCommPercentage = ""
                                    }).ToList();

                    response = (from txn in response
                                select new OperatorSaleViewModel
                                {
                                    UserName = txn.UserName,
                                    ServiceName = txn.ServiceName,
                                    OperatorName = txn.OperatorName,
                                    OperatorID = txn.OperatorID,
                                    SuccessCount = txn.SuccessCount,
                                    SuccessAmount = txn.SuccessAmount,
                                    ReatilerComm = txn.ReatilerComm,
                                    DistributerComm = txn.DistributerComm,
                                    SuperDistributerComm = txn.SuperDistributerComm,
                                    RetailerCommPercentage = $"{CalculatePercentage(txn.ReatilerComm, txn.SuccessAmount)}%",
                                    DistributerCommPercentage = $"{CalculatePercentage(txn.DistributerComm, txn.SuccessAmount)}%",
                                    SuperDistributerCommPercentage = $"{CalculatePercentage(txn.SuperDistributerComm, txn.SuccessAmount)}%"
                                }).ToList();

                    return response;
                }
                else
                {
                    var response = (from txn in mainList
                                    orderby txn.ServiceName, txn.OperatorName ascending
                                    group txn by new { txn.ServiceName, txn.OperatorName, txn.OperatorID } into grp

                                    select new OperatorSaleViewModel
                                    {
                                        UserName = "All",
                                        ServiceName = grp.Key.ServiceName,
                                        OperatorName = grp.Key.OperatorName,
                                        OperatorID = grp.Key.OperatorID,
                                        SuccessCount = grp.Count(),
                                        SuccessAmount = grp.Sum(X => X.Amount),
                                        ReatilerComm = grp.Sum(X => X.RetailerCommission),
                                        DistributerComm = grp.Sum(X => X.DistributerCommission),
                                        SuperDistributerComm = grp.Sum(X => X.SuperDistributerCommission),
                                        RetailerCommPercentage = "",
                                        DistributerCommPercentage = "",
                                        SuperDistributerCommPercentage = "",
                                    }).ToList();


                    response = (from txn in response
                                select new OperatorSaleViewModel
                                {
                                    UserName = txn.UserName,
                                    ServiceName = txn.ServiceName,
                                    OperatorName = txn.OperatorName,
                                    OperatorID = txn.OperatorID,
                                    SuccessCount = txn.SuccessCount,
                                    SuccessAmount = txn.SuccessAmount,
                                    ReatilerComm = txn.ReatilerComm,
                                    DistributerComm = txn.DistributerComm,
                                    SuperDistributerComm = txn.SuperDistributerComm,
                                    RetailerCommPercentage = $"{CalculatePercentage(txn.ReatilerComm, txn.SuccessAmount)}%",
                                    DistributerCommPercentage = $"{CalculatePercentage(txn.DistributerComm, txn.SuccessAmount)}%",
                                    SuperDistributerCommPercentage = $"{CalculatePercentage(txn.SuperDistributerComm, txn.SuccessAmount)}%",
                                }).ToList();

                    return response;
                }
            }
        }


        private static string CalculatePercentage(decimal commission, decimal totalAmount)
        {
            return totalAmount != 0
                ? ((commission / totalAmount) * 100).ToString("0.00")
                : "0";
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForLast10(int ServiceID, int UserID)
        {
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        where TransactionMaster.UserID == UserID
                        orderby TransactionMaster.TransactionMasterID descending

                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,
                            WalletID = TransactionMaster.WalletID,
                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameMobileAliasName(TransactionMaster.UserMaster.MobileNumber, TransactionMaster.UserMaster.AliasName),
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,
                            Logo = TransactionMaster.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
                            BeneName = TransactionMaster.DMT_BeneficiaryName,
                            SenderNo = TransactionMaster.DMT_SenderNumber,
                            IFSC = TransactionMaster.DMT_IFSC,
                            BankName = TransactionMaster.DMT_Bank,
                            Bank = TransactionMaster.DMT_Bank,
                            OpeningBalance = TransactionMaster.OpeningBalance,
                            Amount = TransactionMaster.Amount,
                            DebitAmount = TransactionMaster.DebitAmount,
                            ClosingBalance = TransactionMaster.ClosingBalance,
                            ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                            Status = TransactionMaster.Status,
                            ComplaintStatus = TransactionMaster.ComplaintStatus,
                            ReferenceNo = (TransactionMaster.Status == StatusType.Failure && ShowTxnIDinFailure.EqualsIgnoreCase("Yes")) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                            PaymentType = TransactionMaster.DMT_PaymentType,
                            Through = TransactionMaster.Through,

                            DOC = TransactionMaster.DOC.DOCFormatDate(),
                            ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                            OperatorID = TransactionMaster.OperatorID,
                            ServiceID = TransactionMaster.ServiceID,
                            ProviderRequestID = TransactionMaster.ProviderRequestID,
                            ProviderID = TransactionMaster.ProviderID,

                            IPAddress = TransactionMaster.IPAddress,
                            APIUserRequestID = TransactionMaster.APIUserRequestID,
                            Circle = TransactionMaster.Circle ?? "",
                            RetailerCommission = TransactionMaster.RetailerCommission,
                            DistributerCommission = TransactionMaster.DistributerCommission,
                            SuperDistributerCommission = TransactionMaster.SuperDistributerCommission
                        }).ToList();

                if (ServiceID == (int)ServiceCode.MoneyTransfer || ServiceID == (int)ServiceCode.Payout || ServiceID == (int)ServiceCode.UPITransfer || ServiceID == (int)ServiceCode.LeadGeneration)
                {
                    list = (from item in list
                            where item.ServiceID == ServiceID
                            select item).Take(10).ToList();
                }
                else
                {
                    int Wallet = new WalletServiceMappingDAL().SelectWalletIDByServiceID(ServiceID);
                    list = (from item in list
                            where item.WalletID == Wallet
                            select item).Take(10).ToList();
                }
            }
            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForLast10ByServiceID(int ServiceID, int UserID)
        {
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        where TransactionMaster.UserID == UserID
                        && TransactionMaster.ServiceID == ServiceID
                        orderby TransactionMaster.TransactionMasterID descending

                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,
                            WalletID = TransactionMaster.WalletID,
                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameMobileAliasName(TransactionMaster.UserMaster.MobileNumber, TransactionMaster.UserMaster.AliasName),
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,
                            Logo = TransactionMaster.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
                            BeneName = TransactionMaster.DMT_BeneficiaryName,
                            SenderNo = TransactionMaster.DMT_SenderNumber,
                            IFSC = TransactionMaster.DMT_IFSC,
                            BankName = TransactionMaster.DMT_Bank,
                            Bank = TransactionMaster.DMT_Bank,
                            OpeningBalance = TransactionMaster.OpeningBalance,
                            Amount = TransactionMaster.Amount,
                            DebitAmount = TransactionMaster.DebitAmount,
                            ClosingBalance = TransactionMaster.ClosingBalance,
                            ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                            Status = TransactionMaster.Status,
                            ComplaintStatus = TransactionMaster.ComplaintStatus,
                            ReferenceNo = TransactionMaster.ReferenceNo,

                            PaymentType = TransactionMaster.DMT_PaymentType,
                            Through = TransactionMaster.Through,

                            DOC = TransactionMaster.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                            OperatorID = TransactionMaster.OperatorID,
                            ServiceID = TransactionMaster.ServiceID,
                            ProviderRequestID = TransactionMaster.ProviderRequestID,
                            ProviderID = TransactionMaster.ProviderID,

                            IPAddress = TransactionMaster.IPAddress,
                            APIUserRequestID = TransactionMaster.APIUserRequestID,
                            Circle = TransactionMaster.Circle ?? "",
                            RetailerCommission = TransactionMaster.RetailerCommission,
                            DistributerCommission = TransactionMaster.DistributerCommission,
                            SuperDistributerCommission = TransactionMaster.SuperDistributerCommission
                        }).Take(10).ToList();
            }
            return list;
        }

        internal List<MonthlySuccessView> UserwiseMonthlySuccessData(int MonthStart, int MonthEnd, int UserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.TransactionMaster
                                where txn.DOCInt >= MonthStart && txn.DOCInt <= MonthEnd &&
                                txn.Status == StatusType.Success
                                select new MonthlySuccessView
                                {
                                    UserID = txn.UserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    AdminCommission = txn.AdminCommission,
                                    Amount = txn.Amount,
                                    GivenCommission = txn.RetailerCommission + txn.DistributerCommission + txn.SuperDistributerCommission,
                                    DOC = Convert.ToInt32(txn.DOC.ToString(Messages.DOCDayFormat, CultureInfo.InvariantCulture)),
                                }).ToList();

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
                if (UserTypeID != (int)UserType.Admin)
                {
                    if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                    {
                        mainList = (from item in mainList
                                    where item.UserID == UserID
                                    select item).ToList();
                    }
                    if (UserTypeID == (int)UserType.Distributer)
                    {
                        mainList = (from item in mainList
                                    where item.DistributerUserID == UserID
                                    select item).ToList();
                    }
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainList = (from item in mainList
                                    where item.SuperDistributerUserID == UserID
                                    select item).ToList();
                    }
                }

                return mainList;
            }
        }

        internal List<MonthlySummaryViewModel> MonthPerDaySale(int diffMonth, int UserID, out bool IsbothSale)
        {
            DateTime first = DateTime.Now.AddMonths(diffMonth).StartOfMonth();
            DateTime last = DateTime.Now.AddMonths(diffMonth).LastOfMonth();

            int MonthStart = Convert.ToInt32(first.ToString(Messages.DOCIntegerFomrat));
            int MonthEnd = Convert.ToInt32(last.ToString(Messages.DOCIntegerFomrat));

            int month = Convert.ToInt32(DateTime.Now.Month.ToString("0#"));

            IsbothSale = false;

            List<DaysViewModel> monthdayslist = new Util().MonthDaysList(month);
            List<MonthlySuccessView> monthlysuccess = UserwiseMonthlySuccessData(MonthStart, MonthEnd, UserID);

            if (monthlysuccess.Count <= 0)
                IsbothSale = true;

            var mainList = (from days in monthdayslist
                            join data in monthlysuccess on days.DOC equals data.DOC into leftData
                            from currentList in leftData.DefaultIfEmpty()
                            select new MonthlySuccessView
                            {
                                DOC = days.DOC,
                                AdminCommission = currentList?.AdminCommission ?? 0,
                                Amount = currentList?.Amount ?? 0,
                                GivenCommission = currentList?.GivenCommission ?? 0,
                            }).ToList();

            var groupList = (from txn in mainList
                             orderby txn.DOC ascending
                             group txn by new { txn.DOC } into grp

                             select new
                             {
                                 DOC = grp.Key.DOC,
                                 SucessAMT = grp?.Sum(x => x.Amount) ?? 0,
                                 GivenCommissionAMT = grp?.Sum(x => x.GivenCommission) ?? 0,
                                 AdminCommissionAMT = grp?.Sum(x => x.AdminCommission) ?? 0,
                             }).ToList();

            var FinalList = (from list in groupList
                             select new
                             {
                                 Day = list.DOC,
                                 Amount = list?.SucessAMT ?? 0,
                                 EarningAmount = list?.AdminCommissionAMT ?? 0,
                                 GivenAmount = list?.GivenCommissionAMT ?? 0,
                                 Profit = (list?.AdminCommissionAMT - list?.GivenCommissionAMT) ?? 0,
                             }).ToList();

            var response = (from grplist in FinalList

                            select new MonthlySummaryViewModel
                            {
                                Day = grplist.Day,
                                Amount = grplist.Amount,
                                Earning = grplist.EarningAmount,
                                Given = grplist.GivenAmount,
                                Profit = grplist.Profit,
                            }).ToList();

            return response;
        }

        internal List<TwoMonthPerDaySaleViewModel> MonthlySummary(int UserID)
        {
            bool IsCurrentSale = false; bool IsLastSale = false;
            List<MonthlySummaryViewModel> CurrentMonthList = MonthPerDaySale(0, UserID, out IsCurrentSale);

            List<MonthlySummaryViewModel> LastMonthList = MonthPerDaySale(-1, UserID, out IsLastSale);

            List<TwoMonthPerDaySaleViewModel> list = new List<TwoMonthPerDaySaleViewModel>();

            if (IsCurrentSale && IsLastSale)
            {
                list = (from last in LastMonthList
                        join current in CurrentMonthList on last.Day equals current.Day into leftData
                        from currentList in leftData.DefaultIfEmpty()
                        orderby last.Day ascending
                        select new TwoMonthPerDaySaleViewModel
                        {
                            Day = last.Day,
                            LastMonth = "10",
                            LastEarning = last?.Earning ?? 0,
                            LastGiven = last?.Given ?? 0,
                            LastProfit = last?.Profit ?? 0,

                            CurrentMonth = "10",
                            CurrentEarning = currentList?.Earning ?? 0,
                            CurrentGiven = currentList?.Given ?? 0,
                            CurrentProfit = currentList?.Profit ?? 0,
                        }).OrderBy(s => s.Day).ToList();
                return list;
            }

            list = (from last in LastMonthList
                    join current in CurrentMonthList on last.Day equals current.Day into leftData
                    from currentList in leftData.DefaultIfEmpty()
                    orderby last.Day ascending
                    select new TwoMonthPerDaySaleViewModel
                    {
                        Day = last.Day,
                        LastMonth = last?.Amount.TwoDigitDecimal() ?? "0",
                        LastEarning = last?.Earning ?? 0,
                        LastGiven = last?.Given ?? 0,
                        LastProfit = last?.Profit ?? 0,

                        CurrentMonth = currentList?.Amount.TwoDigitDecimal() ?? "0",
                        CurrentEarning = currentList?.Earning ?? 0,
                        CurrentGiven = currentList?.Given ?? 0,
                        CurrentProfit = currentList?.Profit ?? 0,
                    }).OrderBy(s => s.Day).ToList();

            return list;
        }

        internal List<ProviderFailureGraphViewModel> ProviderFailureDataByDate(int docint)
        {
            using (var context = new DatabaseContext())
            {
                var mainList = (from txn in context.TransactionMaster
                                where txn.Status == StatusType.Failure
                                                   && txn.DOCInt == docint
                                select new ProviderFailureGraphViewModel
                                {
                                    FailureMessage = txn.Description,
                                    Amount = txn.Amount,
                                    DOCStr = txn.DOC.ToLongDateString()
                                }).ToList();

                var Totallist = (from item in mainList
                                 group item by new { item.FailureMessage, item.DOCStr } into grp
                                 select new ProviderFailureGraphViewModel
                                 {
                                     FailureMessage = grp.Key.FailureMessage,
                                     DOCStr = grp.Key.DOCStr,
                                     Amount = grp.Sum(x => x.Amount),
                                 }).Take(5).ToList();

                return Totallist;
            }
        }

        internal List<ProviderFailureGraphViewModel> ProviderFailureGraph()
        {
            List<ProviderFailureGraphViewModel> list = new List<ProviderFailureGraphViewModel>();

            for (int i = 6; i >= 0; i--)
            {
                int doCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Today.AddDays(-i)).ToString(Messages.DOCIntegerFomrat));
                List<ProviderFailureGraphViewModel> WeeklyFailure = ProviderFailureDataByDate(doCInt);
                list.AddRange(WeeklyFailure);
            }
            return list;
        }

        internal List<OperatorWiseGSTViewModel> GSTOperatorWise(GSTSummaryRequest selectRequest)
        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<OperatorWiseGSTViewModel> list = new List<OperatorWiseGSTViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var MainList = (from item in context.TransactionMaster
                                where item.DOCInt >= From && item.DOCInt <= To && item.Status == StatusType.Success
                                group item by new { item.UserID, item.OperatorID } into grp
                                select new
                                {
                                    UserID = grp.Key.UserID,
                                    OperatorID = grp.Key.OperatorID,
                                    Amount = grp.Sum(x => x.Amount),
                                    RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                    SuperDistributerCommission = grp.Sum(x => x.SuperDistributerCommission),
                                    DistributerCommission = grp.Sum(x => x.DistributerCommission),
                                    CommLess = Math.Abs(grp.Sum(x => x.Amount) - grp.Sum(x => x.RetailerCommission)),
                                }).ToList();

                if (selectRequest.UserID > 0)
                {
                    MainList = (from item in MainList
                                where item.UserID == selectRequest.UserID
                                select item).ToList();
                }

                List<UserMaster> usersList = new UserDAL().Select();
                usersList = usersList.Where(u => u.IsInvoiceApplicable == true && u.GSTNo.Length > 3).ToList();

                List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
                List<OperatorMaster> listoperator = new OperatorMasterDAL().SelectAll();

                var Finallist = (from main in MainList
                                 join user in usersList on main.UserID equals user.UserID
                                 join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                                 join operators in listoperator on main.OperatorID equals operators.OperatorID
                                 select new
                                 {
                                     InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                                     FirmName = user.FirmName,
                                     Operator = operators.OperatorName,
                                     UserType = usertype.UserType,
                                     GSTNo = user.GSTNo,
                                     Amount = main?.Amount ?? 0,
                                     RetailerCommission = main?.RetailerCommission ?? 0,
                                     SDCommission = main?.SuperDistributerCommission ?? 0,
                                     DCommission = main?.DistributerCommission ?? 0,
                                     Charge = 0,
                                     CommLess = main?.CommLess ?? 0,
                                     GivenCommission = main?.RetailerCommission ?? 0,
                                     ReviceGST = Math.Abs(main.CommLess / ((118 / 100) - main.CommLess)),
                                 }
                              ).ToList();

                list = (from main in Finallist
                        select new OperatorWiseGSTViewModel
                        {
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            FirmName = main.FirmName,
                            Operator = main.Operator,
                            UserType = main.UserType,
                            GSTNo = main.GSTNo,
                            Amount = main.Amount,
                            RetailerCommission = main.RetailerCommission,
                            SDCommission = 0,
                            DCommission = 0,
                            Charge = 0,
                            CommLess = main.CommLess.ToString("0.##"),
                            GivenCommission = main.GivenCommission,
                            ReviceGST = main.ReviceGST.ToString("0.##"),
                            Percentage = ((main.GivenCommission * 100) / main.Amount).ToString("0.##") + "%",
                            TaxableValue = (main.Amount - (main.GivenCommission + main.ReviceGST)),
                        }
                    ).ToList();
            }
            return list;
        }

        internal List<PurchaseViewModel> PurchaseReport(PurchaseReportRequest selectRequest)
        {
            List<PurchaseViewModel> list = new List<PurchaseViewModel>();
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<UserMaster> usersList = new UserDAL().Select();

            UserMaster Companyuser = null;

            Companyuser = new UserDAL().SelectByIDFromCache(Company.AdminUserID);

            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var CreditList = (from item in context.FundTransaction
                                  join user in context.UserMaster on item.ReceiverID equals user.UserID
                                  join usertype in context.UserTypeMaster on user.UserTypeID equals usertype.UserTypeID
                                  where item.DOCInt >= From
                                  && item.DOCInt <= To
                                  && item.Status == StatusType.Success
                                  group item by new { item.ReceiverID, user.FirmName, user.State.Name, usertype.UserType } into grp
                                  select new
                                  {
                                      FirmName = grp.Key.FirmName,
                                      State = grp.Key.Name,
                                      UserType = grp.Key.UserType,
                                      UserID = grp.Key.ReceiverID,
                                      CreditAmount = grp.Sum(x => x.Amount),
                                      Month = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                                  }).ToList();

                if (selectRequest.UserID > 0)
                {
                    CreditList = (from item in CreditList
                                  where item.UserID == selectRequest.UserID
                                  select item).ToList();
                }

                var DebitList = (from item in context.FundTransaction
                                 join user in context.UserMaster on item.SenderID equals user.UserID
                                 join usertype in context.UserTypeMaster on user.UserTypeID equals usertype.UserTypeID

                                 where item.DOCInt >= From
                                      && item.DOCInt <= To
                                      && item.Status == StatusType.Success
                                 group item by new { item.SenderID, user.FirmName, usertype.UserType } into grp
                                 select new
                                 {
                                     FirmName = grp.Key.FirmName,
                                     UserType = grp.Key.UserType,
                                     UserID = grp.Key.SenderID,
                                     DebitAmount = grp.Sum(x => x.Amount),
                                     Month = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                                 }).ToList();

                if (selectRequest.UserID > 0)
                {
                    DebitList = (from item in DebitList
                                 where item.UserID == selectRequest.UserID
                                 select item).ToList();
                }
                List<StateMaster> statelist = new StateDAL().Select();
                list = (from credit in CreditList
                        join user in usersList on credit.UserID equals user.UserID
                        join debit in DebitList on credit.UserID equals debit.UserID into leftData
                        from currentList in leftData.DefaultIfEmpty()
                        orderby credit.UserType ascending
                        select new PurchaseViewModel
                        {
                            Month = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            Credit = credit?.CreditAmount ?? 0,
                            Debit = currentList?.DebitAmount ?? 0,
                            Address = user?.Address ?? "",
                            GSTNo = user?.GSTNo ?? "",
                            Pincode = user?.Pincode ?? "",
                            State = credit?.State ?? "",
                            FirmName = credit?.FirmName ?? "",
                            UserType = credit?.UserType ?? "",
                            UserID = credit?.UserID ?? 0,
                            CompanyAddress = Companyuser.Address,
                            CompanyGST = Companyuser.GSTNo,
                            CompanyPincode = Companyuser.Pincode,
                        }
                            ).ToList();
            }
            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForFilterByUserID(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        where TransactionMaster.DOCInt >= FromDate
                        && TransactionMaster.DOCInt <= ToDate
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                        && TransactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                        && TransactionMaster.ServiceID != (int)ServiceCode.AEPS
                        && TransactionMaster.ServiceID != (int)ServiceCode.PaymentGateway
                        && TransactionMaster.ServiceID != (int)ServiceCode.MATM
                        && TransactionMaster.ServiceID != (int)ServiceCode.Payout
                        orderby TransactionMaster.TransactionMasterID descending

                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,

                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameAliasName(TransactionMaster.UserMaster.AliasName),
                            UserMobile = TransactionMaster.UserMaster.MobileNumber,
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,
                            Logo = TransactionMaster.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
                            OpeningBalance = TransactionMaster.OpeningBalance,
                            Amount = TransactionMaster.Amount,
                            DebitAmount = TransactionMaster.DebitAmount,
                            ClosingBalance = TransactionMaster.ClosingBalance,
                            Status = TransactionMaster.Status,
                            ReferenceNo = (TransactionMaster.Status == StatusType.Failure) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,
                            DOC = TransactionMaster.DOC.LongFormatDate(),
                            OperatorID = TransactionMaster.OperatorID,
                            ServiceID = TransactionMaster.ServiceID,
                            ProviderRequestID = TransactionMaster.ProviderRequestID,
                            IPAddress = TransactionMaster.IPAddress,
                            APIUserRequestID = TransactionMaster.APIUserRequestID,
                            RetailerCommission = TransactionMaster.RetailerCommission,
                            DistributerCommission = TransactionMaster.DistributerCommission,
                            SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                            SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                            DistributerUserID = TransactionMaster.DistributerUserID,
                        }).ToList();

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        list = (from item in list
                                where item.SuperDistributerUserID == UserID
                                select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.Distributer)
                    {
                        list = (from item in list
                                where item.DistributerUserID == UserID
                                select item).ToList();
                    }
                    else
                    {
                        list = (from item in list
                                where item.UserID == UserID
                                select item).ToList();
                    }
                }
                if (Util.isRoleAdminOrSupport(UserTypeID))
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(txReportRequest.UserID);
                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        list = (from item in list
                                where item.SuperDistributerUserID == txReportRequest.UserID
                                select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        list = (from item in list
                                where item.DistributerUserID == txReportRequest.UserID
                                select item).ToList();
                    }
                    else
                    {
                        list = (from item in list
                                where item.UserID == txReportRequest.UserID
                                select item).ToList();
                    }
                }
            }
            return list;
        }



        private bool isReatilerAPIClient(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Retailer)
            {
                return true;
            }
            if (UserTypeID == (int)UserType.APIUser)
            {
                return true;
            }
            if (UserTypeID == (int)UserType.Client)
            {
                return true;
            }

            return false;
        }

        private int GetRole(int UserTypeID)
        {
            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                return (int)UserType.Admin;
            }
            return UserTypeID;
        }

        private int GetSelectedUserRole(int UserTypeID, int selectedUserID)
        {
            //if (GetRole(UserTypeID) != (int)UserType.Admin)
            //{
            //    return -1;
            //}

            if (selectedUserID == 0)
            {
                return -1;
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && selectedUserID == 0)
            {
                return (int)UserType.Admin;
            }
            int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(selectedUserID);

            return RequestedUserIDUserTypeID;
        }

        internal List<Response.TransactionMaster.RechargeReportViewModel> SelectForRechargeFilter(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.RechargeReportViewModel> list = new List<Response.TransactionMaster.RechargeReportViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();

            /*     int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway
                 ,(int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.CashDeposit,(int)ServiceCode.CreditCardPayment,(int)ServiceCode.WalletTopUp
                 ,(int)ServiceCode.UPITransfer,(int)ServiceCode.AEPSOnboarding,  (int)ServiceCode.MATMOnboarding,(int)ServiceCode.VendorPay};*/

            int[] ids = new int[] { (int)ServiceCode.Mobile, (int)ServiceCode.Dth, (int)ServiceCode.DTHBooking };

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID > 0 && ids.Contains(x.ServiceID)).ToList();

            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                       && ids.Contains(TransactionMaster.ServiceID)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? TransactionMaster.ReferenceNo == txReportRequest.CriteriaText :
                                       txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? TransactionMaster.ConsumerNumber == txReportRequest.CriteriaText :
                                       txReportRequest.Criteria.EqualsIgnoreCase("APIUserRequestID") ? TransactionMaster.APIUserRequestID == txReportRequest.CriteriaText : 2 > 1)

                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                && ids.Contains(TransactionMaster.ServiceID)
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? TransactionMaster.ReferenceNo == txReportRequest.CriteriaText :
                                txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? TransactionMaster.ConsumerNumber == txReportRequest.CriteriaText :
                                txReportRequest.Criteria.EqualsIgnoreCase("APIUserRequestID") ? TransactionMaster.APIUserRequestID == txReportRequest.CriteriaText : 2 > 1)

                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID

                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.RechargeReportViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        IsBBPS = operators?.IsBBPS,

                        Logo = operators?.OperatorImagePath?.ConcatWebAPIURL(),
                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",
                        ReferenceNo = TransactionMaster.ReferenceNo,

                        Through = TransactionMaster.Through,
                        DOC = TransactionMaster.DOC.DOCFormatDate(),
                        UpdatedDOC = TransactionMaster.UpdatedDOC.LongFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName,
                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission + TransactionMaster.DistributerCommissionHold,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission + TransactionMaster.SuperDistributerCommissionHold,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        CashBack = TransactionMaster.CashBack,
                        AdminCommission = TransactionMaster.AdminCommission,
                        AdminProfit = TransactionMaster.AdminProfit,
                        ResponseType = TransactionMaster.ResponseType,
                        ROfferCommission = TransactionMaster.ROfferCommission,
                        OrignalFirmName = User?.FirmName,
                        GST = TransactionMaster.RetailerGST,
                        TDS = TransactionMaster.RetailerTDS,
                        DistributerGST = TransactionMaster.DistributerGST,
                        DistributerTDS = TransactionMaster.DistributerTDS,
                        SuperDistributerGST = TransactionMaster.SuperDistributerGST,
                        SuperDistributerTDS = TransactionMaster.SuperDistributerTDS,
                        Latitude = TransactionMaster.SimCardDetails,
                        Longitude = TransactionMaster.Location,
                        Optional1 = TransactionMaster.Bill_Optional1,
                        Response = (TransactionMaster.Status == StatusType.Failure) ?(ShowTxnIDinFailure.EqualsIgnoreCase("Yes")? 
                        TransactionMaster.Description :TransactionMaster.ReferenceNo): string.Empty,
                        NetCommission = TransactionMaster.NetCommission
                    }).ToList();

            return list;
        }

        internal List<Response.TransactionMaster.RechargeReportViewModel> SelectForBBPSFilter(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.RechargeReportViewModel> list = new List<Response.TransactionMaster.RechargeReportViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();

            int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway
                 ,(int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.CashDeposit,(int)ServiceCode.CreditCardPayment,(int)ServiceCode.WalletTopUp
                 ,(int)ServiceCode.UPITransfer,(int)ServiceCode.AEPSOnboarding,  (int)ServiceCode.MATMOnboarding,(int)ServiceCode.VendorPay,(int)ServiceCode.Dth,(int)ServiceCode.Mobile,(int)ServiceCode.DTHBooking};


            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID > 0 && !ids.Contains(x.ServiceID)).ToList();

            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                       && !ids.Contains(TransactionMaster.ServiceID)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                       && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? TransactionMaster.ReferenceNo == txReportRequest.CriteriaText :
                                       txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? TransactionMaster.ConsumerNumber == txReportRequest.CriteriaText :
                                       txReportRequest.Criteria.EqualsIgnoreCase("APIUserRequestID") ? TransactionMaster.APIUserRequestID == txReportRequest.CriteriaText : 2 > 1)

                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                && !ids.Contains(TransactionMaster.ServiceID)
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? TransactionMaster.ReferenceNo == txReportRequest.CriteriaText :
                                txReportRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? TransactionMaster.ConsumerNumber == txReportRequest.CriteriaText :
                                txReportRequest.Criteria.EqualsIgnoreCase("APIUserRequestID") ? TransactionMaster.APIUserRequestID == txReportRequest.CriteriaText : 2 > 1)

                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID

                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.RechargeReportViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        AgentID = User?.AliasName,
                        Name = User?.FirmName,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        IsBBPS = operators?.IsBBPS,

                        Logo = operators?.OperatorImagePath?.ConcatWebAPIURL(),
                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",
                        ReferenceNo = (TransactionMaster.Status == StatusType.Failure && ShowTxnIDinFailure.EqualsIgnoreCase("Yes")) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                        Through = TransactionMaster.Through,
                        DOC = TransactionMaster.DOC.DOCFormatDate(),
                        UpdatedDOC = TransactionMaster.UpdatedDOC.LongFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName,
                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission + TransactionMaster.DistributerCommissionHold,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission + TransactionMaster.SuperDistributerCommissionHold,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        CashBack = TransactionMaster.CashBack,
                        AdminCommission = TransactionMaster.AdminCommission,
                        AdminProfit = TransactionMaster.AdminProfit,
                        ResponseType = TransactionMaster.ResponseType,
                        ROfferCommission = TransactionMaster.ROfferCommission,
                        OrignalFirmName = User?.FirmName,
                        GST = TransactionMaster.RetailerGST,
                        TDS = TransactionMaster.RetailerTDS,
                        DistributerGST = TransactionMaster.DistributerGST,
                        DistributerTDS = TransactionMaster.DistributerTDS,
                        SuperDistributerGST = TransactionMaster.SuperDistributerGST,
                        SuperDistributerTDS = TransactionMaster.SuperDistributerTDS,
                        Latitude = TransactionMaster.SimCardDetails,
                        Longitude = TransactionMaster.Location,
                        Optional1 = TransactionMaster.Bill_Optional1,
                        NetCommission = TransactionMaster.NetCommission,
                        BillerID = TransactionMaster.Bill_Optional5,

                    }).ToList();

            return list;
        }


        internal Response.TransactionMaster.HeaderViewModel GetHeaderSummaryData(List<CommissionHoldResponse> list)
        {
            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.TransactionAmount),
                               Commission = grp.Sum(X => X.Commission),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.Commission = Success?.Commission ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount;
            headerlist.TotalCount = headerlist.SuccessCount;

            return headerlist;
        }

        internal Response.TransactionMaster.HeaderViewModel GetHeaderSummaryDataDemat(List<Response.DematGrowthLossMaster.ViewModel> list)
        {
            var Growth = (from item in list
                          where item.GrowthLoss == "Growth"
                          group item by new { item.GrowthLoss } into grp
                          select new
                          {
                              GrowthAmount = grp.Sum(X => X.Amount),
                              GrowthCount = grp.Count(),
                          }).FirstOrDefault();

            var Loss = (from item in list
                        where item.GrowthLoss == "Loss"
                        group item by new { item.GrowthLoss } into grp
                        select new
                        {
                            LossAmount = grp.Sum(X => X.Amount),
                            LossCount = grp.Count(),
                        }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.GrowthAmount = Growth?.GrowthAmount ?? 0;
            headerlist.GrowthCount = Growth?.GrowthCount ?? 0;

            headerlist.LossAmount = Loss?.LossAmount ?? 0;
            headerlist.LossCount = Loss?.LossCount ?? 0;

            headerlist.TotalAmount = headerlist.GrowthAmount + headerlist.LossAmount;
            headerlist.TotalCount = headerlist.GrowthCount + headerlist.LossCount;

            return headerlist;
        }

        internal List<Response.Redeemcommission.CommissionHoldResponse> SelectReport(Project.Requests.CommissionHold.SelectReportRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.Redeemcommission.CommissionHoldResponse> list = new List<Response.Redeemcommission.CommissionHoldResponse>();
            int UserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
            List<WalletMaster> walletList = new WalletMasterDal().Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.MoneyTransfer || x.ServiceID == (int)ServiceCode.Mobile).ToList();

            List<Response.Redeemcommission.CommissionHoldResponse> mainlist = new List<Response.Redeemcommission.CommissionHoldResponse>();
            using (var context = new DatabaseContext())
            {

                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    mainlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From
                                && txn.DOCInt <= To
                                && txn.Status == StatusType.Success
                                && txn.SuperDistributerUserID == selectRequest.UserID
                                && txn.WalletID == selectRequest.WalletID
                                && (txn.SuperDistributerCommission > 0 || txn.SuperDistributerCommissionHold > 0)
                                orderby txn.TransactionMasterID descending
                                select new Response.Redeemcommission.CommissionHoldResponse
                                {
                                    TransactionMasterID = txn.TransactionMasterID,
                                    WalletID = txn.WalletID,
                                    OperatorID = txn.OperatorID,
                                    UserID = txn.SuperDistributerUserID,
                                    Status = txn.Status,
                                    Commission = (txn.SuperDistributerCommission <= 0 ? txn.SuperDistributerCommissionHold : txn.SuperDistributerCommission),
                                    TransactionAmount = txn.Amount,
                                    DOC = txn.DOC.LongFormatDate(),

                                }).ToList();
                }
                else
                {
                    mainlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From
                                && txn.DOCInt <= To
                                && txn.Status == StatusType.Success
                                && txn.DistributerUserID == selectRequest.UserID
                                && txn.WalletID == selectRequest.WalletID
                                && (txn.DistributerCommission > 0 || txn.DistributerCommissionHold > 0)
                                orderby txn.TransactionMasterID descending
                                select new Response.Redeemcommission.CommissionHoldResponse
                                {
                                    TransactionMasterID = txn.TransactionMasterID,
                                    WalletID = txn.WalletID,
                                    OperatorID = txn.OperatorID,
                                    UserID = txn.DistributerUserID,
                                    Status = txn.Status,
                                    Commission = (txn.DistributerCommission <= 0 ? txn.DistributerCommissionHold : txn.DistributerCommission),
                                    TransactionAmount = txn.Amount,
                                    DOC = txn.DOC.LongFormatDate(),

                                }).ToList();
                }

            }
            list = (from txn in mainlist
                    join User in usersList on txn.UserID equals User.UserID
                    join wallet in walletList on txn.WalletID equals wallet.WalletID
                    join operators in operatorList on txn.OperatorID equals operators.OperatorID
                    into Operatorgroup
                    from operators in Operatorgroup.DefaultIfEmpty()

                    select new Response.Redeemcommission.CommissionHoldResponse
                    {
                        TransactionMasterID = txn.TransactionMasterID,
                        FirmName = User.FirmName.ConcatNameAliasName(User.AliasName),
                        WalletName = wallet.Name,
                        Commission = txn.Commission,
                        OperatorName = operators?.OperatorName ?? "",
                        Status = txn.Status,
                        DOC = txn.DOC,
                        TransactionAmount = txn.TransactionAmount,
                    }).ToList();

            return list;
        }

        internal List<TransactionMaster> SelectByArchive(List<Archive_TransactionMaster> tranListArchive)
        {
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            foreach (Archive_TransactionMaster row in tranListArchive)
            {
                TransactionMaster totalitem = new TransactionMaster
                {
                    TransactionMasterID = row.TransactionMasterID,
                    UserID = row.UserID,
                    ServiceID = row.ServiceID,
                    OperatorID = row.OperatorID,
                    InboxID = row.InboxID,
                    WalletID = row.WalletID,
                    ConsumerNumber = row.ConsumerNumber,
                    Amount = row.Amount,
                    DebitAmount = row.DebitAmount,
                    OpeningBalance = row.OpeningBalance,
                    ClosingBalance = row.ClosingBalance,
                    Status = row.Status,
                    Description = row.Description,
                    ProviderID = row.ProviderID,
                    ProviderOpeningBalance = row.ProviderOpeningBalance,
                    ProviderCommission = row.ProviderCommission,
                    ProviderClosingBalance = row.ProviderClosingBalance,
                    ProviderRequestID = row.ProviderRequestID,
                    ReferenceNo = row.ReferenceNo,
                    APIUserRequestID = row.APIUserRequestID,
                    ProviderDOCNo = row.ProviderDOCNo,
                    ProviderRefNo = row.ProviderRefNo,
                    AdminGST = row.AdminGST,
                    RetailerTDS = row.RetailerTDS,
                    DistributerTDS = row.DistributerTDS,
                    SuperDistributerTDS = row.SuperDistributerTDS,
                    RetailerGST = row.RetailerGST,
                    DistributerGST = row.DistributerGST,
                    SuperDistributerGST = row.SuperDistributerGST,
                    Bill_CustomerMessageMobileNo = row.Bill_CustomerMessageMobileNo,
                    RetailerCommission = row.RetailerCommission,
                    DistributerCommission = row.DistributerCommission,
                    SuperDistributerCommission = row.SuperDistributerCommission,
                    CashBack = row.CashBack,
                    HitBackCount = row.HitBackCount,
                    //LapuNo = row.LapuNo,
                    Bill_Optional1 = row.Bill_Optional1,
                    Bill_Optional2 = row.Bill_Optional2,
                    Bill_Optional3 = row.Bill_Optional3,
                    Bill_Optional4 = row.Bill_Optional4,
                    Bill_Optional5 = row.Bill_Optional5,
                    Bill_Optional6 = row.Bill_Optional6,
                    DMT_SenderID = row.DMT_SenderID,
                    DMT_SenderName = row.DMT_SenderName,
                    DMT_SenderNumber = row.DMT_SenderNumber,
                    DMT_BeneficiaryID = row.DMT_BeneficiaryID,
                    DMT_BeneficiaryName = row.DMT_BeneficiaryName,
                    DMT_Bank = row.DMT_Bank,
                    DMT_IFSC = row.DMT_IFSC,
                    DMT_BeneficaryNameByBank = row.DMT_BeneficaryNameByBank,
                    DMT_PaymentType = row.DMT_PaymentType,
                    DMT_BillNumber = row.DMT_BillNumber,
                    DMT_OriginalAmount = row.DMT_OriginalAmount,
                    DMT_StatusCode = row.DMT_StatusCode,
                    IsDispute = row.IsDispute,
                    Location = row.Location,
                    SimCardDetails = row.SimCardDetails,
                    Through = row.Through,
                    IPAddress = row.IPAddress,
                    Response = row.Response,
                    Circle = row.Circle,
                    DistributerUserID = row.DistributerUserID,
                    SuperDistributerUserID = row.SuperDistributerUserID,
                    ComplaintStatus = row.ComplaintStatus,
                    AdminCommission = row.AdminCommission,
                    CardType = row.CardType,
                    StateID = row.StateID,
                    IsROffer = row.IsROffer,
                    IsOnlineDMT = row.IsOnlineDMT,
                    IsProviderLowBalance = row.IsProviderLowBalance,
                    ChangeStatus = row.ChangeStatus,
                    ChangeStatusDOC = row.ChangeStatusDOC,
                    AEPSResponseJSON = row.AEPSResponseJSON,
                    PendingProviderStatus = row.PendingProviderStatus,
                    AdminProfit = row.AdminProfit,
                    ResponseType = row.ResponseType,
                    ROfferCommission = row.ROfferCommission,
                    RetailerCommissionHold = row.RetailerCommissionHold,
                    DistributerCommissionHold = row.DistributerCommissionHold,
                    SuperDistributerCommissionHold = row.SuperDistributerCommissionHold,
                    SettlementAtProviderEnd = row.SettlementAtProviderEnd,
                    BBPS_CustomerName = row.BBPS_CustomerName,
                    DOC = row.DOC,
                    UpdatedDOC = row.UpdatedDOC,
                    DOCInt = row.DOCInt,
                };
                tranList.Add(totalitem);
            }
            return tranList;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForDMTFilter(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();

            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.MoneyTransfer).ToList();
            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                        && TransactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)
                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                 && TransactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)
                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
                }
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID

                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.ViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Logo = operators?.OperatorImagePath.ConcatWebAPIURL(),
                        BeneName = TransactionMaster.DMT_BeneficiaryName,
                        SenderNo = TransactionMaster.DMT_SenderNumber,
                        IFSC = TransactionMaster.DMT_IFSC,
                        BankName = TransactionMaster.DMT_Bank,
                        Bank = TransactionMaster.DMT_Bank,
                        SenderName = TransactionMaster.DMT_SenderName,
                        BeneficaryNameByBank = TransactionMaster.DMT_BeneficaryNameByBank,
                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",
                        ReferenceNo = (TransactionMaster.Status == StatusType.Failure && ShowTxnIDinFailure.EqualsIgnoreCase("Yes")) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                        PaymentType = TransactionMaster.DMT_PaymentType,
                        Through = TransactionMaster.Through,

                        DOC = TransactionMaster.DOC.DOCFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName ?? "",

                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        GST = TransactionMaster.RetailerGST,
                        TDS = TransactionMaster.RetailerTDS,
                        DistributerGST = TransactionMaster.DistributerGST,
                        DistributerTDS = TransactionMaster.DistributerTDS,
                        SuperDistributerGST = TransactionMaster.SuperDistributerGST,
                        SuperDistributerTDS = TransactionMaster.SuperDistributerTDS,
                        CashBack = TransactionMaster.CashBack,
                        AdminCommission = TransactionMaster.AdminCommission,
                        Remark = TransactionMaster.Bill_Optional6,
                        Charge = TransactionMaster.Charge,
                        NetCommission = TransactionMaster.NetCommission
                    }).ToList();

            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (txReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                if (txReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                list = (from item in list
                        where fAmount <= item.Amount && item.Amount <= tAmount
                        select item).ToList();
            }

            return list;
        }

        internal List<Response.TransactionMaster.RechargeReportViewModel> SelectForRechargeFilterAndExcelDownload(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.RechargeReportViewModel> list = new List<Response.TransactionMaster.RechargeReportViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();

            List<UserMaster> usersList = new UserDAL().Select();

            /*int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway
            ,(int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.CashDeposit,(int)ServiceCode.CreditCardPayment,(int)ServiceCode.WalletTopUp
            ,(int)ServiceCode.UPITransfer,(int)ServiceCode.AEPSOnboarding,  (int)ServiceCode.MATMOnboarding,(int)ServiceCode.VendorPay};*/

            int[] ids = new int[] { (int)ServiceCode.Mobile, (int)ServiceCode.Dth, (int)ServiceCode.DTHBooking };


            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID > 0 && !ids.Contains(x.ServiceID)).ToList();

            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                       && ids.Contains(TransactionMaster.ServiceID)
                                       && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                && ids.Contains(TransactionMaster.ServiceID)
                                && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).ToList();
                }
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID
                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.RechargeReportViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Logo = operators?.OperatorImagePath?.ConcatWebAPIURL(),
                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",

                        ReferenceNo = (TransactionMaster.Status == StatusType.Failure && ShowTxnIDinFailure.EqualsIgnoreCase("Yes")) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,
                        Through = TransactionMaster.Through,
                        DOC = TransactionMaster.DOC.DOCFormatDate(),
                        UpdatedDOC = TransactionMaster.UpdatedDOC.LongFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName,
                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        CashBack = TransactionMaster.CashBack,
                        AdminCommission = TransactionMaster.AdminCommission,
                        AdminProfit = TransactionMaster.AdminProfit,
                        ResponseType = TransactionMaster.ResponseType,
                        ROfferCommission = TransactionMaster.ROfferCommission,
                        ProviderRefNo = TransactionMaster.ProviderRefNo,
                        OrignalFirmName = User?.FirmName,
                        GST = TransactionMaster.RetailerGST,
                        TDS = TransactionMaster.RetailerTDS,
                        DistributerGST = TransactionMaster.DistributerGST,
                        DistributerTDS = TransactionMaster.DistributerTDS,
                        SuperDistributerGST = TransactionMaster.SuperDistributerGST,
                        SuperDistributerTDS = TransactionMaster.SuperDistributerTDS,
                        Latitude = TransactionMaster.SimCardDetails,
                        Longitude = TransactionMaster.Location,
                        Optional1 = TransactionMaster.Bill_Optional1,
                        NetCommission = TransactionMaster.NetCommission

                    }).ToList();

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == UserID
                            select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == UserID
                            select item).ToList();
                }
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && txReportRequest.UserID != 0)
            {
                int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(txReportRequest.UserID);
                if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == txReportRequest.UserID
                            select item).ToList();
                }
                else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == txReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == txReportRequest.UserID
                            select item).ToList();
                }
            }
            return list;
        }

        internal List<Response.TransactionMaster.RechargeReportViewModel> SelectBBPSForRechargeFilterAndExcelDownload(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.RechargeReportViewModel> list = new List<Response.TransactionMaster.RechargeReportViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();

            List<UserMaster> usersList = new UserDAL().Select();

            int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway
            ,(int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.CashDeposit,(int)ServiceCode.CreditCardPayment,(int)ServiceCode.WalletTopUp
            ,(int)ServiceCode.UPITransfer,(int)ServiceCode.AEPSOnboarding,  (int)ServiceCode.MATMOnboarding,(int)ServiceCode.VendorPay,(int)ServiceCode.Dth,
            (int)ServiceCode.Mobile};

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID > 0 && !ids.Contains(x.ServiceID)).ToList();

            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                       && !ids.Contains(TransactionMaster.ServiceID)
                                       && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                && !ids.Contains(TransactionMaster.ServiceID)
                                && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).ToList();
                }
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID
                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.RechargeReportViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        AgentID = User?.AliasName,
                        Name = User?.FirmName,
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Logo = operators?.OperatorImagePath?.ConcatWebAPIURL(),
                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",

                        ReferenceNo = (TransactionMaster.Status == StatusType.Failure && ShowTxnIDinFailure.EqualsIgnoreCase("Yes")) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,
                        Through = TransactionMaster.Through,
                        DOC = TransactionMaster.DOC.DOCFormatDate(),
                        UpdatedDOC = TransactionMaster.UpdatedDOC.LongFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName,
                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        CashBack = TransactionMaster.CashBack,
                        AdminCommission = TransactionMaster.AdminCommission,
                        AdminProfit = TransactionMaster.AdminProfit,
                        ResponseType = TransactionMaster.ResponseType,
                        ROfferCommission = TransactionMaster.ROfferCommission,
                        ProviderRefNo = TransactionMaster.ProviderRefNo,
                        OrignalFirmName = User?.FirmName,
                        GST = TransactionMaster.RetailerGST,
                        TDS = TransactionMaster.RetailerTDS,
                        DistributerGST = TransactionMaster.DistributerGST,
                        DistributerTDS = TransactionMaster.DistributerTDS,
                        SuperDistributerGST = TransactionMaster.SuperDistributerGST,
                        SuperDistributerTDS = TransactionMaster.SuperDistributerTDS,
                        Latitude = TransactionMaster.SimCardDetails,
                        Longitude = TransactionMaster.Location,
                        Optional1 = TransactionMaster.Bill_Optional1,
                        NetCommission = TransactionMaster.NetCommission,
                        BillerID = TransactionMaster.Bill_Optional5,

                    }).ToList();

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == UserID
                            select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == UserID
                            select item).ToList();
                }
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && txReportRequest.UserID != 0)
            {
                int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(txReportRequest.UserID);
                if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == txReportRequest.UserID
                            select item).ToList();
                }
                else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == txReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == txReportRequest.UserID
                            select item).ToList();
                }
            }
            return list;
        }


        internal List<Response.TransactionMaster.ViewModel> SelectForDMTFilterAndExcelDownload(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ComplaintMaster> complaintlist = new ComplaintMasterDal().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.MoneyTransfer).ToList();
            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                        && TransactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                       && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).ToList();
                    tranList = SelectByArchive(tranListArchive);
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                 && TransactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)
                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).ToList();
                }
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID
                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    join complaint in complaintlist on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                    from CLeft in comp.DefaultIfEmpty()
                    select new Response.TransactionMaster.ViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        UserID = TransactionMaster.UserID,
                        WalletID = TransactionMaster.WalletID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        AgentID = User?.AliasName,
                        AgentName = User?.FirmName,
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,

                        Logo = operators?.OperatorImagePath.ConcatWebAPIURL(),
                        BeneName = TransactionMaster.DMT_BeneficiaryName,
                        SenderNo = TransactionMaster.DMT_SenderNumber,
                        IFSC = TransactionMaster.DMT_IFSC,
                        BankName = TransactionMaster.DMT_Bank,
                        Bank = TransactionMaster.DMT_Bank,
                        SenderName = TransactionMaster.DMT_SenderName,
                        BeneficaryNameByBank = TransactionMaster.DMT_BeneficaryNameByBank,
                        OpeningBalance = TransactionMaster.OpeningBalance,
                        Amount = TransactionMaster.Amount,
                        DebitAmount = TransactionMaster.DebitAmount,
                        ClosingBalance = TransactionMaster.ClosingBalance,
                        ProviderClosingBalance = TransactionMaster.ProviderClosingBalance,
                        Status = TransactionMaster.Status,
                        ComplaintStatus = CLeft?.ComplaintStatus ?? "",
                        ReferenceNo = (TransactionMaster.Status == StatusType.Failure && ShowTxnIDinFailure.EqualsIgnoreCase("Yes")) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                        PaymentType = TransactionMaster.DMT_PaymentType,
                        Through = TransactionMaster.Through,

                        DOC = TransactionMaster.DOC.DOCFormatDate(),
                        ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                        OperatorID = TransactionMaster.OperatorID,
                        ServiceID = TransactionMaster.ServiceID,
                        ProviderRequestID = TransactionMaster.ProviderRequestID,
                        ProviderID = TransactionMaster.ProviderID,
                        ProviderName = provider?.ProviderName ?? "",
                        ProviderCommission = TransactionMaster.ProviderCommission,
                        IPAddress = TransactionMaster.IPAddress,
                        APIUserRequestID = TransactionMaster.APIUserRequestID,
                        Circle = TransactionMaster.Circle ?? "",
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        DistributerUserID = TransactionMaster.DistributerUserID,
                        GST = TransactionMaster.RetailerGST,
                        TDS = TransactionMaster.RetailerTDS,
                        DistributerGST = TransactionMaster.DistributerGST,
                        DistributerTDS = TransactionMaster.DistributerTDS,
                        SuperDistributerGST = TransactionMaster.SuperDistributerGST,
                        SuperDistributerTDS = TransactionMaster.SuperDistributerTDS,
                        CashBack = TransactionMaster.CashBack,
                        AdminCommission = TransactionMaster.AdminCommission,
                        Latitude = TransactionMaster.SimCardDetails,
                        Longitude = TransactionMaster.Location,
                        Charge = TransactionMaster.Charge,
                        NetCommission = TransactionMaster.NetCommission
                    }).ToList();

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == UserID
                            select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == UserID
                            select item).ToList();
                }
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && txReportRequest.UserID != 0)
            {
                int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(txReportRequest.UserID);
                if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == txReportRequest.UserID
                            select item).ToList();
                }
                else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == txReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == txReportRequest.UserID
                            select item).ToList();
                }
            }

            if (txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (txReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;
                if (txReportRequest.CriteriaTextAmount.EqualsIgnoreCase(""))
                    return list;

                decimal fAmount = Convert.ToDecimal(txReportRequest.CriteriaText);
                decimal tAmount = Convert.ToDecimal(txReportRequest.CriteriaTextAmount);

                list = (from item in list
                        where item.Amount >= fAmount && item.Amount <= tAmount
                        select item).ToList();
            }

            return list;
        }

        internal int GetTransactionCount(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            /*int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway
            ,(int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.CashDeposit,(int)ServiceCode.CreditCardPayment,(int)ServiceCode.WalletTopUp
            ,(int)ServiceCode.UPITransfer,(int)ServiceCode.AEPSOnboarding,  (int)ServiceCode.MATMOnboarding,(int)ServiceCode.VendorPay};*/

            int[] ids = new int[] { (int)ServiceCode.Mobile, (int)ServiceCode.Dth, (int)ServiceCode.DTHBooking };


            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                        && ids.Contains(TransactionMaster.ServiceID)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).ToList();
                    return tranListArchive.Count();
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                 && ids.Contains(TransactionMaster.ServiceID)
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).ToList();
                    return tranList.Count();
                }
            }
        }

        internal int GetBBPSTransactionCount(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway
            ,(int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.CashDeposit,(int)ServiceCode.CreditCardPayment,(int)ServiceCode.WalletTopUp
            ,(int)ServiceCode.UPITransfer,(int)ServiceCode.AEPSOnboarding,  (int)ServiceCode.MATMOnboarding,(int)ServiceCode.VendorPay,(int)ServiceCode.Dth,
            (int)ServiceCode.Mobile};

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (txReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= FromDate
                                       && TransactionMaster.DOCInt <= ToDate
                                        && !ids.Contains(TransactionMaster.ServiceID)
                                       && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                       && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                       && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                       && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                       && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                       && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                       && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                       && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                       && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).ToList();
                    return tranListArchive.Count();
                }
                else
                {
                    tranList = (from TransactionMaster in context.TransactionMaster
                                where TransactionMaster.DOCInt >= FromDate
                                && TransactionMaster.DOCInt <= ToDate
                                 && !ids.Contains(TransactionMaster.ServiceID)
                                && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                                && (txReportRequest.ServiceID == 0 ? 2 > 1 : TransactionMaster.ServiceID == txReportRequest.ServiceID)
                                && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                                && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                                && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                                && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                                && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                                && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == UserID :
                                GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : TransactionMaster.UserID == UserID)

                                && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? TransactionMaster.SuperDistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? TransactionMaster.DistributerUserID == txReportRequest.UserID :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)

                                orderby TransactionMaster.TransactionMasterID descending
                                select TransactionMaster).ToList();
                    return tranList.Count();
                }
            }
        }

        internal int GetDMTTransactionCount(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<TransactionMaster> tranList = new List<TransactionMaster>();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from TransactionMaster in context.TransactionMaster
                            where TransactionMaster.DOCInt >= FromDate
                            && TransactionMaster.DOCInt <= ToDate
                            && TransactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                            && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                            && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                            && (!txReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == txReportRequest.Status : 2 > 1)
                            && (!txReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == txReportRequest.Complaint : 2 > 1)
                            && (txReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= txReportRequest.MinAmount : 2 > 1)
                            && (txReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= txReportRequest.MaxAmount : 2 > 1)

                            orderby TransactionMaster.TransactionMasterID descending
                            select TransactionMaster).ToList();

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        tranList = (from item in tranList
                                    where item.SuperDistributerUserID == UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.Distributer)
                    {
                        tranList = (from item in tranList
                                    where item.DistributerUserID == UserID
                                    select item).ToList();
                    }
                }
                if (Util.isRoleAdminOrSupport(UserTypeID) && txReportRequest.UserID != 0)
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(txReportRequest.UserID);
                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        tranList = (from item in tranList
                                    where item.SuperDistributerUserID == txReportRequest.UserID
                                    select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        tranList = (from item in tranList
                                    where item.DistributerUserID == txReportRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        tranList = (from item in tranList
                                    where item.UserID == txReportRequest.UserID
                                    select item).ToList();
                    }
                }
                return tranList.Count();
            }
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForROfferRechargeFilterAndExcelDownload(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.Mobile).ToList();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from TransactionMaster in context.TransactionMaster
                            where TransactionMaster.DOCInt >= FromDate
                            && TransactionMaster.DOCInt <= ToDate
                             && TransactionMaster.Status == StatusType.Success
                             && TransactionMaster.ROfferCommission > 0
                            && TransactionMaster.ServiceID == (int)ServiceCode.Mobile
                            && (isReatilerAPIClient(UserTypeID) ? TransactionMaster.UserID == UserID : 2 > 1)
                            && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                            && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                            && (txReportRequest.UserID == 0 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)
                            orderby TransactionMaster.TransactionMasterID descending
                            select TransactionMaster).ToList();
            }
            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID
                    join provider in providerList on
                    TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()
                    select new Response.TransactionMaster.ViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Amount = TransactionMaster.Amount,
                        ROfferCommission = TransactionMaster.ROfferCommission,
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        ProviderName = provider?.ProviderName,
                        DOC = TransactionMaster.DOC.LongFormatDate()
                    }).ToList();
            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForROfferRechargeFilter(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.Mobile).ToList();

            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from TransactionMaster in context.TransactionMaster
                            where TransactionMaster.DOCInt >= FromDate
                            && TransactionMaster.DOCInt <= ToDate
                            && TransactionMaster.Status == StatusType.Success
                             && TransactionMaster.ROfferCommission > 0
                             && TransactionMaster.ServiceID == (int)ServiceCode.Mobile
                      && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                         && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                         && (txReportRequest.UserID == 0 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)
                            orderby TransactionMaster.TransactionMasterID descending
                            select TransactionMaster).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID
                    join provider in providerList on
                      TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()

                    select new Response.TransactionMaster.ViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Amount = TransactionMaster.Amount,
                        ROfferCommission = TransactionMaster.ROfferCommission,
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        ProviderName = provider?.ProviderName,
                        DOC = TransactionMaster.DOC.LongFormatDate()
                    }).ToList();

            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForROfferAndHeaderSummary(ReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> tranList = new List<TransactionMaster>();
            List<UserMaster> usersList = new UserDAL().Select();

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.Mobile).ToList();
            string ShowTxnIDinFailure = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowTxnIDinFailure), "Yes");

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from TransactionMaster in context.TransactionMaster
                            where TransactionMaster.DOCInt >= FromDate
                            && TransactionMaster.DOCInt <= ToDate
                            && TransactionMaster.Status == StatusType.Success
                             && TransactionMaster.ROfferCommission > 0
                             && TransactionMaster.ServiceID == (int)ServiceCode.Mobile
                      && (txReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == txReportRequest.ProviderID)
                         && (txReportRequest.OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == txReportRequest.OperatorID)
                          && (txReportRequest.UserID == 0 ? 2 > 1 : TransactionMaster.UserID == txReportRequest.UserID)
                            orderby TransactionMaster.TransactionMasterID descending
                            select TransactionMaster).ToList();
            }

            list = (from TransactionMaster in tranList
                    join User in usersList on TransactionMaster.UserID equals User.UserID
                    join operators in operatorList on TransactionMaster.OperatorID equals operators.OperatorID
                    join provider in providerList on
                      TransactionMaster.ProviderID equals provider.ProviderID into g
                    from provider in g.DefaultIfEmpty()

                    select new Response.TransactionMaster.ViewModel
                    {
                        TransactionMasterID = TransactionMaster.TransactionMasterID,
                        FirmName = User?.FirmName.ConcatNameAliasName(User?.AliasName),
                        UserMobile = User?.MobileNumber,
                        ConsumerNumber = TransactionMaster.ConsumerNumber,
                        OperatorName = operators?.OperatorName,
                        Amount = TransactionMaster.Amount,
                        Status = TransactionMaster.Status,
                        ROfferCommission = TransactionMaster.ROfferCommission,
                        RetailerCommission = TransactionMaster.RetailerCommission,
                        DistributerCommission = TransactionMaster.DistributerCommission,
                        SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                        ProviderName = provider?.ProviderName,
                        DOC = TransactionMaster.DOC.LongFormatDate()
                    }).ToList();

            return list;
        }

        internal List<TransactionMaster> SelectTransactionDatewise(int FromInt, int ToInt, int ServiceID)
        {
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.TransactionMaster.Where(c => c.DOCInt >= FromInt &&
                                         c.DOCInt <= ToInt &&
                                         c.Status == StatusType.Success
                                         && c.ServiceID == ServiceID).ToList();
            }
            return list;
        }

        internal List<RemainderViewModel> SelectRemainderTransaction(int OperatorID, decimal Amount, int Days)
        {
            int LessTheeDays = Days - 3;
            int FromDOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-LessTheeDays)).ToString(Messages.DOCIntegerFomrat));
            int ToDOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-Days)).ToString(Messages.DOCIntegerFomrat));

            List<RemainderViewModel> list = new List<RemainderViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from Trans in context.TransactionMaster
                        join operators in context.OperatorMaster on Trans.OperatorID equals operators.OperatorID
                        where Trans.DOCInt >= FromDOCInt
                        && Trans.DOCInt <= ToDOCInt
                        && Trans.OperatorID == OperatorID
                        && Trans.Amount == Amount

                        select new RemainderViewModel
                        {
                            ConsumerNumber = Trans.ConsumerNumber,
                            OperatorName = operators.OperatorName,
                            Date = Trans.DOC.ShortFormatDate()
                        }).ToList();
            }
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForLeadGenerationExcelDownload(LeadGenerationReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.LeadGeneration).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.LeadGeneration
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
            }
            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        UserID = t.UserID,
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = (t.Status == StatusType.Failure) ? t.Description : t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        ProviderRequestID = t.ProviderRequestID
                    }).ToList();

            return list;
        }

        internal Response.TransactionMaster.HeaderViewModel LeadGenerationHeaderSummary(LeadGenerationReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Tran in context.TransactionMaster
                        where Tran.DOCInt >= FromDate
                        && Tran.DOCInt <= ToDate
                        && Tran.ServiceID == (int)ServiceCode.LeadGeneration
                        && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                        && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                        && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                        && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                        GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                        && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                        GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                        && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                        orderby Tran.TransactionMasterID descending
                        select Tran).ToList();
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForLeadGenerationReport(LeadGenerationReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.LeadGeneration).ToList();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.LeadGeneration
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }

            list = (from t in transactionMaster
                    join User in usersList on t.UserID equals User.UserID
                    join provider in providerList on t.ProviderID equals provider.ProviderID
                    join op in operatorList on t.OperatorID equals op.OperatorID
                    select new Response.TransactionMaster.ViewModel
                    {
                        FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                        DOC = t.DOC.LongFormatDate(),
                        UpdatedDOC = t.UpdatedDOC.LongFormatDate(),
                        DistributerUserID = t.DistributerUserID,
                        SuperDistributerUserID = t.SuperDistributerUserID,
                        OperatorName = op.OperatorName,
                        OpeningBalance = t.OpeningBalance,
                        Amount = t.Amount,
                        DebitAmount = t.DebitAmount,
                        ClosingBalance = t.ClosingBalance,
                        ProviderName = provider.ProviderName,
                        Status = t.Status,
                        Description = t.Description,
                        RetailerCommission = t.RetailerCommission,
                        DistributerCommission = t.DistributerCommission,
                        SuperDistributerCommission = t.SuperDistributerCommission,
                        IPAddress = t.IPAddress,
                        ComplaintStatus = t.ComplaintStatus,
                        Through = t.Through,
                        TransactionMasterID = t.TransactionMasterID,
                        ReferenceNo = t.ReferenceNo,
                        ConsumerNumber = t.ConsumerNumber,
                        ProviderRequestID = t.ProviderRequestID,
                    }).ToList();

            return list;
        }

        internal int SelectForLeadGenerationCount(LeadGenerationReportRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<TransactionMaster> tranList = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                tranList = (from Tran in context.TransactionMaster
                            where Tran.DOCInt >= FromDate
                            && Tran.DOCInt <= ToDate
                            && Tran.ServiceID == (int)ServiceCode.LeadGeneration
                                     && (isReatilerAPIClient(UserTypeID) ? Tran.UserID == UserID : 2 > 1)
                                     && (txReportRequest.ProviderID == 0 ? 2 > 1 : Tran.ProviderID == txReportRequest.ProviderID)
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Distributer ? Tran.DistributerUserID == UserID :
                                     GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : Tran.UserID == UserID)
                                     && (GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.SuperDistributer ? Tran.SuperDistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Distributer ? Tran.DistributerUserID == txReportRequest.UserID :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                     GetSelectedUserRole(UserTypeID, txReportRequest.UserID) == -1 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     && (txReportRequest.Criteria.EqualsIgnoreCase("TransactionID") ? Tran.ReferenceNo == txReportRequest.CriteriaText : 2 > 1)
                            select Tran).ToList();
            }

            return tranList.Count();
        }

        internal List<Response.SDK.ViewModel> SelectForLast10ByAgentCode(string AgentCode)
        {
            List<Response.SDK.ViewModel> list = new List<Response.SDK.ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from TransactionMaster in context.TransactionMaster
                        where TransactionMaster.Bill_Optional5 == AgentCode
                        orderby TransactionMaster.TransactionMasterID descending

                        select new Response.SDK.ViewModel
                        {
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,
                            Amount = TransactionMaster.Amount,
                            Status = TransactionMaster.Status,
                            ReferenceNo = TransactionMaster.ReferenceNo,
                            DOC = TransactionMaster.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();
            }
            return list;
        }
    }
}