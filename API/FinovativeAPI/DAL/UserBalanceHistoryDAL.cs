using Project.Model;
using Project.Processing;
using Project.Response.Wallet;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class UserBalanceHistoryDAL
    {
        public static readonly string key = "_UserBalanceHistory";

        internal List<UserBalanceHistory> SelectWithoutCacheForToday()
        {
            List<UserBalanceHistory> list;
            int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                list = (from history in context.UserBalanceHistory
                        where history.DOCInt == DOCInt
                        orderby history.UserID, history.WalletID
                        select history).ToList();
            }
            return list;
        }

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<UserBalanceHistory>>();
            cache.Remove(key);
        }

        internal void InsertBulk()
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                var list = context.UserWalletMaster;

                foreach (var item in list)
                {
                    var obj = new UserBalanceHistory()
                    {
                        UserID = item.UserID,
                        WalletID = item.WalletID,
                        Balance = item.Balance,
                    };
                    context.UserBalanceHistory.Add(obj);
                }
                context.SaveChanges();
            }
        }

        internal List<UserBalanceHistory> SelectForToday()
        {
            int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            return SelectAll().Where(s => s.DOCInt == DOCInt).ToList();
        }

        public List<UserBalanceHistory> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<UserBalanceHistory>>();

            List<UserBalanceHistory> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        internal List<UserBalanceHistory> SelectWithoutCache()
        {
          
            int From = Convert.ToInt32(DateTime.Now.AddDays(-90).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
      
            List<UserBalanceHistory> list;

            using (var context = new DatabaseContext())
            {
                list = (from history in context.UserBalanceHistory
                        where history.DOCInt >= From
                                 && history.DOCInt <= To
                        orderby history.UserID, history.WalletID
                        select history).ToList();
            }
            return list;
        }

        private List<UserBalanceHistory> SelectByDate(string Date)
        {
            int DOCInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));

            return SelectAll().Where(s => s.DOCInt == DOCInt).ToList();
        }

        internal List<UserBalanceHistory> SelectByDOCINT(int DOCInt)
        {
            return SelectAll().Where(s => s.DOCInt == DOCInt).ToList();
        }

        internal async Task<decimal> SelectOpeningBalance(string Date)
        {
            List<UserMaster> userList = new UserDAL().Select()
                .Where(user => user.UserTypeID != (int)UserType.Admin &&
                       user.UserTypeID != (int)UserType.Support).ToList();

            List<UserBalanceHistory> historyList = SelectByDate(Date);

            var Mainlist = (from User in userList
                            join userWalletMaster in historyList on User.UserID equals userWalletMaster.UserID

                            select new
                            {
                                Balance = userWalletMaster.Balance,
                                WalletID = userWalletMaster.WalletID
                            }).ToList();

            var response = (from User in Mainlist
                            select new DashboardViewModel
                            {
                                Balance = User.Balance
                            }).ToList().Sum(o => o.Balance);

            await Task.Delay(1);

            return response;
        }

        internal decimal SelectOpeningBalance(int UserID, int WalletID, int FromDate)
        {
            List<UserBalanceHistory> historyList = SelectAll();

            var OpeningMainBal = (from history in historyList
                                  where history.DOCInt == FromDate
                                  && history.UserID == UserID
                                  && (WalletID != 0 ? history.WalletID == WalletID : 2 > 1)
                                  select new
                                  {
                                      OpeningBalance = history.Balance,
                                  }).FirstOrDefault();

            decimal opBal = OpeningMainBal?.OpeningBalance ?? 0;
            return opBal;
        }

        internal decimal SelectOpeningBalanceByWalletID(int UserID, int FromDate, int WalletID)
        {
            List<UserBalanceHistory> historyList = SelectAll();

            var OpeningAllMainBal = (from history in historyList
                                     where history.DOCInt == FromDate
                                     && history.UserID == UserID
                                      && (WalletID != 0 ? history.WalletID == WalletID : 2 > 1)
                                     group history by 1 into grp
                                     select new
                                     {
                                         OpeningBalance = grp.Sum(x => x.Balance)
                                     }).FirstOrDefault();

            decimal opBal = OpeningAllMainBal?.OpeningBalance ?? 0;

            return opBal;
        }

        internal decimal SelectAllUsersOpeningBalance(int FromDate)
        {
            List<UserBalanceHistory> historyList = SelectAll();

            var OpeningAllMainBal = (from history in historyList
                                     where history.DOCInt == FromDate
                                     group history by 1 into grp
                                     select new
                                     {
                                         OpeningBalance = grp.Sum(x => x.Balance)
                                     }).FirstOrDefault();

            decimal opBal = OpeningAllMainBal?.OpeningBalance ?? 0;

            return opBal;
        }

        internal decimal SelectClosingBalance(int UserID, int WalletID, int Date)
        {
            List<UserBalanceHistory> historyList = SelectAll();

            int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            return (from history in historyList
                    where history.DOCInt == Date
                    && history.UserID == UserID
                     && history.WalletID == WalletID
                    select history.Balance).FirstOrDefault();
        }

        internal decimal SelectAllClosingBalance(int UserID, int Date)
        {
            List<UserBalanceHistory> historyList = SelectAll();

            int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            var MainBal = (from history in historyList
                           where history.DOCInt == Date
                           && history.UserID == UserID
                           group history by 1 into grp
                           select new
                           {
                               OpeningBalance = grp.Sum(x => x.Balance)
                           }).FirstOrDefault();

            decimal Bal = MainBal?.OpeningBalance ?? 0;
            return Bal;
        }

        internal decimal SelectClosingBalanceByWalletID(int UserID, int Date, int WalletID)
        {
            List<UserBalanceHistory> historyList = SelectAll();

            var MainBal = (from history in historyList
                           where history.DOCInt == Date
                           && history.UserID == UserID
                           && history.WalletID == WalletID
                           group history by 1 into grp
                           select new
                           {
                               OpeningBalance = grp.Sum(x => x.Balance)
                           }).FirstOrDefault();

            decimal Bal = MainBal?.OpeningBalance ?? 0;
            return Bal;
        }

        internal async Task<decimal> SelectOpeningBalanceByUserID(int UserID, string Date)
        {
            List<UserBalanceHistory> historyList = SelectByDate(Date);

            var response = (from User in historyList
                            where User.UserID == UserID
                            select new DashboardViewModel
                            {
                                Balance = User.Balance
                            }).ToList().Sum(o => o.Balance);

            await Task.Delay(1);

            return response;
        }

        internal decimal SelectOpeningBalanceAllByWallet(int WalletID, string Date)
        {
            List<UserMaster> userList = new UserDAL().Select();
            List<UserBalanceHistory> historyList = SelectByDate(Date);

            var Mainlist = (from User in userList
                            where User.UserTypeID != (int)UserType.Admin && User.UserTypeID != (int)UserType.Support
                            join userWalletMaster in historyList on User.UserID equals userWalletMaster.UserID

                            select new
                            {
                                Balance = userWalletMaster.Balance,
                                WalletID = userWalletMaster.WalletID
                            }).ToList();
            if (WalletID != 0)
            {
                Mainlist = (from item in Mainlist
                            where item.WalletID == WalletID
                            select item).ToList();
            }

            var response = (from User in Mainlist
                            group User by 1 into grp
                            select new
                            {
                                OpeningBalance = grp.Sum(x => x.Balance)
                            }).FirstOrDefault();

            return response.OpeningBalance;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.UserBalanceHistory.Where(U => U.DOCInt <= DeleteDOCInt);
                context.UserBalanceHistory.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}