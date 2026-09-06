using Project.Model;
using Project.Processing;
using Project.Response.GSTState;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ZoneMasterDAL
    {
        internal List<ZoneMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<ZoneMaster>>();

            List<ZoneMaster> states = _list.GetOrCreate(CacheKeys.ZoneMaster, () => SelectWithoutCache());

            return states;
        }

        private List<ZoneMaster> SelectWithoutCache()
        {
            List<ZoneMaster> states;
            using (var context = new DatabaseContext())
            {
                states = context.ZoneMaster.OrderBy(S => S.Name).ToList();
            }
            return states;
        }

        internal ZoneMaster SelectByID(int StateID)
        {
            return Select().FirstOrDefault(T => T.ZoneMasterID == StateID);
        }

        internal int SelectByName(string Name)
        {
            return Select().FirstOrDefault(T => T.Name.Contains(Name)).ZoneMasterID;
        }

        public List<ZoneViewModel> SelectAll()
        {
            using (var context = new DatabaseContext())
            {
                return (from menu in context.ZoneMaster

                        select new ZoneViewModel
                        {
                            ID = menu.ZoneMasterID,
                            Name = menu.Name,
                        }).ToList();
            }
        }
    }
}