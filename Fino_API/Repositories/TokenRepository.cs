using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationDbContext _context;

        public TokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> CreateRefreshToken(int userId, string token, string jwtId, string ipAddress, string userAgent, string deviceType)
        {
            var refreshToken = new RefreshToken
            {
                UserID = userId,
                Token = token,
                JwtId = jwtId,
                ExpiryDateTime = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
                IPAddress = ipAddress,
                UserAgent = userAgent,
                DeviceType = deviceType,
                IsUsed = false,
                IsRevoked = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshToken(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted);
        }

        public async Task<bool> RevokeRefreshToken(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted);

            if (refreshToken == null)
                return false;

            refreshToken.IsRevoked = true;
            refreshToken.UpdatedateTime = DateTime.UtcNow;

            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RevokeAllUserTokens(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserID == userId && !rt.IsRevoked && !rt.IsDeleted)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.UpdatedateTime = DateTime.UtcNow;
            }

            _context.RefreshTokens.UpdateRange(tokens);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkTokenAsUsed(long refreshTokenId)
        {
            var token = await _context.RefreshTokens.FindAsync(refreshTokenId);
            if (token == null)
                return false;

            token.IsUsed = true;
            token.UpdatedateTime = DateTime.UtcNow;

            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
