using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IWalletMasterRepository
    {
        Task<List<WalletMaster>> Select();
        Task<WalletMaster> SelectByID(int WalletID);

        // All wallets incl. inactive (admin management view). Select() only returns active ones.
        Task<List<WalletMaster>> SelectAll();

        // Direct DB fetch (bypasses the active-only cache) so admin can edit/disable any wallet.
        Task<WalletMaster?> GetForUpdate(int WalletID);
        Task<int> Update(WalletMaster wallet);
    }
}
