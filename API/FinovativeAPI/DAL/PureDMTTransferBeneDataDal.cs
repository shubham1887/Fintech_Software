using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PureDMTTransferBeneDataDal
    {
        public readonly string key = "_PureDMTTransferBeneDetail";

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<PureDMTTransferBeneDetail>>();
            cache.Remove(key);
        }

        internal int Insert(PureDMTTransferBeneDetail PureDMTTransferBeneDetail)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.PureDMTTransferBeneDetail.Add(PureDMTTransferBeneDetail);
                return context.SaveChanges();
            }
        }

        internal int Delete(PureDMTTransferBeneDetail ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.PureDMTTransferBeneDetail.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.PureDMTTransferBeneDetailID;
        }

        internal List<PureDMTTransferBeneDetail> Select()
        {
            var _list = new MemoryCacheProcessing<List<PureDMTTransferBeneDetail>>();

            List<PureDMTTransferBeneDetail> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<PureDMTTransferBeneDetail> SelectWithoutCache()
        {
            List<PureDMTTransferBeneDetail> list;
            using (var context = new DatabaseContext())
            {
                list = context.PureDMTTransferBeneDetail.ToList();
            }
            return list;
        }     

        internal List<PureDMTTransferBeneDetail> CheckDuplicate(string BeneficaryID, int ProviderID)
        {
            List<PureDMTTransferBeneDetail> list = Select();
            list = list.Where(S => S.BeneID == BeneficaryID && S.ProviderID == ProviderID).ToList();
            return list;
        }

        internal int Update(PureDMTTransferBeneDetail ObjModel)
        {
            removeCache();

            using var context = new DatabaseContext();
            context.PureDMTTransferBeneDetail.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.PureDMTTransferBeneDetailID;
        }

        internal PureDMTTransferBeneDetail SelectByID(string BeneID, int ProviderID)
        {
            List<PureDMTTransferBeneDetail> list = Select();

            return list.FirstOrDefault(U => U.BeneID == BeneID && U.ProviderID == ProviderID);
        }
    }
}