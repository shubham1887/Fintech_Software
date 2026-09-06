using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class WalletMasterDal
    {
        internal List<WalletMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<WalletMaster>>();

            List<WalletMaster> list = _list.GetOrCreate(CacheKeys.WalletMaster, () => SelectWithoutCache());

            return list;
        }

        private List<WalletMaster> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from item in context.WalletMaster
                        where item.IsActive == true
                        orderby item.UISequence
                        select item
                         ).ToList();
            }
        }

        internal List<WalletMaster> SelectForFundTransfer()
        {
            List<WalletMaster> list = Select();

            return (from item in list
                    where item.IsValidForTransfer == true
                    select item).ToList();
        }

        internal List<WalletMaster> SelectForUserCreation()
        {
            List<WalletMaster> list = Select();

            return (from item in list
                    where item.IsValidUserCreation == true
                    select item).ToList();
        }

        internal List<WalletMaster> SelectForPaymentGateway()
        {
            List<WalletMaster> 
                list = Select();

            return (from item in list
                    where item.IsActiveForPaymentGateway == true
                    select item).ToList();
        }

        internal WalletMaster SelectByID(int WalletID)
        {
            List<WalletMaster> list = Select();

            WalletMaster item = list.FirstOrDefault(C => C.WalletID == WalletID);

            return item;
        }

        internal List<WalletMaster> SelectRechargeAndDMT()
        {
            List<WalletMaster> list = Select();
            return (from item in list
                    where item.WalletID != (int)WalletCode.AEPS
                    && item.IsActive == true

                    orderby item.UISequence
                    select item
                          ).ToList();
        }

        internal List<WalletMaster> SelectForStaticQR()
        {
            List<WalletMaster> list = Select();

            return (from item in list
                    where item.IsActiveForStaticQR == true
                    select item).ToList();
        }

        internal List<WalletMaster> SelectForeCollection()
        {
            List<WalletMaster> list = Select();

            return (from item in list
                    where item.IsActiveForeCollection == true
                    select item).ToList();
        }
    }
}