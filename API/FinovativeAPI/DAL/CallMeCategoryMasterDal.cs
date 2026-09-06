using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CallMeCategoryMasterDal
    {
        public static string Key = "_CallMeCategoryMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<CallMeCategoryMaster>>();
            cache.Remove(Key);
        }

        internal List<CallMeCategoryMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<CallMeCategoryMaster>>();
            List<CallMeCategoryMaster> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<CallMeCategoryMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.CallMeCategoryMaster.ToList();
        }
    }
}