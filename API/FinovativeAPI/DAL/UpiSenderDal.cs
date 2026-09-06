using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UpiSenderDal
    {
        private readonly string key = "_UpiTransferSenderDal";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UpiSender>>();
            cache.Remove(key);
        }


        internal List<UpiSender> Select()
        {
            var _List = new MemoryCacheProcessing<List<UpiSender>>();
            List<UpiSender> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;
            //return SelectWithoutCache();
        }


        private List<UpiSender> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.UpiSender.ToList();
            }
        }


        internal UpiSender SelectByNumber(string MobileNumber)
        {
            List<UpiSender> list = Select();
            UpiSender obj;

            obj = (from sender in list
                   where sender.Mobile == MobileNumber
                   && sender.IsActive == true
                   //&& sender.UserID == UserID
                   orderby sender.UpiSenderID descending
                   select sender).FirstOrDefault();

            return obj;
        }

        public int Insert(UpiSender ObjModel)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.UpiSender.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.UpiSenderID;
            }
        }

        internal int VerifyOTP(UpiSender ObjModel)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.UpiSender.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.UpiSenderID;
            }
        }

        internal UpiSender SelectByGUID(string GUID)
        {
            List<UpiSender> list = Select();
            UpiSender obj;

            obj = (from sender in list
                   where sender.GUID == GUID
                   && sender.IsActive == true
                   //&& sender.UserID == UserID
                   orderby sender.UpiSenderID descending
                   select sender).FirstOrDefault();

            return obj;
        }

        internal UpiSender SelectByID(int UpiSenderID)
        {
            List<UpiSender> list = Select();
            UpiSender obj;

            obj = (from sender in list
                   where sender.UpiSenderID == UpiSenderID
                   && sender.IsActive == true
                   orderby sender.UpiSenderID descending
                   select sender).FirstOrDefault();

            return obj;
        }
    }
}
