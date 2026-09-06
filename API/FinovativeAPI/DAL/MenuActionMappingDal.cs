using Project.Model;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class MenuActionMappingDal
    {
        internal List<MenuActionMapping> SelectByMenuMasterID(string GUID)
        {
            using var context = new DatabaseContext();
            return (from item in context.MenuActionMapping
                    where item.MenuMasterGUID == GUID
                    select item).ToList();
        }
    }
}