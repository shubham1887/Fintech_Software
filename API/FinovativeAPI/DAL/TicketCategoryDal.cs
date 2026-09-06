using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class TicketCategoryDal
    {
        private readonly string key = "_TicketCategory";

        internal List<TicketCategory> Select()
        {
            var _list = new MemoryCacheProcessing<List<TicketCategory>>();

            List<TicketCategory> category = _list.GetOrCreate(key, () => SelectWithoutCache());

            return category;
        }

        private List<TicketCategory> SelectWithoutCache()
        {
            List<TicketCategory> states;
            using (var context = new DatabaseContext())
            {
                states = context.TicketCategory.OrderBy(S => S.TicketCategoryID).ToList();
            }
            return states;
        }
    }
}