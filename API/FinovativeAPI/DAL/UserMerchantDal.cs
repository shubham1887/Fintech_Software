using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserMerchantDal
    {
        private readonly string UserMasterKEY = "_UserMerchantMaster";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserMerchantMaster>>();
            cache.Remove(UserMasterKEY);
        }

        internal List<UserMerchantMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserMerchantMaster>>();
            List<UserMerchantMaster> list = _list.GetOrCreate(UserMasterKEY, () => SelectWithoutCache());
            return list;
        }

        private List<UserMerchantMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();

            return context.UserMerchantMaster.ToList();
        }

        internal int Update(UserMerchantMaster ObjModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.UserMerchantMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.UserMerchantMasterID;
        }

        internal int Insert(UserMerchantMaster ObjModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.UserMerchantMaster.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.UserMerchantMasterID;
        }

        internal UserMerchantMaster SelectByID(int UserID)
        {
            return Select().FirstOrDefault(U => U.UserID == UserID);
        }

        internal UserMerchantMaster SelectByRefID(string UPIRefid)
        {
            return Select().FirstOrDefault(U => U.PaySprintUPIRefid == UPIRefid);
        }
    }
}