using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UpiBeneficiaryDal
    {
        private readonly string key = "_UpiTransferBeneficiaryDAL";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UpiBeneficiary>>();
            cache.Remove(key);
        }

        internal List<UpiBeneficiary> Select()
        {
            var _List = new MemoryCacheProcessing<List<UpiBeneficiary>>();
            List<UpiBeneficiary> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;

        }

        private List<UpiBeneficiary> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.UpiBeneficiary.ToList();
            }
        }

        public int Insert(UpiBeneficiary ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UpiBeneficiary.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.UpiBeneficiaryID;
            }
        }

        internal UpiBeneficiary SelectByUpiID(string UpiID, int UpiSenderID)
        {
            List<UpiBeneficiary> list = Select();
            UpiBeneficiary obj;

            obj = (from bene in list
                   where bene.UpiID == UpiID
                   //&& bene.IFSC == IFSC
                   && bene.UpiSenderID == UpiSenderID
                   && bene.IsActive == true
                   orderby bene.UpiBeneficiaryID descending
                   select bene).FirstOrDefault();

            return obj;
        }

        internal UpiBeneficiary SelectByGUID(string GUID)
        {
            List<UpiBeneficiary> list = Select();
            UpiBeneficiary obj;

            obj = (from sender in list
                   where sender.GUID == GUID
                   && sender.IsActive == true
                   //&& sender.UserID == UserID
                   orderby sender.UpiBeneficiaryID descending
                   select sender).FirstOrDefault();

            return obj;
        }

        internal int VerifyOTP(UpiBeneficiary ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UpiBeneficiary.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.UpiBeneficiaryID;
            }
        }

        internal List<UpiBeneficiary> SelectBySenderID(int UpiSenderID)
        {
            List<UpiBeneficiary> list = Select();

            List<UpiBeneficiary> obj;

            obj = (from bene in list
                   where bene.UpiSenderID == UpiSenderID
                   && bene.IsActive == true && bene.IsOTPVerified == true
                   orderby bene.UpiBeneficiaryID descending
                   select bene).ToList();

            return obj;
        }

        internal int Delete(UpiBeneficiary ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UpiBeneficiary.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.UpiBeneficiaryID;
        }
        public UpiBeneficiary SelectByID(int UpiBeneficiaryID)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return context.UpiBeneficiary.Find(UpiBeneficiaryID);
        }


    }
}
