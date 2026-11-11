using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Requests.Login;
using FintechApp.Responses.Login;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace FintechApp.Services
{
    public interface ITokenService
    {
        Task<TokenResponse?> RefreshToken(RefreshTokenRequest request, string ipAddress, string userAgent, string deviceType);
        Task<bool> RevokeToken(string refreshToken);
        Task<bool> RevokeAllUserTokens(int userId);
    }

    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IAuthRepository _authRepository;
        private readonly JwtTokenService _jwtTokenService;

        public TokenService(ITokenRepository tokenRepository, IAuthRepository authRepository, JwtTokenService jwtTokenService)
        {
            _tokenRepository = tokenRepository;
            _authRepository = authRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<TokenResponse?> RefreshToken(RefreshTokenRequest request, string ipAddress, string userAgent, string deviceType)
        {
            // Validate access token format (don't validate expiry)
            var jwtId = _jwtTokenService.GetJwtIdFromToken(request.AccessToken);
            var userId = _jwtTokenService.GetUserIdFromToken(request.AccessToken);

            if (string.IsNullOrEmpty(jwtId) || userId == null)
                return null;

            // Get refresh token from database
            var storedToken = await _tokenRepository.GetRefreshToken(request.RefreshToken);

            if (storedToken == null)
                return null;

            // Validate refresh token
            if (storedToken.IsUsed || storedToken.IsRevoked || storedToken.IsDeleted)
                return null;

            if (storedToken.ExpiryDateTime < DateTime.UtcNow)
                return null;

            if (storedToken.JwtId != jwtId)
                return null;

            // Mark old token as used
            await _tokenRepository.MarkTokenAsUsed(storedToken.RefreshTokenID);

            // Get user details
            var user = await _authRepository.GetUserById(userId.Value);
            if (user == null || !user.IsActive)
                return null;

            // Generate new tokens
            var tokenResponse = _jwtTokenService.GenerateJwtToken(
                user.UserMasterID,
                user.UserTypeID ?? 0,
                user.UserType?.UserType ?? string.Empty,
                user.MobileNumber,
                user.EmailID,
                user.SessionEpoch
            );

            // Store new refresh token
            await _tokenRepository.CreateRefreshToken(
                user.UserMasterID,
                tokenResponse.RefreshToken,
                tokenResponse.JwtId,
                ipAddress,
                userAgent,
                deviceType
            );

            return tokenResponse;
        }

        public async Task<bool> RevokeToken(string refreshToken)
        {
            return await _tokenRepository.RevokeRefreshToken(refreshToken);
        }

        public async Task<bool> RevokeAllUserTokens(int userId)
        {
            return await _tokenRepository.RevokeAllUserTokens(userId);
        }
    }
}
