using FintechApp.Responses;
using FintechApp.Responses.Login;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FintechApp.Helpers
{
    public class JwtTokenService
    {
        /// <summary>
        /// Claim carrying UserMaster.SessionEpoch - the generation of sessions this token belongs
        /// to. Named here rather than spelled out at each use so the minting and the checking side
        /// cannot drift apart; a typo on one side would silently accept every revoked token.
        /// </summary>
        public const string SessionEpochClaim = "sepoch";

        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Mint an access + refresh token pair for a user.
        ///
        /// <paramref name="sessionEpoch"/> is UserMaster.SessionEpoch as it stands right now. It
        /// is stamped into the token and re-checked on every request, which is what makes a
        /// signed-out access token stop working instead of living out its configured lifetime -
        /// a day, on a deployment using the shipped settings.
        /// </summary>
        public TokenResponse GenerateJwtToken(int userId, int userTypeId, string userType, string mobileNumber, string email, int sessionEpoch = 0)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "FintechApp";
            var audience = jwtSettings["Audience"] ?? "FintechAppUsers";
            var expiryMinutes = ResolveExpiryMinutes(jwtSettings);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtId = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("mobile", mobileNumber),
                new Claim("userTypeId", userTypeId.ToString()),
                new Claim("userType", userType),
                new Claim(SessionEpochClaim, sessionEpoch.ToString()),
                new Claim(ClaimTypes.Role, userType)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = GenerateRefreshToken(),
                ExpiresIn = expiryMinutes * 60,
                TokenType = "Bearer",
                JwtId = jwtId
            };
        }

        /// <summary>
        /// How long an access token lives, in minutes.
        ///
        /// Every appsettings this product ships - the base file, the installer's template, and the
        /// live site's - writes <c>ExpiryInMinutes</c>. This read asked for <c>ExpiryMinutes</c>,
        /// which no config has ever contained, so the lookup always missed and every deployment
        /// silently ran on the 60-minute fallback while its own configuration said 1440. Nobody
        /// could have found that by reading the settings file.
        ///
        /// Both spellings are accepted because a white-label copy may carry either, and the
        /// configured one wins over the fallback.
        ///
        /// TryParse rather than Parse: a typo in this field would otherwise throw inside token
        /// minting, which is to say nobody on that deployment could sign in at all. A value that
        /// is not a positive number falls back instead, and a session that is an hour short is a
        /// far better failure than a platform that cannot issue one.
        /// </summary>
        private static int ResolveExpiryMinutes(IConfigurationSection jwtSettings)
        {
            foreach (string key in new[] { "ExpiryInMinutes", "ExpiryMinutes" })
            {
                if (int.TryParse(jwtSettings[key], out int minutes) && minutes > 0)
                    return minutes;
            }
            return DefaultExpiryMinutes;
        }

        /// <summary>Used only when the configuration names no usable lifetime.</summary>
        private const int DefaultExpiryMinutes = 60;

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

                var tokenHandler = new JwtSecurityTokenHandler();
                // Preserve original JWT claim names (e.g. "sub") instead of mapping them to long
                // WS-* URIs, so JwtAuthorization can read JwtRegisteredClaimNames.Sub / userTypeId.
                tokenHandler.InboundClaimTypeMap.Clear();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string? GetJwtIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            }
            catch
            {
                return null;
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                return userIdClaim != null ? int.Parse(userIdClaim) : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
