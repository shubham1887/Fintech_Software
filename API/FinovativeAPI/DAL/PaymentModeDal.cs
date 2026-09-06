using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PaymentModeDal
    {
        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<PaymentModeMaster>>();
            cache.Remove(CacheKeys.PaymentModeMaster);
        }

        internal List<PaymentModeMaster> Select()
        {
            var _paymentModes = new MemoryCacheProcessing<List<PaymentModeMaster>>();
            List<PaymentModeMaster> collectionList = _paymentModes.GetOrCreate(CacheKeys.PaymentModeMaster, () => SelectWithoutCache());
            return collectionList;
        }

        private List<PaymentModeMaster> SelectWithoutCache()
        {
            List<PaymentModeMaster> list = new List<PaymentModeMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.PaymentModeMaster.Where(p => p.IsActive == true).ToList();
            }
            return list;
        }
    }
}