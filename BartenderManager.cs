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
using System.Linq.Expressions;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using static System.Net.WebRequestMethods;

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
            Client.BaseAddress = URI;
            HttpResponseMessage code = Client.GetAsync(Website).Result;
            AccessToken = Application.GetToken();
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");
/*            try
            {
;
;
            } catch {   
                throw new Exception("Website connection failed.");
            }*/
        }

        // Method to Extract URI from Access Token
        static string ExtractDataCenterURI(string accessToken)
        {
            DecodedToken decodedToken = AccessTokenDecoder.Decode(accessToken);

            if (decodedToken.Payload.ContainsKey(DataCenterClaim))
                return decodedToken.Payload[AccessTokenDecoder.DataCenterClaim] as string;
            throw new Exception("DataCenterURI missing");

        }
        public static async Task<String> DisplayFolderDir(string folderName)
        {
            String folderId = "";
            try
            {
                folderId = GetFolder(AccessToken.access_token, folderName).Result.Id;
            }
            catch (AggregateException ae)
            {
                foreach (Exception ex in ae.InnerExceptions)
                {
                    if (ex is ArgumentException)
                    {
                        return (ex.Message);
                    }
                }
            }

            String dirs = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken.access_token}");

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
                    RequestUri = new Uri($"{GlobalConfigManager.GetWebsite()}/api/librarian/items/{folderId}"),
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
                        {
                            dirs += "Base Folder: " + targetFolder.Name + "\n";
                            for (int i = 0; i < subfolders.Count; i++)
                            {
                                dirs += "\tFolder: " + subfolders[i].Name + "\n";
                            }
                            for (int i = 0; i < files.Count; i++)
                            {
                                dirs += "\tFile: " + files[i].Name + "\n";
                            }
                            return dirs;       // All files and subfolders have been retrieved.

                        }
                    }
                }
                else
                {
                    throw new Exception($"Unable to query items: {msg.StatusCode}");

                }
            } while (true);

        }

        internal class ValidationResult { public string Message { get; set; } public bool Status { get; set; } }

        public static async Task<ValidationResult> TestDir(string folderName)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken.access_token}");
            ValidationResult result = new();


            try
            {
                if(await GetFolder(AccessToken.access_token, folderName) is null)
                {
                    throw new ArgumentException("Path is invalid.");
                }
                result.Message = "Path validated";
                result.Status = true;
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
                result.Status = false;
            }

            return result;
        }

        static async Task<Space> GetSpaceID(TokenInfo accessToken, string space)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.access_token}");

            try
            {
                HttpResponseMessage msg = await client.GetAsync($"https://am1.development.bartendercloud.com/api/librarian/spaces/name/{space}");
                if (msg.IsSuccessStatusCode)
                {
                    var response = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Space>(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in getting space ID");
            }

            return null;
        }

        public static async void CloudUpload(String fileName)
        {
            HttpClient client = new HttpClient();
            Folder folder = GetFolder(AccessToken.access_token, GlobalConfigManager.GetDirectoryEntry()).Result;
            string fileType = fileName.Substring(fileName.LastIndexOf(".")).Substring(1);
            var fileAddRequest = new FileAddRequest()
            {
                Name = fileName,
                Comment = "Uploaded file",
                Encryption = "",               // Not encrypted.
                FileContentType = fileType,
                InheritFromFolderPermissions = true,
                IsHidden = false,
                VersionDescription = "Initial Checkin",
                FolderId = folder.Id
            };

            if (!System.IO.File.Exists(fileName))
            {
                throw new ArgumentException($"{fileName} does not exist.");
            }

            StreamContent streamContent = new StreamContent(System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read));
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(fileAddRequest)), "fileAdd" },
                { streamContent, "formFile", fileAddRequest.Name }
            };

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken.access_token}");
            var dest = GlobalConfigManager.GetDirectoryEntry();
            var msg = await client.PostAsync($"https://am1.development.bartendercloud.com/api/librarian/spaces/{GetSpaceID(AccessToken, dest.Split("/")[2]).Result.SpaceId}/files", multipartFormDataContent);
            if (!msg.IsSuccessStatusCode)
            {
                throw new Exception();
            }
        }

        static async Task<Folder> GetFolder(String accessToken, string dest)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            string folder_name = dest;
            string folderPath = HttpUtility.UrlEncode(folder_name); // Encodes folder name to usable format
            // Get folders, including those that are marked as hidden.
            try
            {
                var requestURI = $"{GlobalConfigManager.GetWebsite()}/api/librarian/folders/path/{folderPath}/properties";
                HttpResponseMessage msg = await client.GetAsync(requestURI); // tries to access cloud using get method
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

        public static async Task<string> CloudDownload(string filePath, string fileDestination)
        {
            //string fileId = GetFile(access_token, filePath).Result.Id;
            string access_token = AccessToken.access_token;
            filePath = HttpUtility.UrlEncode(filePath);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {access_token}");
            HttpResponseMessage msg = await client.GetAsync($"{Website}/api/librarian/files/path/{filePath}/content?versionMajor={1}");

            if (msg.IsSuccessStatusCode || msg.StatusCode == HttpStatusCode.NoContent)
            {
                var stream = await msg.Content.ReadAsStreamAsync();
                try
                {
                    string checkPath = fileDestination + "\\" + GetFile(access_token, filePath).Result.Name;
                    if (System.IO.File.Exists(checkPath) == true)
                    {
                        return "File already exists in specified destination. The file download will be cancelled";
                    }
                    using (FileStream file = new FileStream(checkPath, FileMode.Create, System.IO.FileAccess.Write))
                        stream.CopyTo(file);
                    return "Your transfer was a success!";
                }
                catch
                {
                    throw new ArgumentException("The destination you specified for the download doesn't exist\nMake sure that the path exists and is a directory (no file attached)");
                }

            }
            else
            {
                throw new ArgumentException("The cloud path that you specified was invalid. Please input a proper path.");
            }
        }

        public static async Task<String> CloudDelete(string filePath)
            {
            // Assume that fileId is the ID of the target file.
            string accessToken = AccessToken.access_token;
            HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                string fileId = "";
                try
                {
                    fileId = GetFile(accessToken, filePath).Result.Id;
                }
                catch (AggregateException ae)
                {
                    foreach (Exception ex in ae.InnerExceptions)
                    {
                        if (ex is ArgumentException)
                        {
                            Console.WriteLine(ex.Message);
                            return (ex.Message);

                        }
                    }
                }

                var requests = new FileUpdateRequest()
                {
                    Comment = "Deleting File"
                };
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri($"{GlobalConfigManager.GetWebsite()}/api/librarian/files/{fileId}/delete"))
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requests), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage msg = await client.SendAsync(request); //(CODE FOR DELETING FILES)
                //HttpResponseMessage msg = await client.DeleteAsync($"https://bartendercloud.com/api/librarian/files/{fileId}/purge"); // CODE fo


                if (msg.IsSuccessStatusCode)
                {

                    //return JsonConvert.DeserializeObject<FileChange>(await msg.Content.ReadAsStringAsync());
                    Console.WriteLine("File deleted!");
                    return "File was deleted successfully!";

                }
                else if (msg.StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("File doesn't exist to delete");
                    return "";
                }
                else
                {
                    Console.WriteLine("Something went wrong, your file couldn't be deleted.");
                    throw new ArgumentException("Something went wrong, your file couldn't be deleted.");
                }
            }



            static async Task<FileChange> GetFile(String accessToken, string path) {
                HttpClient client = new HttpClient();
                string filePath = HttpUtility.UrlEncode(path);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                HttpResponseMessage msg = await client.GetAsync($"{GlobalConfigManager.GetWebsite()}/api/librarian/files/path/{filePath}/properties?versionMajor={1}&versionMinor={0}");

                if (msg.IsSuccessStatusCode)
                {
                    var response = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FileChange>(response);
                }
                else {
                    throw new ArgumentException("Your file path is not valid. Please input a proper path.\nEx) libraian://Main/Test.txt\nIf you are trying to reach a space, use librarian://[Space Name]/");
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

        public TokenInfo GetToken()
            {
                TokenInfo token = null;

            string clientId = GlobalConfigManager.GetApplicationID();     // BarTenderCloud app client-id
            string clientSecret = GlobalConfigManager.GetSecretID(); // BarTenderCloud app client-secret
            string username = GlobalConfigManager.GetUsername() ;
            string password = GlobalConfigManager.GetPassword();

            RetrieveAuthenticationConfiguration(GlobalConfigManager.GetWebsite());

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