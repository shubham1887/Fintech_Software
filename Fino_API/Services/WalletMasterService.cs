using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class WalletMasterService : IWalletMasterService
    {
        private readonly IWalletMasterRepository _walletMasterRepository;
        public WalletMasterService(IWalletMasterRepository walletMasterRepository) { 
        
        _walletMasterRepository = walletMasterRepository;
        }
        public async Task<List<WalletMaster>> Select()
        {
            return await _walletMasterRepository.Select();
        }

        public async Task<WalletMaster> SelectByID(int WalletID)
        {
            return await _walletMasterRepository.SelectByID(WalletID);
        }

        public async Task<List<WalletMaster>> SelectAll()
        {
            return await _walletMasterRepository.SelectAll();
        }

        public async Task<string> UpdateField(int walletID, string fieldName, string newValue)
        {
            var wallet = await _walletMasterRepository.GetForUpdate(walletID);
            if (wallet == null) return WalletMessages.NotFound;

            switch ((fieldName ?? string.Empty).ToLower())
            {
                case "name":
                    if (string.IsNullOrWhiteSpace(newValue)) return WalletMessages.NameRequired;
                    wallet.Name = newValue.Trim();
                    break;
                case "isactive":
                    if (!bool.TryParse(newValue, out bool isActive)) return WalletMessages.NotFound;
                    wallet.IsActive = isActive;
                    break;
                default:
                    return WalletMessages.NotFound;
            }

            int result = await _walletMasterRepository.Update(wallet);
            return result > 0 ? StatusType.Ok : WalletMessages.NotFound;
        }
    }
}
