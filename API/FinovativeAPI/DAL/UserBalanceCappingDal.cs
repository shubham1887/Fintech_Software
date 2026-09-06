using Project.Model;
using Project.Processing;
using Project.Response.UserBalanceCapping;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserBalanceCappingDal
    {
        private static readonly string Key = "_UserBalanceCapping";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<UserBalanceCapping>>();
            cache.Remove(Key);
        }

        private List<UserBalanceCapping> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<UserBalanceCapping>>();
            List<UserBalanceCapping> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<UserBalanceCapping> SelectWithoutCache()
        {
            List<UserBalanceCapping> list = new List<UserBalanceCapping>();
            using (var context = new DatabaseContext())
            {
                list = context.UserBalanceCapping.ToList();
            }
            return list;
        }

        internal UserBalanceCapping SelectWalletID(int WalletID)
        {
            using var context = new DatabaseContext();
            return context.UserBalanceCapping.FirstOrDefault(U => U.WalletID == WalletID && U.IsDelete == false);
        }

        internal UserBalanceCapping SelectByWalletIDAndUserID(int UserID, int WalletID)
        {
            using var context = new DatabaseContext();
            return context.UserBalanceCapping.FirstOrDefault(U => U.UserID == UserID && U.WalletID == WalletID);
        }

        internal UserBalanceCapping SelectByWalletIDAndUserIDWithCache(int UserID, int WalletID)
        {
            return SelectAll().Where(x => x.UserID == UserID && x.WalletID == WalletID).FirstOrDefault();
        }

        internal List<CappingViewModel> SelectBalanceCapping(int UserID)
        {
            List<CappingViewModel> list = new List<CappingViewModel>();

            List<UserBalanceCapping> listSingle = SelectAll().Where(x => x.UserID == UserID).ToList();

            List<WalletMaster> walletList = new WalletMasterDal().Select();

            list = (from wallet in walletList
                    join capping in listSingle on wallet.WalletID equals capping.WalletID into cappingmapping
                    from capLeft in cappingmapping.DefaultIfEmpty()

                    select new CappingViewModel
                    {
                        WalletID = wallet.WalletID,
                        Name = wallet.Name,
                        ShortName = wallet.ShortName,
                        Amount = capLeft == null ? 0 : capLeft.Amount
                    }).ToList();

            return list;
        }

        internal UserBalanceCapping SelectByID(int UserBalanceCappingID)
        {
            return SelectAll().FirstOrDefault(T => T.UserBalanceCappingID == UserBalanceCappingID && T.IsDelete == false);
        }

        internal int Insert(UserBalanceCapping objModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.UserBalanceCapping.Add(objModel);
            context.SaveChanges();
            return objModel.UserBalanceCappingID;
        }

        internal int DeleteByWalletID(int WalletID)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                var slectedWalletID = context.UserBalanceCapping.Where(U => U.WalletID == WalletID);
                context.UserBalanceCapping.RemoveRange(slectedWalletID);
                return context.SaveChanges();
            }
        }

        internal List<ViewModel> Select()
        {
            List<UserBalanceCapping> list = SelectAll();

            List<WalletMaster> walletlist = new WalletMasterDal().Select();

            List<UserMaster> userList = new UserDAL().Select();
            return (from item in list
                    join user in userList on item.UserID equals user.UserID
                    join wallet in walletlist on item.WalletID equals wallet.WalletID
                    orderby user.UserTypeID ascending
                    select new ViewModel
                    {
                        WalletID = item.WalletID,
                        UserID = item.UserID,
                        UserBalanceCappingID = item.UserBalanceCappingID,
                        Amount = item.Amount,
                        UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        Wallet = wallet.Name,
                    }).ToList();
        }

        public int Update(UserBalanceCapping ObjModel)
        {
            removeCache();

            using var context = new DatabaseContext();
            context.UserBalanceCapping.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.UserBalanceCappingID;
        }

        internal int Delete(UserBalanceCapping ObjModel)
        {
            removeCache();

            using var context = new DatabaseContext();
            context.UserBalanceCapping.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.UserBalanceCappingID;
        }
    }
}