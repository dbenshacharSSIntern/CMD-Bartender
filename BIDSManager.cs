using Barrista.Models;
using BasicAuthLogon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace PasswordBasedAuthLogon
{
    internal class BIDSManager
    {

        public static RestInfo GetTokenInfo(Alius userInfo)
        {
            App app = new App();
            RestInfo restInfo = new RestInfo();
            TokenInfo accessToken = app.GetToken(userInfo);
            string dataCenterURI = ExtractDataCenterURI(accessToken.access_token);

            restInfo.accessToken = accessToken;
            restInfo.dataCenterURI = dataCenterURI;
            return restInfo;
        }

        public class RestInfo
        {
            public TokenInfo accessToken { get; set; }
            public string dataCenterURI { get; set; }
        }

        static string ExtractDataCenterURI(string accessToken)
        {
            DecodedToken decodedToken = AccessTokenDecoder.Decode(accessToken);

            if (decodedToken.Payload.ContainsKey(AccessTokenDecoder.DataCenterClaim))
                return decodedToken.Payload[AccessTokenDecoder.DataCenterClaim] as string;
            throw new Exception("DataCenterURI missing");
        }
        static string ExtractIssuer(string accessToken)
        {
            DecodedToken decodedToken = AccessTokenDecoder.Decode(accessToken);

            if (decodedToken.Payload.ContainsKey("iss"))
                return decodedToken.Payload["iss"] as string;
            throw new Exception("iss missing");
        }

        static async Task<SpacesCollection> RetrieveSpaces(string dataCenterUri, TokenInfo tokenInfo)
        {
            string GetSpaces_Url = "api/librarian/spaces?includeHidden={0}";
            bool includeHidden = false;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.BaseAddress = new Uri(dataCenterUri);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenInfo.access_token}");

            try
            {
                string requestUrl = string.Format(GetSpaces_Url, includeHidden);
                HttpResponseMessage msg = await httpClient.GetAsync(requestUrl);

                if (msg.IsSuccessStatusCode)
                {
                    string json = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SpacesCollection>(json) ?? new SpacesCollection();
                }
                else
                    throw new Exception(msg.ReasonPhrase);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    class App
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        public string ClaimsIssuer { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public bool BIDS { get; set; } = true;
        public string CloudCluster { get; set; } = "https://am1.bartendercloud.com";

        public TokenInfo GetToken(Alius userInfo)
        {
            HttpStatusCode code;
            string errorMessage;
            TokenInfo token = null;

            RetrieveAuthenticationConfiguration(CloudCluster);

            HttpClient client = new HttpClient();
            Uri uri = new Uri(TokenEndpoint);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.BaseAddress = uri;
            var contentBodyList = new List<KeyValuePair<string, string>>();
            contentBodyList.Add(new KeyValuePair<string, string>("grant_type", "password"));
            contentBodyList.Add(new KeyValuePair<string, string>("username", userInfo.Email));
            contentBodyList.Add(new KeyValuePair<string, string>("password", userInfo.Password));
            contentBodyList.Add(new KeyValuePair<string, string>("client_id", userInfo.ApplicationID));
            contentBodyList.Add(new KeyValuePair<string, string>("client_secret", userInfo.SecretID));
            contentBodyList.Add(new KeyValuePair<string, string>("audience", "https://BarTenderCloudServiceApi"));
            contentBodyList.Add(new KeyValuePair<string, string>("scope", "openid profile BarTenderServiceApi"));

            var request = new HttpRequestMessage(HttpMethod.Post, $"?OrganizationDnsName={userInfo.OrganizationDNSName}") { Content = new FormUrlEncodedContent(contentBodyList) };
            HttpResponseMessage msg = client.SendAsync(request).Result;

            code = msg.StatusCode;
            if (msg.IsSuccessStatusCode)
            {
                var resultPayload = msg.Content.ReadAsStringAsync().Result;
                token = JsonConvert.DeserializeObject<TokenInfo>(resultPayload);
                errorMessage = string.Empty;
            }
            else
            {
                errorMessage = msg.Content?.ReadAsStringAsync()?.Result;
            }

            return token;
        }

        void RetrieveAuthenticationConfiguration(string barTenderCloudCluster)
        {
            HttpClient httpClient = new HttpClient();
            string url = barTenderCloudCluster + (!barTenderCloudCluster.EndsWith('/') ? "/" : "") + ".well-known/openid-configuration";
            HttpResponseMessage msg = httpClient.GetAsync(url).Result;
            if (!msg.IsSuccessStatusCode)
                throw new Exception("Failure to retrieve OIDC discovery document");

            string result = msg.Content.ReadAsStringAsync().Result;
            IDictionary<string, object> dictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(result)!;
            var i = (string)dictionary["issuer"];
            ClaimsIssuer = i.EndsWith('/') ? i.TrimEnd(new char[] { '/' }) : i;
            i = (string)dictionary["token_endpoint"];
            TokenEndpoint = i.EndsWith('/') ? i.TrimEnd(new char[] { '/' }) : i;
        }
    }
    class TokenInfo
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; } = 0;
        public string token_type { get; set; }
    }

    public class DecodedToken
    {
        public Dictionary<string, string> Header { get; set; } = new();
        public Dictionary<string, object> Payload { get; set; } = new();
        public string Signature { get; set; } = String.Empty;
    }

    /// <summary>
    /// Decode an access token
    /// </summary>
    public static class AccessTokenDecoder
    {
        public static string TenantIDClaim = "https://BarTenderCloud.com/TenantID";
        public static string UserIDClaim = "https://BarTenderCloud.com/UserID";
        public static string DataCenterClaim = "https://BarTenderCloud.com/DataCenterURI";

        /// <summary>
        /// Decode an access token (NOTE: Does not validate the token signature)
        /// </summary>
        /// <param name="encodedToken">access token to validate</param>
        /// <returns>DecodedToken</returns>
        /// <throws>Exception on invalid token</throws>
        public static DecodedToken Decode(string encodedToken)
        {
            DecodedToken decodedToken = new();

            try
            {
                // Split the supplied access token into it's three parts
                string[] parts = encodedToken.Split('.');
                if (parts.Length != 3)
                    throw new Exception("Invalid Access Token. Incorrect number of parts");

                string header = DecodeBase64(parts[0]);
                string payload = DecodeBase64(parts[1]);
                string signature = DecodeBase64(parts[2]);

                var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);
                if (headerDictionary == null)
                    throw new Exception("Invalid Access Token. Header empty");

                var payloadDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);
                if (payloadDictionary == null)
                    throw new Exception("Invalid Access Token. Payload empty");

                // Validate the token expiration
                if (!payloadDictionary!.ContainsKey("exp"))
                    throw new Exception("Invalid Access Token. Expiration missing");

                decodedToken.Signature = signature;
                decodedToken.Header = headerDictionary;
                decodedToken.Payload = payloadDictionary;
            }
            catch
            {
                throw;
            }

            return decodedToken;
        }

        // Decode a Base64 string to a string
        private static string DecodeBase64(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            value = value.Replace(' ', '+').Replace('-', '+').Replace('_', '/').PadRight(4 * ((value.Length + 3) / 4), '=');

            var valueBytes = System.Convert.FromBase64String(value);
            return System.Text.Encoding.UTF8.GetString(valueBytes);
        }
    }
}