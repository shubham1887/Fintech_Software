using Project.Model;
using Project.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CircleAPIDal
    {
        private readonly string key = "_CircleAPIMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<CircleAPIMaster>>();
            cache.Remove(key);
        }

        internal List<CircleAPIMaster> Select()
        {
            var _cirleAPIList = new MemoryCacheProcessing<List<CircleAPIMaster>>();
            List<CircleAPIMaster> cirleAPIList = _cirleAPIList.GetOrCreate(key, () => SelectWithoutCache());
            return cirleAPIList;
        }

        private List<CircleAPIMaster> SelectWithoutCache()
        {
            List<CircleAPIMaster> list = new List<CircleAPIMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = context.CircleAPIMaster.ToList();
            }
            return list;
        }

        internal CircleAPIMaster SelectByID(int circleAPIMasterID)
        {
            using var context = new DatabaseContext();
            return context.CircleAPIMaster.Find(circleAPIMasterID);
        }

        internal string SelectActiveURL()
        {
            string URL = "";
            List<CircleAPIMaster> list = Select();
            if (list == null || list.Count <= 0)
            {
                return "";
            }

            CircleAPIMaster item = list.Where(l => l.IsActive == true).FirstOrDefault();
            if (item == null)
            {
                return "";
            }

            URL = item.CircleAPIURL;
            return URL;
        }

        internal int Insert(CircleAPIMaster ObjModel)
        {
            RemoveCache();
            int result = 1;
            try
            {
                using var context = new DatabaseContext();
                context.CircleAPIMaster.Add(ObjModel);
                context.SaveChanges();
                result = ObjModel.CircleAPIMasterID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal int Update(CircleAPIMaster ObjModel)
        {
            RemoveCache();
            int result = 1;
            try
            {
                using var context = new DatabaseContext();
                context.CircleAPIMaster.Update(ObjModel);
                context.SaveChanges();
                result = ObjModel.CircleAPIMasterID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }
    }
}