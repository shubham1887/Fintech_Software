using Project.Model;
using Project.Processing;
using Project.Requests.UserwiseSaleTarget;
using Project.Response.UserwiseSaleTarget;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class UserwiseSaleTargetDal
    {
        public static string Key
        { get { return "_UserwiseSaleTarget"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserwiseSaleTarget>>();
            cache.Remove(Key);
        }

        internal List<UserwiseSaleTarget> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserwiseSaleTarget>>();

            List<UserwiseSaleTarget> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        internal List<ViewModel> SelectAll(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
                return (from Objdb in context.UserwiseSaleTarget
                        where Objdb.IsDelete == false && Objdb.ParentID == UserID
                        select new ViewModel
                        {
                            UserID = Objdb.UserID,
                            UserwiseSaleTargetID = Objdb.UserwiseSaleTargetID,
                            ParnetID = Objdb.ParentID,
                            UserName = Objdb.UserMaster.FirmName.ConcatNameMobileAliasName(Objdb.UserMaster.MobileNumber, Objdb.UserMaster.AliasName),
                            WalletID = Objdb.WalletID,
                            Wallet = Objdb.WalletMaster.Name,
                            TargetAmount = Objdb.TargetAmount,
                            Commission = Objdb.Commission,
                            CommissionType = Objdb.CommissionType,
                        }).ToList();
        }

        private List<UserwiseSaleTarget> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.UserwiseSaleTarget.Where(U => U.IsDelete == false).ToList();
        }

        internal int Update(UserwiseSaleTarget ObjModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.UserwiseSaleTarget.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.UserID;
        }

        internal int Insert(UserwiseSaleTarget ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserwiseSaleTarget.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.UserID;
        }

        internal UserwiseSaleTarget SelectByID(int UserwiseSaleTargetID)
        {
            using var context = new DatabaseContext();
            return context.UserwiseSaleTarget.Find(UserwiseSaleTargetID);
        }

        internal UserwiseSaleTarget SelectByUserID(int UserID, int WalletID)
        {
            using var context = new DatabaseContext();
            return context.UserwiseSaleTarget.FirstOrDefault(u => u.UserID == UserID && u.WalletID == WalletID && u.IsDelete == false);
        }

        internal List<MonthlySuccessViewModel> MonthlySuccessData(SaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime FromDate = new DateTime(DateTime.Now.Year, selectRequest.Month, 1);
            if (selectRequest.Month == 12)
                FromDate = new DateTime(DateTime.Now.Year - 1, selectRequest.Month, 1);

            DateTime LastDate = FromDate.LastOfMonth();

            int MonthStart = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int MonthEnd = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                var mainList = (from txn in context.TransactionMaster
                                join user in context.UserMaster on
                                 txn.UserID equals user.UserID
                                where txn.Status == StatusType.Success
                                       && user.ParentID == UserID
                                       && txn.DOCInt >= MonthStart
                                       && txn.DOCInt <= MonthEnd

                                select new MonthlySuccessViewModel
                                {
                                    UserID = txn.UserID,
                                    WalletID = txn.WalletID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Amount = txn.Amount,
                                    DOC = Convert.ToInt32(txn.DOC.ToString(Messages.DOCDayFormat, CultureInfo.InvariantCulture)),
                                }).ToList();

                return mainList;
            }
        }

        internal List<SaleViewModel> SaleTargetSelect(SaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime date = new DateTime(DateTime.Now.Year, selectRequest.Month, DateTime.Now.Day);
            string year = DateTime.Now.Year.ToString();
            if (selectRequest.Month == 12)
            {
                date = new DateTime(DateTime.Now.Year - 1, selectRequest.Month, 1);

                int yearint = DateTime.Now.Year - 1;
                year = yearint.ToString();
            }

            string month = date.ToString("MMMM");

            List<SaleViewModel> response = new List<SaleViewModel>();

            List<MonthlySuccessViewModel> monthlysuccess = MonthlySuccessData(selectRequest, UserID);

            List<ViewModel> list = SelectAll(UserID);
            List<TargetSaleCommission> transferedlist = new TargetSaleCommissionDal().Select();

            using (var context = new DatabaseContext())
            {
                var groupList = (from txn in monthlysuccess
                                 group txn by new { txn.UserID, txn.WalletID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     WalletID = grp.Key.WalletID,
                                     SaleAmount = grp?.Sum(x => x.Amount) ?? 0,
                                 }).ToList();

                var FinalList = (from grplist in groupList
                                 join target in list on grplist.UserID equals target.UserID
                                 where grplist.WalletID == target.WalletID
                                 select new
                                 {
                                     UserID = grplist.UserID,
                                     WalletID = grplist.WalletID,
                                     UserName = target.UserName,
                                     Wallet = target.Wallet,
                                     SaleAmount = grplist.SaleAmount,
                                     TargetAmount = target.TargetAmount,
                                     Commission = target.Commission,
                                     CommissionType = target.CommissionType,
                                     SaleMonthDOC = month + "/" + year,
                                 }).ToList();

                response = (from finallist in FinalList

                            join current in transferedlist
                            on new { X1 = finallist.SaleMonthDOC, X2 = finallist.UserID, X3 = finallist.WalletID } equals new { X1 = current.SaleMonthDOC, X2 = current.UserID, X3 = current.WalletID }

                            into leftData
                            from currentList in leftData.DefaultIfEmpty()

                            where finallist.SaleAmount >= finallist.TargetAmount
                            select new SaleViewModel
                            {
                                UserID = finallist.UserID,
                                WalletID = finallist.WalletID,
                                UserName = finallist.UserName,
                                Wallet = finallist.Wallet,
                                SaleAmount = finallist.SaleAmount,
                                TargetAmount = finallist.TargetAmount,
                                Difference = finallist.TargetAmount - finallist.SaleAmount,
                                Commission = finallist.Commission,
                                CommissionType = finallist.CommissionType,
                                Status = currentList?.Status ?? "",
                                SaleMonthDOC = finallist.SaleMonthDOC,
                            }).ToList();

                return response;
            }
        }

        internal List<MonthlySuccessViewModel> MonthlyFundTransferData(SaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime FromDate = new DateTime(DateTime.Now.Year, selectRequest.Month, 1);
            if (selectRequest.Month == 12)
                FromDate = new DateTime(DateTime.Now.Year - 1, selectRequest.Month, 1);

            DateTime LastDate = FromDate.LastOfMonth();

            int MonthStart = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int MonthEnd = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                var mainList = (from txn in context.FundTransaction
                                join user in context.UserMaster on
                              txn.SenderID equals user.UserID
                                where txn.Status == StatusType.Success
                                      && user.ParentID == UserID
                                       && txn.DOCInt >= MonthStart
                                       && txn.DOCInt <= MonthEnd

                                select new MonthlySuccessViewModel
                                {
                                    UserID = txn.SenderID,
                                    UserTypeID = user.UserTypeID,
                                    WalletID = txn.WalletID,
                                    Amount = txn.Amount,
                                    DOC = Convert.ToInt32(txn.DOC.ToString(Messages.DOCDayFormat, CultureInfo.InvariantCulture)),
                                }).ToList();

                if (selectRequest.UserTypeID != 0)
                {
                    mainList = (from item in mainList
                                where item.UserTypeID == selectRequest.UserTypeID
                                select item).ToList();
                }
                return mainList;
            }
        }

        internal List<SaleViewModel> FundSaleTargetSelect(SaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime date = new DateTime(DateTime.Now.Year, selectRequest.Month, DateTime.Now.Day);
            string year = DateTime.Now.Year.ToString();
            if (selectRequest.Month == 12)
            {
                date = new DateTime(DateTime.Now.Year - 1, selectRequest.Month, DateTime.Now.Day);
                int yearint = DateTime.Now.Year - 1;
                year = yearint.ToString();
            }

            string month = date.ToString("MMMM");

            List<SaleViewModel> response = new List<SaleViewModel>();

            List<MonthlySuccessViewModel> monthlyFundTransfer = MonthlyFundTransferData(selectRequest, UserID);

            List<ViewModel> list = SelectAll(UserID);

            List<TargetSaleCommission> transferedlist = new TargetSaleCommissionDal().Select();

            using (var context = new DatabaseContext())
            {
                var groupList = (from txn in monthlyFundTransfer
                                 group txn by new { txn.UserID, txn.WalletID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     WalletID = grp.Key.WalletID,
                                     SaleAmount = grp?.Sum(x => x.Amount) ?? 0,
                                 }).ToList();

                var FinalList = (from grplist in groupList
                                 join target in list on grplist.UserID equals target.UserID
                                 where grplist.WalletID == target.WalletID
                                 select new
                                 {
                                     UserID = grplist.UserID,
                                     WalletID = grplist.WalletID,
                                     UserName = target.UserName,
                                     Wallet = target.Wallet,
                                     SaleAmount = grplist.SaleAmount,
                                     TargetAmount = target.TargetAmount,
                                     Commission = target.Commission,
                                     CommissionType = target.CommissionType,
                                     SaleMonthDOC = month + "/" + year,
                                 }).ToList();

                response = (from finallist in FinalList

                            join current in transferedlist
                            on new { X1 = finallist.SaleMonthDOC, X2 = finallist.UserID, X3 = finallist.WalletID } equals new { X1 = current.SaleMonthDOC, X2 = current.UserID, X3 = current.WalletID }
                              into leftData
                            from currentList in leftData.DefaultIfEmpty()
                                //    where finallist.SaleAmount >= finallist.TargetAmount
                            select new SaleViewModel
                            {
                                UserID = finallist.UserID,
                                WalletID = finallist.WalletID,
                                UserName = finallist.UserName,
                                Wallet = finallist.Wallet,
                                SaleAmount = finallist.SaleAmount,
                                TargetAmount = finallist.TargetAmount,
                                Difference = finallist.TargetAmount - finallist.SaleAmount,
                                Commission = finallist.Commission,
                                CommissionType = finallist.CommissionType,
                                SaleMonthDOC = finallist.SaleMonthDOC,
                                Status = currentList?.Status ?? "",
                            }).ToList();

                return response;
            }
        }

        internal List<SaleViewModel> SaleTargetSelectUserwise(UserwiseSaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime date = new DateTime(DateTime.Now.Year, selectRequest.Month, DateTime.Now.Day);

            string month = date.ToString("MMMM");

            List<SaleViewModel> response = new List<SaleViewModel>();

            List<MonthlySuccessViewModel> monthlysuccess = MonthlySuccessDataUserwise(selectRequest, UserID);

            List<ViewModel> list = SelectAllUserwise(UserID);
            List<TargetSaleCommission> transferedlist = new TargetSaleCommissionDal().Select();

            using (var context = new DatabaseContext())
            {
                var groupList = (from txn in monthlysuccess
                                 group txn by new { txn.UserID, txn.WalletID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     WalletID = grp.Key.WalletID,
                                     SaleAmount = grp?.Sum(x => x.Amount) ?? 0,
                                 }).ToList();

                var FinalList = (from grplist in groupList
                                 join target in list on grplist.UserID equals target.UserID
                                 where grplist.WalletID == target.WalletID
                                 select new
                                 {
                                     UserID = grplist.UserID,
                                     WalletID = grplist.WalletID,
                                     UserName = target.UserName,
                                     Wallet = target.Wallet,
                                     SaleAmount = grplist.SaleAmount,
                                     TargetAmount = target.TargetAmount,
                                     Commission = target.Commission,
                                     CommissionType = target.CommissionType,
                                     SaleMonthDOC = month + "/" + DateTime.Now.Year.ToString(),
                                 }).ToList();

                response = (from finallist in FinalList

                            join current in transferedlist
                            on new { X1 = finallist.SaleMonthDOC, X2 = finallist.UserID, X3 = finallist.WalletID } equals new { X1 = current.SaleMonthDOC, X2 = current.UserID, X3 = current.WalletID }

                            into leftData
                            from currentList in leftData.DefaultIfEmpty()

                            where finallist.SaleAmount >= finallist.TargetAmount
                            select new SaleViewModel
                            {
                                UserID = finallist.UserID,
                                WalletID = finallist.WalletID,
                                UserName = finallist.UserName,
                                Wallet = finallist.Wallet,
                                SaleAmount = finallist.SaleAmount,
                                TargetAmount = finallist.TargetAmount,
                                Difference = finallist.TargetAmount - finallist.SaleAmount,
                                Commission = finallist.Commission,
                                CommissionType = finallist.CommissionType,
                                Status = currentList?.Status ?? "",
                                SaleMonthDOC = finallist.SaleMonthDOC,
                            }).ToList();

                return response;
            }
        }

        internal List<ViewModel> SelectAllUserwise(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
                return (from Objdb in context.UserwiseSaleTarget
                        where Objdb.IsDelete == false && Objdb.UserID == UserID
                        select new ViewModel
                        {
                            UserID = Objdb.UserID,
                            UserwiseSaleTargetID = Objdb.UserwiseSaleTargetID,
                            ParnetID = Objdb.ParentID,
                            UserName = Objdb.UserMaster.FirmName.ConcatNameMobileAliasName(Objdb.UserMaster.MobileNumber, Objdb.UserMaster.AliasName),
                            WalletID = Objdb.WalletID,
                            Wallet = Objdb.WalletMaster.Name,
                            TargetAmount = Objdb.TargetAmount,
                            Commission = Objdb.Commission,
                            CommissionType = Objdb.CommissionType,
                        }).ToList();
        }

        internal List<MonthlySuccessViewModel> MonthlySuccessDataUserwise(UserwiseSaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime FromDate = new DateTime(DateTime.Now.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int MonthStart = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int MonthEnd = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                var mainList = (from txn in context.TransactionMaster
                                join user in context.UserMaster on
                                 txn.UserID equals user.UserID
                                where txn.Status == StatusType.Success
                                       && user.UserID == UserID
                                       && txn.DOCInt >= MonthStart
                                       && txn.DOCInt <= MonthEnd

                                select new MonthlySuccessViewModel
                                {
                                    UserID = txn.UserID,
                                    WalletID = txn.WalletID,
                                    DistributerUserID = txn.DistributerUserID,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    Amount = txn.Amount,
                                    DOC = Convert.ToInt32(txn.DOC.ToString(Messages.DOCDayFormat, CultureInfo.InvariantCulture)),
                                }).ToList();

                return mainList;
            }
        }

        internal List<SaleViewModel> FundSaleTargetSelectUserwise(UserwiseSaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime date = new DateTime(DateTime.Now.Year - 1, selectRequest.Month, DateTime.Now.Day);
            string month = date.ToString("MMMM");

            List<SaleViewModel> response = new List<SaleViewModel>();

            List<MonthlySuccessViewModel> monthlyFundTransfer = MonthlyFundTransferDataUserwise(selectRequest, UserID);

            List<ViewModel> list = SelectAll(UserID);

            List<TargetSaleCommission> transferedlist = new TargetSaleCommissionDal().Select();

            using (var context = new DatabaseContext())
            {
                var groupList = (from txn in monthlyFundTransfer
                                 group txn by new { txn.UserID, txn.WalletID } into grp
                                 select new
                                 {
                                     UserID = grp.Key.UserID,
                                     WalletID = grp.Key.WalletID,
                                     SaleAmount = grp?.Sum(x => x.Amount) ?? 0,
                                 }).ToList();

                var FinalList = (from grplist in groupList
                                 join target in list on grplist.UserID equals target.UserID
                                 where grplist.WalletID == target.WalletID
                                 select new
                                 {
                                     UserID = grplist.UserID,
                                     WalletID = grplist.WalletID,
                                     UserName = target.UserName,
                                     Wallet = target.Wallet,
                                     SaleAmount = grplist.SaleAmount,
                                     TargetAmount = target.TargetAmount,
                                     Commission = target.Commission,
                                     CommissionType = target.CommissionType,
                                     SaleMonthDOC = month + "/" + DateTime.Now.Year.ToString(),
                                 }).ToList();

                response = (from finallist in FinalList

                            join current in transferedlist
                            on new { X1 = finallist.SaleMonthDOC, X2 = finallist.UserID, X3 = finallist.WalletID } equals new { X1 = current.SaleMonthDOC, X2 = current.UserID, X3 = current.WalletID }
                              into leftData
                            from currentList in leftData.DefaultIfEmpty()

                            select new SaleViewModel
                            {
                                UserID = finallist.UserID,
                                WalletID = finallist.WalletID,
                                UserName = finallist.UserName,
                                Wallet = finallist.Wallet,
                                SaleAmount = finallist.SaleAmount,
                                TargetAmount = finallist.TargetAmount,
                                Difference = finallist.TargetAmount - finallist.SaleAmount,
                                Commission = finallist.Commission,
                                CommissionType = finallist.CommissionType,
                                SaleMonthDOC = finallist.SaleMonthDOC,
                                Status = currentList?.Status ?? "",
                            }).ToList();

                return response;
            }
        }

        internal List<MonthlySuccessViewModel> MonthlyFundTransferDataUserwise(UserwiseSaleTargetSelectRequest selectRequest, int UserID)
        {
            DateTime FromDate = new DateTime(DateTime.Now.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int MonthStart = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int MonthEnd = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                var mainList = (from txn in context.FundTransaction
                                join user in context.UserMaster on
                              txn.SenderID equals user.UserID
                                where txn.Status == StatusType.Success
                                      && user.UserID == UserID
                                       && txn.DOCInt >= MonthStart
                                       && txn.DOCInt <= MonthEnd
                                       && txn.Through != FundTransferType.ThroughDirectReverse

                                select new MonthlySuccessViewModel
                                {
                                    UserID = txn.SenderID,
                                    UserTypeID = user.UserTypeID,
                                    WalletID = txn.WalletID,
                                    Amount = txn.Amount,
                                    DOC = Convert.ToInt32(txn.DOC.ToString(Messages.DOCDayFormat, CultureInfo.InvariantCulture)),
                                }).ToList();

                return mainList;
            }
        }
    }
}