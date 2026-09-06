using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISenderOTPRepository
    {
        Task<List<SenderOTP>> GetAll();
        Task<SenderOTP?> GetByID(int id);
        Task<List<SenderOTP>> GetByUserID(int userid);
        Task<int> Insert(SenderOTP entity);
        Task<int> Update(SenderOTP entity);
        Task<int> Delete(int id);
    }
}
