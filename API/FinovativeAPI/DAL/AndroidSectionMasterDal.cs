using Project.Model;
using Project.Response.MenuMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class AndroidSectionMasterDal
    {
        public List<AndroidSectionViewModel> Select()
        {
            using (var context = new DatabaseContext())
            {
                return (from menu in context.AndroidSectionMaster

                        select new AndroidSectionViewModel
                        {
                            AndroidSectionMasterID = menu.AndroidSectionMasterID,
                            SectionName = menu.SectionName,
                        }).ToList();
            }
        }
    }
}