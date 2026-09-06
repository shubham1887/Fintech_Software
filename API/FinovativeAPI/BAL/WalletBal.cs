using Project.DAL;
using Project.Model;
using System.Collections.Generic;

namespace Project.BAL
{
    public class WalletBal
    {
        public List<WalletMaster> Select()
        {
            return new WalletMasterDal().Select();
        }

        internal List<WalletMaster> SelectForPaymentGateway()
        {
            return new WalletMasterDal().SelectForPaymentGateway();
        }

        internal List<WalletMaster> SelectForFundTransfer()
        {
            return new WalletMasterDal().SelectForFundTransfer();
        }

        internal List<WalletMaster> SelectForUserCreation()
        {
            return new WalletMasterDal().SelectForUserCreation();
        }

        internal WalletMaster SelectByID(int walletID)
        {
            return new WalletMasterDal().SelectByID(walletID);
        }

        internal List<WalletMaster> SelectRechargeAndDMT()
        {
            return new WalletMasterDal().SelectRechargeAndDMT();
        }

        internal List<WalletMaster> SelectForStaticQR()
        {
            return new WalletMasterDal().SelectForStaticQR();
        }

        internal List<WalletMaster> SelectForeCollection()
        {
            return new WalletMasterDal().SelectForeCollection();
        }
    }
}