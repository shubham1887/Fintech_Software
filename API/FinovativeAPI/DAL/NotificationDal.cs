using Project.Model;
using Project.Processing;
using System.Linq;

namespace Project.DAL
{
    public class NotificationDal
    {
        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<Notification>();
            cache.Remove(CacheKeys.Notification);
        }

        internal Notification Select()
        {
            var _response = new MemoryCacheProcessing<Notification>();
            Notification details = _response.GetOrCreate(CacheKeys.Notification, () => SelectWithoutCache());
            return details;
        }

        private Notification SelectWithoutCache()
        {
            Notification list = new Notification();
            using (var context = new DatabaseContext())
            {
                list = context.Notification.FirstOrDefault();
            }
            return list;
        }

        internal int Insert(Notification objModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.Notification.Add(objModel);
                context.SaveChanges();
                return objModel.NotificationID;
            }
        }

        internal Notification SelectByID(int NotificationID)
        {
            using (var context = new DatabaseContext())
            {
                return context.Notification.Find(NotificationID);
            }
        }

        internal int Update(Notification objModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.Notification.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.Notification.Where(U => U.DOCInt <= DeleteDOCInt);
                context.Notification.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}