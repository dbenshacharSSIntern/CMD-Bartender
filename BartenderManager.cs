using BasicAuthLogon.Models;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using PasswordBasedAuthLogon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using static BasicAuthLogon.AccessTokenDecoder;
using System.Threading.Tasks;

namespace BasicAuthLogon
{
    internal static class BartenderManager
    {
        private static HttpClient Client = new HttpClient();
        private static TokenInfo AccessToken;
        private static App Application = new App();
        private static string Website = GlobalConfigManager.GetWebsite();
        private static Uri URI = new Uri(Website);

        public static void Initalize() {
            try
            {
                Client.BaseAddress = URI;
                HttpResponseMessage code = Client.GetAsync(Website).Result;
                AccessToken = Application.GetToken(code.StatusCode);
                Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");
            } catch {
                throw new Exception("Website connection failed.");
            }
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

    class App
    {
        public static string ClaimsIssuer { get; set; } = string.Empty;

        public TokenInfo GetToken(HttpStatusCode code)
        {
            TokenInfo token = null;

            Console.WriteLine("Please enter information about an Native application created\nwith grant_type:\"password refresh_token\"\n");

            string clientId;     // BarTenderCloud app client-id
            string clientSecret; // BarTenderCloud app client-secret
            string username;
            string password;

            do
            {
                // 
                Console.Write("BarTenderCloud Cluster (e.g. https://am1.bartendercloud.com) : ");
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    try
                    {
                        RetrieveAuthenticationConfiguration(input);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to retrieve OIDC document. Ex={ex.Message}");
                        continue;
                    }
                }
            } while (true);

            do
            {
                Console.Write("Application Client Id: ");
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    clientId = input;
                    break;
                }
            } while (true);

            do
            {
                Console.Write("Application Client Secret: ");
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    clientSecret = input;
                    break;
                }
            } while (true);

            do
            {
                Console.Write("Username: ");
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    username = input;
                    break;
                }
            } while (true);

            do
            {
                Console.Write("Password: ");
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    password = input;
                    break;
                }
            } while (true);

            HttpClient client = new HttpClient();
            Uri uri = new Uri(ClaimsIssuer);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.BaseAddress = uri;
            var contentBodyList = new List<KeyValuePair<string, string>>();
            contentBodyList.Add(new KeyValuePair<string, string>("grant_type", "password"));
            contentBodyList.Add(new KeyValuePair<string, string>("username", username));
            contentBodyList.Add(new KeyValuePair<string, string>("password", password));
            contentBodyList.Add(new KeyValuePair<string, string>("client_id", clientId));
            contentBodyList.Add(new KeyValuePair<string, string>("client_string", clientSecret));
            contentBodyList.Add(new KeyValuePair<string, string>("audience", "https://BarTenderCloudServiceApi"));
            contentBodyList.Add(new KeyValuePair<string, string>("scope", "openid profile user:query offline_access"));

            var request = new HttpRequestMessage(HttpMethod.Post, "oauth/token") { Content = new FormUrlEncodedContent(contentBodyList) };
            HttpResponseMessage msg = client.SendAsync(request).Result;

            code = msg.StatusCode;
            if (msg.IsSuccessStatusCode)
            {
                var resultPayload = msg.Content.ReadAsStringAsync().Result;
                token = JsonConvert.DeserializeObject<TokenInfo>(resultPayload);
            }

            return token;
        }

        static void RetrieveAuthenticationConfiguration(string barTenderCloudCluster)
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
        }
    }

    /// <summary>
    /// Decode an access token
    /// </summary>
    internal static class AccessTokenDecoder
    {
        public static string TenantIDClaim = "https://BarTenderCloud.com/TenantID";
        public static string UserIDClaim = "https://BarTenderCloud.com/UserID";
        public static string DataCenterClaim = "https://BarTenderCloud.com/DataCenterURI";

        public class DecodedToken
        {
            public Dictionary<string, string> Header { get; set; } = new();
            public Dictionary<string, object> Payload { get; set; } = new();
            public string Signature { get; set; } = String.Empty;
        }

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