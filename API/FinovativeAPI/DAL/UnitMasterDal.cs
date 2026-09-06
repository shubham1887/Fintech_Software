using Project.Model;
using Project.Processing;
using Project.Response.UnitMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UnitMasterDal
    {
        public static string CacheKey
        { get { return "_UnitMaster"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(CacheKey);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(CacheKey, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return (from unit in context.UnitMaster

                    select new ViewModel
                    {
                        UnitMasterID = unit.UnitMasterID,
                        Name = unit.Name,
                    }).ToList();
        }
    }
}