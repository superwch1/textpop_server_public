using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MessageWebServer.Services
{
    public class TextpopJwtToken
    {
        private readonly TokenValidationParameters _validationParameters;
        private readonly IConfiguration _config;

        public TextpopJwtToken(IConfiguration config)
        {
            _config = config;

            _validationParameters = new TokenValidationParameters()
            {
                ValidIssuer = config["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateLifetime = false,
            };
        }


        /// <summary>
        /// Create a JwtToken token for authentication in Message App
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <returns>JwtToken</returns>
        public string CreateToken(string userId, string userName, string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claim = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email)
            };

            var loginToken = new JwtSecurityToken(_config["Jwt:Issuer"], null, claim, DateTime.Now, null, credentials);
            return new JwtSecurityTokenHandler().WriteToken(loginToken);
        }
    }
}

