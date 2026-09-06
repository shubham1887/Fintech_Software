using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class StateDAL
    {
        internal List<StateMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<StateMaster>>();

            List<StateMaster> states = _list.GetOrCreate(CacheKeys.StateMaster, () => SelectWithoutCache());

            return states;
        }

        private List<StateMaster> SelectWithoutCache()
        {
            List<StateMaster> states;
            using (var context = new DatabaseContext())
            {
                states = context.StateMaster.OrderBy(S => S.Name).ToList();
            }
            return states;
        }

        internal StateMaster SelectByID(int StateID)
        {
            return Select().FirstOrDefault(T => T.ID == StateID);
        }

        internal int SelectByName(string Name)
        {
            return Select().FirstOrDefault(T => T.Name.Contains(Name)).ID;
        }
    }
}