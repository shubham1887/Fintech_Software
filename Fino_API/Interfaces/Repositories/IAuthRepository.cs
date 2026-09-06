using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        Task<UserMaster?> GetUserByMobileNumber(string mobileNumber);
        Task<UserMaster?> GetUserById(int userId);
        Task LogWrongPassword(int userId, string wrongPassword, string ipAddress, string mobileNumber);
    }
}
