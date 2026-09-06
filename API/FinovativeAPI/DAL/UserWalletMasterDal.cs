using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using Project.BAL;
using Project.Model;
using Project.Response.Wallet;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserWalletMasterDal
    {
        internal int Insert(UserWalletMaster userWalletMaster)
        {
            using var context = new DatabaseContext();
            context.UserWalletMaster.Add(userWalletMaster);
            return context.SaveChanges();
        }

        internal List<BackupMailWalletViewModel> SelectAllUsersWallet()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from wallet in context.UserWalletMaster
                        where wallet.WalletMaster.IsActive == true
                        select new BackupMailWalletViewModel
                        {
                            UserID = wallet.UserID,
                            WalletID = wallet.WalletID,
                            Name = wallet.WalletMaster.Name,
                            ShortName = wallet.WalletMaster.ShortName,
                            Balance = (float)System.Math.Round(wallet.Balance, 5)
                        }).ToList();
            }
        }

        internal List<DetailViewModel> SelectByUserID(int UserID)
        {
            List<DetailViewModel> list = new List<DetailViewModel>();
            int CashFreeAiUserID = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.CashFreeAiUserID), 0);
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from wallet in context.UserWalletMaster
                        where wallet.UserID == UserID && wallet.WalletMaster.IsActive == true
                        select new DetailViewModel
                        {
                            WalletID = wallet.WalletID,
                            Name = wallet.WalletMaster.Name,
                            ShortName = wallet.WalletMaster.ShortName,
                            Balance = (float)System.Math.Round(wallet.Balance, 2),
                            BalanceHash = wallet.BalanceHash,
                        }).ToList();
                if (CashFreeAiUserID == UserID)
                {
                    list = (from wallet in list
                            where wallet.WalletID != (int)WalletCode.AEPS 
                              && wallet.WalletID != (int)WalletCode.DMT
                            select wallet).ToList();
                }
            }
            return list;
        }

        internal string SelectBalanceInStringByUserID(int UserID)
        {
            string Balance = "";
            List<DetailViewModel> list = SelectByUserID(UserID);

            float RechargeBalance = 0, DMTBalance = 0, AEPSBalance = 0;

            foreach (DetailViewModel item in list)
            {
                if (item.WalletID == (int)WalletCode.Recharge)
                    RechargeBalance = (float)System.Math.Round(item.Balance);

                if (item.WalletID == (int)WalletCode.DMT)
                    DMTBalance = (float)System.Math.Round(item.Balance);

                if (item.WalletID == (int)WalletCode.AEPS)
                    AEPSBalance = (float)System.Math.Round(item.Balance);

                Balance = $"RCH :{RechargeBalance}{Environment.NewLine}DMT :{DMTBalance}{Environment.NewLine}AEPS :{AEPSBalance}";
            }

            return Balance;
        }

        internal UserWalletMaster SelectByWalletIDAndUserID(UserWalletMaster userWalletMaster)
        {
            using var context = new DatabaseContext();
            return context.UserWalletMaster
                .FirstOrDefault(W => W.UserID == userWalletMaster.UserID &&
                                     W.WalletID == userWalletMaster.WalletID);
        }

        internal decimal SelectSumByUserID(int UserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var allBal = (from wallet in context.UserWalletMaster
                              where wallet.UserID == UserID
                              group wallet by 1 into grp
                              select new
                              {
                                  OpeningBalance = grp.Sum(x => x.Balance)
                              }).FirstOrDefault();

                decimal bal = allBal?.OpeningBalance ?? 0;
                return bal;
            }
        }

        internal decimal SelectSumByUserIDAndWalletID(int UserID, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                var allBal = (from wallet in context.UserWalletMaster
                              where wallet.UserID == UserID
                              && wallet.WalletID == WalletID
                              group wallet by 1 into grp
                              select new
                              {
                                  OpeningBalance = grp.Sum(x => x.Balance)
                              }).FirstOrDefault();

                decimal bal = allBal?.OpeningBalance ?? 0;
                return bal;
            }
        }

        internal List<UserBalanceViewModel> SelectUserWalletBalance()
        {
            List<UserBalanceViewModel> response = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                var main = (from wallet in context.UserWalletMaster
                            select new
                            {
                                UserID = wallet.UserID,
                                Balance = wallet.Balance,
                                WalletID = wallet.WalletID
                            }).ToList();
                response = (from item in main
                            group item by new { item.UserID } into grp
                            select new UserBalanceViewModel
                            {
                                UserID = grp.Key.UserID,
                                RechargeBalance = grp.Where(q => q.WalletID == (int)WalletCode.Recharge).Sum(x => x.Balance),
                                DMTBalance = grp.Where(q => q.WalletID == (int)WalletCode.DMT).Sum(X => X.Balance),
                            }).ToList();
                return response;
            }
        }

        private List<UserBalanceHistory> SelectByDate(string Date)
        {
            List<UserBalanceHistory> list;
            int DOCInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from history in context.UserBalanceHistory
                        where history.DOCInt == DOCInt
                        orderby history.UserID, history.WalletID
                        select history).ToList();
            }
            return list;
        }

        internal List<UserWalletBalanceViewModel> SelectForDashboardByUserID(int UserID, string Date)
        {
            List<UserWalletBalanceViewModel> list = new List<UserWalletBalanceViewModel>();

            List<UserBalanceHistory> historyList = SelectByDate(Date);

            int CashFreeAiUserID = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.CashFreeAiUserID), 0);
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var Closinglist = (from wallet in context.UserWalletMaster
                                   where wallet.UserID == UserID && wallet.WalletMaster.IsActive == true
                                   select new UserWalletBalanceViewModel
                                   {
                                       WalletID = wallet.WalletID,
                                       WalletName = wallet.WalletMaster.Name,
                                       OpeningBalance = 00,
                                       ClosingBalance = (float)System.Math.Round(wallet.Balance, 2),
                                   }).ToList();
                if (CashFreeAiUserID == UserID)
                {
                    Closinglist = (from Cllist in Closinglist
                                   where Cllist.WalletID != (int)WalletCode.AEPS
                                   && Cllist.WalletID != (int)WalletCode.DMT
                                   select Cllist).ToList();
                }
                var Openinglist = (from user in historyList
                                   join wallet in context.WalletMaster on user.WalletID equals wallet.WalletID
                                   where user.UserID == UserID && wallet.IsActive == true
                                   select new UserWalletBalanceViewModel
                                   {
                                       WalletID = user.WalletID,
                                       WalletName = wallet.Name,
                                       OpeningBalance = (float)System.Math.Round(user.Balance, 2),
                                       ClosingBalance = 00,
                                   }).ToList();
                if (CashFreeAiUserID == UserID)
                {
                    Openinglist = (from Oplist in Openinglist
                                   where Oplist.WalletID != (int)WalletCode.AEPS
                                     && Oplist.WalletID != (int)WalletCode.DMT
                                   select Oplist).ToList();
                }

                var listFinal = Closinglist.Union(Openinglist);
                list = (from item in listFinal
                        group item by new { item.WalletID, item.WalletName } into grp
                        select new UserWalletBalanceViewModel
                        {
                            WalletName = grp.Key.WalletName,
                            OpeningBalance = grp.Sum(X => X.OpeningBalance),
                            ClosingBalance = grp.Sum(X => X.ClosingBalance)
                        }).ToList();

                return list;
            }
        }

        internal decimal SelectClosingBalance(int UserID, int WalletID, int ToDate, int AddDayInt)
        {
            int CurrentdateInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            var ClosingBalance = 0m;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                if (CurrentdateInt == ToDate)
                {
                    ClosingBalance = (from bal in context.UserWalletMaster
                                      where bal.UserID == UserID
                                      && bal.WalletID == WalletID
                                      select bal.Balance).FirstOrDefault();
                }
                else
                {
                    ClosingBalance = new UserBalanceHistoryDAL().SelectClosingBalanceByWalletID(UserID, AddDayInt, WalletID);
                }
                return ClosingBalance;
            }
        }

        public int UserWalletUpdate(UserWalletMaster userWalletMaster)
        {
            using (var context = new DatabaseContext())
            {
                context.UserWalletMaster.Update(userWalletMaster);
                return context.SaveChanges();
            }
        }
    }
}