using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserCreationWalletDal
    {
        private static readonly string Key = "_UserCreationWallet";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserCreationWallet>>();
            cache.Remove(Key);
        }

        internal int Insert(UserCreationWallet model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.UserCreationWallet.Add(model);
                context.SaveChanges();
                result = model.UserCreationWalletID;
            }
            return result;
        }

        internal int Update(UserCreationWallet model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.UserCreationWallet.Update(model);
                context.SaveChanges();
                result = model.UserCreationWalletID;
            }
            return result;
        }

        internal UserCreationWallet SelectByID(int UserID)
        {
            List<UserCreationWallet> list = SelectAll();
            return (from item in list
                    where item.UserID == UserID
                    select item).FirstOrDefault();
        }

        private List<UserCreationWallet> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<UserCreationWallet>>();
            List<UserCreationWallet> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<UserCreationWallet> SelectWithoutCache()
        {
            List<UserCreationWallet> list = new List<UserCreationWallet>();
            using (var context = new DatabaseContext())
            {
                list = context.UserCreationWallet.ToList();
            }
            return list;
        }

        internal UserCreationWallet CheckDuplicate(int UserID)
        {
            List<UserCreationWallet> list = SelectAll();
            return (from item in list
                    where item.UserID == UserID
                    select item).FirstOrDefault();
        }

        internal UserCreationWallet SelectUserCreationWalletByUserID(int UserID)
        {
            List<UserCreationWallet> list = SelectAll();
            return (from item in list
                    where item.UserID == UserID
                    select item).FirstOrDefault();
        }
    }
}