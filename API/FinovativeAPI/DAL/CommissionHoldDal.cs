using Project.Model;
using Project.Processing;
using Project.Requests.CommissionHold;
using Project.Response.CommissionHold;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CommissionHoldDal
    {
        internal List<ViewModel> SelectCommissionHold(SearchRequest selectRequest, int UserID)
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

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<ViewModel> list = new List<ViewModel>();
            List<CommissionViewModel> commlist = new List<CommissionViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Client)
                {
                    commlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To && txn.Status == StatusType.Success
                                 && txn.UserID == UserID
                                 && txn.RetailerCommissionHold > 0M
                                select new CommissionViewModel
                                {
                                    UserID = txn.UserID,
                                    WalletID = txn.WalletID,
                                    Commission = txn.RetailerCommissionHold,
                                    Amount = txn.Amount,
                                }).ToList();
                }
                if (UserTypeID == (int)UserType.Distributer)
                {
                    commlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To && txn.Status == StatusType.Success
                                 && txn.DistributerUserID == UserID
                                 && txn.DistributerCommissionHold > 0M
                                select new CommissionViewModel
                                {
                                    UserID = txn.DistributerUserID,
                                    WalletID = txn.WalletID,
                                    Commission = txn.DistributerCommissionHold,
                                    Amount = txn.Amount,
                                }).ToList();
                }

                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    commlist = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                 && txn.Status == StatusType.Success
                                 && txn.SuperDistributerUserID == UserID
                                 && txn.SuperDistributerCommissionHold > 0M
                                select new CommissionViewModel
                                {
                                    UserID = txn.SuperDistributerUserID,
                                    WalletID = txn.WalletID,
                                    Commission = txn.SuperDistributerCommissionHold,
                                    Amount = txn.Amount,
                                }).ToList();
                }
            }

            var MainList = (from item in commlist
                            group item by new { item.UserID, item.WalletID } into grp
                            select new
                            {
                                UserID = grp.Key.UserID,
                                WalletID = grp.Key.WalletID,
                                TotalCommissionHoldAmount = grp.Sum(x => x.Commission),
                                TotalAmount = grp.Sum(x => x.Amount),
                            }).ToList();

            List<UserMaster> usersList = new UserDAL().Select().Where(u => u.UserID == UserID).ToList();
            List<WalletMaster> walletList = new WalletMasterDal().Select();

            list = (from main in MainList
                    join user in usersList on main.UserID equals user.UserID
                    join wallet in walletList on main.WalletID equals wallet.WalletID
                    where main.UserID == UserID
                    select new ViewModel
                    {
                        Year = selectRequest.Year,
                        Month = selectRequest.Month,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        UserID = user.UserID,
                        WalletID = main.WalletID,
                        LastYearMonth = month + "-" + lastyearmonth,
                        CommissionHoldAmount = main.TotalCommissionHoldAmount,
                        TotalSuccessAmount = main.TotalAmount,
                        WalletName = wallet.Name,
                        Status = new RedeemcommissionBalDal().SelectStatus(UserID, selectRequest.Year, selectRequest.Month)
                    }).ToList();

            return list;
        }

        internal List<Response.Redeemcommission.ViewModel> SelectCommissionHoldAllRole(SelectAllRequest selectRequest,int UserID)
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

            List<Response.Redeemcommission.ViewModel> list = new List<Response.Redeemcommission.ViewModel>();
            List<CommissionViewModel> commlist = new List<CommissionViewModel>();

            int LoginUserTypeID = new UserDAL().SelectUserTypeID(UserID);

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                List<CommissionViewModel> commlistds = (from txn in context.TransactionMaster
                                                        where txn.DOCInt >= From && txn.DOCInt <= To && txn.Status == StatusType.Success

                                                         && txn.DistributerCommissionHold > 0M
                                                        select new CommissionViewModel
                                                        {
                                                            UserID = txn.DistributerUserID,
                                                            UserTypeID = (int)UserType.Distributer,
                                                            WalletID = txn.WalletID,
                                                            Commission = txn.DistributerCommissionHold,
                                                            Amount = txn.Amount,
                                                        }).ToList();

                List<CommissionViewModel> commlistsd = (from txn in context.TransactionMaster
                                                        where txn.DOCInt >= From && txn.DOCInt <= To && txn.Status == StatusType.Success

                                                         && txn.SuperDistributerCommissionHold > 0M
                                                        select new CommissionViewModel
                                                        {
                                                            UserID = txn.SuperDistributerUserID,
                                                            UserTypeID = (int)UserType.SuperDistributer,
                                                            WalletID = txn.WalletID,
                                                            Commission = txn.SuperDistributerCommissionHold,
                                                            Amount = txn.Amount,
                                                        }).ToList();

                commlist = commlist.Union(commlistds).ToList();
                commlist = commlist.Union(commlistsd).ToList();
            }
            if (selectRequest.UserTypeID > 0)
            {
                commlist = commlist.Where(W => W.UserTypeID == selectRequest.UserTypeID).ToList();

            }          

            if (Util.isRoleSuperDistOrDist(LoginUserTypeID))
            {
                commlist = commlist.Where(U => U.UserID == UserID).ToList();
            }

            var MainList = (from item in commlist
                            group item by new { item.UserID, item.WalletID } into grp
                            select new
                            {
                                UserID = grp.Key.UserID,
                                WalletID = grp.Key.WalletID,
                                TotalCommissionHoldAmount = grp.Sum(x => x.Commission),
                                TotalAmount = grp.Sum(x => x.Amount),
                            }).ToList();

            List<UserMaster> usersList = new UserDAL().Select().ToList();

            List<WalletMaster> walletList = new WalletMasterDal().Select();
            if (selectRequest.WalletID > 0)
            {
                walletList = walletList.Where(W => W.WalletID == selectRequest.WalletID).ToList();
            }

            list = (from main in MainList
                    join user in usersList on main.UserID equals user.UserID
                    join wallet in walletList on main.WalletID equals wallet.WalletID

                    select new Response.Redeemcommission.ViewModel
                    {
                        Year = selectRequest.Year,
                        Month = selectRequest.Month,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        UserID = user.UserID,
                        WalletID = main.WalletID,
                        LastYearMonth = month + "-" + lastyearmonth,
                        Amount = main.TotalCommissionHoldAmount,
                        //  TotalSuccessAmount = main.TotalAmount,
                        WalletName = wallet.Name,
                        Status = new RedeemcommissionBalDal().SelectStatus(main.UserID, selectRequest.Year, selectRequest.Month)
                    }).ToList();

            if (selectRequest.Status.EqualsIgnoreCase(StatusType.ALL))
            {
                list = list.Where(W => W.Status == selectRequest.Status).ToList();
            }

            return list;
        }
    }
}