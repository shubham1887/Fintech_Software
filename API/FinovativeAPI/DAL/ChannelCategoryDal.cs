using Project.Model;
using Project.Processing;
using Project.Response.ChannelCategory;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ChannelCategoryDal
    {
        private static string key = "_ChannelCategory";

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from item in context.ChannelCategory

                        select new ViewModel
                        {
                            ChannelCategoryID = item.ChannelCategoryID,
                            Category = item.Category,
                        }).ToList();
            }
        }
    }
}