using Microsoft.EntityFrameworkCore;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderPurchase;
using Project.Response.ProviderPurchase;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class ProviderPurchaseDal
    {
        public static readonly string Key = "_ProviderPurchase";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(ProviderPurchase model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.ProviderPurchase.Add(model);
                return context.SaveChanges();
            }
        }

        internal int Update(ProviderPurchase model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.ProviderPurchase.Update(model);
                return context.SaveChanges();
            }
        }

        internal ProviderPurchase SelectByID(int ProviderPurchaseID)
        {
            using var context = new DatabaseContext();
            return context.ProviderPurchase.Find(ProviderPurchaseID);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from purchase in context.ProviderPurchase
                        where purchase.IsDelete == false
                        orderby purchase.ProviderPurchaseID descending
                        select new ViewModel
                        {
                            ProviderPurchaseID = purchase.ProviderPurchaseID,
                            ProviderID = purchase.ProviderID,
                            ProviderName = purchase.ProviderMaster.ProviderName,
                            WalletID = purchase.WalletID,
                            WalletName = purchase.WalletMaster.Name,
                            Amount = purchase.Amount,
                            DOC = purchase.PurchaseDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                            PurchaseDOCInt = purchase.PurchaseDOCInt,
                            Remark = purchase.Remark
                        }).ToList();
            }
        }

        internal List<ViewModel> SelectFromDateToDate(SelectRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<ViewModel> response = null;
            List<ViewModel> list = Select();
            response = (from item in list
                        where item.PurchaseDOCInt >= From && item.PurchaseDOCInt <= To
                        select item).ToList();
            if (selectRequest.ProviderID != 0)
            {
                response = (from item in response
                            where item.ProviderID == selectRequest.ProviderID
                            select item).ToList();
            }
            if (selectRequest.WalletID != 0)
            {
                response = (from item in response
                            where item.WalletID == selectRequest.WalletID
                            select item).ToList();
            }
            return response;
        }

        internal List<PurchaseViewModel> SelectPurchase(Requests.TransactionReport.ProviderSaleRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = Select();
            var Mainlist = (from item in list
                            where item.PurchaseDOCInt >= From && item.PurchaseDOCInt <= To
                            select new
                            {
                                ProviderID = item.ProviderID,
                                Amount = item.Amount,
                            }).ToList();
            if (selectRequest.ProviderID != 0)
            {
                Mainlist = (from item in Mainlist
                            where item.ProviderID == selectRequest.ProviderID
                            select item).ToList();
            }
            return (from txn in Mainlist
                    group txn by new { txn.ProviderID } into grp
                    select new PurchaseViewModel
                    {
                        ProviderID = grp.Key.ProviderID,
                        Purchase = grp.Sum(X => X.Amount),
                    }).ToList();
        }

        internal List<DashboardViewModel> SelectForDashboard(string Date)
        {
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            List<DashboardViewModel> response = new List<DashboardViewModel>();
            List<ViewModel> list = Select();
            var Mainlist = (from item in list
                            where item.PurchaseDOCInt == dateInt
                            select new
                            {
                                ProviderID = item.ProviderID,
                                ProviderName = item.ProviderName,
                                Amount = item.Amount,
                            }).ToList();

            var Grouplist = (from txn in Mainlist
                             group txn by new { txn.ProviderID } into grp
                             select new
                             {
                                 ProviderID = grp.Key.ProviderID,
                                 Purchase = grp.Sum(X => X.Amount),
                             }).ToList();

            using var context = new DatabaseContext();

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var GroupTransactionlist = (from txn in context.TransactionMaster
                                        where txn.DOCInt == dateInt
                                        group txn by new { txn.ProviderID } into grp
                                        select new
                                        {
                                            ProviderID = grp.Key.ProviderID,
                                            ProviderCommission = grp.Sum(X => X.ProviderCommission),
                                        }).ToList();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            return (from Provider in providerList
                    join purchase in Grouplist on Provider.ProviderID equals purchase.ProviderID into purchasedetail
                    from purchasevalue in purchasedetail.DefaultIfEmpty()
                    join trans in GroupTransactionlist on Provider.ProviderID equals trans.ProviderID into transdetail
                    from transValue in transdetail.DefaultIfEmpty()
                    where Provider.IsActive == true
                    orderby Provider.RechargeBalance descending

                    select new DashboardViewModel
                    {
                        ProviderName = Provider.ProviderName,
                        Purchase = purchasevalue?.Purchase ?? 00,
                        RechargeBalance = Provider.RechargeBalance,
                        DMTBalance = Provider.DMTBalance,
                        Commission = transValue?.ProviderCommission ?? 00
                    }).ToList();
        }
    }
}