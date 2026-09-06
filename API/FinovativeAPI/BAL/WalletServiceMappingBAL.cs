using Project.DAL;
using Project.Response.Wallet;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.BAL
{
    public class WalletServiceMappingBAL
    {
        internal int SelectWalletIDByServiceID(int ServiceID)
        {
            return new WalletServiceMappingDAL().SelectWalletIDByServiceID(ServiceID);
        }

        internal List<ServicesWiseWalletViewModel> SelectServiceWiseWallet(out string failureMessage)
        {
            failureMessage = StatusType.Ok;
            return new WalletServiceMappingDAL().SelectServiceWiseWallet();
        }
    }
}