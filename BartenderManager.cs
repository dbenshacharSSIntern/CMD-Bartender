using Barrista.Models;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using PasswordBasedAuthLogon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
using System.Reflection.Emit;
using System.Text.Json.Nodes;
using System.Net.Http.Json;

namespace BasicAuthLogon
{
    internal static class BartenderManager
    {
        private static BIDSManager BIDSManager = new BIDSManager();
        private static BIDSManager.RestInfo AccessToken;
        private static HttpClient client = new HttpClient();
        private static Uri URI;

        public static void Initalize() {
            AccessToken = BIDSManager.GetTokenInfo(GlobalConfigManager.GetAlius());
            URI = new Uri(AccessToken.dataCenterURI);
            client.BaseAddress = URI;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken.accessToken.access_token}");
        }

        public static async Task<String> DisplayFolderDir(string folderName)
        {
            String folderId = "";
            try
            {
                folderId = GetFolder(AccessToken.accessToken.access_token, folderName).Result.Id;
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
            ValidationResult result = new();


            try
            {
                if(await GetFolder(AccessToken.accessToken.access_token, folderName) is null)
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

        public static async Task<ValidationResult> MakeFolder(string name)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken.accessToken.access_token}");
            var parentFolderID = GlobalConfigManager.GetDirectoryEntry();
            ValidationResult result = new ValidationResult();

            var createRequest = new FolderCreateRequest()
            {
                Name = name,
                ParentFolderId = GetFolder(AccessToken.accessToken.access_token, parentFolderID).Result.Id,
                IsHidden = false,
                InheritPermissionsFromParent = true
            };

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://am1.development.bartendercloud.com/api/librarian/spaces/{1}/folders"),
                Content = new StringContent(JsonConvert.SerializeObject(createRequest), Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post
            };

            var msg = await client.SendAsync(request);

            if (msg.IsSuccessStatusCode)
            {
                Folder folder = JsonConvert.DeserializeObject<Folder>(await msg.Content.ReadAsStringAsync());
                result.Status = true;
                result.Message = $"Successfully made folder with ID {folder.Id}";
            } else
            {
                result.Status = false;
                result.Message = "Failed to create folder.";
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
            catch
            {
                Console.WriteLine("Error in getting space ID");
            }

            return null;
        }

        public static async Task<Barrista.Models.FileChange> CloudUpload(String fileName)
        {
            HttpClient client = new HttpClient();
            Folder folder = GetFolder(AccessToken.accessToken.access_token, GlobalConfigManager.GetDirectoryEntry()).Result;
            string fileType = fileName.Substring(fileName.LastIndexOf(".")).Substring(1);
            string name = fileName.Substring(fileName.LastIndexOf("\\") + 1);
            var fileAddRequest = new FileAddRequest()
            {
                Name = name,
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

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken.accessToken.access_token}");
            var msg = await client.PostAsync($"https://am1.development.bartendercloud.com/api/librarian/spaces/{1}/files", multipartFormDataContent);
            if (msg.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<FileChange>(await msg.Content.ReadAsStringAsync());
            } else
            {
                throw new ArgumentException("There was an error with the client");
            }
        }

        static async Task<Folder> GetFolder(String accessToken, string path)
        {
            string folderPath = HttpUtility.UrlEncode(path); // Encodes folder name to usable format
            // Get folders, including those that are marked as hidden.
            try
            {
                var requestURI = $"{GlobalConfigManager.GetWebsite()}/api/librarian/folders/path/{folderPath}/properties";
                HttpResponseMessage msg = await client.GetAsync(requestURI);
                if (msg.IsSuccessStatusCode)
                {
                    var response = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Folder>(response);
                }
                else
                {

                    throw new Exception(msg.ReasonPhrase);
                }
            }
            catch (Exception)
            {

                return null;
            }
        }

        public static async Task<string> CloudDownload(string filePath, string fileDestination)
        {
            //string fileId = GetFile(access_token, filePath).Result.Id;
            string access_token = AccessToken.accessToken.access_token;
            filePath = HttpUtility.UrlEncode(filePath);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {access_token}");
            HttpResponseMessage msg = await client.GetAsync($"{GlobalConfigManager.GetWebsite()}/api/librarian/files/path/{filePath}/content?versionMajor={1}");

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
            string accessToken = AccessToken.accessToken.access_token;
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
                    return "File was deleted successfully!";

                }
                else if (msg.StatusCode == HttpStatusCode.BadRequest)
                {
                    return "File doesn't exist to delete";
                }
                else
                {
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
}