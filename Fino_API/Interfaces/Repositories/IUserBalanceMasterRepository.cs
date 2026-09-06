using FintechApp.Entities;
using FintechApp.Responses.Wallet;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserBalanceMasterRepository
    {
        Task<int> Insert(UserBalanceMaster userBalanceMaster);
        Task<int> Update(UserBalanceMaster userBalanceMaster);
        Task<List<BackupMailWalletViewModel>> SelectAllUsersWallet();
        Task<List<DetailViewModel>> SelectByUserID(int UserID);
        Task<UserBalanceMaster?> SelectByWalletIDAndUserID(UserBalanceMaster userBalanceMaster);
    }
}
