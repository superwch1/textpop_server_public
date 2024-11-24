using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace textpop_server.Services.LoginProviderValidation
{
    public class AppleValidation
    {
        private readonly HttpClient _HttpClient;

        public AppleValidation()
        {
            _HttpClient = new HttpClient();
        }


        /// <summary>
        /// Create a JwtToken token for authentication in Textpop
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Boolean</returns>
        public async Task<Tuple<bool, string, string>> ValidateAppleTokenAndGetInfo(string token)
        {
            try
            {
                // Get Apple's public keys
                var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync("https://appleid.apple.com/auth/keys");
                var keys = JObject.Parse(response)["keys"].ToObject<List<JObject>>();

                // Get the key used to sign the token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var key = keys!.First(x => x["kid"].ToString() == jwtToken.Header.Kid);

                // Create the parameters for token validation
                var tokenParams = new TokenValidationParameters()
                {
                    ValidIssuer = "https://appleid.apple.com",
                    ValidAudience = "com.wch.textpop.ios",
                    IssuerSigningKeys = new[] { new JsonWebKey(key.ToString()) },
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true
                };

                // Validate the token
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenParams, out _);

                var subClaim = jwtToken.Claims.First(claim => claim.Type == "sub").Value;
                var emailClaim = jwtToken.Claims.First(claim => claim.Type == "email").Value;

                return Tuple.Create(true, subClaim, emailClaim);
            }
            catch
            {
                return Tuple.Create(false, "", "");
            }
        }
    }
}
