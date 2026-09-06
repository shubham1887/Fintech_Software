using Project.Model;
using Project.Processing;
using Project.Requests.PaymentGateway;
using Project.Response.PaymentGateway;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PaymentGatewayDAL
    {
        public static string Key = "_PaymentGateway";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<PaymentGatewayViewModel>>();
            cache.Remove(Key);
        }

        internal List<PaymentGatewayViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<PaymentGatewayViewModel>>();
            List<PaymentGatewayViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<PaymentGatewayViewModel> SelectWithoutCache()
        {
            List<PaymentGatewayViewModel> list = new List<PaymentGatewayViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from setting in context.PaymentGateway
                        join wallet in context.WalletMaster on setting.WalletID equals wallet.WalletID
                        join usermaster in context.UserMaster on setting.UserID equals usermaster.UserID
                        orderby setting.PaymentGatewayID descending
                        select new PaymentGatewayViewModel
                        {
                            PaymentGatewayID = setting.PaymentGatewayID,
                            UserName = usermaster.FirmName.ConcatNameMobileAliasName(usermaster.MobileNumber, usermaster.AliasName),
                            CompanyName = Company.ClientName,
                            OrderID = setting.OrderID,
                            Amount = setting.Amount,
                            Commission = setting.Commission,
                            GatewayCharge = setting.GatewayCharge,
                            PayMode = setting.PayMode,
                            TransactionID = setting.TransactionID,
                            WalletID = setting.WalletID,
                            Status = setting.Status,

                            WalletName = wallet.Name,
                            UserID = setting.UserID,
                            Response = setting.Response,
                            IsUpdate = setting.IsUpdate,
                            DOC = setting.DOC,
                            DisplayDOC = setting.DOC.LongFormatDate(),

                            OpeningBalance = setting.OpeningBalance,
                            ClosingBalance = setting.ClosingBalance,

                            DistributerCommission = setting.DistributerCommission,
                            SuperDistributerCommission = setting.SuperDistributerCommission,

                            DistributerUserID = setting.DistributerUserID,
                            SuperDistributerUserID = setting.SuperDistributerUserID,

                            Remark = setting.Remark,
                        }).ToList();
            }
        }

        internal PaymentGatewayViewModel SelectByUserID(int userID)
        {
            DateTime Date = DateTime.Now;
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.PaymentGateway
                                where txn.DOCInt == dateInt && txn.UserID == userID
                                select new
                                {
                                    PaymentGatewayID = txn.PaymentGatewayID,
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    DOCInt = txn.DOCInt
                                }).ToList();

                var totalList = (from txn in mainList
                                 group txn by new { txn.DOCInt } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

                PaymentGatewayViewModel report = new PaymentGatewayViewModel();

                report.TotalCount = totalList?.Count ?? 0;
                report.TotalAmount = totalList?.Amount ?? 0;

                return report;
            }
        }

        internal int Update(PaymentGateway objmodel)
        {
            if (objmodel.Status.TrimIfNotNull().Length <= 2) objmodel.Status = StatusType.Process;
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.PaymentGateway.Update(objmodel);
                context.SaveChanges();
                return objmodel.PaymentGatewayID;
            }
        }

        internal PaymentGatewayViewModel CheckDuplicate(string OrderID)
        {
            List<PaymentGatewayViewModel> list = Select();

            PaymentGatewayViewModel response = null;

            response = (from Detail in list
                        where Detail.OrderID == OrderID                
                       select Detail).FirstOrDefault();

            return response;
        }


        internal int Insert(PaymentGateway paymentGateway)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.PaymentGateway.Add(paymentGateway);
                context.SaveChanges();
                return paymentGateway.PaymentGatewayID;
            }
        }

        internal PaymentGateway SelectTransactionByOrderID(string orderID)
        {
            using (var context = new DatabaseContext())
            {
                PaymentGateway pg = context.PaymentGateway.FirstOrDefault(obj => obj.OrderID == orderID);
                return pg;
            }
        }

        internal PaymentGateway SelectTransactionByOrderIDWithProcessOnly(string orderID)
        {
            using (var context = new DatabaseContext())
            {
                PaymentGateway pg = context.PaymentGateway.FirstOrDefault(obj => obj.OrderID == orderID &&
                obj.Status == StatusType.Process && obj.IsUpdate == 0);
                return pg;
            }
        }
        internal PaymentGateway SelectTransactionByOrderIDWithProcessAndFromDate(string orderID, int fromDateInt)
        {
            using (var context = new DatabaseContext())
            {
                PaymentGateway pg = context.PaymentGateway.FirstOrDefault(obj => obj.DOCInt >= fromDateInt &&
                obj.OrderID == orderID &&
                obj.Status == StatusType.Process &&
                obj.IsUpdate == 0);
                return pg;
            }
        }

        internal PaymentGatewayViewModel SelectByOrderID(string OrderID)
        {
            List<PaymentGatewayViewModel> list = Select();

            PaymentGatewayViewModel view = list.FirstOrDefault(obj => obj.OrderID == OrderID);

            return view;
        }

        internal PaymentGateway SelectByID(int iD)
        {
            using (var context = new DatabaseContext())
            {
                PaymentGateway pg = context.PaymentGateway.FirstOrDefault(obj => obj.PaymentGatewayID == iD);
                return pg;
            }
        }

        internal PaymentGatewayViewModel SelectInfoByUserID(int UserID)
        {
            List<PaymentGatewayViewModel> list = Select();

            return (from item in list
                    where item.UserID == UserID
                    orderby item.DOC descending
                    select item).FirstOrDefault();
        }

        internal List<PaymentGatewayViewModel> SelectReport(SelectRequest selectRequest, int CurrentUserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<PaymentGatewayViewModel> list = new List<PaymentGatewayViewModel>();
            List<ViewModel> pgSettingList = new PaymentGatewaySettingDal().Select();
            List<StateMaster> StateList = new StateDAL().Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<WalletMaster> walletList = new WalletMasterDal().Select();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var PGlist = (from paymentGateway in context.PaymentGateway
                              where paymentGateway.DOCInt >= From
                              && paymentGateway.DOCInt <= To
                              && (selectRequest.ProviderID == 0 ? 2 > 1 : paymentGateway.PaymentProviderID == selectRequest.ProviderID)
                              && (selectRequest.WalletID == 0 ? 2 > 1 : paymentGateway.WalletID == selectRequest.WalletID)
                              //&& (selectRequest.ServiceID == 0 ? 2 > 1 : paymentGateway.ServiceID == selectRequest.ServiceID)
                              && (!selectRequest.Status.EqualsIgnoreCase("All") ? paymentGateway.Status == selectRequest.Status : 2 > 1)
                              && (!selectRequest.PayMode.EqualsIgnoreCase("All") ? paymentGateway.PayMode.ToLower() == selectRequest.PayMode.ToLower() : 2 > 1)
                              select paymentGateway).ToList();

                if (PGlist == null)
                    return list;

                list = (from paymentGateway in PGlist
                        join user in usersList on
                        paymentGateway.UserID equals user.UserID
                        join parent in usersList on user.ParentID equals parent.UserID

                        join state in context.StateMaster on user.StateID equals state.ID
                        into stateGroup
                        from LFstate in stateGroup.DefaultIfEmpty()

                        join wallet in walletList on paymentGateway.WalletID equals wallet.WalletID
                        join pro in pgSettingList on paymentGateway.PaymentProviderID equals pro.PaymentGatewaySettingID
                        into providerdetail
                        from providerleft in providerdetail.DefaultIfEmpty()
                        orderby paymentGateway.DOC descending

                        select new PaymentGatewayViewModel
                        {
                            PaymentGatewayID = paymentGateway.PaymentGatewayID,
                            UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            ParentName = parent.FirmName.ConcatNameMobileAliasName(parent.MobileNumber, user.AliasName),
                            ParentID = user.ParentID,
                            State = LFstate?.Name?? "",
                            Amount = paymentGateway.Amount,
                            CompanyName = Company.ClientName,
                            WalletName = wallet.Name,
                            Status = paymentGateway.Status,
                            PayMode = paymentGateway.PayMode,
                            OrderID = paymentGateway.OrderID,
                            TransactionID = paymentGateway.TransactionID,
                            DisplayDOC = paymentGateway.DOC.LongFormatDate(),
                            DOC = paymentGateway.DOC,
                            UserID = paymentGateway.UserID,
                            ProviderName = providerleft?.Name?? "",
                            OpeningBalance = paymentGateway.OpeningBalance,
                            ClosingBalance = paymentGateway.ClosingBalance,
                            Commission = paymentGateway.Commission,
                            Remark = paymentGateway.Remark,
                            DistributerUserID = paymentGateway.DistributerUserID,
                            SuperDistributerUserID = paymentGateway.SuperDistributerUserID,
                            Sender = paymentGateway.Sender,
                            ServiceID = paymentGateway.ServiceID,
                            PaymentStatus = paymentGateway.PaymentStatus?? ""
                        }).ToList();
            }
            int UserTypeID = new UserDAL().SelectUserTypeID(CurrentUserID);
            if (selectRequest.ServiceID == (int)ServiceCode.CreditCardToBank)
            {
                list = (from item in list
                        where item.ServiceID == selectRequest.ServiceID
                        select item).ToList();
            }
            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (selectRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
                }
            }
            if (UserTypeID == (int)UserType.SuperDistributer || UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SDFOS)
            {
                if (selectRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.UserID == CurrentUserID
                             || item.ParentID == CurrentUserID
                            select item).ToList();
                }
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Client)
            {
                list = (from item in list
                        where item.UserID == CurrentUserID
                        select item).ToList();
            }

            return list;
        }

        internal Response.TransactionMaster.HeaderViewModel GetHeaderSummaryData(List<PaymentGatewayViewModel> list)
        {
            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               Commission = grp.Sum(X => X.Commission),
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
            headerlist.Commission = Success?.Commission ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }

        internal decimal SelectPGCommission(int dateInt)
        {
            decimal comm = 0m;

            using var context = new DatabaseContext();
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainPaymentGateway = (from txn in context.PaymentGateway
                                          where txn.DOCInt == dateInt
                                          && txn.Status == StatusType.Success
                                          select new
                                          {
                                              DOCInt = txn.DOCInt,
                                              DistributerCommission = txn.DistributerCommission,
                                              SuperDistributerCommission = txn.SuperDistributerCommission,
                                          }).ToList();
                if (mainPaymentGateway.Count <= 0 || mainPaymentGateway == null)
                    return comm;
                var PaymentGatewaycommi = (from txn in mainPaymentGateway
                                           group txn by new { txn.DOCInt } into grp
                                           select new
                                           {
                                               DistributerCommission = grp.Sum(x => x.DistributerCommission),
                                               SuperDistributerCommission = grp.Sum(x => x.SuperDistributerCommission),
                                           }).FirstOrDefault();

                return comm = (PaymentGatewaycommi?.DistributerCommission ?? 0) + (PaymentGatewaycommi?.SuperDistributerCommission ?? 0);
            }
        }

        internal decimal SelectPGCommissionforUpline(int userTypeID, int dateInt)
        {
            decimal comm = 0m;
            using var context = new DatabaseContext();
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainPaymentGateway = (from txn in context.PaymentGateway
                                          where txn.DOCInt == dateInt
                                          && txn.Status == StatusType.Success
                                          select new
                                          {
                                              DistributerCommission = txn.DistributerCommission,
                                              SuperDistributerCommission = txn.SuperDistributerCommission,
                                          }).ToList();
                if (mainPaymentGateway.Count <= 0 || mainPaymentGateway == null)
                    return comm;

                if (userTypeID == (int)UserType.Distributer)
                {
                    comm = mainPaymentGateway.Sum(i => i.DistributerCommission);
                }
                if (userTypeID == (int)UserType.SuperDistributer)
                {
                    comm = mainPaymentGateway.Sum(i => i.SuperDistributerCommission);
                }
                return comm;
            }
        }

        internal PaymentGatewayViewModel SelectByTransactionID(string bankRrn)
        {
            List<PaymentGatewayViewModel> list = Select();

            PaymentGatewayViewModel view = list.FirstOrDefault(obj => obj.TransactionID == bankRrn);

            return view;
        }

        internal List<Response.TransactionMaster.CommissionViewModel> SelectRoleWithCommission(CommissionRequest selectRequest)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from tran in context.PaymentGateway
                        join dist in context.UserMaster on tran.DistributerUserID equals dist.UserID into DistDetails
                        from DL in DistDetails.DefaultIfEmpty()
                        join super in context.UserMaster on tran.SuperDistributerUserID equals super.UserID into superDetails
                        from SL in superDetails.DefaultIfEmpty()
                        where tran.PaymentGatewayID == selectRequest.PaymentGatewayID
                        && tran.UserID == selectRequest.UserID
                        select new Response.TransactionMaster.CommissionViewModel
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

        internal List<PaymentGatewayPendingViewModel> SelectPendingPGTransactionForStatusCheck()
        {
            List<PaymentGatewayPendingViewModel> list = new List<PaymentGatewayPendingViewModel>();

            int yesterday = Convert.ToInt32(DateTime.Now.AddDays(-2).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from paymentGateway in context.PaymentGateway
                        join pro in context.PaymentGatewaySetting on paymentGateway.PaymentProviderID equals pro.PaymentGatewaySettingID
                        where paymentGateway.Status == StatusType.Process && paymentGateway.IsUpdate == 0 && paymentGateway.DOCInt >= yesterday
                        orderby paymentGateway.DOC descending

                        select new PaymentGatewayPendingViewModel
                        {
                            PaymentGatewaySettingID = pro.PaymentGatewaySettingID,
                            Name = pro.Name,
                            PaymentGatewayID = paymentGateway.PaymentGatewayID,
                            Status = paymentGateway.Status,
                            OrderID = paymentGateway.OrderID,
                            UserID = paymentGateway.UserID,
                            DOC = paymentGateway.DOC,
                        }).ToList();
            }

            return list;
        }

        internal List<PaymentGatewayViewModel> SelectLast10PaymentGateway(int UserID, int ServiceID)
        {
            List<PaymentGatewayViewModel> list = new List<PaymentGatewayViewModel>();

            List<ViewModel> pgList = new PaymentGatewaySettingDal().Select();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from paymentGateway in context.PaymentGateway
                        join user in context.UserMaster on
                        paymentGateway.UserID equals user.UserID
                        join wallet in context.WalletMaster on paymentGateway.WalletID equals wallet.WalletID
                        where paymentGateway.UserID == UserID
                         && (ServiceID == 0 ? 2 > 1 : paymentGateway.ServiceID == ServiceID)
                        orderby paymentGateway.DOC descending

                        select new PaymentGatewayViewModel
                        {
                            PaymentGatewayID = paymentGateway.PaymentGatewayID,
                            UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            Amount = paymentGateway.Amount,
                            CompanyName = Company.ClientName,
                            WalletName = wallet.Name,
                            Status = paymentGateway.Status,
                            PayMode = paymentGateway.PayMode,
                            OrderID = paymentGateway.OrderID,
                            TransactionID = paymentGateway.TransactionID,
                            DisplayDOC = paymentGateway.DOC.LongFormatDate(),
                            UserID = paymentGateway.UserID,
                            OpeningBalance = paymentGateway.OpeningBalance,
                            ClosingBalance = paymentGateway.ClosingBalance,
                            Commission = paymentGateway.Commission,
                            Remark = paymentGateway.Remark,
                        }).Take(10).ToList();
            }
            return list;
        }

        internal PaymentGateway SelectTransactionByRefernceIDWithProcessOnly(string RefernceID)
        {
            using (var context = new DatabaseContext())
            {
                PaymentGateway pg = context.PaymentGateway.FirstOrDefault(obj => obj.ReferenceID == RefernceID &&
                obj.Status == StatusType.Process && obj.IsUpdate == 0);
                return pg;
            }
        }
    }
}