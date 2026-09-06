using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class TargetSaleCommissionDal
    {
        public static readonly string Key = "_TargetSaleCommission";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<TargetSaleCommission>>();
            cache.Remove(Key);
        }

        internal List<TargetSaleCommission> Select()
        {
            var _list = new MemoryCacheProcessing<List<TargetSaleCommission>>();

            List<TargetSaleCommission> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<TargetSaleCommission> SelectWithoutCache()
        {
            List<TargetSaleCommission> list;
            using (var context = new DatabaseContext())
            {
                list = context.TargetSaleCommission.ToList();
            }
            return list;
        }

        internal int Insert(TargetSaleCommission objModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.TargetSaleCommission.Add(objModel);
            context.SaveChanges();
            return objModel.TargetSaleCommissionID;
        }

        internal TargetSaleCommission SelectByIDAndMonth(string SaleMonthDOC, int UserID, int WalletID)
        {
            using DatabaseContext context = new DatabaseContext();
            return context.TargetSaleCommission.FirstOrDefault(s => s.SaleMonthDOC == SaleMonthDOC && s.UserID == UserID && s.WalletID == WalletID);
        }
    }
}