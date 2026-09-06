using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SupportUserAccessMasterDal
    {
        public static string CacheKey
        { get { return "_SupportUserAccessMaster"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<SupportUserAccessMaster>>();
            cache.Remove(CacheKey);
        }

        internal List<SupportUserAccessMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<SupportUserAccessMaster>>();

            List<SupportUserAccessMaster> list = _list.GetOrCreate(CacheKey, () => SelectWithoutCache());

            return list;
        }

        private List<SupportUserAccessMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.SupportUserAccessMaster.ToList();
        }

        internal List<SupportUserAccessMaster> SelectByUserID(int UserID)
        {
            return Select().Where(l => l.UserID == UserID).ToList();
        }

        internal int Insert(SupportUserAccessMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SupportUserAccessMaster.Add(objModel);
            context.SaveChanges();
            return objModel.SupportUserAccessMasterID;
        }

        internal int Update(SupportUserAccessMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SupportUserAccessMaster.Update(objModel);
            context.SaveChanges(); return objModel.SupportUserAccessMasterID;
        }

        internal SupportUserAccessMaster CheckDuplicate(int userID, string MenuGUID)
        {
            using var context = new DatabaseContext();
            return context.SupportUserAccessMaster.FirstOrDefault
                 (S => S.UserID == userID && S.MenuGUID == MenuGUID);
        }
    }
}