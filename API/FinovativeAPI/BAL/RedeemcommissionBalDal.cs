using Project.Model;
using Project.Processing;
using Project.Requests.CommissionHold;
using Project.Requests.Redeemcommission;
using Project.Response.Redeemcommission;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    internal class RedeemcommissionBalDal
    {
        public static string Key
        { get { return "_Redeemcommission"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<Redeemcommission>>();
            cache.Remove(Key);
        }

        public int Insert(Redeemcommission ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.Redeemcommission.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.RedeemcommissionID;
        }

        public int Update(Redeemcommission ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.Redeemcommission.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.RedeemcommissionID;
        }

        internal Redeemcommission SelectByRedeemcommissionID(int RedeemcommissionID)
        {
            return SelectAll().Where(C => C.RedeemcommissionID == RedeemcommissionID).FirstOrDefault();
        }

        internal List<Redeemcommission> SelectAll()
        {
            var _redeemcommissionList = new MemoryCacheProcessing<List<Redeemcommission>>();
            List<Redeemcommission> RedeemcommissionList = _redeemcommissionList.GetOrCreate(Key, () => SelectWithoutCache());

            return RedeemcommissionList.OrderByDescending(c => c.DOC).ToList();
        }

        private List<Redeemcommission> SelectWithoutCache()
        {
            List<Redeemcommission> list = new List<Redeemcommission>();
            using (var context = new DatabaseContext())
            {
                list = context.Redeemcommission.ToList();
            }
            return list;
        }

        internal Redeemcommission CheckDuplicate(InsertRequest mainRequest)
        {
            List<Redeemcommission> list = SelectAll();

            return (from item in list
                    where item.Year == mainRequest.Year
                    && item.Month == mainRequest.Month
                    && item.UserID == mainRequest.UserID
                    && item.WalletID == mainRequest.WalletID
                     && item.Status == StatusType.Process
                    select item).FirstOrDefault();
        }

        internal List<ViewModel> Select(SelectRequest mainRequest)
        {
            List<ViewModel> response = new List<ViewModel>();
            List<Redeemcommission> list = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            List<WalletMaster> walletList = new WalletMasterDal().Select();
            response = (from item in list
                        join user in userList on item.UserID equals user.UserID
                        join wallet in walletList on item.WalletID equals wallet.WalletID
                        where item.Year == mainRequest.Year
                           && item.Month == mainRequest.Month

                        select new ViewModel
                        {
                            RedeemcommissionID = item.RedeemcommissionID,
                            WalletName = wallet.Name,
                            WalletID = wallet.WalletID,
                            UserID = item.UserID,
                            UserTypeID = user.UserTypeID,
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            Amount = item.Amount,
                            Month = item.Month,
                            Year = item.Year,
                            Status = item.Status,
                            AdminRemark = item.AdminRemark,
                            DOC = item.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            UpdatedDOC = item.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            LastYearMonth = item.Month + "-" + item.Year,
                        }).ToList();

            if (!mainRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.Status.ToUpper() == mainRequest.Status.ToUpper()
                            select item).ToList();
            }

            if (mainRequest.WalletID > 0)
            {
                response = (from item in response
                            where item.WalletID == mainRequest.WalletID
                            select item).ToList();
            }

            if (mainRequest.UserTypeID > 0)
            {
                response = (from item in response
                            where item.UserTypeID == mainRequest.UserTypeID
                            select item).ToList();
            }

            return response;
        }

        internal string SelectStatus(int UserID, int year, int month)
        {
            string Status = "";
            Redeemcommission response = null;
            List<Redeemcommission> list = SelectAll();
            response = (from item in list
                        where item.UserID == UserID
                        && item.Year == year
                        && item.Month == month
                        select item).FirstOrDefault();
            if (response != null)
                Status = response.Status;

            return Status;
        }

        internal ViewModel SelectByID(int RedeemcommissionID)
        {
            ViewModel response = new ViewModel();
            List<Redeemcommission> list = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            List<WalletMaster> walletList = new WalletMasterDal().Select();

            response = (from item in list
                        join user in userList on item.UserID equals user.UserID
                        join wallet in walletList on item.WalletID equals wallet.WalletID

                        where item.RedeemcommissionID == RedeemcommissionID
                        select new ViewModel
                        {
                            LastYearMonth = item.LastYearMonth,
                            RedeemcommissionID = item.RedeemcommissionID,
                            UserID = item.UserID,
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            Amount = item.Amount,
                            Status = item.Status,
                            WalletName = wallet.Name,
                            WalletID = wallet.WalletID,
                            DOC = item.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).FirstOrDefault();

            return response;
        }

        internal List<ViewModel> SelectForAllUser(SelectAllRequest mainRequest,int UserID)
        {
            List<ViewModel> response = new List<ViewModel>();
            List<Redeemcommission> list = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            List<WalletMaster> walletList = new WalletMasterDal().Select();       
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            response = (from item in list
                        join user in userList on item.UserID equals user.UserID
                        join wallet in walletList on item.WalletID equals wallet.WalletID
                        where item.Year == mainRequest.Year
                           && item.Month == mainRequest.Month

                        select new ViewModel
                        {
                            RedeemcommissionID = item.RedeemcommissionID,
                            WalletName = wallet.Name,
                            WalletID = wallet.WalletID,
                            UserID = item.UserID,
                            UserTypeID = user.UserTypeID,
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            Amount = item.Amount,
                            Month = item.Month,
                            Year = item.Year,
                            Status = item.Status,
                            AdminRemark = item.AdminRemark,
                            DOC = item.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            UpdatedDOC = item.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            LastYearMonth = item.Month + "-" + item.Year,
                        }).ToList();

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                response = (from item in response
                            where item.UserID==UserID
                            select item).ToList();
            }

            if (!mainRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.Status.ToUpper() == mainRequest.Status.ToUpper()
                            select item).ToList();
            }
            
            if (mainRequest.WalletID > 0)
            {
                response = (from item in response
                            where item.WalletID == mainRequest.WalletID
                            select item).ToList();
            }

            if (mainRequest.UserTypeID > 0)
            {
                response = (from item in response
                            where item.UserTypeID == mainRequest.UserTypeID
                            select item).ToList();
            }
       
            return response;
        }

        internal List<CommHoldViewModel> CommisssionHoldByUserID(int UserID)
        {
            DateTime FromDate = DateTime.Now;
            DateTime LastDate = FromDate.LastOfMonth();
            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<CommHoldViewModel> list = new List<CommHoldViewModel>();
            List<CommHoldViewModel> commlist = new List<CommHoldViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Client)
                {
                    commlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To && txn.Status == StatusType.Success
                                 && txn.UserID == UserID
                                 && txn.RetailerCommissionHold > 0M
                                select new CommHoldViewModel
                                {
                                    WalletID = txn.WalletID,
                                    Amount = txn.RetailerCommissionHold,
                                }).ToList();
                }
                if (UserTypeID == (int)UserType.Distributer)
                {
                    commlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To && txn.Status == StatusType.Success
                                 && txn.DistributerUserID == UserID
                                 && txn.DistributerCommissionHold > 0M
                                select new CommHoldViewModel
                                {
                                    WalletID = txn.WalletID,
                                    Amount = txn.DistributerCommissionHold,
                                }).ToList();
                }
             
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    commlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To && txn.Status == StatusType.Success
                                 && txn.SuperDistributerUserID == UserID
                                 && txn.SuperDistributerCommissionHold > 0M
                                select new CommHoldViewModel
                                {
                                    WalletID = txn.WalletID,
                                    Amount = txn.SuperDistributerCommissionHold
                                }).ToList();
                }
            }

            var MainList = (from item in commlist
                            group item by new { item.WalletID } into grp
                            select new
                            {
                                WalletID = grp.Key.WalletID,
                                TotalCommissionHoldAmount = grp.Sum(x => x.Amount),
                            }).ToList();

            List<WalletMaster> walletList = new WalletMasterDal().Select();

            list = (from main in MainList
                    join wallet in walletList on main.WalletID equals wallet.WalletID
                    select new CommHoldViewModel
                    {
                        WalletID = main.WalletID,
                        Amount = main.TotalCommissionHoldAmount,
                        WalletName = wallet.Name,
                    }).ToList();

            return list;
        }
    }
}