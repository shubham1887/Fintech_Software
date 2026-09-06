using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class RealProviderMappingDal
    {
        private readonly string key = "_RealProviderMapping";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<RealProviderMapping>>();
            cache.Remove(key);
        }

        internal List<RealProviderMapping> Select()
        {
            var _realProviderList = new MemoryCacheProcessing<List<RealProviderMapping>>();
            List<RealProviderMapping> realProviderList = _realProviderList.GetOrCreate(key, () => SelectWithoutCache());
            return realProviderList;
        }

        private List<RealProviderMapping> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.RealProviderMapping.ToList();
        }

        internal int Insert(RealProviderMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.RealProviderMapping.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.RealProviderMappingID;
        }

        internal RealProviderMapping SelectByOperatorID(int OperatorID)
        {
            List<RealProviderMapping> OpList = Select();
            RealProviderMapping op = null;
            return op = OpList.FirstOrDefault(obj => obj.OperatorID == OperatorID);
        }

        internal int Update(RealProviderMapping ObjModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.RealProviderMapping.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.RealProviderMappingID;
        }

        internal RealProviderMapping SelectByProviderIDAndOperatorID(int OperatorID, int ProviderID)
        {
            List<RealProviderMapping> list = Select();
            return list.FirstOrDefault(l => l.OperatorID == OperatorID && l.ProviderID == ProviderID);
        }

        internal int SelectRealProvider(int OperatorID)
        {
            List<RealProviderMapping> list = Select();

            return list.FirstOrDefault(l => l.OperatorID == OperatorID)?.ProviderID ?? 0;
        }
    }
}