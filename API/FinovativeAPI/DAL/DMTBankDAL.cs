using Project.Model;
using Project.Processing;
using Project.Response.DMTBank;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DMTBankDAL
    {
        public static readonly string CacheKey = "_DMTBank";

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
                list = (from dMTBank in context.DMTBank
                        where dMTBank.IsActive == true

                        select new ViewModel
                        {
                            Name = dMTBank.Name,
                            DefaultIFSC = dMTBank.DefaultIFSC,
                            IsIMPS = dMTBank.IsIMPS,
                        }).ToList();
            }

            return list;
        }

        internal List<BankViewModel> SelectAll()
        {
            using (var context = new DatabaseContext())
            {
                return (from bank in context.DMTBank

                        select new BankViewModel
                        {
                            Name = bank.Name,
                            DefaultIFSC = bank.DefaultIFSC,
                            IsActive = bank.IsActive,
                            IsIMPS = bank.IsIMPS,
                            DMTBankID = bank.DMTBankID,
                        }).ToList();
            }
        }

        internal DMTBank SelectByID(int DMTBankID)
        {
            using (var context = new DatabaseContext())
            {
                return context.DMTBank.Find(DMTBankID);
            }
        }

        internal DMTBank SelectByIFSC(string DefaultIFSC)
        {
            using (var context = new DatabaseContext())
            {
                return context.DMTBank.Find(DefaultIFSC);
            }
        }

        internal int Update(DMTBank Request)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.DMTBank.Update(Request);
                context.SaveChanges();
                return Request.DMTBankID;
            }
        }

        internal bool IsIMPS(string DefaultIFSC)
        {
            ViewModel item = Select().FirstOrDefault(X => X.DefaultIFSC == DefaultIFSC);

            if (item == null)
            {
                return true;
            }

            return item.IsIMPS;
        }

        internal bool IsIMPSByBankName(string BankName)
        {
            ViewModel item = Select().FirstOrDefault(X => X.Name.ToLower().Trim().Contains(BankName.ToLower().Trim()) || BankName.ToLower().Trim().Contains(X.Name.ToLower().Trim()));

            if (item == null)
            {
                return true;
            }

            return item.IsIMPS;
        }
    }
}