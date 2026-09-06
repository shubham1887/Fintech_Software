using Project.Model;
using Project.Processing;
using Project.Response.DateFormat;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DateFormatMasterDAL
    {
        public static string key
        { get { return "_DateFormatMasterDAL"; } }

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> list;
            using (var context = new DatabaseContext())
            {
                list = (from item in context.DateFormatMaster
                        where item.IsActive == true
                        select new ViewModel
                        {
                            DateFormat = item.DateFormat
                        }
                        ).ToList();
            }
            return list;
        }
    }
}