using Project.Model;
using Project.Processing;
using Project.Response.UserwiseLowBalance;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserwiseLowBalanceDAL
    {
        public static readonly string Key = "_UserwiseLowBalance";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        internal UserwiseLowBalance SelectByWalletID(int walletID, int userID)
        {
            using var context = new DatabaseContext();
            return context.UserwiseLowBalance.FirstOrDefault(x => x.WalletID == walletID && x.UserID == userID);
        }

        public int Insert(UserwiseLowBalance model)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.UserwiseLowBalance.Add(model);
            context.SaveChanges();
            return model.UserwiseLowBalanceID;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.UserwiseLowBalance
                        orderby setting.UserwiseLowBalanceID descending
                        where setting.Amount >= 0.0M
                        select new ViewModel
                        {
                            UserwiseLowBalanceID = setting.UserwiseLowBalanceID,
                            WalletName = setting.WalletMaster.Name,
                            WalletID = setting.WalletID,
                            UserID = setting.UserID,
                            Amount = setting.Amount,
                        }).ToList();
            }
        }

        internal int Update(UserwiseLowBalance model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.UserwiseLowBalance.Update(model);
                return context.SaveChanges();
            }
        }

        internal UserwiseLowBalance SelectByID(int userwiseLowBalanceID)
        {
            using var context = new DatabaseContext();
            return context.UserwiseLowBalance.Find(userwiseLowBalanceID);
        }

        internal List<ViewModel> SelectByUserID(int userID)
        {
            List<ViewModel> list = Select();

            List<ViewModel> obj = (from item in list
                                   where item.UserID == userID
                                   select item).ToList();

            return obj;
        }
    }
}