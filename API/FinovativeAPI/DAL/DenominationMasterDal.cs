using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DenominationMasterDal
    {
        public static string Key
        { get { return "_DenominationMaster"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DenominationMaster>>();
            cache.Remove(Key);
        }

        internal int Insert(DenominationMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.DenominationMaster.Add(objModel);
            context.SaveChanges();
            return objModel.DenominationMasterID;
        }

        internal List<DenominationMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<DenominationMaster>>();
            List<DenominationMaster> list = _list.GetOrCreate(CacheKeys.DenominationMaster, () => SelectWithoutCache());
            return list;
        }

        private List<DenominationMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.DenominationMaster.ToList();
        }

        //internal DenominationMaster SelectByAmount(int minAmount, int maxAmount)
        //{
        //    return SelectAll().FirstOrDefault(d => d.MinAmount == minAmount
        //          && d.MaxAmount == maxAmount);
        //}

        internal DenominationMaster SelectByID(int DenominationMasterID)
        {
            using var context = new DatabaseContext();
            return context.DenominationMaster.Find(DenominationMasterID);
        }

        public int Update(DenominationMaster denomination)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.DenominationMaster.Update(denomination);
            context.SaveChanges();
            return denomination.DenominationMasterID;
        }

        internal int Delete(DenominationMaster objmodel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.DenominationMaster.Remove(objmodel);
            return context.SaveChanges();
        }
    }
}