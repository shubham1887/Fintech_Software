using FintechApp.Requests.WalletToWalletTransfer;
using FintechApp.Responses.WalletToWalletTransfer;

namespace FintechApp.Interfaces.Services
{
    public interface IWalletToWalletTransferService
    {
        // Transfers funds from the user's AEPS wallet to another of their wallets.
        Task<(string failureMessage, WalletTransferResponse? data)> AEPSToOther(AEPSToOtherRequest request, int currentUserID);

        // The general form: any of the caller's own wallets to any other of them.
        Task<(string failureMessage, WalletTransferResponse? data)> Transfer(WalletTransferRequest request, int currentUserID);

        // The caller's own wallets with balances — the From / To pickers on the transfer screen.
        Task<List<MyWalletResponse>> MyWallets(int currentUserID);

        /// <summary>
        /// The wallets an AEPS transfer may land in — the caller's own wallets minus the AEPS
        /// wallet itself. Offering AEPS as its own destination only earns the user a refusal.
        /// </summary>
        Task<List<MyWalletResponse>> AEPSDestinations(int currentUserID);
    }
}
