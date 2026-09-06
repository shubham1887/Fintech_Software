using FintechApp.Entities;

namespace FintechApp.Interfaces.Services
{
    public interface IWalletMasterService
    {
        Task<List<WalletMaster>> Select();
        Task<WalletMaster> SelectByID(int WalletID);

        // All wallets incl. inactive (admin management view).
        Task<List<WalletMaster>> SelectAll();

        // Admin can rename a wallet and enable/disable it (no create/delete).
        // Field-level update: fieldName in { "name", "isactive" }.
        Task<string> UpdateField(int walletID, string fieldName, string newValue);
    }
}
