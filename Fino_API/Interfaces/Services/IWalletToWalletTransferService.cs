using FintechApp.Requests.WalletToWalletTransfer;
using FintechApp.Responses.WalletToWalletTransfer;

namespace FintechApp.Interfaces.Services
{
    public interface IWalletToWalletTransferService
    {
        // Transfers funds from the user's AEPS wallet to another of their wallets.
        Task<(string failureMessage, WalletTransferResponse? data)> AEPSToOther(AEPSToOtherRequest request, int currentUserID);
    }
}
