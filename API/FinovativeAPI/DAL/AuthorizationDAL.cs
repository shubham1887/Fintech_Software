using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class AuthorizationDAL
    {
        public readonly string key = "_AuthorizationMaster";

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<AuthorizationMaster>>();
            cache.Remove(key);
        }

        internal List<AuthorizationMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<AuthorizationMaster>>();

            List<AuthorizationMaster> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<AuthorizationMaster> SelectWithoutCache()
        {
            List<AuthorizationMaster> list;
            using (var context = new DatabaseContext())
            {
                list = context.AuthorizationMaster.OrderBy(S => S.DOC).ToList();
            }
            return list;
        }
    }
}