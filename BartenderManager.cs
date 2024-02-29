using Barrista.Models;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using PasswordBasedAuthLogon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using static BasicAuthLogon.AccessTokenDecoder;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using System.Collections.Immutable;

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

        public static void PrintDir() {
            var id = GetFolder(AccessToken, GlobalConfigManager.GetDirectoryEntry()).Result.Id;

            var result = DisplayFolderDir(id, AccessToken).Result;
            if (result == null)
            {
                Console.WriteLine("Wawfawe");
            }
            //temporary method to display dir
            var folder = result.Folder;
            Console.WriteLine(folder.Name);
            for (int i = 0; i < result.Subfolders.Count; i++)
            {
                Console.WriteLine("\tFolder: " + result.Subfolders[i].Name);
            }
            for (int i = 0; i < result.Files.Count; i++)
            {
                Console.WriteLine("\tFile: " + result.Files[i].Name);
            }

        }

        // Method to Extract URI from Access Token
        static string ExtractDataCenterURI(string accessToken)
        {
            DecodedToken decodedToken = AccessTokenDecoder.Decode(accessToken);

            if (decodedToken.Payload.ContainsKey(DataCenterClaim))
                return decodedToken.Payload[AccessTokenDecoder.DataCenterClaim] as string;
            throw new Exception("DataCenterURI missing");

        }
        static async Task<Items> DisplayFolderDir(string folderId, TokenInfo accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.access_token}");

            var itemsRequest = new ItemsRequest()
            {
                FoldersLimit = 100,        // Read folders in batches of 100.
                FilesLimit = 100,        // Read files in batches of 100.
                FoldersSkip = 0,        // The first read of the folder starts at the beginning.
                FilesSkip = 0,            // The first read of the files starts at the beginning.
                IncludeHidden = false,    // Do not include hidden folders or files.
                IncludeDeleted = false    // Do not include deleted folders or files.
            };

            FolderNoChildren targetFolder = null;
            var files = new List<Barrista.Models.File>();
            var subfolders = new List<FolderNoChildren>();
            do
            {

                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://am1.development.bartendercloud.com/api/librarian/items/{folderId}"),
                    Content = new StringContent(JsonConvert.SerializeObject(itemsRequest), Encoding.UTF8, "application/json"),
                    Method = HttpMethod.Post
                };

                HttpResponseMessage msg = await client.SendAsync(request);

                if (msg.IsSuccessStatusCode)
                {

                    {
                        var items = JsonConvert.DeserializeObject<Items>(await msg.Content.ReadAsStringAsync());

                        targetFolder = items.Folder;
                        files.AddRange(items.Files);
                        subfolders.AddRange(items.Subfolders);

                        if (items.MoreItemsToGet)
                            itemsRequest = items.NextItemsRequest;
                        else
                            Console.WriteLine(items.Subfolders[0].Name);
                        return items;       // All files and subfolders have been retrieved.
                    }
                }
                else
                {
                    throw new Exception($"Unable to query items: {msg.StatusCode}");

                }
            } while (true);
        }
        static async Task<Folder> GetFolder(TokenInfo accessToken, string dest)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.access_token}");
            string folder_name = dest;
            string folderPath = HttpUtility.UrlEncode(folder_name); // Encodes folder name to usable format
            // Get folders, including those that are marked as hidden.
            try
            {
                HttpResponseMessage msg = await client.GetAsync($"https://am1.development.bartendercloud.com/api/librarian/folders/path/{folderPath}/properties"); // tries to access cloud using get method
                if (msg.IsSuccessStatusCode) // if it can connect
                {
                    var response = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Folder>(response);
                }
                else
                { // if for some reason something is wrong with msg, throw error

                    throw new Exception(msg.ReasonPhrase);
                }
            }
            catch (Exception)
            { // throws the error given above (msg.ReasonPhrase)

                return null;


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