using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PaysprintOnboardedUserDAL
    {
        public readonly string key = "_PaysprintOnboardedUserr";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<PaysprintOnboardedUser>>();
            cache.Remove(key);
        }

        internal int Insert(PaysprintOnboardedUser objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.PaysprintOnboardedUser.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal List<PaysprintOnboardedUser> Select()
        {
            var _list = new MemoryCacheProcessing<List<PaysprintOnboardedUser>>();
            List<PaysprintOnboardedUser> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<PaysprintOnboardedUser> SelectWithoutCache()
        {
            List<PaysprintOnboardedUser> list;
            using (var context = new DatabaseContext())
            {
                list = (from item in context.PaysprintOnboardedUser
                        select item).ToList();
            }

            return list;
        }

        internal PaysprintOnboardedUser SelectByID(int UserID)
        {
            PaysprintOnboardedUser item = Select().FirstOrDefault(S => S.UserID == UserID);
            return item;
        }
    }
}