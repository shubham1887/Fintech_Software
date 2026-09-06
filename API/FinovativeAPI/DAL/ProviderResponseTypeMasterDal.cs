using Project.Model;
using Project.Processing;
using Project.Response.ProviderResponseType;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderResponseTypeMasterDal
    {
        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(CacheKeys.ProviderResponseTypeMaster);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list =
                _list.GetOrCreate(CacheKeys.ProviderResponseTypeMaster, () => SelectWithoutCache());

            return list;
        }

        internal List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> list = null;
            using var context = new DatabaseContext();

            list = (from ResponseType in context.ProviderResponseTypeMaster
                    select new ViewModel
                    {
                        Type = ResponseType.Type,
                        ID = ResponseType.ProviderResponseTypeMasterID
                    }).ToList();

            return list;
        }
    }
}