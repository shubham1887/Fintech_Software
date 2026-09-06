using Project.Model;
using Project.Processing;
using Project.Response.Sender;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SenderDAL
    {
        private readonly string key = "_SenderDAL";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<Sender>>();
            cache.Remove(key);
        }

        internal List<Sender> Select()
        {
            var _List = new MemoryCacheProcessing<List<Sender>>();
            List<Sender> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;
            //return SelectWithoutCache();
        }

        private List<Sender> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.Sender.ToList();
            }
        }

        public int Insert(Sender ObjModel)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.Sender.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.SenderID;
            }
        }

        internal int Update(Sender ObjModel)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.Sender.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.SenderID;
            }
        }

        internal Sender SelectByID(int SenderID)
        {
            List<Sender> list = Select();
            Sender obj;

            obj = (from sender in list
                   where sender.SenderID == SenderID
                   && sender.IsActive == true
                   orderby sender.SenderID descending
                   select sender).FirstOrDefault();

            return obj;
        }

        internal Sender SelectBySenderID(int SenderID)
        {
            List<Sender> list = Select();
            Sender obj;

            obj = (from sender in list
                   where sender.SenderID == SenderID

                   orderby sender.SenderID descending
                   select sender).FirstOrDefault();

            return obj;
        }

        internal Sender SelectByNumber(string MobileNumber)
        {
            List<Sender> list = Select();
            Sender obj;

            obj = (from sender in list
                   where sender.Mobile == MobileNumber
                   && sender.IsActive == true
                   //&& sender.UserID == UserID
                   orderby sender.SenderID descending
                   select sender).FirstOrDefault();

            return obj;
        }

        internal string SelectBySenderNumber(string MobileNumber)
        {
            string result = "";
            List<Sender> list = Select();

            Sender obj;

            obj = (from sender in list
                   where sender.Mobile == MobileNumber
                   && sender.IsActive == true
                   //&& sender.UserID == UserID
                   orderby sender.SenderID descending
                   select sender).FirstOrDefault();
            if (obj != null)
                result = obj.GUID;

            return result;
        }

        internal Sender SelectByGUID(string GUID)
        {
            List<Sender> list = Select();
            Sender obj;

            obj = (from sender in list
                   where sender.GUID == GUID
                   && sender.IsActive == true
                   //&& sender.UserID == UserID
                   orderby sender.SenderID descending
                   select sender).FirstOrDefault();

            return obj;
        }

        internal List<SenderViewModel> SelectAllSender()
        {
            List<Sender> list = Select();

            List<SenderViewModel> obj;

            obj = (from sender in list
                   orderby sender.SenderID descending
                   select new SenderViewModel
                   {
                       Name = sender.Name,
                       Mobile = sender.Mobile,
                       Address = sender.Address,
                       GUID = sender.GUID,
                       SenderID = sender.SenderID,
                       Pincode = sender.Pincode,
                       IsOTPVerified = sender.IsOTPVerified,
                       IsActive = sender.IsActive,
                   }).ToList();

            return obj;
        }
    }
}