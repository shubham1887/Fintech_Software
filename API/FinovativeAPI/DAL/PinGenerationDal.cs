using Project.Model;
using Project.Processing;
using Project.Response.PinGeneration;

using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    
    public class PinGenerationDal
    {
        private readonly string PinGenerationKEY = "_PinGenerationDal";
        internal int Insert(MPinHistory ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.MPinHistory.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.UserID;
        }

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(PinGenerationKEY);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(PinGenerationKEY, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.MPinHistory
                        orderby setting.MPinHistoryID descending
                        select new ViewModel
                        {
                            UserID = setting.UserID,
                            DOCInt = setting.DOCInt,
                            DOC = setting.DOC.LongFormatDate(),
                            Mpin = setting.Mpin,
                            DeviceID = setting.DeviceID,
                        }).ToList();
            }
        }
        public List<ViewModel> SelectPin(int UserID)
        {
            List<ViewModel> mainList = Select(); 

            var list = from setting in mainList
                       where setting.UserID == UserID
                       select new ViewModel
                       {
                           UserID = setting.UserID,
                           Mpin = setting.Mpin,
                           DOCInt = setting.DOCInt,
                           DOC = setting.DOC,
                           DeviceID = setting.DeviceID,
                       };

            return list.Take(3).ToList();
        }
    }
}
