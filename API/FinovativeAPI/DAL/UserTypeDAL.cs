using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserTypeDAL
    {
        private static readonly string Key = "_UserTypeMaster";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<UserTypeMaster>>();
            cache.Remove(Key);
        }

        internal List<UserTypeMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserTypeMaster>>();

            List<UserTypeMaster> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<UserTypeMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.UserTypeMaster.Where(type => type.UserTypeID != (int)UserType.Developer && type.IsActive == true).ToList();
        }

        internal UserTypeMaster SelectByID(int UserTypeID)
        {
            using (var context = new DatabaseContext())
            {
                return context.UserTypeMaster.Find(UserTypeID);
            }
        }

        internal int Update(UserTypeMaster objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.UserTypeMaster.Update(objModel);
                return context.SaveChanges();
            }
        }
    }
}