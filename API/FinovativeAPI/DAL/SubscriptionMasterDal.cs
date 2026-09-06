using Project.Model;
using Project.Processing;
using System.Linq;

namespace Project.DAL
{
    public class SubscriptionMasterDal
    {
        private readonly string key = "_SubscriptionMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<SubscriptionMaster>();
            cache.Remove(key);
        }

        internal SubscriptionMaster Select()
        {
            var list = new MemoryCacheProcessing<SubscriptionMaster>();
            SubscriptionMaster Subscription = list.GetOrCreate(key, () => SelectWithoutCache());
            return Subscription;
        }

        private SubscriptionMaster SelectWithoutCache()
        {
            SubscriptionMaster list = new SubscriptionMaster();
            using (var context = new DatabaseContext())
            {
                list = context.SubscriptionMaster.FirstOrDefault();
            }
            return list;
        }

        internal int Update(SubscriptionMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SubscriptionMaster.Update(objModel);
            return context.SaveChanges();
        }

        internal SubscriptionMaster Insert(SubscriptionMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SubscriptionMaster.Add(objModel);
            context.SaveChanges();
            return objModel;
        }
    }
}