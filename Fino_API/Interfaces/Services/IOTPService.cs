namespace FintechApp.Interfaces.Services
{
    public interface IOTPService
    {
        Task<string> SendOtp(int userId, string type);
    }
}
