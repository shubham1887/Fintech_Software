using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Project.Response.User;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Project.Processing
{
    public class TokenManager
    {
        private IConfiguration _config;

        public TokenManager(IConfiguration config)
        {
            _config = config;
        }

        private int getExpiryTime()
        {
            int expiryTime = 120;
            try
            {
                string time = _config["Jwt:ExpiryInMinute"];
                expiryTime = Convert.ToInt16(time);
            }
            catch (Exception ex)
            {
            }

            return expiryTime;
        }

        private string jwtKey(string key)
        {
            key = key.TrimIfNotNull();
            try
            {
                string appendString = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

                if (key.Length <= 31)
                {
                    key = key + appendString.Substring(0, 32 - key.Length);
                    return key;
                }

                if (key.Length >= 33)
                {
                    key = key.Substring(0, 32);
                    return key;
                }
            }
            catch (Exception ex)
            {
            }

            return key;
        }

        public string GenerateJSONWebToken(LoginResponse userInfo)
        {
            //"Jwt": {"Key": "0000@SecretKey",    "Issuer": "http://localhost.com/","ExpiryInMinute": "120",  },

            int ExpiryInMinute = getExpiryTime();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey(_config["Jwt:Key"])));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            string guid = Guid.NewGuid().ToString();
            var claims = new[] {
                            new Claim(JwtRegisteredClaimNames.Sub, EncryptJWT(userInfo.UserID.ToString(),guid)),
                            new Claim(ClaimTypes.Role, EncryptJWT(userInfo.UserTypeID.ToString(),guid)),
                            new Claim(JwtRegisteredClaimNames.Email, userInfo.MobileNumber),
                            new Claim(JwtRegisteredClaimNames.Jti,guid ) };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(ExpiryInMinute),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string EncryptJWT(string data, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged
            {
                Mode = CipherMode.ECB, //remember this parameter
                Padding = PaddingMode.ISO10126, //remember this parameter
                KeySize = 0x80,
                BlockSize = 0x80
            };
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;

            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }

            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(data);

            return Convert.ToBase64String
            (transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }

        public static string DecryptJWT(string data, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.ISO10126,
                KeySize = 0x80,
                BlockSize = 0x80
            };
            byte[] encryptedData = Convert.FromBase64String(data);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;

            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }

            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock
                        (encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(plainText);
        }
    }
}