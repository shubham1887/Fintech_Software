using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ViewProviderMappingDal
    {
        private readonly string key = "_ViewProviderMapping";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewProviderMapping>>();
            cache.Remove(key);
        }

        internal List<ViewProviderMapping> Select()
        {
            var _realProviderList = new MemoryCacheProcessing<List<ViewProviderMapping>>();
            List<ViewProviderMapping> ViewProviderList = _realProviderList.GetOrCreate(key, () => SelectWithoutCache());
            return ViewProviderList;
        }

        private List<ViewProviderMapping> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.ViewProviderMapping.ToList();
        }

        internal int Insert(ViewProviderMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ViewProviderMapping.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.ViewProviderMappingID;
        }

        internal ViewProviderMapping SelectByOperatorID(int OperatorID)
        {
            List<ViewProviderMapping> OpList = Select();
            ViewProviderMapping op = null;
            return op = OpList.FirstOrDefault(obj => obj.OperatorID == OperatorID);
        }

        internal int Update(ViewProviderMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ViewProviderMapping.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.ViewProviderMappingID;
        }

        internal int SelectViewProvider(int OperatorID)
        {
            List<ViewProviderMapping> OpList = Select();
            return OpList.FirstOrDefault(obj => obj.OperatorID == OperatorID)?.ProviderID ?? 0;
        }

        internal int SelectProviderID(int OperatorID)
        {
            List<ViewProviderMapping> list = Select();
            ViewProviderMapping Obj = list.FirstOrDefault(l => l.OperatorID == OperatorID);
            return (Obj == null) ? 0 : Obj.ProviderID;
        }
    }
}