using Project.Model;
using Project.Response.MenuMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class AndroidMenuMasterDal
    {
        public List<AndroidViewModel> Select()
        {
            using (var context = new DatabaseContext())
            {
                return (from menu in context.AndroidMenuMaster

                        select new AndroidViewModel
                        {
                            AndroidMenuMasterID = menu.AndroidMenuMasterID,
                            IsActive = menu.IsActive,
                            MenuName = menu.MenuName,
                            Key = menu.Key,
                            URL = menu.URL,
                            Sequence = menu.Sequence,
                            SectionName = menu.SectionName,
                            UserTypeID = menu.UserTypeID,
                        }).ToList();
            }
        }
    }
}