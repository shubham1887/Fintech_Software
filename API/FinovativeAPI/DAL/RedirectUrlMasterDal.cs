using Project.Model;
using Project.Processing;
using Project.Response.RedirectUrl;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class RedirectUrlMasterDal
    {
        private readonly string Key = "_RedirectUrlMaster";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<RedirectUrlMaster>>();
            cache.Remove(Key);
        }

        internal List<RedirectUrlMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<RedirectUrlMaster>>();
            List<RedirectUrlMaster> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        internal ViewModel SelectUrlByName(string name)
        {
            ViewModel viewModel = null;
            List<RedirectUrlMaster> UrlList = Select();
            using (var context = new DatabaseContext())
            {
               return viewModel=(from url in UrlList
                        where url.Name == name
                        select new ViewModel
                        { 
                         Name= url.Name,
                         Url=url.RedirectUrl
                        }).FirstOrDefault();
            }
        }

        private List<RedirectUrlMaster> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from url in context.RedirectUrlMaster
                        where url.IsActive == true
                        orderby url.RedirectUrlMasterID descending
                        select url ).ToList();
            }
        }
    }
}