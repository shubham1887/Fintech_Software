using Microsoft.EntityFrameworkCore;
using project.Requests.Transaction;
using project.Requests.TransactionReport;
using project.Response.TransactionMaster;
using Project.Model;
using Project.Processing;
using Project.Requests.Ledger;
using Project.Requests.Provider;
using Project.Requests.Transaction;
using Project.Requests.TransactionReport;
using Project.Response.Ledger;
using Project.Response.TransactionMaster;
using Project.Response.TransactionReport;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class TransactionDAL
    {

        internal int Insert(TransactionMaster transactionMaster)
        {
            using (var context = new DatabaseContext())
            {
                context.TransactionMaster.Add(transactionMaster);
                context.SaveChanges();
                return transactionMaster.TransactionMasterID;
            }
        }

        internal int Update(TransactionMaster objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.TransactionMaster.Update(objModel);
                context.SaveChanges();
                return objModel.TransactionMasterID;
            }
        }

        internal List<TransactionMaster> SelectHoldCommission(Redeemcommission selectRequest, int UserTypeID)
        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();
            DateTime date = new DateTime(DateTime.Now.Year, selectRequest.Month, DateTime.Now.Day);
            string lastyearmonth = FromDate.Year.ToString();

            if (selectRequest.Month == 12)
            {
                date = new DateTime(DateTime.Now.Year - 1, selectRequest.Month, 1);

                int yearint = DateTime.Now.Year - 1;
                lastyearmonth = yearint.ToString();
            }
            string month = FromDate.ToString("MMMM");

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<TransactionMaster> list = new List<TransactionMaster>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (Util.isRoleRetailerOrClientOrAPIUser(UserTypeID))
                {
                    list = context.TransactionMaster.Where(txn => txn.DOCInt >= From && txn.DOCInt <= To
                            && txn.UserID == selectRequest.UserID
                            && txn.Status == StatusType.Success
                            && txn.RetailerCommissionHold > 0M
                      ).ToList();
                }

                if (UserTypeID == (int)UserType.Distributer)
                {
                    list = context.TransactionMaster.Where(txn => txn.DOCInt >= From && txn.DOCInt <= To
                           && txn.Status == StatusType.Success
                           && txn.DistributerUserID == selectRequest.UserID
                               && txn.DistributerCommissionHold > 0M
                     ).ToList();
                }

                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = context.TransactionMaster.Where(txn => txn.DOCInt >= From && txn.DOCInt <= To
                          && txn.Status == StatusType.Success
                          && txn.SuperDistributerUserID == selectRequest.UserID
                              && txn.SuperDistributerCommissionHold > 0M
                    ).ToList();
                }

                return list;
            }
        }

        internal TransactionMaster SelectByAPIUserRequestID(string aPIUserRequestID, int currentUserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.FirstOrDefault(T => T.UserID == currentUserID && T.APIUserRequestID == aPIUserRequestID);
            }
        }

        internal List<TransactionMaster> SelectPendingUPITransferTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt <= today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.UPITransfer
                ).ToList();
            }
            return list;
        }

        internal List<TransactionMaster> SelectPendingPayoutTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.Payout
                ).ToList();
            }
            return list;
        }

        internal List<TransactionMaster> SelectPendingVendorPayTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.VendorPay
                ).ToList();
            }
            return list;
        }

        internal List<TransactionMaster> SelectPendingCMSTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.CMS
                ).ToList();
            }
            return list;
        }

        internal List<TransactionMaster> SelectPendingPanTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-1)).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt >= today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.PanCard
                ).ToList();
            }
            return list;
        }

        internal List<TransactionMaster> SelectPendingCreditCardTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.CreditCardPayment
                ).ToList();
            }
            return list;
        }



        internal string SelectConsumerNumberByTransactionMasterID(int transactionMasterID)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return context.TransactionMaster.Find(transactionMasterID).ConsumerNumber;
        }

        internal Response.ComplaintResponse.ViewModel SelectForComplaint(int ID)
        {
            List<Project.Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();

            Response.ComplaintResponse.ViewModel ObjModel = new Response.ComplaintResponse.ViewModel();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                ObjModel = (from transaction in context.TransactionMaster
                            join op in context.OperatorMaster on transaction.OperatorID equals (op.OperatorID)
                            join s in context.ServiceMaster on transaction.ServiceID equals (s.ServiceID)
                            join comp in context.ComplaintMaster on transaction.TransactionMasterID equals comp.TransactionMasterID into CompDet
                            from CompDetails in CompDet.DefaultIfEmpty()

                            where transaction.TransactionMasterID == ID
                            select new Response.ComplaintResponse.ViewModel
                            {
                                ComplaintMasterID = CompDetails == null ? 0 : CompDetails.ComplaintMasterID,
                                CustomerRemark = CompDetails.CustomerRemark ?? "",
                                AdminRemark = CompDetails.AdminRemark ?? "",
                                Logo = op.OperatorImagePath.ConcatWebAPIURL(),
                                TransactionMasterID = transaction.TransactionMasterID,
                                ConsumerNumber = transaction.ConsumerNumber,
                                ProviderRefNo = transaction.ProviderRefNo,
                                Amount = transaction.Amount,
                                Operator = op.OperatorName,
                                IsBBPS = op.IsBBPS,
                                Service = s.Name,
                                TransactionDate = transaction.DOC.LongFormatDate(),
                                ComplaintStatus = StatusType.Process
                            }
                            ).FirstOrDefault();
            }
            return ObjModel;
        }

        internal TransactionMaster SelectByID(int ID)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context.TransactionMaster.Find(ID);
        }

        internal int SelectConsumedLimitBySenderID(string SenderID)
        {
            int limit = 0;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                ConsumedLimit result = (from Obj in context.TransactionMaster
                                        where Obj.DOC.Month == DateTime.Now.Month &&
                                        Obj.DOC.Year == DateTime.Now.Year &&
                                        Obj.DMT_SenderID == SenderID &&
                                        Obj.OperatorID != (int)OperatorCode.Verify_DMT &&
                                        (Obj.Status == StatusType.Success || Obj.Status == StatusType.Process)
                                        group Obj by Obj.DMT_SenderID into g
                                        select new ConsumedLimit
                                        {
                                            Limit = g.Sum(pc => pc.Amount),
                                        }).FirstOrDefault();

                if (result == null)
                {
                    limit = 0;
                    return 0;
                }

                limit = Convert.ToInt32(result.Limit);
                return limit;
            }
        }

        internal TransactionMaster CheckDuplicateTransactionForAEPS(string ConsumerNumber)
        {
            DateTime docLess30 = DateTime.Now.AddMinutes(-30);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        orderby transactionMaster.DOC descending
                        where transactionMaster.DOC > docLess30 &&
                            transactionMaster.ConsumerNumber == ConsumerNumber &&
                            transactionMaster.ServiceID == (int)ServiceCode.AEPS &&
                            transactionMaster.Status == StatusType.Success
                        select transactionMaster).FirstOrDefault();
            }
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForFilterAndHeaderSummary(ReportRequest transactionReportRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> Translist = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway,
            (int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.POS,(int)ServiceCode.UPITransfer,
            (int)ServiceCode.CMS,(int)ServiceCode.MATMOnboarding,(int)ServiceCode.AEPSOnboarding,(int)ServiceCode.AccountOpening,
            (int)ServiceCode.PanCard
            };

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (transactionReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= From
                                       && TransactionMaster.DOCInt <= To
                                       && !ids.Contains(TransactionMaster.ServiceID)
                                       && (transactionReportRequest.ServiceID != 0 ? TransactionMaster.ServiceID == transactionReportRequest.ServiceID : 2 > 1)
                                       && (!transactionReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == transactionReportRequest.Status : 2 > 1)
                                       && (transactionReportRequest.OperatorID != 0 ? TransactionMaster.OperatorID == transactionReportRequest.OperatorID : 2 > 1)
                                       && (transactionReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == transactionReportRequest.ProviderID)
                                       && (transactionReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= transactionReportRequest.MinAmount : 2 > 1)
                                       && (transactionReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= transactionReportRequest.MaxAmount : 2 > 1)
                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).ToList();
                    Translist = new TransactionReportDAL().SelectByArchive(tranListArchive);
                }
                else
                {
                    Translist = (from TransactionMaster in context.TransactionMaster
                                 where TransactionMaster.DOCInt >= From
                                 && TransactionMaster.DOCInt <= To
                                 && !ids.Contains(TransactionMaster.ServiceID)
                                 && (transactionReportRequest.ServiceID != 0 ? TransactionMaster.ServiceID == transactionReportRequest.ServiceID : 2 > 1)
                                 && (!transactionReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == transactionReportRequest.Status : 2 > 1)
                                 && (transactionReportRequest.OperatorID != 0 ? TransactionMaster.OperatorID == transactionReportRequest.OperatorID : 2 > 1)
                                 && (transactionReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == transactionReportRequest.ProviderID)
                                 && (transactionReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= transactionReportRequest.MinAmount : 2 > 1)
                                 && (transactionReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= transactionReportRequest.MaxAmount : 2 > 1)
                                 orderby TransactionMaster.TransactionMasterID descending
                                 select TransactionMaster).ToList();
                }
                list = (from TransactionMaster in Translist

                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,
                            ServiceID = TransactionMaster.ServiceID,
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            APIUserRequestID = TransactionMaster.APIUserRequestID,
                            OpeningBalance = TransactionMaster.OpeningBalance,
                            ReferenceNo = TransactionMaster.ReferenceNo,
                            Amount = TransactionMaster.Amount,
                            DebitAmount = TransactionMaster.DebitAmount,
                            ClosingBalance = TransactionMaster.ClosingBalance,
                            Status = TransactionMaster.Status,
                            DOC = TransactionMaster.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            RetailerCommission = TransactionMaster.RetailerCommission,
                            DistributerCommission = TransactionMaster.DistributerCommission,
                            SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                            Description = TransactionMaster.Description,
                            DistributerUserID = TransactionMaster.DistributerUserID,
                            SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                        }).ToList();
            }

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == userID
                            select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == userID
                            select item).ToList();
                }
                {
                    list = (from item in list
                            where item.UserID == userID
                            select item).ToList();
                }
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && transactionReportRequest.UserID != 0)
            {
                int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(transactionReportRequest.UserID);
                if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == transactionReportRequest.UserID
                            select item).ToList();
                }
                else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == transactionReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == transactionReportRequest.UserID
                            select item).ToList();
                }
            }

            if (transactionReportRequest.ServiceID == 0)// this for recharge report
            {
                list = (from item in list
                        where item.ServiceID != (int)ServiceCode.MoneyTransfer
                        && item.ServiceID != (int)ServiceCode.AEPS
                        && item.ServiceID != (int)ServiceCode.PaymentGateway
                        && item.ServiceID != (int)ServiceCode.Payout
                        && item.ServiceID != (int)ServiceCode.MATM
                        && item.ServiceID != (int)ServiceCode.UPITransfer
                        && item.ServiceID != (int)ServiceCode.MATMOnboarding
                        && item.ServiceID != (int)ServiceCode.AccountOpening
                        && item.ServiceID != (int)ServiceCode.PanCard
                        && item.ServiceID != (int)ServiceCode.AEPSOnboarding
                        && item.ServiceID != (int)ServiceCode.CMS
                        && item.ServiceID != (int)ServiceCode.POS

                        select item).ToList();
            }
            list = list.AsQueryable().WhereDynamic(transactionReportRequest.CriteriaText).ToList();

            return list;
        }

        internal TransactionMaster SelectByConsumerNumberAndServiceID(string VPA, int ServiceID)
        {
            int priousdateInt = Convert.ToInt32((DateTime.Now.AddDays(-30)).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        orderby transactionMaster.DOC descending
                        where transactionMaster.DOCInt >= priousdateInt &&
                            transactionMaster.ConsumerNumber == VPA &&
                            transactionMaster.ServiceID == ServiceID &&
                            transactionMaster.Amount == 1 &&
                            transactionMaster.Status == StatusType.Success
                        select transactionMaster).FirstOrDefault();
            }
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForFilterAndDMTHeaderSummary(ReportRequest transactionReportRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            List<TransactionMaster> Translist = new List<TransactionMaster>();
            List<Archive_TransactionMaster> tranListArchive = new List<Archive_TransactionMaster>();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                if (transactionReportRequest.ReportBy.EqualsIgnoreCase(ReportType.Archive))
                {
                    tranListArchive = (from TransactionMaster in context.Archive_TransactionMaster
                                       where TransactionMaster.DOCInt >= From
                                       && TransactionMaster.DOCInt <= To
                                       && TransactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                       && (!transactionReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == transactionReportRequest.Status : 2 > 1)
                                       && (transactionReportRequest.OperatorID != 0 ? TransactionMaster.OperatorID == transactionReportRequest.OperatorID : 2 > 1)
                                       && (transactionReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == transactionReportRequest.ProviderID)
                                       && (transactionReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= transactionReportRequest.MinAmount : 2 > 1)
                                       && (transactionReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= transactionReportRequest.MaxAmount : 2 > 1)
                                       orderby TransactionMaster.TransactionMasterID descending
                                       select TransactionMaster).ToList();
                    Translist = new TransactionReportDAL().SelectByArchive(tranListArchive);
                }
                else
                {
                    Translist = (from TransactionMaster in context.TransactionMaster
                                 where TransactionMaster.DOCInt >= From
                                 && TransactionMaster.DOCInt <= To
                                 && TransactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                 && (!transactionReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == transactionReportRequest.Status : 2 > 1)
                                 && (transactionReportRequest.OperatorID != 0 ? TransactionMaster.OperatorID == transactionReportRequest.OperatorID : 2 > 1)
                                 && (transactionReportRequest.ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == transactionReportRequest.ProviderID)
                                 && (transactionReportRequest.MinAmount >= 1 ? TransactionMaster.Amount >= transactionReportRequest.MinAmount : 2 > 1)
                                 && (transactionReportRequest.MaxAmount >= 1 ? TransactionMaster.Amount <= transactionReportRequest.MaxAmount : 2 > 1)
                                 orderby TransactionMaster.TransactionMasterID descending
                                 select TransactionMaster).ToList();
                }

                list = (from item in Translist
                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = item.TransactionMasterID,
                            UserID = item.UserID,
                            ServiceID = item.ServiceID,
                            ConsumerNumber = item.ConsumerNumber,
                            OpeningBalance = item.OpeningBalance,
                            Amount = item.Amount,
                            ReferenceNo = item.ReferenceNo,
                            SenderNo = item.DMT_SenderNumber,
                            DebitAmount = item.DebitAmount,
                            ClosingBalance = item.ClosingBalance,
                            Status = item.Status,
                            DOC = item.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            RetailerCommission = item.RetailerCommission,
                            DistributerCommission = item.DistributerCommission,
                            SuperDistributerCommission = item.SuperDistributerCommission,
                            Description = item.Description,
                            DistributerUserID = item.DistributerUserID,
                            SuperDistributerUserID = item.SuperDistributerUserID,
                        }).ToList();
            }

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == userID
                            select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == userID
                            select item).ToList();
                }
                {
                    list = (from item in list
                            where item.UserID == userID
                            select item).ToList();
                }
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && transactionReportRequest.UserID != 0)
            {
                int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(transactionReportRequest.UserID);
                if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == transactionReportRequest.UserID
                            select item).ToList();
                }
                else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == transactionReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == transactionReportRequest.UserID
                            select item).ToList();
                }
            }

            if (transactionReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
            {
                if (transactionReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                if (transactionReportRequest.CriteriaText.EqualsIgnoreCase(""))
                    return list;

                decimal fAmount = Convert.ToDecimal(transactionReportRequest.CriteriaText);
                decimal tAmount = Convert.ToDecimal(transactionReportRequest.CriteriaTextAmount);

                list = (from item in list
                        where fAmount <= item.Amount && item.Amount <= tAmount
                        select item).ToList();
            }

            return list;
        }

        internal TransactionMaster CheckTransactionFor2FA_AEPS(int UserID)
        {
            int priousdateInt = Convert.ToInt32((DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        orderby transactionMaster.DOC descending
                        where transactionMaster.DOCInt == priousdateInt &&
                            transactionMaster.ServiceID == (int)ServiceCode.AEPS &&
                            transactionMaster.UserID == UserID
                        select transactionMaster).FirstOrDefault();
            }
        }

        internal List<Response.TransactionMaster.ViewModel> SelectForFilterAndExcelDownload(ReportRequest transactionReportRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int takeCount = transactionReportRequest.TakeCount;
            if (transactionReportRequest.isLast10 == true)
            {
                From = Convert.ToInt32(DateTime.Now.AddDays(-30).ToString(Messages.DOCIntegerFomrat));
                takeCount = 10;
            }

            int To = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        join provider in context.ProviderMaster on
                        TransactionMaster.ProviderID equals provider.ProviderID into g
                        from provider in g.DefaultIfEmpty()
                        where TransactionMaster.DOCInt >= From
                        && TransactionMaster.DOCInt <= To
                        // && (UserTypeID != (int)UserType.Admin ? TransactionMaster.UserID == userID : 2 > 1)
                        && (transactionReportRequest.ServiceID != 0 ? TransactionMaster.ServiceID == transactionReportRequest.ServiceID : 2 > 1)
                        && (!transactionReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == transactionReportRequest.Status : 2 > 1)
                         && (!transactionReportRequest.Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == transactionReportRequest.Complaint : 2 > 1)
                        orderby TransactionMaster.TransactionMasterID descending

                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,
                            WalletID = TransactionMaster.WalletID,
                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameAliasName(TransactionMaster.UserMaster.AliasName),
                            UserMobile = TransactionMaster.UserMaster.MobileNumber,
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,

                            Logo = TransactionMaster.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
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
                            ComplaintStatus = TransactionMaster.ComplaintStatus,
                            ReferenceNo = (TransactionMaster.Status == StatusType.Failure) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                            PaymentType = TransactionMaster.DMT_PaymentType,
                            Through = TransactionMaster.Through,

                            DOC = TransactionMaster.DOC.LongFormatDate(),
                            ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                            OperatorID = TransactionMaster.OperatorID,
                            ServiceID = TransactionMaster.ServiceID,
                            ProviderRequestID = TransactionMaster.ProviderRequestID,
                            ProviderID = TransactionMaster.ProviderID,
                            ProviderName = provider.ProviderName,
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
            }

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == userID
                            select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == userID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == userID
                            select item).ToList();
                }
            }

            if (!transactionReportRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == transactionReportRequest.Status
                        select item).ToList();
            }

            if (transactionReportRequest.ServiceID != 0)
            {
                list = (from item in list
                        where item.ServiceID == transactionReportRequest.ServiceID
                        select item).ToList();
            }

            if (transactionReportRequest.OperatorID != 0)
            {
                list = (from item in list
                        where item.OperatorID == transactionReportRequest.OperatorID
                        select item).ToList();
            }

            if (UserTypeID == (int)UserType.Admin)
            {
                if (transactionReportRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == transactionReportRequest.UserID
                            select item).ToList();
                }
            }
            if (UserTypeID == (int)UserType.Admin)
            {
                if (transactionReportRequest.ProviderID != 0)
                {
                    list = (from item in list
                            where item.ProviderID == transactionReportRequest.ProviderID
                            select item).ToList();
                }
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            return list;
        }

        internal List<TransactionMaster> SelectTransaction(DateTime currentDateTime, string DMT_IFSC)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            List<TransactionMaster> list = new List<TransactionMaster>();
            return list = context.TransactionMaster.Where(t => t.DOC >= currentDateTime &&
                        t.Status == StatusType.Failure &&
                        t.DMT_IFSC == DMT_IFSC &&
                        t.Description.Contains(Messages.DMTError18)).ToList();
        }

        internal string SelectBenificiaryNameForPDMT(string ConsumerNumber)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            TransactionMaster trans = context.TransactionMaster.OrderByDescending(t => t.TransactionMasterID)
                .FirstOrDefault(t => t.ProviderID == Company.PaytmProviderID &&
                                                                                t.ConsumerNumber == ConsumerNumber &&
                                                                                t.Amount == 1 &&
                                                                                t.Status == StatusType.Success &&
                                                                                t.OperatorID == (int)OperatorCode.Verify_DMT);
            if (trans == null)
            {
                return ConsumerNumber;
            }

            return trans.DMT_BeneficiaryName;
        }

        internal List<ManualSuccessFailViewModel> SelectManualSuccessFail(ManualSuccessFailRequest mainRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ManualSuccessFailViewModel> list = new List<ManualSuccessFailViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        join manualupdate in context.TransactionManualUpdate on
                        TransactionMaster.TransactionMasterID equals manualupdate.TransactionMasterID
                        join providerMaster in context.ProviderMaster on
                        TransactionMaster.ProviderID equals providerMaster.ProviderID
                        join user in context.UserMaster on manualupdate.ManualUpdatedBy equals user.UserID
                        where manualupdate.DOCInt >= From && manualupdate.DOCInt <= To
                        orderby manualupdate.DOC descending

                        select new ManualSuccessFailViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,
                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameMobileAliasName(TransactionMaster.UserMaster.MobileNumber, TransactionMaster.UserMaster.AliasName),
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,
                            ProviderName = providerMaster.ProviderName,
                            ProviderID = providerMaster.ProviderID,
                            Amount = TransactionMaster.Amount,
                            OldStatus = manualupdate.OldStatus,
                            NewStatus = manualupdate.NewStatus,
                            ReferenceNo = manualupdate.ProviderrefNo,
                            DOC = TransactionMaster.DOC.LongFormatDate(),
                            ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalMinutes, 2),
                            UpdatedBy = user.FirmName,
                            UpdatedDOC = manualupdate.DOC.LongFormatDate(),
                        }).ToList();

                if (mainRequest.ProviderID > 0)
                {
                    list = (from item in list
                            where item.ProviderID == mainRequest.ProviderID
                            select item).ToList();
                }
            }
            return list;
        }

        internal List<PastSuccessFailViewModel> SelectPastSuccessFail(PastSuccessFailRequest mainRequest, int UserID)
        {
            int DateInt = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            List<PastSuccessFailViewModel> list = new List<PastSuccessFailViewModel>();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().ToList();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var MainList = (from txn in context.TransactionMaster
                                where txn.DOCInt != DateInt
                                orderby txn.UpdatedDOC descending

                                select new
                                {
                                    TransactionMasterID = txn.TransactionMasterID,
                                    UserID = txn.UserID,
                                    ConsumerNumber = txn.ConsumerNumber,
                                    Amount = txn.Amount,
                                    DebitAmount = txn.DebitAmount,
                                    Status = txn.Status,
                                    ReferenceNo = txn.ReferenceNo,
                                    ProviderID = txn.ProviderID,
                                    OperatorID = txn.OperatorID,

                                    DOC = txn.DOC,
                                    UpdatedDOC = txn.UpdatedDOC,
                                    UpdatedDOCInt = Convert.ToInt32(Convert.ToDateTime(txn.UpdatedDOC).ToString(Messages.DOCIntegerFomrat))
                                }).ToList();

                list = (from txn in MainList
                        join user in usersList on txn.UserID equals user.UserID
                        join Operator in operatorList on txn.OperatorID equals Operator.OperatorID
                        join providerMaster in providerList on txn.ProviderID equals providerMaster.ProviderID into
                        PDetails
                        from leftP in PDetails.DefaultIfEmpty()

                        where txn.UpdatedDOCInt == DateInt

                        orderby txn.DOC descending

                        select new PastSuccessFailViewModel
                        {
                            TransactionMasterID = txn.TransactionMasterID,
                            UserID = txn.UserID,
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            ConsumerNumber = txn.ConsumerNumber,
                            OperatorName = Operator.OperatorName,
                            ProviderName = leftP?.ProviderName ?? "",
                            Amount = txn.Amount,
                            DebitAmount = txn.DebitAmount,
                            Status = txn.Status,
                            ReferenceNo = txn.ReferenceNo,
                            DOC = txn.DOC.LongFormatDate(),
                            UpdatedDOC = txn.UpdatedDOC.LongFormatDate(),
                            ProcTime = Math.Round((txn.UpdatedDOC - txn.DOC).TotalMinutes, 2),
                        }).ToList();

                if (mainRequest.ProviderID > 0)
                {
                    list = (from item in list
                            where item.ProviderID == mainRequest.ProviderID
                            select item).ToList();
                }
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    list = (from item in list
                            where item.UserID == UserID
                            select item).ToList();
                }

                list = (from item in list
                        where (!mainRequest.Status.EqualsIgnoreCase("All") ? item.Status == mainRequest.Status : 2 > 1)
                        select item).ToList();
            }
            return list;
        }

        internal List<Response.TransactionMaster.ViewModel> SelectByDate(int FromDate, int ToDate, int UserTypeID, int UserID, string Status, int ServiceID, int OperatorID, int TakeCount, int SkipCount, decimal MinAmount, decimal MaxAmount, int RequestedUserID, int ProviderID, string Complaint)
        {
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        join provider in context.ProviderMaster on
                        TransactionMaster.ProviderID equals provider.ProviderID into g
                        from provider in g.DefaultIfEmpty()
                        join complaint in context.ComplaintMaster on TransactionMaster.TransactionMasterID equals complaint.TransactionMasterID into comp
                        from CLeft in comp.DefaultIfEmpty()
                        where TransactionMaster.DOCInt >= FromDate
                        && TransactionMaster.DOCInt <= ToDate

                        && (ProviderID == 0 ? 2 > 1 : TransactionMaster.ProviderID == ProviderID)
                        && (ServiceID != 0 ? TransactionMaster.ServiceID == ServiceID : 2 > 1)
                        && (OperatorID == 0 ? 2 > 1 : TransactionMaster.OperatorID == OperatorID)
                        && (!Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == Status : 2 > 1)
                        && (!Complaint.EqualsIgnoreCase("All") ? TransactionMaster.ComplaintStatus == Complaint : 2 > 1)

                        && (MinAmount >= 1 ? TransactionMaster.Amount >= MinAmount : 2 > 1)
                        && (MaxAmount >= 1 ? TransactionMaster.Amount <= MaxAmount : 2 > 1)

                        orderby TransactionMaster.TransactionMasterID descending

                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,
                            WalletID = TransactionMaster.WalletID,
                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameMobileAliasName(TransactionMaster.UserMaster.MobileNumber, TransactionMaster.UserMaster.AliasName),
                            UserMobile = TransactionMaster.UserMaster.MobileNumber,
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,
                            Logo = TransactionMaster.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
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
                            ComplaintStatus = CLeft.ComplaintStatus ?? "",
                            ReferenceNo = (TransactionMaster.Status == StatusType.Failure) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                            PaymentType = TransactionMaster.DMT_PaymentType,
                            Through = TransactionMaster.Through,

                            DOC = TransactionMaster.DOC.LongFormatDate(),
                            ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                            OperatorID = TransactionMaster.OperatorID,
                            ServiceID = TransactionMaster.ServiceID,
                            ProviderRequestID = TransactionMaster.ProviderRequestID,
                            ProviderID = TransactionMaster.ProviderID,
                            ProviderName = provider.ProviderName,

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
                if (Util.isRoleAdminOrSupport(UserTypeID) && RequestedUserID != 0)
                {
                    int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(RequestedUserID);
                    if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                    {
                        list = (from item in list
                                where item.SuperDistributerUserID == RequestedUserID
                                select item).ToList();
                    }
                    else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                    {
                        list = (from item in list
                                where item.DistributerUserID == RequestedUserID
                                select item).ToList();
                    }
                    else
                    {
                        list = (from item in list
                                where item.UserID == RequestedUserID
                                select item).ToList();
                    }
                }
                if (ServiceID == 0)// this for recharge report
                {
                    list = (from item in list
                            where item.ServiceID != (int)ServiceCode.MoneyTransfer
                            && item.ServiceID != (int)ServiceCode.AEPS
                            && item.ServiceID != (int)ServiceCode.PaymentGateway
                             && item.ServiceID != (int)ServiceCode.MATM
                             && item.ServiceID != (int)ServiceCode.Payout
                             && item.ServiceID != (int)ServiceCode.AccountOpening
                             && item.ServiceID != (int)ServiceCode.AEPSOnboarding
                             && item.ServiceID != (int)ServiceCode.POS
                             && item.ServiceID != (int)ServiceCode.CMS
                             && item.ServiceID != (int)ServiceCode.UPITransfer
                             && item.ServiceID != (int)ServiceCode.MATMOnboarding
                             && item.ServiceID != (int)ServiceCode.PanCard
                            select item).ToList();
                }

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                });

                list = list.Skip(SkipCount).Take(TakeCount).ToList();
            }
            return list;
        }
        internal List<HourlyViewModel> HourlyPerformance()
        {
            using (var context = new DatabaseContext())
            {
                DateTime startOfLast2Hours = DateTime.Now.AddHours(-2);

                var hourlyData = context.TransactionMaster
                    .Where(transaction => transaction.DOC >= startOfLast2Hours && transaction.DOC <= DateTime.Now && transaction.Status == "SUCCESS")
                    .OrderBy(transaction => transaction.DOC)
                    .AsEnumerable()
                    .GroupBy(transaction => Math.Round((transaction.UpdatedDOC - transaction.DOC).TotalSeconds, 0))
                    .Select(group => new HourlyViewModel
                    {
                        ProcTime = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                var result = hourlyData
                    .GroupBy(item => item.ProcTime > 60 ? "60+" : item.ProcTime.ToString())
                    .Select(group => new HourlyViewModel
                    {
                        ProcTime = group.Key == "60+" ? 60 : double.Parse(group.Key),
                        Count = group.Sum(item => item.Count)
                    })
                    .OrderBy(item => item.Count)
                    .ToList();


                var aggregatedResult = result
                    .GroupBy(item => item.Count)
                    .Select(group => new HourlyViewModel
                    {
                        ProcTime = Math.Round(group.Average(item => item.ProcTime), 0),
                        Count = group.Key
                    })
                    .OrderBy(item => item.Count)
                    .ToList();


                aggregatedResult = aggregatedResult
                    .GroupBy(item => item.Count)
                    .Select(group => group.First())
                    .ToList();

                return aggregatedResult;
            }

        }

        internal List<Last12hrViewModel> Last12hrPerformance()
        {

            List<Last12hrViewModel> hourlyData = new List<Last12hrViewModel>();

            using (var context = new DatabaseContext())
            {
                DateTime endOfCurrentHour = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
                DateTime startOfLast12Hours = endOfCurrentHour.AddHours(-12);

                var hourlyQuery = context.TransactionMaster
                    .Where(t => t.DOC >= startOfLast12Hours && t.DOC <= endOfCurrentHour && t.Status == "SUCCESS")
                    .OrderBy(t => t.DOC)
                    .Select(transaction => new Last12hrViewModel
                    {
                        DOC = transaction.DOC,
                        ProcTime = Math.Round((transaction.UpdatedDOC - transaction.DOC).TotalSeconds, 2),
                    })
                    .ToList();

                var groupedData = hourlyQuery
                    .GroupBy(item => (int)((item.DOC - startOfLast12Hours).TotalHours) / 2)
                    .Select(group => new Last12hrViewModel
                    {
                        IntervalStart = startOfLast12Hours.AddHours(group.Key * 2).ToString("hh:mm tt"),
                        IntervalEnd = startOfLast12Hours.AddHours((group.Key + 1) * 2).ToString("hh:mm tt"),
                        AvgProcTime = Math.Round(group.Average(item => item.ProcTime), 2),
                    })
                    .ToList();

                hourlyData.AddRange(groupedData);
            }

            return hourlyData;
        }
        internal List<Last12hrViewModel> ProviderWisePerformance(ProviderWisePerformanceGraphRequest mainRequest)
        {
            List<Last12hrViewModel> hourlyData = new List<Last12hrViewModel>();

            using (var context = new DatabaseContext())
            {
                DateTime selectedDate = Convert.ToDateTime(mainRequest.Date);
                DateTime startDate = selectedDate.Date;
                DateTime endDate = startDate.AddDays(1).AddSeconds(-1);

                var hourlyQuery = context.TransactionMaster
                    .Where(t => t.DOC >= startDate && t.DOC <= endDate && t.Status == "SUCCESS" && t.ProviderID == mainRequest.ProviderID)
                    .OrderBy(t => t.DOC)
                    .Select(transaction => new Last12hrViewModel
                    {
                        DOC = transaction.DOC,
                        ProcTime = Math.Round((transaction.UpdatedDOC - transaction.DOC).TotalSeconds, 2),
                    })
                    .ToList();

                var groupedData = hourlyQuery
                    .GroupBy(item => (int)((item.DOC - startDate).TotalHours) / 2)
                    .Select(group => new Last12hrViewModel
                    {
                        IntervalStart = startDate.AddHours(group.Key * 2).ToString("hh:mm tt"),
                        IntervalEnd = startDate.AddHours((group.Key + 1) * 2).ToString("hh:mm tt"),
                        AvgProcTime = Math.Round(group.Average(item => item.ProcTime), 2),
                    })
                    .ToList();

                hourlyData.AddRange(groupedData);
            }

            return hourlyData;
        }



        internal List<AllProviderWisePerformenceViewModel> AllProviderWisePerformance(ProviderWisePerformanceGraphRequest mainRequest)
        {
            if (mainRequest.Date == null)
            {
                mainRequest.Date = DateTime.Now.ToLongDateString();
            }
            List<AllProviderWisePerformenceViewModel> hourlyData = new List<AllProviderWisePerformenceViewModel>();
            using (var context = new DatabaseContext())
            {

                DateTime selectedDate = Convert.ToDateTime(mainRequest.Date);
                DateTime startOfDay = selectedDate.Date;
                DateTime endOfDay = selectedDate.Date.AddDays(1).AddSeconds(-1);
                var hourlyQuery = (from transaction in context.TransactionMaster
                                   join provider in context.ProviderMaster
                                   on transaction.ProviderID equals provider.ProviderID into providerJoin
                                   from provider in providerJoin.DefaultIfEmpty()
                                   where transaction.DOC >= startOfDay
                                      && transaction.DOC <= endOfDay
                                      && transaction.Status == "SUCCESS"
                                   orderby transaction.DOC
                                   select new
                                   {
                                       transaction.DOC,
                                       transaction.UpdatedDOC,
                                       transaction.ProviderID,
                                       ProviderName = provider.ProviderName
                                   }).ToList();

                var groupedData = hourlyQuery
                        .GroupBy(item => item.ProviderName)
                        .Select(group => new AllProviderWisePerformenceViewModel
                        {
                            AvgProcTime = Math.Round(group.Average(item => (item.UpdatedDOC - item.DOC).TotalSeconds), 2),
                            ProviderName = group.FirstOrDefault()?.ProviderName
                        })
                        .ToList();

                hourlyData.AddRange(groupedData);
            }

            return hourlyData;
        }





        internal List<Response.TransactionMaster.ViewModel> SelectForFilter(ReportRequest transactionReportRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int takeCount = transactionReportRequest.TakeCount;

            if (takeCount >= 100)
            {
                takeCount = 100;
            }

            int To = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            int serviceID = transactionReportRequest.ServiceID;
            list = SelectByDate(From, To, UserTypeID, userID, transactionReportRequest.Status, serviceID, transactionReportRequest.OperatorID, takeCount, transactionReportRequest.SkipCount, transactionReportRequest.MinAmount, transactionReportRequest.MaxAmount, transactionReportRequest.UserID, transactionReportRequest.ProviderID, transactionReportRequest.Complaint);

            if (list.Count <= 0) return list;

            return list;
        }

        internal async Task<List<Response.TransactionMaster.ViewModel>> LiveTransaction(ReportRequest transactionReportRequest, int userID)
        {
            int DOCInt = Convert.ToInt32(DateTime.Now.Date.ToString(Messages.DOCIntegerFomrat));

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        join provider in context.ProviderMaster on
                        TransactionMaster.ProviderID equals provider.ProviderID into g
                        from provider in g.DefaultIfEmpty()
                        where TransactionMaster.DOCInt == DOCInt
                         && TransactionMaster.ServiceID != (int)ServiceCode.AEPS
                         && TransactionMaster.ServiceID != (int)ServiceCode.MATM
                         && TransactionMaster.ServiceID != (int)ServiceCode.Payout
                        && (!Util.isRoleAdminOrSupport(UserTypeID) ? TransactionMaster.UserID == userID : 2 > 1)
                        && (!transactionReportRequest.Status.EqualsIgnoreCase("All") ? TransactionMaster.Status == transactionReportRequest.Status : 2 > 1)
                        orderby TransactionMaster.TransactionMasterID descending
                        select new Response.TransactionMaster.ViewModel
                        {
                            TransactionMasterID = TransactionMaster.TransactionMasterID,
                            UserID = TransactionMaster.UserID,
                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameMobileAliasName(TransactionMaster.UserMaster.MobileNumber, TransactionMaster.UserMaster.AliasName),
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            OperatorName = TransactionMaster.OperatorMaster.OperatorName,
                            BeneName = TransactionMaster.DMT_BeneficiaryName,
                            SenderNo = TransactionMaster.DMT_SenderNumber,
                            Bank = TransactionMaster.DMT_Bank,
                            OpeningBalance = TransactionMaster.OpeningBalance,
                            Amount = TransactionMaster.Amount,
                            DebitAmount = TransactionMaster.DebitAmount,
                            ClosingBalance = TransactionMaster.ClosingBalance,
                            Status = TransactionMaster.Status,

                            ReferenceNo = (TransactionMaster.Status == StatusType.Failure) ? TransactionMaster.Description : TransactionMaster.ReferenceNo,

                            PaymentType = TransactionMaster.DMT_PaymentType,
                            Through = TransactionMaster.Through,

                            DOC = TransactionMaster.DOC.LongFormatDate(),
                            ProcTime = Math.Round((TransactionMaster.UpdatedDOC - TransactionMaster.DOC).TotalSeconds, 2),
                            OperatorID = TransactionMaster.OperatorID,
                            ServiceID = TransactionMaster.ServiceID,
                            ProviderRequestID = TransactionMaster.ProviderRequestID,
                            ProviderID = TransactionMaster.ProviderID,
                            ProviderName = provider.ProviderName,
                            IPAddress = TransactionMaster.IPAddress,
                            APIUserRequestID = TransactionMaster.APIUserRequestID,
                            Circle = TransactionMaster.Circle ?? "",
                            RetailerCommission = TransactionMaster.RetailerCommission,
                            DistributerCommission = TransactionMaster.DistributerCommission,
                            SuperDistributerCommission = TransactionMaster.SuperDistributerCommission,
                            AdminProfit = TransactionMaster.AdminProfit,
                            ResponseType = TransactionMaster.ResponseType,
                            ROfferCommission = TransactionMaster.ROfferCommission,
                        }).Take(50).ToList();
            }

            await Task.Delay(1);

            return list;
        }

        internal async Task<HeaderViewModel> LiveHeaderSummary(ReportRequest txReportRequest, int userID)
        {
            int DOCInt = Convert.ToInt32(DateTime.Now.Date.ToString(Messages.DOCIntegerFomrat));

            List<TransactionMaster> list = new List<TransactionMaster>();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                list = (from transactionMaster in context.TransactionMaster
                        where transactionMaster.DOCInt == DOCInt
                           && (!Util.isRoleAdminOrSupport(UserTypeID) ? transactionMaster.UserID == userID : 2 > 1)
                        select transactionMaster).ToList();

                var Success = (from item in list
                               where item.Status == StatusType.Success
                               group item by new { item.Status } into grp
                               select new
                               {
                                   SuccessAmount = grp.Sum(X => X.Amount),
                                   RetCommi = grp.Sum(X => X.RetailerCommission),
                                   DistCommi = grp.Sum(X => X.DistributerCommission),
                                   SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
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

                var ManualFail = (from item in list
                                  where item.Description == StatusType.ManualFail
                                  group item by new { item.Description } into grp
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

                HeaderViewModel headerlist = new HeaderViewModel();
                headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
                headerlist.SuccessCount = Success?.SuccessCount ?? 0;

                headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
                headerlist.FailedCount = Fail?.FailedCount ?? 0;

                headerlist.ManualFailAmount = ManualFail?.FailedAmount ?? 0;
                headerlist.ManualFailCont = ManualFail?.FailedCount ?? 0;

                headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
                headerlist.PendingCount = Pending?.PendingCount ?? 0;

                headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
                headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

                if (UserTypeID == (int)UserType.Admin)
                {
                    headerlist.Commission = (Success?.RetCommi ?? 0) + (Success?.DistCommi ?? 0) + (Success?.SuperCommi ?? 0);
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

                await Task.Delay(1);

                return headerlist;
            }
        }

        internal int GetTransactionCount(ReportRequest transactionReportRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(transactionReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            List<TransactionMaster> list = new List<TransactionMaster>();
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            list = (from tran in context.TransactionMaster
                    where tran.DOCInt >= From && tran.DOCInt <= To
                     && (transactionReportRequest.ProviderID == 0 ? 2 > 1 : tran.ProviderID == transactionReportRequest.ProviderID)
                     && (transactionReportRequest.MinAmount >= 1 ? tran.Amount >= transactionReportRequest.MinAmount : 2 > 1)
                     && (transactionReportRequest.MaxAmount >= 1 ? tran.Amount <= transactionReportRequest.MaxAmount : 2 > 1)
                     && (transactionReportRequest.ServiceID != 0 ? tran.ServiceID == transactionReportRequest.ServiceID : 2 > 1)
                     && (transactionReportRequest.OperatorID == 0 ? 2 > 1 : tran.OperatorID == transactionReportRequest.OperatorID)
                     && (!transactionReportRequest.Status.EqualsIgnoreCase("All") ? tran.Status == transactionReportRequest.Status : 2 > 1)
                     && (!transactionReportRequest.Complaint.EqualsIgnoreCase("All") ? tran.Status == transactionReportRequest.Complaint : 2 > 1)
                    select tran).ToList();

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == userID
                            select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == userID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == userID
                            select item).ToList();
                }
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && transactionReportRequest.UserID != 0)
            {
                int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(transactionReportRequest.UserID);
                if (RequestedUserIDUserTypeID == (int)UserType.SuperDistributer)
                {
                    list = (from item in list
                            where item.SuperDistributerUserID == transactionReportRequest.UserID
                            select item).ToList();
                }
                else if (RequestedUserIDUserTypeID == (int)UserType.Distributer)
                {
                    list = (from item in list
                            where item.DistributerUserID == transactionReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == transactionReportRequest.UserID
                            select item).ToList();
                }
            }

            if (transactionReportRequest.ServiceID == 0)// this for recharge report
            {
                list = (from item in list
                        where item.ServiceID != (int)ServiceCode.MoneyTransfer
                        && item.ServiceID != (int)ServiceCode.AEPS
                        && item.ServiceID != (int)ServiceCode.PaymentGateway
                        && item.ServiceID != (int)ServiceCode.MATM
                        && item.ServiceID != (int)ServiceCode.Payout
                        select item).ToList();
            }
            return list.Count;
        }

        internal List<Response.TransactionMaster.ReceiptViewModel> SelectByBillNumber(string billNumber)
        {
            List<Response.TransactionMaster.ReceiptViewModel> list = new List<Response.TransactionMaster.ReceiptViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from TransactionMaster in context.TransactionMaster
                        where TransactionMaster.DMT_BillNumber == billNumber
                        orderby TransactionMaster.TransactionMasterID descending
                        select new Response.TransactionMaster.ReceiptViewModel
                        {
                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameAliasName(TransactionMaster.UserMaster.AliasName),
                            FirmNameWithNumber = TransactionMaster.UserMaster.FirmName.ConcatNameMobileAliasName(TransactionMaster.UserMaster.MobileNumber, TransactionMaster.UserMaster.AliasName),
                            FirmAddress = TransactionMaster.UserMaster.Address,
                            CompanyName = Company.ClientName,
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            BeneName = TransactionMaster.DMT_BeneficiaryName,
                            SenderNo = TransactionMaster.DMT_SenderNumber,
                            Bank = TransactionMaster.DMT_Bank,
                            AmountDecimal = TransactionMaster.Amount,
                            ChargeDecimal = TransactionMaster.DebitAmount - TransactionMaster.Amount,
                            GSTDecimal = TransactionMaster.RetailerGST,
                            Status = TransactionMaster.Status,
                            ReferenceNo = TransactionMaster.ReferenceNo,
                            PaymentType = TransactionMaster.DMT_PaymentType,
                            IFSC = TransactionMaster.DMT_IFSC,
                            DOC = TransactionMaster.DOC.DOCFormatDate(),
                        }).ToList();
            }

            return list;
        }

        internal RechargeReceiptViewModel SelectDetailByID(int ID)
        {
            RechargeReceiptViewModel list = new RechargeReceiptViewModel();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from TransactionMaster in context.TransactionMaster
                        where TransactionMaster.TransactionMasterID == ID
                        join serviceMaster in context.ServiceMaster
                        on TransactionMaster.ServiceID equals serviceMaster.ServiceID
                        orderby TransactionMaster.TransactionMasterID descending
                        select new RechargeReceiptViewModel
                        {
                            UserID = TransactionMaster.UserID,

                            FirmName = TransactionMaster.UserMaster.FirmName.ConcatNameAliasName(TransactionMaster.UserMaster.AliasName),
                            FirmAddress = TransactionMaster.UserMaster.Address,
                            MobileNumber = TransactionMaster.UserMaster.MobileNumber,
                            CompanyName = Company.ClientName,
                            ConsumerNumber = TransactionMaster.ConsumerNumber,
                            ServiceID = TransactionMaster.ServiceID,
                            Operator = TransactionMaster.OperatorMaster.OperatorName,
                            IsBBPS = TransactionMaster.OperatorMaster.IsBBPS,
                            OperatorID = TransactionMaster.OperatorMaster.OperatorID,
                            OperatorImagePath = TransactionMaster.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
                            AmountDecimal = TransactionMaster.Amount,
                            Status = TransactionMaster.Status,
                            ReferenceNo = TransactionMaster.ReferenceNo,
                            ProviderRefNo = TransactionMaster.ProviderRefNo,
                            DOC = TransactionMaster.DOC.ToString("dd-MM-yyyy HH:mm:ss"),
                            IFSC = TransactionMaster.DMT_IFSC,
                            DMT_BeneficiaryName = TransactionMaster.DMT_BeneficiaryName,
                            DistributorID = TransactionMaster.DistributerUserID,
                            SuperDistributorID = TransactionMaster.SuperDistributerUserID,
                            AEPSResponseJSON = TransactionMaster.AEPSResponseJSON,
                            BBPS_CustomerName = TransactionMaster.BBPS_CustomerName,
                            CustomerNumber = TransactionMaster.Bill_CustomerMessageMobileNo,
                            ServiceName = serviceMaster.Name,
                            ProviderRequestID = TransactionMaster.ProviderRequestID,
                            BillerID = TransactionMaster.Bill_Optional5,
                            BillDate = TransactionMaster.Bill_Optional4,
                            BankName = TransactionMaster.DMT_Bank,
                            STANNo = TransactionMaster.Bill_Optional1,
                            AgentID = TransactionMaster.Bill_Optional2,
                            DebitAmount = TransactionMaster.DebitAmount,
                            RetailerGST = TransactionMaster.RetailerGST,
                            AgentName = TransactionMaster.UserMaster.PersonName,
                            NetCommission = TransactionMaster.NetCommission
                            //PickupLocation = TransactionMaster.Bill_Optional6,
                        }).FirstOrDefault();
            }
           

            return list;
        }

        internal Project.Response.BusBooking.PrintResponse SelectBus(int ID)
        {
            Project.Response.BusBooking.PrintResponse list = new Project.Response.BusBooking.PrintResponse();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from TransactionMaster in context.TransactionMaster
                        where TransactionMaster.TransactionMasterID == ID
                        orderby TransactionMaster.TransactionMasterID descending
                        select new Project.Response.BusBooking.PrintResponse
                        {
                            UserID = TransactionMaster.UserID,
                            name = TransactionMaster.UserMaster.FirmName.ConcatNameAliasName(TransactionMaster.UserMaster.AliasName),
                            CompanyName = Company.ClientName,
                            DOC = TransactionMaster.DOC.LongFormatDate(),
                            AEPSResponseJSON = TransactionMaster.AEPSResponseJSON,
                            Operator = TransactionMaster.OperatorMaster.OperatorName,
                            OperatorID = TransactionMaster.OperatorMaster.OperatorID,
                        }).FirstOrDefault();
            }

            return list;
        }

        internal TransactionMaster SelectByProviderRequestID(string providerRequestID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.FirstOrDefault(T => T.ProviderRequestID == providerRequestID);
            }
        }
        internal TransactionMaster SelectByProviderRequestIDAndDate(string providerRequestID, int date)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.FirstOrDefault(T => T.ProviderRequestID == providerRequestID && T.DOCInt == date);
            }
        }
        

        internal TransactionMaster SelectByProviderRequestIDTodayOnly(string providerRequestID)
        {
            int DOCInt = Convert.ToInt32(DateTime.Now.Date.ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.FirstOrDefault(T => T.DOCInt == DOCInt && T.ProviderRequestID == providerRequestID);
            }
        }
        internal TransactionMaster SelectByAPIUserRequestIDTodayOnly(int UserID, string APIUserRequestID)
        {
            int DOCInt = Convert.ToInt32(DateTime.Now.Date.ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.FirstOrDefault(T => T.DOCInt == DOCInt && T.UserID == UserID && T.APIUserRequestID == APIUserRequestID);
            }
        }

        internal TransactionMaster SelectByProviderRefernceID(string ProviderRefernceID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.FirstOrDefault(T => T.ProviderRefNo == ProviderRefernceID);
            }
        }

        internal TransactionMaster SelectByReferenceNo(string ReferenceNo)
        {
            if (ReferenceNo == "00" || ReferenceNo == "") return null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.FirstOrDefault(T => T.ReferenceNo == ReferenceNo);
            }
        }

        internal ViewModel SelectByRequestID(string ProviderRequestID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        where transactionMaster.ProviderRequestID == ProviderRequestID
                        select new ViewModel
                        {
                            UserID = transactionMaster.UserID,
                            ConsumerNumber = transactionMaster.ConsumerNumber,
                            Amount = transactionMaster.Amount,
                            OperatorID = transactionMaster.OperatorID,
                            ServiceID = transactionMaster.ServiceID,
                            ProviderRequestID = transactionMaster.ProviderRequestID,
                            ProviderID = transactionMaster.ProviderID,
                            TransactionMasterID = transactionMaster.TransactionMasterID,
                            Status = transactionMaster.Status,
                            DOC = transactionMaster.DOC.LongFormatDate(),
                        }).FirstOrDefault();
            }
        }
        internal ViewModel SelectByRequestIDWithProviderID(int ProivderID, string ProviderRequestID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        where transactionMaster.ProviderID == ProivderID &&
                        transactionMaster.ProviderRequestID == ProviderRequestID
                        select new ViewModel
                        {
                            UserID = transactionMaster.UserID,
                            ConsumerNumber = transactionMaster.ConsumerNumber,
                            Amount = transactionMaster.Amount,
                            OperatorID = transactionMaster.OperatorID,
                            ServiceID = transactionMaster.ServiceID,
                            ProviderRequestID = transactionMaster.ProviderRequestID,
                            ProviderID = transactionMaster.ProviderID,
                            TransactionMasterID = transactionMaster.TransactionMasterID,
                            Status = transactionMaster.Status,
                            DOC = transactionMaster.DOC.LongFormatDate(),
                        }).FirstOrDefault();
            }
        }

        internal HeaderViewModel SelectPendingHeaderSummary()
        {
            List<TransactionMaster> list = new List<TransactionMaster>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                list = (from transactionMaster in context.TransactionMaster
                        where transactionMaster.Status == StatusType.Process && (
                               transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.VendorPay
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPS
                            && transactionMaster.ServiceID != (int)ServiceCode.MATM
                            && transactionMaster.ServiceID != (int)ServiceCode.Payout
                            && transactionMaster.ServiceID != (int)ServiceCode.UPITransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AccountOpening
                            && transactionMaster.ServiceID != (int)ServiceCode.PanCard
                            && transactionMaster.ServiceID != (int)ServiceCode.CreditCardPayment
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPSOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.MATMOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.CashDeposit
                            && transactionMaster.ServiceID != (int)ServiceCode.PaymentGateway)

                        join provider in context.ProviderMaster on
                        transactionMaster.ProviderID equals provider.ProviderID into providerDetails
                        from PL in providerDetails.DefaultIfEmpty()
                        select transactionMaster).ToList();

                var Pending = (from item in list
                               group item by new { item.Status } into grp
                               select new
                               {
                                   PendingAmount = grp.Sum(X => X.Amount),
                                   PendingCount = grp.Count(),
                               }).FirstOrDefault();

                HeaderViewModel headerlist = new HeaderViewModel();

                headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
                headerlist.PendingCount = Pending?.PendingCount ?? 0;

                headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
                headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

                return headerlist;
            }
        }

        internal HeaderViewModel SelectPendingProviderHeaderSummary(List<PendingResponse> list)
        {
            var Pending = (from item in list
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal HeaderViewModel SelectPendingHeaderSummaryWithFilter(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            List<TransactionMaster> list = new List<TransactionMaster>();

            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            list = (from transactionMaster in context.TransactionMaster
                    where transactionMaster.Status == StatusType.Process
                    && (
                     transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                      && transactionMaster.ServiceID != (int)ServiceCode.AEPS
                      && transactionMaster.ServiceID != (int)ServiceCode.MATM
                      && transactionMaster.ServiceID != (int)ServiceCode.Payout
                      && transactionMaster.ServiceID != (int)ServiceCode.UPITransfer
                      && transactionMaster.ServiceID != (int)ServiceCode.AccountOpening
                      && transactionMaster.ServiceID != (int)ServiceCode.PanCard
                      && transactionMaster.ServiceID != (int)ServiceCode.CreditCardPayment
                      && transactionMaster.ServiceID != (int)ServiceCode.AEPSOnboarding
                      && transactionMaster.ServiceID != (int)ServiceCode.CashDeposit
                      && transactionMaster.ServiceID != (int)ServiceCode.MATMOnboarding
                      && transactionMaster.ServiceID != (int)ServiceCode.PaymentGateway)
                    join provider in context.ProviderMaster on
                    transactionMaster.ProviderID equals provider.ProviderID
                    select transactionMaster).ToList();

            if (mainRequest.ProviderID != 0)
            {
                list = (from item in list
                        where item.ProviderID == mainRequest.ProviderID
                        select item).ToList();
            }
            if (mainRequest.OperatorID != 0)
            {
                list = (from item in list
                        where item.OperatorID == mainRequest.OperatorID
                        select item).ToList();
            }
            var Pending = (from item in list
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal HeaderViewModel SelectCMSOfflineDMTHeaderSummary(CMSOfflineDMTRequest mainrequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainrequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainrequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<TransactionMaster> list = new List<TransactionMaster>();

            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            list = (from transactionMaster in context.TransactionMaster
                    join provider in context.ProviderMaster on
                    transactionMaster.ProviderID equals provider.ProviderID
                    where transactionMaster.Status == StatusType.Process
                          && transactionMaster.Amount >= 1

                                    && transactionMaster.DOCInt >= From
                                && transactionMaster.DOCInt <= To
                    && transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                    && transactionMaster.IsOnlineDMT == false
                    select transactionMaster).ToList();

            var Pending = (from item in list
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal TransactionMaster SelectByRequestUserAndServiceID(string ProviderRequestID, int UserID, int ServiceID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        where transactionMaster.ProviderRequestID == ProviderRequestID &&
                        transactionMaster.UserID == UserID &&
                        transactionMaster.ServiceID == ServiceID
                        select transactionMaster).FirstOrDefault();
            }
        }

        internal TransactionMaster SelectByRequestIDAndServiceID(string ProviderRequestID, int ServiceID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        where transactionMaster.ProviderRequestID == ProviderRequestID &&
                        transactionMaster.ServiceID == ServiceID
                        select transactionMaster).FirstOrDefault();
            }
        }

        internal List<SearchViewModel> SelectByConsumerNumber(SearchRequest searchRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(searchRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(searchRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<SearchViewModel> Record = new List<SearchViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID into
                          providerDetails
                          from PL in providerDetails.DefaultIfEmpty()
                          where transactionMaster.DOCInt >= From && transactionMaster.DOCInt <= To &&
                          (transactionMaster.ConsumerNumber == searchRequest.ConsumerNumber
                          || transactionMaster.ReferenceNo.ToLower().Contains(searchRequest.ConsumerNumber.ToLower())
                          || transactionMaster.ProviderRequestID.ToLower().Contains(searchRequest.ConsumerNumber.ToLower())
                          || transactionMaster.APIUserRequestID.ToLower().Contains(searchRequest.ConsumerNumber.ToLower())
                          || transactionMaster.DMT_SenderNumber == searchRequest.ConsumerNumber)
                          orderby transactionMaster.TransactionMasterID descending
                          select new SearchViewModel
                          {
                              UserID = transactionMaster.UserID,
                              DistributerUserID = transactionMaster.DistributerUserID,
                              SuperDistributerUserID = transactionMaster.SuperDistributerUserID,
                              OperatorID = transactionMaster.OperatorID,
                              ServiceID = transactionMaster.ServiceID,
                              ServiceName = transactionMaster.ServiceMaster.Name,
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              OpeningBalance = transactionMaster.OpeningBalance,
                              ClosingBalance = transactionMaster.ClosingBalance,
                              Status = transactionMaster.Status,
                              ProviderName = PL.ProviderName ?? "",
                              FirmName = transactionMaster.UserMaster.FirmName.ConcatNameMobileAliasName(transactionMaster.UserMaster.MobileNumber, transactionMaster.UserMaster.AliasName),
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              ReferenceNo = transactionMaster.ReferenceNo ?? "0",
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID
                          }).ToList();

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    // need to providername
                    Record = (from item in Record
                              select new SearchViewModel
                              {
                                  UserID = item.UserID,
                                  DistributerUserID = item.DistributerUserID,
                                  SuperDistributerUserID = item.SuperDistributerUserID,
                                  OperatorID = item.OperatorID,
                                  ServiceID = item.ServiceID,
                                  ServiceName = item.ServiceName,
                                  OperatorName = item.OperatorName,
                                  ConsumerNumber = item.ConsumerNumber,
                                  Amount = item.Amount,
                                  OpeningBalance = item.OpeningBalance,
                                  ClosingBalance = item.ClosingBalance,
                                  Status = item.Status,
                                  ProviderName = "",
                                  FirmName = item.FirmName,
                                  TransactionMasterID = item.TransactionMasterID,
                                  ReferenceNo = item.ReferenceNo ?? "0",
                                  DOC = item.DOC,
                              }).ToList();

                    if (UserTypeID == (int)UserType.Distributer)
                    {
                        Record = (from item in Record
                                  where item.DistributerUserID == UserID
                                  select item).ToList();
                    }
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        Record = (from item in Record
                                  where item.SuperDistributerUserID == UserID
                                  select item).ToList();
                    }
                    if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                    {
                        Record = (from item in Record
                                  where item.UserID == UserID
                                  select item).ToList();
                    }
                }
            }
            return Record;
        }

        internal List<ChangeStatusViewModel> SelectChangeStatusByCallback(SearchRequest searchRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(searchRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(searchRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<ChangeStatusViewModel> Record = new List<ChangeStatusViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID into
                          providerDetails
                          from PL in providerDetails.DefaultIfEmpty()
                          where
                          transactionMaster.DOCInt >= From
                          && transactionMaster.DOCInt <= To
                          && transactionMaster.ChangeStatus.Length >= 2
                          && transactionMaster.ChangeStatus != null
                          orderby transactionMaster.TransactionMasterID descending
                          select new ChangeStatusViewModel
                          {
                              UserID = transactionMaster.UserID,
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              OpeningBalance = transactionMaster.OpeningBalance,
                              ClosingBalance = transactionMaster.ClosingBalance,
                              Status = transactionMaster.Status,
                              ChangeStatus = transactionMaster.ChangeStatus,
                              ProviderName = PL.ProviderName ?? "",
                              FirmName = transactionMaster.UserMaster.FirmName,
                              ReferenceNo = transactionMaster.ReferenceNo ?? "0",
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ChangeStatusDOC = transactionMaster.ChangeStatusDOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID
                          }).ToList();
            }
            return Record;
        }

        internal HeaderViewModel SelectDMTPendingHeaderSummary()
        {
            List<TransactionMaster> list = new List<TransactionMaster>();

            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            list = (from transactionMaster in context.TransactionMaster
                    where transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                     && transactionMaster.Status == StatusType.Process
                     && transactionMaster.IsOnlineDMT == true
                    join provider in context.ProviderMaster on
                     transactionMaster.ProviderID equals provider.ProviderID
                    select transactionMaster).ToList();

            var Pending = (from item in list
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal HeaderViewModel SelectDMTPendingHeaderSummaryWithFilter(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            List<TransactionMaster> list = new List<TransactionMaster>();

            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            list = (from transactionMaster in context.TransactionMaster
                    where transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                        && transactionMaster.Status == StatusType.Process
                        && (!mainRequest.PaymentType.Contains("All") ? transactionMaster.DMT_PaymentType == mainRequest.PaymentType : 2 > 1)
                        && (!mainRequest.ConsumerNumber.EqualsIgnoreCase("") ? transactionMaster.ConsumerNumber == mainRequest.ConsumerNumber : 2 > 1)
                    join provider in context.ProviderMaster on
                     transactionMaster.ProviderID equals provider.ProviderID
                    select transactionMaster).ToList();

            if (mainRequest.ProviderID != 0)
            {
                list = (from item in list
                        where item.ProviderID == mainRequest.ProviderID
                        select item).ToList();
            }
            if (mainRequest.OperatorID != 0)
            {
                list = (from item in list
                        where item.OperatorID == mainRequest.OperatorID
                        select item).ToList();
            }

            var Pending = (from item in list
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal List<ResendDMTModel> SelectResendDMTTransaction()
        {
            List<ResendDMTModel> Record = new List<ResendDMTModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID
                          where transactionMaster.Status == StatusType.Process
                                && transactionMaster.Amount >= 1
                                && transactionMaster.DMT_StatusCode == "51"
                          && transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                          orderby transactionMaster.TransactionMasterID descending
                          select new ResendDMTModel
                          {
                              SenderNo = transactionMaster.DMT_SenderNumber,
                              Bank = transactionMaster.DMT_Bank,
                              IFSC = transactionMaster.DMT_IFSC,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID
                          }).ToList();
            }
            return Record;
        }

        internal RangewiseDMTModel SelectRangewiseDMT(RangewiseDMTRequest mainRequest, decimal Amount1, decimal Amount2)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            RangewiseDMTModel record = new RangewiseDMTModel();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from transactionMaster in context.TransactionMaster
                                where (transactionMaster.Amount >= Amount1 && transactionMaster.Amount <= Amount2)
                                && transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                && (mainRequest.UserID == 0 ? 2 > 1 : transactionMaster.UserID == mainRequest.UserID)
                                && transactionMaster.DOCInt >= From
                                && transactionMaster.DOCInt <= To
                                orderby transactionMaster.TransactionMasterID descending
                                select new
                                {

                                    Status = transactionMaster.Status,
                                    Amount = transactionMaster.Amount,
                                    ServiceID = transactionMaster.ServiceID,
                                    ProviderID = transactionMaster.ProviderID,
                                }).ToList();

                if (mainRequest.ProviderID > 0)
                {
                    mainList = (from item in mainList
                                where item.ProviderID == mainRequest.ProviderID
                                select item).ToList();
                }

                var totalList = (from txn in mainList
                                 group txn by new { txn.ServiceID } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

                var totalFailList = (from txn in mainList
                                     where txn.Status == StatusType.Failure
                                     group txn by new { txn.ServiceID } into grp
                                     select new
                                     {
                                         Count = grp.Count(),
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();

                var totalSuccessList = (from txn in mainList
                                        where txn.Status == StatusType.Success
                                        group txn by new { txn.ServiceID } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                var totalProcessList = (from txn in mainList
                                        where txn.Status == StatusType.Process
                                        group txn by new { txn.ServiceID } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                record.Count = totalList?.Count ?? 0;
                record.Sum = totalList?.Amount ?? 0;
                record.FailCount = totalFailList?.Count ?? 0;
                record.FailSum = totalFailList?.Amount ?? 0;
                record.SuccessCount = totalSuccessList?.Count ?? 0;
                record.SuccessSum = totalSuccessList?.Amount ?? 0;
                record.ProcessCount = totalProcessList?.Count ?? 0;
                record.ProcessSum = totalProcessList?.Amount ?? 0;
                record.Range = mainRequest.Range;
            }
            return record;
        }



        internal List<UserWiseDMTRangeViewModel> SelectUserWiseDMTRangeReport(UserWiseDMTRangeReportRequest mainRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<UserWiseDMTRangeViewModel> userRangeCounts;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                userRangeCounts = context.TransactionMaster
                    .Where(transactionMaster =>
                        transactionMaster.ServiceID == mainRequest.ServiceID &&
                        (mainRequest.UserID == 0 ? 2 > 1 : transactionMaster.UserID == mainRequest.UserID) &&
                        transactionMaster.DOCInt >= From &&
                        transactionMaster.DOCInt <= To &&
                        transactionMaster.Status == StatusType.Success)
                    .OrderByDescending(transactionMaster => transactionMaster.TransactionMasterID)
                    .GroupBy(transactionMaster => new
                    {
                        UserName = transactionMaster.UserMaster.FirmName
                    })
                    .Select(group => new UserWiseDMTRangeViewModel
                    {
                        UserName = group.Key.UserName,
                        Range1_2 = group.Count(transaction => transaction.Amount >= 1 && transaction.Amount <= 2),
                        Range3_1000 = group.Count(transaction => transaction.Amount >= 3 && transaction.Amount <= 1000),
                        Range1001_2000 = group.Count(transaction => transaction.Amount >= 1001 && transaction.Amount <= 2000),
                        Range2001_3000 = group.Count(transaction => transaction.Amount >= 2001 && transaction.Amount <= 3000),
                        Range3001_4000 = group.Count(transaction => transaction.Amount >= 3001 && transaction.Amount <= 4000),
                        Range4001_5000 = group.Count(transaction => transaction.Amount >= 4001 && transaction.Amount <= 5000),
                        Range5001_10000 = group.Count(transaction => transaction.Amount >= 5001 && transaction.Amount <= 10000),
                        Range10001_15000 = group.Count(transaction => transaction.Amount >= 10001 && transaction.Amount <= 15000),
                        Range15001_20000 = group.Count(transaction => transaction.Amount >= 15001 && transaction.Amount <= 20000),
                        Range20001_25000 = group.Count(transaction => transaction.Amount >= 20001 && transaction.Amount <= 25000),
                        Range25001_50000 = group.Count(transaction => transaction.Amount >= 25001 && transaction.Amount <= 50000),
                        Range50001_1L = group.Count(transaction => transaction.Amount >= 50001 && transaction.Amount <= 100000),
                        TotalTransactions = group.Count()
                    })
                    .ToList();
            }

            return userRangeCounts;
        }




        internal RangewiseRechargeModel SelectRangewiseRecharge(RangewiseRechargeRequest mainRequest, decimal Amount1, decimal Amount2, int MinRange, int MaxRange)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            RangewiseRechargeModel record = new RangewiseRechargeModel();

            int[] ids = new int[] { (int)ServiceCode.MoneyTransfer,(int)ServiceCode.AEPS,(int)ServiceCode.PaymentGateway
            ,(int)ServiceCode.MATM,(int)ServiceCode.Payout,(int)ServiceCode.CashDeposit,(int)ServiceCode.CreditCardPayment,(int)ServiceCode.WalletTopUp};

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from transactionMaster in context.TransactionMaster
                                where (transactionMaster.Amount >= Amount1 && transactionMaster.Amount <= Amount2)
                                && !ids.Contains(transactionMaster.ServiceID)
                                && (mainRequest.UserID == 0 ? 2 > 1 : transactionMaster.UserID == mainRequest.UserID)
                                && transactionMaster.DOCInt >= From
                                && transactionMaster.DOCInt <= To

                                orderby transactionMaster.TransactionMasterID descending
                                select new
                                {
                                    Status = transactionMaster.Status,
                                    Amount = transactionMaster.Amount,
                                    ServiceID = transactionMaster.ServiceID,
                                    ProviderID = transactionMaster.ProviderID,
                                }).ToList();

                if (mainRequest.ProviderID > 0)
                {
                    mainList = (from item in mainList
                                where item.ProviderID == mainRequest.ProviderID
                                select item).ToList();
                }

                var totalList = (from txn in mainList
                                 group txn by new { txn.ServiceID } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

                var totalFailList = (from txn in mainList
                                     where txn.Status == StatusType.Failure
                                     group txn by new { txn.ServiceID } into grp
                                     select new
                                     {
                                         Count = grp.Count(),
                                         Amount = grp.Sum(x => x.Amount),
                                     }).FirstOrDefault();

                var totalSuccessList = (from txn in mainList
                                        where txn.Status == StatusType.Success
                                        group txn by new { txn.ServiceID } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                var totalProcessList = (from txn in mainList
                                        where txn.Status == StatusType.Process
                                        group txn by new { txn.ServiceID } into grp
                                        select new
                                        {
                                            Count = grp.Count(),
                                            Amount = grp.Sum(x => x.Amount),
                                        }).FirstOrDefault();

                record.Count = totalList?.Count ?? 0;
                record.Sum = totalList?.Amount ?? 0;
                record.FailCount = totalFailList?.Count ?? 0;
                record.FailSum = totalFailList?.Amount ?? 0;
                record.SuccessCount = totalSuccessList?.Count ?? 0;
                record.SuccessSum = totalSuccessList?.Amount ?? 0;
                record.ProcessCount = totalProcessList?.Count ?? 0;
                record.ProcessSum = totalProcessList?.Amount ?? 0;
                if (string.IsNullOrEmpty(mainRequest.Range))
                {
                    record.Range = $"{mainRequest.MinRange} - {mainRequest.MaxRange}";
                }
                else
                {

                    record.Range = mainRequest.Range;
                }

            }
            return record;
        }

        internal List<PendingResponse> SelectPendingTransaction()
        {
            List<PendingResponse> Record = new List<PendingResponse>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process
                          && (
                           transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                           && transactionMaster.ServiceID != (int)ServiceCode.VendorPay
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPS
                            && transactionMaster.ServiceID != (int)ServiceCode.MATM
                            && transactionMaster.ServiceID != (int)ServiceCode.Payout
                            && transactionMaster.ServiceID != (int)ServiceCode.UPITransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AccountOpening
                            && transactionMaster.ServiceID != (int)ServiceCode.PanCard
                            && transactionMaster.ServiceID != (int)ServiceCode.CreditCardPayment
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPSOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.MATMOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.CashDeposit
                            && transactionMaster.ServiceID != (int)ServiceCode.PaymentGateway)

                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID into providerDetails
                          from PL in providerDetails.DefaultIfEmpty()
                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.PendingProviderStatus,
                              ProviderName = PL.ProviderName ?? "",
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              AdditionalParam1 = transactionMaster.Bill_Optional1 ?? "",
                          }).ToList();
            }
            return Record;
        }

        internal List<PendingResponse> SelectPendingProviderTransaction(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            List<PendingResponse> Record = new List<PendingResponse>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.PendingProviderStatus == StatusType.Process
                          && (
                           transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPS
                            && transactionMaster.ServiceID != (int)ServiceCode.MATM
                            && transactionMaster.ServiceID != (int)ServiceCode.Payout
                            && transactionMaster.ServiceID != (int)ServiceCode.PaymentGateway)

                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID into providerDetails
                          from PL in providerDetails.DefaultIfEmpty()
                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.PendingProviderStatus,
                              ProviderName = PL.ProviderName ?? "",
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              OperatorID = transactionMaster.OperatorID,
                              AdditionalParam1 = transactionMaster.Bill_Optional1 ?? "",
                              AdditionalParam2 = transactionMaster.Bill_Optional2 ?? "",
                          }).ToList();
                if (mainRequest.OperatorID != 0)
                {
                    Record = (from item in Record
                              where item.OperatorID == mainRequest.OperatorID
                              select item).ToList();
                }
            }
            return Record;
        }

        internal List<PendingResponseForStatusCheck> SelectPendingTransactionForStatusCheck()
        {
            List<PendingResponseForStatusCheck> Record = new List<PendingResponseForStatusCheck>();
            int toDaysDateInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.DOCInt == toDaysDateInt &&
                                transactionMaster.Status == StatusType.Process &&
                          (
                               transactionMaster.ServiceID == (int)ServiceCode.Mobile
                            || transactionMaster.ServiceID == (int)ServiceCode.Dth
                            || transactionMaster.ServiceID == (int)ServiceCode.Electricity
                            || transactionMaster.ServiceID == (int)ServiceCode.PostPaid
                            || transactionMaster.ServiceID == (int)ServiceCode.PipedGas
                            || transactionMaster.ServiceID == (int)ServiceCode.Fasttag
                            || transactionMaster.ServiceID == (int)ServiceCode.GasCylinderBooking
                            || transactionMaster.ServiceID == (int)ServiceCode.GeneralInsurance
                            || transactionMaster.ServiceID == (int)ServiceCode.Waterbill
                            || transactionMaster.ServiceID == (int)ServiceCode.LoanRePayment
                            || transactionMaster.ServiceID == (int)ServiceCode.LifeInsurance
                            || transactionMaster.ServiceID == (int)ServiceCode.Broadband)

                          orderby transactionMaster.TransactionMasterID ascending
                          select new PendingResponseForStatusCheck
                          {
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                          }).ToList();
            }
            return Record;
        }

        internal List<PendingResponse> SelectDMTPendingTransactionWithFilter(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            List<PendingResponse> Record = new List<PendingResponse>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process &&
                           transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer && transactionMaster.IsOnlineDMT == true &&
                           (!mainRequest.PaymentType.Contains("All") ? transactionMaster.DMT_PaymentType == mainRequest.PaymentType : 2 > 1)

                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID
                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              OperatorID = transactionMaster.OperatorID,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              ProviderID = transactionMaster.ProviderID,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              SenderNo = transactionMaster.DMT_SenderNumber,                    
                              Bank = transactionMaster.DMT_Bank,
                              Type = transactionMaster.DMT_PaymentType
                          }).ToList();
                if (mainRequest.ProviderID != 0)
                {
                    Record = (from item in Record
                              where item.ProviderID == mainRequest.ProviderID
                              select item).ToList();
                }
                if (mainRequest.OperatorID != 0)
                {
                    Record = (from item in Record
                              where item.OperatorID == mainRequest.OperatorID
                              select item).ToList();
                }
            }
            return Record;
        }

        private List<PendingResponse> SelectPendingTransactionsByProviderID(int ProviderID)
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process
                          && (
                           transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPS
                            && transactionMaster.ServiceID != (int)ServiceCode.MATM
                            && transactionMaster.ServiceID != (int)ServiceCode.Payout
                            && transactionMaster.ServiceID != (int)ServiceCode.UPITransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AccountOpening
                            && transactionMaster.ServiceID != (int)ServiceCode.PanCard
                            && transactionMaster.ServiceID != (int)ServiceCode.CreditCardPayment
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPSOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.MATMOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.CashDeposit
                            && transactionMaster.ServiceID != (int)ServiceCode.PaymentGateway) && transactionMaster.ProviderID == ProviderID

                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID

                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                          }).ToList();
            }
            return Record;
        }

        private List<PendingResponse> SelectPendingTransactionsByOperatorID(int OperatorID)
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process
                          && (
                           transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPS
                            && transactionMaster.ServiceID != (int)ServiceCode.MATM
                            && transactionMaster.ServiceID != (int)ServiceCode.Payout
                            && transactionMaster.ServiceID != (int)ServiceCode.UPITransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AccountOpening
                            && transactionMaster.ServiceID != (int)ServiceCode.PanCard
                            && transactionMaster.ServiceID != (int)ServiceCode.CreditCardPayment
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPSOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.MATMOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.CashDeposit
                            && transactionMaster.ServiceID != (int)ServiceCode.PaymentGateway) && transactionMaster.OperatorID == OperatorID

                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID

                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                          }).ToList();
            }
            return Record;
        }

        private List<PendingResponse> SelectPendingTransactionsByProviderIDAndOperatorID(int ProviderID, int OperatorID)
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process
                          && (
                           transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPS
                            && transactionMaster.ServiceID != (int)ServiceCode.MATM
                            && transactionMaster.ServiceID != (int)ServiceCode.Payout
                            && transactionMaster.ServiceID != (int)ServiceCode.UPITransfer
                            && transactionMaster.ServiceID != (int)ServiceCode.AccountOpening
                            && transactionMaster.ServiceID != (int)ServiceCode.PanCard
                            && transactionMaster.ServiceID != (int)ServiceCode.CreditCardPayment
                            && transactionMaster.ServiceID != (int)ServiceCode.AEPSOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.MATMOnboarding
                            && transactionMaster.ServiceID != (int)ServiceCode.CashDeposit
                            && transactionMaster.ServiceID != (int)ServiceCode.PaymentGateway) && transactionMaster.ProviderID == ProviderID && transactionMaster.OperatorID == OperatorID

                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID

                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                          }).ToList();
            }
            return Record;
        }

        internal List<PendingResponse> SelectPendingTransactionWithFilter(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            if (mainRequest.ProviderID > 0 && mainRequest.OperatorID == 0)
            {
                Record = SelectPendingTransactionsByProviderID(mainRequest.ProviderID);
                return Record;
            }
            if (mainRequest.OperatorID > 0 && mainRequest.ProviderID == 0)
            {
                Record = SelectPendingTransactionsByOperatorID(mainRequest.OperatorID);
                return Record;
            }
            if (mainRequest.OperatorID > 0 && mainRequest.ProviderID > 0)
            {
                Record = SelectPendingTransactionsByProviderIDAndOperatorID(mainRequest.ProviderID, mainRequest.OperatorID);
                return Record;
            }
            return Record;
        }

        internal List<PendingResponse> SelectDmtPendingTransaction()
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID
                          where transactionMaster.Status == StatusType.Process
                                && transactionMaster.Amount >= 1
                           && transactionMaster.IsOnlineDMT == true
                          && transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              SenderNo = transactionMaster.DMT_SenderNumber,
                              Bank = transactionMaster.DMT_Bank,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              Type = transactionMaster.DMT_PaymentType,
                              SenderName = transactionMaster.DMT_SenderName,
                              BeneficiaryName = transactionMaster.DMT_BeneficiaryName,
                          }).ToList();
            }
            return Record;
        }

        internal List<PendingResponse> SelectCMSOfflineDMT(CMSOfflineDMTRequest mainrequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainrequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainrequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID
                          where transactionMaster.Status == StatusType.Process
                                && transactionMaster.Amount >= 1
                                      && transactionMaster.DOCInt >= From
                                && transactionMaster.DOCInt <= To
                          && transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                          && transactionMaster.IsOnlineDMT == false
                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              SenderNo = transactionMaster.DMT_SenderNumber,
                              Bank = transactionMaster.DMT_Bank,
                              ProviderID = provider.ProviderID,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              IFSC = transactionMaster.DMT_IFSC,
                              PaymentType = transactionMaster.DMT_PaymentType
                          }).ToList();
            }
            if (mainrequest.ProviderID > 0)
            {
                Record = (from item in Record
                          where item.ProviderID == mainrequest.ProviderID
                          select item).ToList();
            }

            if (!mainrequest.PaymentType.EqualsIgnoreCase(""))
            {
                Record = (from item in Record
                          where item.PaymentType == mainrequest.PaymentType
                          select item).ToList();
            }
            return Record;
        }

        internal List<CallbackResponse> TransactionSelectForAPIUserCallback()
        {
            int TodaysDateInt = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            List<CallbackResponse> Record = new List<CallbackResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.DOCInt == TodaysDateInt
                          && transactionMaster.HitBackCount <= 0
                          && transactionMaster.UserMaster.UserTypeID == (int)UserType.APIUser
                          && transactionMaster.Status != StatusType.Process
                          select new CallbackResponse
                          {
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              APIUserRequestID = transactionMaster.APIUserRequestID,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ReferenceNo = transactionMaster.ReferenceNo,
                              UserTypeID = transactionMaster.UserMaster.UserTypeID,
                              UserID = transactionMaster.UserID,
                              ServiceID = transactionMaster.ServiceID,
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              OpeningBalance = transactionMaster.OpeningBalance,
                              ClosingBalance = transactionMaster.ClosingBalance,
                          }).Take(10).ToList();
            }
            return Record;
        }

        internal CallbackResponse TransactionSelectForAPIUserCallbackByTransactionID(int TransactionMasterID)
        {
            CallbackResponse Record = new CallbackResponse();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.TransactionMasterID == TransactionMasterID
                          orderby transactionMaster.TransactionMasterID descending

                          select new CallbackResponse
                          {
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              APIUserRequestID = transactionMaster.APIUserRequestID,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ReferenceNo = transactionMaster.ReferenceNo,
                              UserTypeID = transactionMaster.UserMaster.UserTypeID,
                              UserID = transactionMaster.UserID,
                              ServiceID = transactionMaster.ServiceID,
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              OpeningBalance = transactionMaster.OpeningBalance,
                              ClosingBalance = transactionMaster.ClosingBalance,
                          }).FirstOrDefault();
            }
            return Record;
        }

        internal decimal SelectTodaySumByUserIDAndOperatorID(int UserID, int OperatorID)
        {
            int From = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            decimal sumAmt = 0;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                sumAmt = (from TransactionMaster in context.TransactionMaster
                          where TransactionMaster.DOCInt == From
                          && TransactionMaster.UserID == UserID
                          && TransactionMaster.OperatorID == OperatorID
                          && (TransactionMaster.Status == StatusType.Success || TransactionMaster.Status == StatusType.Process)

                          select TransactionMaster).Sum(e => e.Amount);
            }
            return sumAmt;
        }

        internal decimal SelectTodaySumByUserIDAndOperatorIDStateID(int UserID, int OperatorID, int StateID)
        {
            int From = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            decimal sumAmt = 0;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                sumAmt = (from TransactionMaster in context.TransactionMaster
                          where TransactionMaster.DOCInt == From
                          && TransactionMaster.UserID == UserID
                          && TransactionMaster.OperatorID == OperatorID
                          && TransactionMaster.StateID == StateID
                          && (TransactionMaster.Status == StatusType.Success || TransactionMaster.Status == StatusType.Process)

                          select TransactionMaster).Sum(e => e.Amount);
            }
            return sumAmt;
        }

        internal int SelectDailyAEPSFailCountByConsumerNumber(string ConsumerNumber)
        {
            int From = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            List<TransactionMaster> list = new List<TransactionMaster>();
            try
            {
                using var context = new DatabaseContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                list = (from tran in context.TransactionMaster
                        where tran.DOCInt == From
                        && tran.ConsumerNumber == ConsumerNumber
                        && tran.ServiceID == (int)ServiceCode.AEPS
                        && tran.Status == StatusType.Failure
                        select tran).ToList();
            }
            catch (Exception e)
            {
            }

            return list.Count;
        }

        internal StatusCheckResponse SelectByAPIUserRequestIDAndUserID(string APIUserRequestID, int UserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        where transactionMaster.UserID == UserID &&
                        transactionMaster.APIUserRequestID == APIUserRequestID
                        orderby transactionMaster.TransactionMasterID descending
                        select new StatusCheckResponse
                        {
                            TransactionMasterID = transactionMaster.TransactionMasterID,
                            CustomerNumber = transactionMaster.ConsumerNumber,
                            Amount = transactionMaster.Amount,
                            OperatorID = transactionMaster.OperatorID,
                            APIUserRequestID = transactionMaster.APIUserRequestID,
                            Status = transactionMaster.Status,
                            ReferenceNo = transactionMaster.ReferenceNo ?? "0",
                            OpeningBalance = transactionMaster.OpeningBalance,
                            ClosingBalance = transactionMaster.ClosingBalance,
                            Description = transactionMaster.Description,
                        }).FirstOrDefault();
            }
        }

        internal StatusCheckResponse SelectByAPIUserRequestIDAndUserIDByTodaysDate(string APIUserRequestID, int UserID)
        {
            int TodaysDateInt = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return (from transactionMaster in context.TransactionMaster
                        where transactionMaster.DOCInt == TodaysDateInt
                        && transactionMaster.UserID == UserID
                        && transactionMaster.APIUserRequestID == APIUserRequestID
                        orderby transactionMaster.TransactionMasterID descending
                        select new StatusCheckResponse
                        {
                            TransactionMasterID = transactionMaster.TransactionMasterID,
                            CustomerNumber = transactionMaster.ConsumerNumber,
                            Amount = transactionMaster.Amount,
                            OperatorID = transactionMaster.OperatorID,
                            APIUserRequestID = transactionMaster.APIUserRequestID,
                            Status = transactionMaster.Status,
                            ReferenceNo = transactionMaster.ReferenceNo ?? "0",
                            OpeningBalance = transactionMaster.OpeningBalance,
                            ClosingBalance = transactionMaster.ClosingBalance,
                            Description = transactionMaster.Description,
                        }).FirstOrDefault();
            }
        }
        internal bool IsAPIUserRequestIDExists(string APIUserRequestID, int UserID)
        {
            int TodaysDateInt = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster
                      .Any(t => t.DOCInt == TodaysDateInt &&
                                t.UserID == UserID &&
                                t.APIUserRequestID == APIUserRequestID);
            }
        }

        internal StatusCheckResponse SelectByAPIUserRequestIDAndUserIDByDate(string APIUserRequestID, int UserID, int DocInt)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionMaster in context.TransactionMaster
                        where transactionMaster.DOCInt == DocInt
                        && transactionMaster.UserID == UserID
                        && transactionMaster.APIUserRequestID == APIUserRequestID

                        orderby transactionMaster.TransactionMasterID descending
                        select new StatusCheckResponse
                        {
                            TransactionMasterID = transactionMaster.TransactionMasterID,
                            CustomerNumber = transactionMaster.ConsumerNumber,
                            Amount = transactionMaster.Amount,
                            OperatorID = transactionMaster.OperatorID,
                            APIUserRequestID = transactionMaster.APIUserRequestID,
                            Status = transactionMaster.Status,
                            ReferenceNo = transactionMaster.ReferenceNo ?? "0",
                            OpeningBalance = transactionMaster.OpeningBalance,
                            ClosingBalance = transactionMaster.ClosingBalance,
                            Description = transactionMaster.Description,
                        }).FirstOrDefault();
            }
        }

        internal List<CommissionViewModel> SelectRoleWithCommission(int TransactionMasterID, int UserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from tran in context.TransactionMaster
                        join dist in context.UserMaster on tran.DistributerUserID equals dist.UserID into DistDetails
                        from DL in DistDetails.DefaultIfEmpty()
                        join super in context.UserMaster on tran.SuperDistributerUserID equals super.UserID into superDetails
                        from SL in superDetails.DefaultIfEmpty()
                        where tran.TransactionMasterID == TransactionMasterID
                        && tran.UserID == UserID
                        select new CommissionViewModel
                        {
                            DistributorID = tran.DistributerUserID,
                            DistributorName = DL.FirmName.ConcatNameMobileAliasName(DL.MobileNumber, DL.AliasName),
                            DistributorCommission = tran.DistributerCommission,
                            SuperDistributorID = tran.SuperDistributerUserID,
                            SuperDistributorName = SL.FirmName.ConcatNameMobileAliasName(SL.MobileNumber, SL.AliasName),
                            SuperDistributorCommission = tran.SuperDistributerCommission,
                        }).ToList();
            }
        }

        internal async Task<int> SelectPendingTransactionCount()
        {
            int PendingTransactionCount = 0;

            using var context = new DatabaseContext();
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                PendingTransactionCount = await context.TransactionMaster.Where(t => t.Status == StatusType.Process &&
                t.Amount >= 1 &&
                t.ServiceID != (int)ServiceCode.AEPS &&
                t.ServiceID != (int)ServiceCode.MoneyTransfer &&
                t.ServiceID != (int)ServiceCode.MATM &&
                t.ServiceID != (int)ServiceCode.Payout &&
                t.ServiceID != (int)ServiceCode.CashDeposit &&
                t.ServiceID != (int)ServiceCode.PanCard &&
                t.ServiceID != (int)ServiceCode.UPITransfer &&
                t.ServiceID != (int)ServiceCode.CreditCardPayment).CountAsync();
            }
            return PendingTransactionCount;
        }

        internal async Task<int> SelectPendingDMTTransactionCount()
        {
            int PendingTransactionCount = 0;

            using var context = new DatabaseContext();
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                PendingTransactionCount = await context.TransactionMaster.Where(t => t.Status == StatusType.Process &&
                t.ServiceID == (int)ServiceCode.MoneyTransfer).CountAsync();
            }
            return PendingTransactionCount;
        }

        internal async Task<int> PendingUPITransactionCount()
        {
            int PendingTransactionCount = 0;

            using var context = new DatabaseContext();
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                PendingTransactionCount = await context.TransactionMaster.Where(t => t.Status == StatusType.Process &&
                t.ServiceID == (int)ServiceCode.UPITransfer).CountAsync();
            }
            return PendingTransactionCount;
        }

        internal List<Project.Response.Invoice.ServicewiseSaleViewModel> SelectServicewiseSale(Project.Requests.Invoice.SelectByTypeRequest mainRequest)
        {
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<Project.Response.Invoice.ServicewiseSaleViewModel> list = new List<Project.Response.Invoice.ServicewiseSaleViewModel>();

            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();
            List<Project.Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                    var mainlist = (from TransactionMaster in context.TransactionMaster
                                    where
                                    TransactionMaster.Status == StatusType.Success &&
                                    TransactionMaster.DOCInt >= From && TransactionMaster.DOCInt <= To
                                    select new
                                    {
                                        Amount = TransactionMaster.Amount,
                                        DebitAmount = TransactionMaster.DebitAmount,
                                        ServiceID = TransactionMaster.ServiceID,
                                        OperatorID = TransactionMaster.OperatorID,
                                        UserID = TransactionMaster.UserID,
                                        RetailerCommission = TransactionMaster.RetailerCommission,
                                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                                        DistributerUserID = TransactionMaster.DistributerUserID,
                                    }).ToList();

                    int UserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);

                    if (!Util.isRoleAdminOrSupport(UserTypeID))
                    {
                        if (UserTypeID == (int)UserType.SuperDistributer)
                        {
                            mainlist = (from item in mainlist
                                        where item.SuperDistributerUserID == mainRequest.UserID
                                        select item).ToList();
                        }
                        else if (UserTypeID == (int)UserType.Distributer)
                        {
                            mainlist = (from item in mainlist
                                        where item.DistributerUserID == mainRequest.UserID
                                        select item).ToList();
                        }
                        else
                        {
                            mainlist = (from item in mainlist
                                        where item.UserID == mainRequest.UserID
                                        select item).ToList();
                        }
                    }

                    var result = (from item in mainlist
                                  group item by item.ServiceID into t
                                  join s in operators on t.FirstOrDefault().OperatorID equals s.OperatorID
                                  select new Project.Response.Invoice.ServicewiseSaleViewModel
                                  {
                                      ServiceID = t.Key,
                                      Type = s.BusinessModel,
                                      Qty = t.Sum(tr => tr.Amount),
                                      Rate = t.Sum(tr => tr.DebitAmount),
                                      DiscountAmount = t.Sum(tr => tr.RetailerCommission),
                                  }).ToList();

                    list = (from t in result
                            join s in services on t.ServiceID equals s.ServiceID
                            select new Project.Response.Invoice.ServicewiseSaleViewModel
                            {
                                ServiceName = s.ServiceName,
                                ServiceID = t.ServiceID,
                                Type = t.Type,
                                Qty = t.Qty,
                                Rate = t.Rate,
                                DiscountAmount = (t.DiscountAmount <= 0 ? 0 : t.DiscountAmount),
                                TaxableAmount = t.Type == "P2A" ? 0 : (t.Rate * 100) / 118,
                            }
                         ).ToList();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<Project.Response.Invoice.OperatorWiseSaleViewModel> SelectOperatorwiseSale(Project.Requests.Invoice.SelectByTypeRequest mainRequest)
        {
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<Project.Response.Invoice.OperatorWiseSaleViewModel> list = new List<Project.Response.Invoice.OperatorWiseSaleViewModel>();

            List<Project.Response.Operator.ViewModel> operatorsList = new OperatorMasterDAL().Select();
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                    var mainlist = (from TransactionMaster in context.TransactionMaster
                                    where
                                    TransactionMaster.Status == StatusType.Success &&
                                    TransactionMaster.DOCInt >= From && TransactionMaster.DOCInt <= To
                                    select new
                                    {
                                        Amount = TransactionMaster.Amount,
                                        DebitAmount = TransactionMaster.DebitAmount,
                                        OperatorID = TransactionMaster.OperatorID,
                                        UserID = TransactionMaster.UserID,
                                        RetailerCommission = TransactionMaster.RetailerCommission,
                                        SuperDistributerUserID = TransactionMaster.SuperDistributerUserID,
                                        DistributerUserID = TransactionMaster.DistributerUserID,
                                    }).ToList();

                    int UserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);

                    if (!Util.isRoleAdminOrSupport(UserTypeID))
                    {
                        if (UserTypeID == (int)UserType.SuperDistributer)
                        {
                            mainlist = (from item in mainlist
                                        where item.SuperDistributerUserID == mainRequest.UserID
                                        select item).ToList();
                        }
                        else if (UserTypeID == (int)UserType.Distributer)
                        {
                            mainlist = (from item in mainlist
                                        where item.DistributerUserID == mainRequest.UserID
                                        select item).ToList();
                        }
                        else
                        {
                            mainlist = (from item in mainlist
                                        where item.UserID == mainRequest.UserID
                                        select item).ToList();
                        }
                    }

                    var result = (from item in mainlist
                                  group item by item.OperatorID into t
                                  join s in operatorsList on t.FirstOrDefault().OperatorID equals s.OperatorID
                                  select new Project.Response.Invoice.OperatorWiseSaleViewModel
                                  {
                                      OperatorID = t.Key,
                                      Type = s.BusinessModel,
                                      Qty = t.Sum(tr => tr.Amount),
                                      Rate = t.Sum(tr => tr.DebitAmount),
                                      DiscountAmount = t.Sum(tr => tr.RetailerCommission),
                                  }).ToList();

                    list = (from t in result
                            join s in operatorsList on t.OperatorID equals s.OperatorID
                            select new Project.Response.Invoice.OperatorWiseSaleViewModel
                            {
                                OperatorName = s.OperatorName,
                                OperatorID = t.OperatorID,
                                Type = s.BusinessModel,
                                Qty = t.Qty,
                                Rate = t.Rate,
                                DiscountAmount = (t.DiscountAmount <= 0) ? 0 : t.DiscountAmount,
                                TaxableAmount = t.Type == "P2A" ? 0 : (t.Rate * 100) / 118,
                            }
                         ).ToList();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal string SelectLastTransactionDateByID(int userID)
        {
            string LastTransactionDate = "No transaction found";
            try
            {
                using var context = new DatabaseContext();
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                TransactionMaster model = context.TransactionMaster.OrderByDescending(t => t.DOCInt).FirstOrDefault(t => t.UserID == userID);
                if (model == null)
                {
                    return LastTransactionDate;
                }
                LastTransactionDate = model.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return LastTransactionDate;
            }
            return LastTransactionDate;
        }

        internal SucessAmountResponse SelectSucessAmountByWallet(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from master in context.TransactionMaster
                        where master.DOCInt >= From
                         && master.DOCInt <= To
                         && master.Status == StatusType.Success
                        && master.UserID == UserID
                        && master.WalletID == WalletID
                        && master.ServiceID != (int)ServiceCode.AEPS
                        group master by new { master.Status } into grp
                        select new SucessAmountResponse
                        {
                            Request = grp.Sum(x => x.Amount),
                            Debited = grp.Sum(x => x.DebitAmount),
                        }).FirstOrDefault();
            }
        }

        internal SucessAmountResponse SelectSucessAllAmount(int UserID, int From, int To)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from master in context.TransactionMaster
                        where master.DOCInt >= From
                         && master.DOCInt <= To
                         && master.Status == StatusType.Success
                         && master.UserID == UserID
                         && master.ServiceID != (int)ServiceCode.AEPS
                        group master by new { master.Status } into grp
                        select new SucessAmountResponse
                        {
                            Request = grp.Sum(x => x.Amount),
                            Debited = grp.Sum(x => x.DebitAmount),
                        }).FirstOrDefault();
            }
        }

        internal decimal SelectSuperDistributerCommi(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var SuperMainCommission = (from master in context.TransactionMaster
                                           where master.DOCInt >= From
                                               && master.DOCInt <= To
                                               && master.Status == StatusType.Success
                                               && master.SuperDistributerCommission >= 0
                                               && master.SuperDistributerUserID == UserID
                                               && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                           group master by new { master.Status } into grp
                                           select new
                                           {
                                               SuperDistributerCommission = grp.Sum(x => x.SuperDistributerCommission),
                                           }).FirstOrDefault();
                decimal Commi = SuperMainCommission?.SuperDistributerCommission ?? 0;
                return Commi;
            }
        }

        internal decimal SelectSuperDistributerSurcharge(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var SuperMainSurcharge = (from master in context.TransactionMaster
                                          where master.DOCInt >= From
                                              && master.DOCInt <= To
                                              && master.Status == StatusType.Success
                                              && master.SuperDistributerCommission < 0
                                              && master.SuperDistributerUserID == UserID
                                              && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                          group master by new { master.Status } into grp
                                          select new
                                          {
                                              SuperDistributerSurcharge = Math.Abs(grp.Sum(x => x.SuperDistributerCommission)),
                                          }).FirstOrDefault();
                decimal Surcharge = SuperMainSurcharge?.SuperDistributerSurcharge ?? 0;
                return Surcharge;
            }
        }

        internal decimal SelectDistributerCommi(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var DistMainCommission = (from master in context.TransactionMaster
                                          where master.DOCInt >= From
                                              && master.DOCInt <= To
                                              && master.Status == StatusType.Success
                                              && master.DistributerCommission >= 0
                                              && master.DistributerUserID == UserID
                                              && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                          group master by new { master.Status } into grp
                                          select new
                                          {
                                              DistributerCommission = grp.Sum(x => x.DistributerCommission),
                                          }).FirstOrDefault();
                decimal Commi = DistMainCommission?.DistributerCommission ?? 0;
                return Commi;
            }
        }

        internal decimal SelectDistributerSurcharge(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var DistMainSurcharge = (from master in context.TransactionMaster
                                         where master.DOCInt >= From
                                             && master.DOCInt <= To
                                             && master.Status == StatusType.Success
                                             && master.DistributerCommission < 0
                                             && master.DistributerUserID == UserID
                                             && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                         group master by new { master.Status } into grp
                                         select new
                                         {
                                             DistributerSurcharge = Math.Abs(grp.Sum(x => x.DistributerCommission)),
                                         }).FirstOrDefault();
                decimal Surcharge = DistMainSurcharge?.DistributerSurcharge ?? 0;
                return Surcharge;
            }
        }

        internal decimal SelectRetailerCommi(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var RetailerMainCommission = (from master in context.TransactionMaster
                                              where master.DOCInt >= From
                                                  && master.DOCInt <= To
                                                  && master.Status == StatusType.Success
                                                  && master.RetailerCommission >= 0
                                                  && master.UserID == UserID
                                                  && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                              group master by new { master.Status } into grp
                                              select new
                                              {
                                                  RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                              }).FirstOrDefault();
                decimal Commi = RetailerMainCommission?.RetailerCommission ?? 0;
                return Commi;
            }
        }

        internal decimal SelectSuperDistributerTDS(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var SuperMainTDS = (from master in context.TransactionMaster
                                    where master.DOCInt >= From
                                        && master.DOCInt <= To
                                        && master.Status == StatusType.Success
                                        && master.SuperDistributerUserID == UserID
                                        && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                    group master by new { master.Status } into grp
                                    select new
                                    {
                                        SuperDistributerTDS = Math.Abs(grp.Sum(x => x.SuperDistributerTDS)),
                                    }).FirstOrDefault();
                decimal TDS = SuperMainTDS?.SuperDistributerTDS ?? 0;
                return TDS;
            }
        }

        internal decimal SelectDistributerTDS(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var DistMainTDS = (from master in context.TransactionMaster
                                   where master.DOCInt >= From
                                       && master.DOCInt <= To
                                       && master.Status == StatusType.Success
                                       && master.DistributerUserID == UserID
                                       && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                   group master by new { master.Status } into grp
                                   select new
                                   {
                                       DistributerTDS = Math.Abs(grp.Sum(x => x.DistributerTDS)),
                                   }).FirstOrDefault();
                decimal TDS = DistMainTDS?.DistributerTDS ?? 0;
                return TDS;
            }
        }

        internal decimal SelectRetailerTDS(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var RetailerMainTDS = (from master in context.TransactionMaster
                                       where master.DOCInt >= From
                                           && master.DOCInt <= To
                                           && master.Status == StatusType.Success
                                           && master.UserID == UserID
                                           && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                       group master by new { master.Status } into grp
                                       select new
                                       {
                                           RetailerTDS = Math.Abs(grp.Sum(x => x.RetailerTDS)),
                                       }).FirstOrDefault();
                decimal TDS = RetailerMainTDS?.RetailerTDS ?? 0;
                return TDS;
            }
        }

        internal decimal SelectTotalCommi(int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var Commission = (from master in context.TransactionMaster
                                  where master.DOCInt >= From
                                      && master.DOCInt <= To
                                      && master.Status == StatusType.Success
                                      && master.RetailerCommission >= 0
                                      && master.SuperDistributerCommission >= 0
                                      && master.DistributerCommission >= 0
                                      && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                  group master by new { master.Status } into grp
                                  select new
                                  {
                                      RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                      DistributerCommission = grp.Sum(x => x.DistributerCommission),
                                      SuperDistributerCommission = grp.Sum(x => x.SuperDistributerCommission),
                                  }).FirstOrDefault();
                decimal Commi = (Commission?.RetailerCommission ?? 0) + (Commission?.DistributerCommission ?? 0) + (Commission?.SuperDistributerCommission ?? 0);
                return Commi;
            }
        }

        internal decimal SelectAEPSCommi(int From, int To)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var Commission = (from master in context.TransactionMaster
                                  where master.DOCInt >= From
                                      && master.DOCInt <= To
                                      && master.Status == StatusType.Success
                                      && master.ServiceID == (int)ServiceCode.AEPS
                                      && master.RetailerCommission >= 0
                                      && master.SuperDistributerCommission >= 0
                                      && master.DistributerCommission >= 0
                                      && master.WalletID == (int)WalletCode.AEPS
                                  group master by new { master.Status } into grp
                                  select new
                                  {
                                      RetailerCommission = grp.Sum(x => x.RetailerCommission),
                                      DistributerCommission = grp.Sum(x => x.DistributerCommission),
                                      SuperDistributerCommission = grp.Sum(x => x.SuperDistributerCommission),
                                  }).FirstOrDefault();
                decimal Commi = (Commission?.RetailerCommission ?? 0) + (Commission?.DistributerCommission ?? 0) + (Commission?.SuperDistributerCommission ?? 0);
                return Commi;
            }
        }

        internal decimal SelectTotalSurcharge(int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var Commission = (from master in context.TransactionMaster
                                  where master.DOCInt >= From
                                      && master.DOCInt <= To
                                      && master.Status == StatusType.Success
                                      && master.RetailerCommission <= 0
                                      && master.DistributerCommission <= 0
                                      && master.SuperDistributerCommission <= 0
                                      && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                  group master by new { master.Status } into grp
                                  select new
                                  {
                                      RetailerCommission = grp.Sum(x => Math.Abs(x.RetailerCommission)),
                                      DistributerCommission = grp.Sum(x => Math.Abs(x.DistributerCommission)),
                                      SuperDistributerCommission = grp.Sum(x => Math.Abs(x.SuperDistributerCommission)),
                                  }).FirstOrDefault();
                decimal Commi = (Commission?.RetailerCommission ?? 0) + (Commission?.DistributerCommission ?? 0) + (Commission?.SuperDistributerCommission ?? 0);
                return Commi;
            }
        }

        internal decimal SelectRetailerSurcharge(int UserID, int From, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var RetailerMainSurcharge = (from master in context.TransactionMaster
                                             where master.DOCInt >= From
                                                 && master.DOCInt <= To
                                                 && master.Status == StatusType.Success
                                                 && master.RetailerCommission < 0
                                                 && master.UserID == UserID
                                                 && (WalletID != 0 ? master.WalletID == WalletID : 2 > 1)
                                             group master by new { master.Status } into grp
                                             select new
                                             {
                                                 RetailerSurcharge = Math.Abs(grp.Sum(x => x.RetailerCommission)),
                                             }).FirstOrDefault();
                decimal Surcharge = RetailerMainSurcharge?.RetailerSurcharge ?? 0;
                return Surcharge;
            }
        }

        internal PastResponse SelectPastSucessFailAmount(int UserID, DateTime From, DateTime To, int WalletID)
        {
            DateTime datetimefrom = (Convert.ToDateTime(From));
            DateTime datetimeto = (Convert.ToDateTime(To));
            int dateIntfrom = Convert.ToInt32(Convert.ToDateTime(From).ToString(Messages.DOCIntegerFomrat));
            int dateIntto = Convert.ToInt32(Convert.ToDateTime(To).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var oldData = (from txn in context.TransactionMaster
                               where txn.UpdatedDOC.Date >= datetimefrom.Date
                               && txn.UpdatedDOC.Date <= datetimeto.Date && txn.UserID == UserID
                               && (WalletID != 0 ? txn.WalletID == WalletID : 2 > 1)
                               select new
                               {
                                   Status = txn.Status,
                                   TransactionMasterID = txn.TransactionMasterID,
                                   Amount = txn.DebitAmount,
                                   ConsumerNumber = txn.ConsumerNumber,
                                   DOCInt = txn.DOCInt,
                                   UpdatedDOCInt = Int32.Parse(txn.UpdatedDOC.ToString(Messages.DOCIntegerFomrat))
                               }).ToList();

                var skipIdList = oldData.Where(q => q.DOCInt >= dateIntfrom && q.DOCInt <= dateIntto).Select(q => q.TransactionMasterID).ToList();
                oldData = oldData.Where(q => q.TransactionMasterID != null && !skipIdList.Contains(q.TransactionMasterID)).ToList();

                decimal pastFailAmount = (from txn in oldData
                                          where txn.UpdatedDOCInt - txn.DOCInt >= 1 && txn.Status == StatusType.Failure
                                          select txn).Sum(o => o.Amount);

                decimal pastSuccessAmount = (from txn in oldData
                                             where txn.UpdatedDOCInt - txn.DOCInt >= 1 && txn.Status == StatusType.Success
                                             select txn).Sum(o => o.Amount);
                PastResponse response = new PastResponse();
                response.PastFailAmount = pastFailAmount;
                response.PastSuccessAmount = pastSuccessAmount;
                return response;
            }
        }

        internal PastResponse SelectPastSucessFailAllAmount(int UserID, DateTime From, DateTime To)
        {
            DateTime datetimefrom = (Convert.ToDateTime(From));
            DateTime datetimeto = (Convert.ToDateTime(To));
            int dateIntfrom = Convert.ToInt32(Convert.ToDateTime(From).ToString(Messages.DOCIntegerFomrat));
            int dateIntto = Convert.ToInt32(Convert.ToDateTime(To).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var oldData = (from txn in context.TransactionMaster
                               where txn.UpdatedDOC.Date >= datetimefrom.Date
                               && txn.UpdatedDOC.Date <= datetimeto.Date && txn.UserID == UserID
                               select new
                               {
                                   TransactionMasterID = txn.TransactionMasterID,
                                   Status = txn.Status,
                                   Amount = txn.DebitAmount,
                                   ConsumerNumber = txn.ConsumerNumber,
                                   DOCInt = txn.DOCInt,
                                   UpdatedDOCInt = Int32.Parse(txn.UpdatedDOC.ToString(Messages.DOCIntegerFomrat))
                               }).ToList();
                var skipIdList = oldData.Where(q => q.DOCInt >= dateIntfrom && q.DOCInt <= dateIntto).Select(q => q.TransactionMasterID).ToList();
                oldData = oldData.Where(q => q.TransactionMasterID != null && !skipIdList.Contains(q.TransactionMasterID)).ToList();

                decimal pastFailAmount = (from txn in oldData
                                          where txn.UpdatedDOCInt - txn.DOCInt >= 1 && txn.Status == StatusType.Failure
                                          select txn).Sum(o => o.Amount);

                decimal pastSuccessAmount = (from txn in oldData
                                             where txn.UpdatedDOCInt - txn.DOCInt >= 1 && txn.Status == StatusType.Success
                                             select txn).Sum(o => o.Amount);
                PastResponse response = new PastResponse();
                response.PastFailAmount = pastFailAmount;
                response.PastSuccessAmount = pastSuccessAmount;
                return response;
            }
        }

        internal string SelectReferenceNoByTransactionMasterID(int TransactionMasterID)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return context.TransactionMaster.FirstOrDefault(o => o.TransactionMasterID == TransactionMasterID).ReferenceNo;
        }

        internal float SelectTodaysCommission(int UserID)
        {
            decimal Commission = 0;
            int toDaysDateInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            UserMaster ObjModel = new UserDAL().SelectByIDFromCache(UserID);
            if (ObjModel == null) return (float)Commission;

            int UserTypeID = ObjModel.UserTypeID;
            if (UserTypeID == (int)UserType.Retailer)
            {
                Commission = GetTodaysCommissionForRetailer(UserID, toDaysDateInt);
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                Commission = GetTodaysCommissionForDistributer(UserID, toDaysDateInt);
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                Commission = GetTodaysCommissionForSuperDistributer(UserID, toDaysDateInt);
            }

            return (float)Commission;
        }

        private decimal GetTodaysCommissionForRetailer(int userID, int toDaysDateInt)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            decimal sum = context.TransactionMaster
                .Where(t => t.DOCInt == toDaysDateInt &&
                            t.Status == StatusType.Success &&
                            t.UserID == userID &&
                            t.RetailerCommission >= 0)
                .Select(t => t.RetailerCommission)
                .DefaultIfEmpty()
                .Sum();

            return sum;
        }

        private decimal GetTodaysCommissionForDistributer(int userID, int toDaysDateInt)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return context.TransactionMaster.Where(t => t.DOCInt == toDaysDateInt && t.Status == StatusType.Success
            && t.DistributerUserID == userID && t.DistributerCommission >= 0).Sum(t => t.DistributerCommission + t.DistributerCommissionHold);
        }

        private decimal GetTodaysCommissionForSuperDistributer(int userID, int toDaysDateInt)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return context.TransactionMaster.Where(t => t.DOCInt == toDaysDateInt && t.Status == StatusType.Success &&
                                                    t.SuperDistributerUserID == userID &&
                                                    t.SuperDistributerCommission >= 0).Sum(t => t.SuperDistributerCommission + t.SuperDistributerCommission);
        }

        internal bool CheckDuplicateTransaction(int ServiceID, int OperatorID, string ConsumerNumber, decimal Amount, DateTime CurrentDateTime, int UserID)
        {
            int yesterdayInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-1)).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                bool hasDuplicateTransaction = context.TransactionMaster
                    .Where(t => t.DOCInt >= yesterdayInt && t.UserID == UserID)
                    .Any(t => t.ServiceID == ServiceID &&
                              t.OperatorID == OperatorID &&
                              t.ConsumerNumber == ConsumerNumber &&
                              t.Amount == Amount &&
                              t.DOC > CurrentDateTime &&
                              t.Status != StatusType.Failure);

                return hasDuplicateTransaction;
            }
        }
        internal bool CheckDuplicate2FATransaction(int ServiceID, int OperatorID, int UserID)
        {
            int CurrentDateInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                bool hasDuplicateTransaction = context.TransactionMaster
                    .Where(t => t.DOCInt == CurrentDateInt && t.UserID == UserID)
                    .Any(t => t.ServiceID == ServiceID &&
                              t.OperatorID == OperatorID &&
                              t.Status != StatusType.Failure);

                return hasDuplicateTransaction;
            }
        }

        internal List<TransactionMaster> SelectTransactionByOperatorIDAmountForDuplicate(int OperatorID, int SerialCount)
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return (from item in context.TransactionMaster
                    where item.DOCInt == today && item.OperatorID == OperatorID
                    orderby item.TransactionMasterID descending
                    select item).Take(SerialCount).ToList();
        }

        internal TransactionMaster SelectTransactionForCheckDMTDuplicate(int UserID, int ServiceID, int OperatorID, string ConsumerNumber, decimal Amount, DateTime CurrentDateTime)
        {
            int docInt = Convert.ToInt32(Convert.ToDateTime(CurrentDateTime).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.TransactionMaster.OrderByDescending(t => t.TransactionMasterID)
                    .FirstOrDefault(t => t.DOCInt == docInt && t.UserID == UserID && t.ServiceID == ServiceID &&
                                         t.OperatorID == OperatorID && t.ConsumerNumber == ConsumerNumber &&
                                         t.DMT_OriginalAmount == Amount && t.DOC > CurrentDateTime &&
                                         t.Status != StatusType.Failure);
            }
        }

        internal List<TransactionMaster> SelectPendingXmppTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today && c.ProviderID == Company.XMPPProviderID && c.Status == StatusType.Process).ToList();
            }
            return list;
        }

        internal List<PayOutViewModel> PayoutAccountListByMonthAndUserID(int UserID)
        {
            List<PayOutViewModel> list = new List<PayOutViewModel>();
            DateTime first = DateTime.Now.StartOfMonth();
            DateTime last = DateTime.Now.LastOfMonth();

            int MonthStart = Convert.ToInt32(first.ToString(Messages.DOCIntegerFomrat));
            int MonthEnd = Convert.ToInt32(last.ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from txn in context.TransactionMaster
                        where txn.DOCInt >= MonthStart
                        && txn.DOCInt <= MonthEnd && txn.Status == StatusType.Success
                        && txn.UserID == UserID
                        && txn.ServiceID == (int)ServiceCode.Payout
                        group txn by txn.ConsumerNumber into g
                        select new PayOutViewModel
                        {
                            AccountNo = g.Key,
                            Count = g.Count()
                        }).ToList();
            }

            return list;
        }

        internal List<PendingResponse> SelectDMTNEFTPendingTransaction()
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process
                                && transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                                && transactionMaster.DMT_PaymentType == PaymentModeType.NEFT
                                && transactionMaster.Amount >= 1

                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              SenderNo = transactionMaster.DMT_SenderNumber,
                              Bank = transactionMaster.DMT_Bank,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = "",//provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              OriginalDOC = transactionMaster.DOC,
                              ProviderRequestID = transactionMaster.ProviderRequestID
                          }).ToList();
            }
            return Record;
        }

        internal List<PendingResponse> SelectDMTIMPSPendingTransaction()
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process
                               && transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer
                               && transactionMaster.DMT_PaymentType == PaymentModeType.IMPS
                               && transactionMaster.Amount >= 1

                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              SenderNo = transactionMaster.DMT_SenderNumber,
                              Bank = transactionMaster.DMT_Bank,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = "", //provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              OriginalDOC = transactionMaster.DOC,
                              ProviderRequestID = transactionMaster.ProviderRequestID
                          }).ToList();
            }
            return Record;
        }

        internal bool IsSequentialPendingRechargeByProviderAndOperatorID(int ProviderID, int OperatorID)
        {
            List<TransactionMaster> Record = new List<TransactionMaster>();
            ProviderConfiguration config = new ProviderConfigurationDAL().SelectByProviderIDAndOperatorID(ProviderID, OperatorID);
            if (config == null || config.PendingCount <= 0 || config.PendingInMinute <= 0)
                return false;
            int PendingCount = Math.Abs(config.PendingCount);
            int PendingInMinute = Math.Abs(config.PendingInMinute);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var fromDate = DateTime.Now.AddMinutes(-1 * PendingInMinute);

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process
                                && transactionMaster.OperatorID == OperatorID
                                && transactionMaster.ProviderID == ProviderID
                                && transactionMaster.DOC >= fromDate
                          select transactionMaster
                          ).ToList();

                if (Record == null)
                    return false;

                if (Record.Count > PendingCount)
                    return true;

                return false;
            }
        }

        internal bool IsSequentialFailRechargeByProviderAndOperatorID(int ProviderID, int OperatorID)
        {
            List<TransactionMaster> Record = new List<TransactionMaster>();
            ProviderConfiguration config = new ProviderConfigurationDAL().SelectByProviderIDAndOperatorID(ProviderID, OperatorID);

            if (config == null || config.FailCount <= 0 || config.FailInMinute <= 0)
                return false;

            int FailCount = Math.Abs(config.FailCount);
            int FailInMinute = Math.Abs(config.FailInMinute);

            int loopcount = 0;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var fromDate = DateTime.Now.AddMinutes(-1 * FailInMinute);

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.OperatorID == OperatorID
                                && transactionMaster.ProviderID == ProviderID
                                && transactionMaster.DOC >= fromDate
                          orderby transactionMaster.TransactionMasterID descending
                          select transactionMaster
                          ).ToList();
            }

            if (Record == null)
                return false;

            foreach (TransactionMaster transaction in Record)
            {
                if (loopcount <= FailCount)
                {
                    if (transaction.Status.EqualsIgnoreCase(StatusType.Success))
                        return false;
                    loopcount++;
                }
            }

            return true;
        }

        internal List<TransactionMaster> SelectPendingAEPSTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.AEPS
                ).ToList();
            }
            return list;
        }

        internal List<TransactionMaster> SelectPendingMicroATMTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.MATM
                ).ToList();
            }
            return list;
        }

        internal List<TransactionMaster> SelectPendingPOSTransaction()
        {
            // To-Do need to change later
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-1)).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt >= today
                && c.Status == StatusType.Process
                && c.ServiceID == (int)ServiceCode.POS
                ).ToList();
            }
            return list;
        }

        internal List<Response.User.UserViewForBackup> SelectDistinctUserHasTransaction()
        {
            int yesturday = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(0)).ToString(Messages.DOCIntegerFomrat));
            List<Response.User.UserViewForBackup> list = new List<Response.User.UserViewForBackup>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return list = (from txn in context.TransactionMaster

                               where txn.DOCInt == yesturday
                                && (txn.UserMaster.UserTypeID == (int)UserType.Retailer || txn.UserMaster.UserTypeID == (int)UserType.APIUser)
                               orderby txn.DOC descending
                               select new Response.User.UserViewForBackup
                               {
                                   FirmName = txn.UserMaster.FirmName,
                                   EmailID = txn.UserMaster.EmailID,
                                   UserID = txn.UserID,
                               }).Distinct().ToList();
            }
        }

        internal List<TransactionBackup> SelectTransactionBackup(int UserID)
        {
            List<TransactionBackup> txnbackup = new List<TransactionBackup>();
            int yesturday = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(0)).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                txnbackup = (from txn in context.TransactionMaster
                             where txn.DOCInt == yesturday
                             select new TransactionBackup
                             {
                                 FirmName = txn.UserMaster.FirmName,
                                 DateTime = txn.DOC.LongFormatDate(),
                                 Service = txn.ServiceMaster.Name,
                                 Wallet = txn.WalletMaster.Name,
                                 Operator = txn.OperatorMaster.OperatorName,
                                 UserID = txn.UserID,
                                 Amount = txn.Amount,
                                 Status = txn.Status,
                                 OpeningBal = txn.OpeningBalance,
                                 ClosingBal = txn.ClosingBalance,
                                 RetailerCommission = txn.RetailerCommission,
                                 DistributerCommission = txn.DistributerCommission,
                                 SuperDistributerCommission = txn.SuperDistributerCommission,
                             }).ToList();

                if (UserTypeID != (int)UserType.Admin && UserID != 0)
                {
                    txnbackup = (from item in txnbackup
                                 where item.UserID == UserID
                                 select item).ToList();
                }

                return txnbackup;
            }
        }

        internal List<TransactionMaster> SelectTransactionDatewise(int FromInt, int ToInt, int ServiceID)
        {
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt >= FromInt &&
                                         c.DOCInt <= ToInt &&
                                         c.Status == StatusType.Success
                                         && c.ServiceID == ServiceID).ToList();
            }
            return list;
        }

        internal List<FundTransaction> IsLast5DaysActiveUser()
        {
            int Back5DaysDOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-5)).ToString(Messages.DOCIntegerFomrat));
            List<FundTransaction> fundmodel = null;
            using var context = new DatabaseContext();
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                fundmodel = context.FundTransaction
                            .Where(t => t.DOCInt >= Back5DaysDOCInt)
                            .GroupBy(x => x.ReceiverID).Select(g => g.FirstOrDefault()).ToList();
            }

            return fundmodel;
        }

        internal List<PendingResponse> SelectUPIPendingTransactionWithFilter(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            List<PendingResponse> Record = new List<PendingResponse>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from transactionMaster in context.TransactionMaster
                          where transactionMaster.Status == StatusType.Process &&
                           transactionMaster.ServiceID == (int)ServiceCode.UPITransfer
                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID
                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              OperatorID = transactionMaster.OperatorID,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              ProviderID = transactionMaster.ProviderID,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              SenderNo = transactionMaster.DMT_SenderNumber,
                              Bank = transactionMaster.DMT_Bank,
                              Type = transactionMaster.DMT_PaymentType
                          }).ToList();
                if (mainRequest.ProviderID != 0)
                {
                    Record = (from item in Record
                              where item.ProviderID == mainRequest.ProviderID
                              select item).ToList();
                }
                if (mainRequest.OperatorID != 0)
                {
                    Record = (from item in Record
                              where item.OperatorID == mainRequest.OperatorID
                              select item).ToList();
                }
            }
            return Record;
        }

        internal HeaderViewModel SelectUPIPendingHeaderSummaryWithFilter(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            List<TransactionMaster> list = new List<TransactionMaster>();

            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            list = (from transactionMaster in context.TransactionMaster
                    where transactionMaster.ServiceID == (int)ServiceCode.UPITransfer
                        && transactionMaster.Status == StatusType.Process
                       && (!mainRequest.ConsumerNumber.EqualsIgnoreCase("") ? transactionMaster.ConsumerNumber == mainRequest.ConsumerNumber : 2 > 1)
                    join provider in context.ProviderMaster on
                     transactionMaster.ProviderID equals provider.ProviderID
                    select transactionMaster).ToList();

            if (mainRequest.ProviderID != 0)
            {
                list = (from item in list
                        where item.ProviderID == mainRequest.ProviderID
                        select item).ToList();
            }
            if (mainRequest.OperatorID != 0)
            {
                list = (from item in list
                        where item.OperatorID == mainRequest.OperatorID
                        select item).ToList();
            }

            var Pending = (from item in list
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal List<PendingResponse> SelectUPIPendingTransaction()
        {
            List<PendingResponse> Record = new List<PendingResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                Record = (from transactionMaster in context.TransactionMaster
                          join provider in context.ProviderMaster on
                          transactionMaster.ProviderID equals provider.ProviderID
                          where transactionMaster.Status == StatusType.Process
                                && transactionMaster.Amount >= 1
                          && transactionMaster.ServiceID == (int)ServiceCode.UPITransfer
                          orderby transactionMaster.TransactionMasterID descending
                          select new PendingResponse
                          {
                              OperatorName = transactionMaster.OperatorMaster.OperatorName,
                              SenderNo = transactionMaster.DMT_SenderNumber,
                              Bank = transactionMaster.DMT_Bank,
                              ConsumerNumber = transactionMaster.ConsumerNumber,
                              Amount = transactionMaster.Amount,
                              Status = transactionMaster.Status,
                              ProviderName = provider.ProviderName,
                              FirmName = transactionMaster.UserMaster.FirmName,
                              TransactionMasterID = transactionMaster.TransactionMasterID,
                              DOC = transactionMaster.DOC.LongFormatDate(),
                              ProviderRequestID = transactionMaster.ProviderRequestID,
                              Type = transactionMaster.DMT_PaymentType
                          }).ToList();
            }
            return Record;
        }

        internal HeaderViewModel SelectUPIPendingHeaderSummary()
        {
            List<TransactionMaster> list = new List<TransactionMaster>();

            using var context = new DatabaseContext();

            list = (from transactionMaster in context.TransactionMaster
                    where transactionMaster.ServiceID == (int)ServiceCode.UPITransfer
                     && transactionMaster.Status == StatusType.Process

                    join provider in context.ProviderMaster on
                     transactionMaster.ProviderID equals provider.ProviderID
                    select transactionMaster).ToList();

            var Pending = (from item in list
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal decimal PayoutTotalAmountByAccountAndMonth(string AccountNumber)
        {
            DateTime first = DateTime.Now.StartOfMonth();
            DateTime last = DateTime.Now.LastOfMonth();

            int MonthStart = Convert.ToInt32(first.ToString(Messages.DOCIntegerFomrat));
            int MonthEnd = Convert.ToInt32(last.ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                list = (from txn in context.TransactionMaster
                        where txn.DOCInt >= MonthStart
                        && txn.DOCInt <= MonthEnd
                        && txn.Status == StatusType.Success
                        && txn.ConsumerNumber == AccountNumber
                        && txn.ServiceID == (int)ServiceCode.Payout
                        select txn).ToList();
            }
            if (list.Count <= 0 || list == null)
                return 0;

            var Trans = (from item in list
                         group item by new { item.ConsumerNumber } into grp
                         select new
                         {
                             SuccessAmount = grp.Sum(X => X.Amount)
                         }).FirstOrDefault();

            return Trans.SuccessAmount;
        }

        internal bool IsSuccessAllSelectByBillNumber(string billNumber)
        {
            List<TransactionMaster> list = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from item in context.TransactionMaster
                        where item.DMT_BillNumber == billNumber
                        select item).ToList();
            }
            foreach (TransactionMaster item in list)
            {
                if (!item.Status.EqualsIgnoreCase(StatusType.Success))
                    return false;
            }

            return true;
        }

        internal bool IsSuccessSelectByTransactionMasterID(int TransactionMasterID)
        {
            TransactionMaster resp = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                resp = (from item in context.TransactionMaster
                        where item.TransactionMasterID == TransactionMasterID
                        select item).FirstOrDefault();
            }
            if (resp == null)
                return false;
            if (!resp.Status.EqualsIgnoreCase(StatusType.Success))
                return false;

            return true;
        }

        internal bool IsProcessSelectByTransactionMasterID(int TransactionMasterID)
        {
            TransactionMaster resp = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                resp = (from item in context.TransactionMaster
                        where item.TransactionMasterID == TransactionMasterID
                        select item).FirstOrDefault();
            }
            if (resp == null)
                return false;
            if (!resp.Status.EqualsIgnoreCase(StatusType.Process))
                return false;

            return true;
        }

        internal TransactionMaster SelectTransactionByBillNumber(string billNumber)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from item in context.TransactionMaster
                        where item.DMT_BillNumber == billNumber
                        select item).FirstOrDefault();
            }
        }

        internal TransactionMaster SelectTransactionByTransactionMasterID(int TransactionMasterID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from item in context.TransactionMaster
                        where item.TransactionMasterID == TransactionMasterID
                        select item).FirstOrDefault();
            }
        }

        internal void Delete(int DeleteDOCInt, int fromDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.TransactionMaster.Where(U => U.DOCInt >= fromDOCInt && U.DOCInt <= DeleteDOCInt);
                context.TransactionMaster.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
       
    }
}