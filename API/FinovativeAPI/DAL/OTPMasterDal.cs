using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OTPMasterDal
    {
        public static string key
        { get { return "_OTPMaster"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<OTPMaster>>();
            cache.Remove(key);
        }

        internal int Insert(OTPMaster objModel)
        {
            RemoveCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.OTPMaster.Add(objModel);
                context.SaveChanges();
                Result = objModel.OTPMasterID;
            }
            return Result;
        }

        internal int Update(OTPMaster objModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.OTPMaster.Update(objModel);
                context.SaveChanges();
                return objModel.OTPMasterID;
            }
        }
     

        internal OTPMaster SelectOtp(int OTPMasterID, string OTP,string Mobile)
        {
            List<OTPMaster> list = SelectAll();

            return list.FirstOrDefault(U => U.OTP == OTP && U.OTPMasterID== OTPMasterID && U.IsVerified == false && U.MobileNumber == Mobile);
        }


        internal OTPMaster Select(int userID, string type)
        {
            List<OTPMaster> list = SelectAll();

            OTPMaster oTPMaster = list.FirstOrDefault(o => o.UserID == userID && o.Type == type && o.IsVerified == false);

            return oTPMaster;
        }

        private List<OTPMaster> SelectAll()
        {
            var _OTPMaster = new MemoryCacheProcessing<List<OTPMaster>>();

            List<OTPMaster> oTPMaster = _OTPMaster.GetOrCreate(key, () => SelectWithoutCache());

            return oTPMaster;
        }

        private List<OTPMaster> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return context.OTPMaster.OrderByDescending(o => o.DOC).ToList();
            }
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.OTPMaster.Where(U => U.DOCInt <= DeleteDOCInt);
                context.OTPMaster.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}