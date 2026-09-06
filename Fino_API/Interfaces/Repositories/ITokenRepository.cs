using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ITokenRepository
    {
        Task<RefreshToken> CreateRefreshToken(int userId, string token, string jwtId, string ipAddress, string userAgent, string deviceType);
        Task<RefreshToken?> GetRefreshToken(string token);
        Task<bool> RevokeRefreshToken(string token);
        Task<bool> RevokeAllUserTokens(int userId);
        Task<bool> MarkTokenAsUsed(long refreshTokenId);
    }
}
