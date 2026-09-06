using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Responses.Wallet;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserBalanceMasterRepository : IUserBalanceMasterRepository
    {
        private readonly ApplicationDbContext _context;


        public UserBalanceMasterRepository(ApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<int> Insert(UserBalanceMaster userBalanceMaster)
        {
            await _context.UserBalanceMaster.AddAsync(userBalanceMaster);
            var result = await _context.SaveChangesAsync();
            return result;
        }

        public async Task<int> Update(UserBalanceMaster userBalanceMaster)
        {
            _context.UserBalanceMaster.Update(userBalanceMaster);
            var result = await _context.SaveChangesAsync();
            return result;
        }
        public async Task<List<BackupMailWalletViewModel>> SelectAllUsersWallet()
        {
            return await _context.UserBalanceMaster
                .AsNoTracking()
                .Where(w =>  w.WalletMaster != null && w.WalletMaster.IsActive )
                .Select(w => new BackupMailWalletViewModel
                {
                    UserID = w.UserID,
                    WalletID = w.WalletID,
                    Name = w.WalletMaster!.Name,
                    ShortName = w.WalletMaster.ShortName,
                    Balance = (float)Math.Round(w.Balance, 5)
                })
                .ToListAsync();
        }
        public async Task<List<DetailViewModel>> SelectByUserID(int UserID)
        {


            return await _context.UserBalanceMaster
     .AsNoTracking()
     .Where(w => w.UserID == UserID
              && w.WalletMaster != null
              && w.WalletMaster.IsActive)
     .Select(w => new DetailViewModel
     {
         WalletID = w.WalletID,
         Name = w.WalletMaster!.Name,
         ShortName = w.WalletMaster!.ShortName,
         Balance = (float)Math.Round(w.Balance, 2)
     })
     .ToListAsync();

        }
        public async Task<UserBalanceMaster?> SelectByWalletIDAndUserID(UserBalanceMaster userBalanceMaster)
        {
            return await _context.UserBalanceMaster
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserID == userBalanceMaster.UserID &&
                                         w.WalletID == userBalanceMaster.WalletID);
        }
    }
}
