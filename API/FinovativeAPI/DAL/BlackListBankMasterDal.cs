using Project.Model;
using Project.Processing;
using Project.Response.BlackListBank;
using System.Collections.Generic;
using System.Linq;


namespace Project.DAL
{
    public class BlackListBankMasterDal
    {
        public static readonly string CacheKey = "_BlackListBankMaster";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<BlackListBankMaster>>();
            cache.Remove(CacheKey);
        }

        internal List<BlackListBankMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<BlackListBankMaster>>();

            List<BlackListBankMaster> list = _list.GetOrCreate(CacheKey, () => SelectWithoutCache());

            return list;
        }

        private List<BlackListBankMaster> SelectWithoutCache()
        {
            List<BlackListBankMaster> list;
            using (var context = new DatabaseContext())
            {
                list = (from BlackListBankMaster in context.BlackListBankMaster

                        select new BlackListBankMaster
                        {
                            BankName = BlackListBankMaster.BankName,
                            IFSC = BlackListBankMaster.IFSC,
                            AccountNumber = BlackListBankMaster.AccountNumber

                        }).ToList();
            }

            return list;
        }

        internal BlackListBankMaster SelectByIFSC(string IFSC, string BankName, string AccountNumber)
        {
            List<BlackListBankMaster> list = Select();
            BlackListBankMaster obj;

            obj = (from modal in list
                   where modal.IFSC == IFSC && modal.BankName == BankName && modal.AccountNumber == AccountNumber

                   select modal).FirstOrDefault();

            return obj;
        }

    }
}
