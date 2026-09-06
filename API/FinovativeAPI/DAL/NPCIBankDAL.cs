using Project.Model;
using Project.Processing;
using Project.Response.NPCIBank;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class NPCIBankDAL
    {
        public static readonly string CacheKey = "_NPCIBank";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(CacheKey);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(CacheKey, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> list;
            using (var context = new DatabaseContext())
            {
                list = (from bank in context.NPCIBank
                        where bank.IsActive == true

                        select new ViewModel
                        {
                            BankName = bank.BankName,
                            IIN=bank.IIN,
                        }).ToList();
            }

            return list;
        }

    }
}
