using System.Net;
using System.Text.Json;

namespace textpop_server.Services.LoginProviderValidation
{
    public class FacebookValidation
    {

        private readonly string _validateUrl = "https://graph.facebook.com/me?fields=id,name,email&access_token=";
        private readonly HttpClient _HttpClient;

        public FacebookValidation()
        {
            _HttpClient = new HttpClient();
        }

        /// <summary>
        /// Validate the facebook token and get the user information
        /// </summary>
        /// <param name="facebookToken"></param>
        /// <returns>Success, UserId, UserEmail</returns>
        public async Task<Tuple<bool, string?, string?>> ValidateFacebookTokenAndGetInfo(string facebookToken)
        {
            var response = await _HttpClient.GetAsync(_validateUrl + facebookToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseCotenet = await response.Content.ReadAsStringAsync();
                var facebookUser = JsonDocument.Parse(responseCotenet);

                var facebookUserId = facebookUser.RootElement.GetProperty("id").GetString();
                var facebookUserEmail = facebookUser.RootElement.GetProperty("email").GetString();

                return Tuple.Create<bool, string?, string?>(true, facebookUserId, facebookUserEmail);
            }
            return Tuple.Create<bool, string?, string?>(false, null, null);
        }
    }
}
