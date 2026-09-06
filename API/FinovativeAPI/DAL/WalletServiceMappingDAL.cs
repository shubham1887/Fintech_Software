using Project.Model;
using Project.Processing;
using Project.Response.Wallet;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class WalletServiceMappingDAL
    {
        public static readonly string key = "_WalletServiceMapping";

        private List<WalletServiceMapping> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<WalletServiceMapping>>();

            List<WalletServiceMapping> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        internal List<ServicesWiseWalletViewModel> SelectServiceWiseWallet()
        {
            List<ServicesWiseWalletViewModel> list;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from service in context.ServiceMaster
                        join walletMapping in context.WalletServiceMapping on service.ServiceID equals walletMapping.ServiceID into Details
                        from m in Details.DefaultIfEmpty()
                        where service.ServiceID != 1
                        orderby m.WalletID
                        select new ServicesWiseWalletViewModel
                        {
                            WalletName = m.WalletMaster.Name,
                            ServiceName = service.Name,
                        }).ToList();
            }

            foreach (ServicesWiseWalletViewModel res in list)
            {
                if (res.WalletName.EqualsIgnoreCase(""))
                {
                    res.WalletName = "Not Mapped";
                }
            }

            return list;
        }

        private List<WalletServiceMapping> SelectWithoutCache()
        {
            List<WalletServiceMapping> list;
            using (var context = new DatabaseContext())
            {
                list = (from service in context.WalletServiceMapping
                        select service).ToList();
            }
            return list;
        }

        internal int SelectWalletIDByServiceID(int ServiceID)
        {
            List<WalletServiceMapping> list = SelectAll();

            WalletServiceMapping obj = (from service in list
                                        where service.ServiceID == ServiceID
                                        select service).FirstOrDefault();

            if (obj == null)
            {
                return -1;
            }
            return obj.WalletID;
        }
    }
}