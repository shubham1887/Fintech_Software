using Project.Model;
using Project.Processing;
using Project.Response.MosambeeAEPSBank;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class MosambeeAEPSBankDal
    {
        public static string CacheKey
        { get { return "_MosambeeAEPSBank"; } }

        private void RemoveCache()
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
            using var context = new DatabaseContext();
            return (from bank in context.MosambeeAEPSBank
                    orderby bank.Sequence
                    select new ViewModel
                    {
                        ID = bank.ID,
                        BankName = bank.BankName,
                        IIN = bank.IIN
                    }).ToList();
        }
    }
}