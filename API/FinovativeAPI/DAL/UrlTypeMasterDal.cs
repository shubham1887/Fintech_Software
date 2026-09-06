using Project.Model;
using Project.Response.URLType;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UrlTypeMasterDal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> list = null;
            using (var context = new DatabaseContext())
            {
                list = (from UrlType in context.UrlTypeMaster
                        select new ViewModel
                        {
                            UrlTypeID = UrlType.UrlTypeID,
                            UrlType = UrlType.UrlType,
                        }).ToList();

                return list;
            }
        }

        internal List<UrlTypeMaster> SelectAll()

        {
            List<UrlTypeMaster> list = new List<UrlTypeMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.UrlTypeMaster.ToList();
            }
            return list;
        }
    }
}