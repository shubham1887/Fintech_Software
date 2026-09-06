using FintechApp.Processing;

using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Responses.Wallet;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    public class UserBalanceMasterService
    {
        private readonly IUserBalanceMasterRepository _userBalanceMasterRepository;

        public UserBalanceMasterService(IUserBalanceMasterRepository userBalanceMasterRepository)
        {
            _userBalanceMasterRepository = userBalanceMasterRepository;
        }
        internal async Task<UserBalanceMaster?> SelectByWalletIDAndUserID(int userId, int walletId)
        {
            using (ConcurrencyControl.AcquireLock(userId, walletId.ToString()))
            {
                var userWallet =await _userBalanceMasterRepository.SelectByWalletIDAndUserID(new UserBalanceMaster
                {
                    UserID = userId,
                    WalletID = walletId,
                });

                if (userWallet == null)
                    return null;

                return userWallet;
            }
        }
        internal async Task<(List<DetailViewModel>,string)> SelectByUserID(int UserID)
        {
        

            List<DetailViewModel> wallet = await _userBalanceMasterRepository.SelectByUserID(UserID);

            if (wallet == null || wallet.Count <= 0)
                return (null, WalletMessages.NotFound);

            return (wallet, StatusType.Ok);
        }

        public async Task<int> UpdateBalanceByConfirmation(int UserID, string Balance)
        {
            decimal clBal = decimal.Parse(Balance);
            clBal = decimal.Round(clBal, 5);
            decimal formattedClBal = decimal.Parse(clBal.ToString("0.00000"));
            UserBalanceMaster UserWallet = await SelectByWalletIDAndUserID(UserID, (int)WalletCode.Recharge);
            UserWallet.Balance = formattedClBal;
          
            try
            {
                int Result = await _userBalanceMasterRepository.Update(UserWallet);
                if (Result <= 0)
                {
                    return Result;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return 1;
        }
    }
}
