using Barrista.Models;
using BasicAuthLogon;
using Newtonsoft.Json;
using System;
using static BasicAuthLogon.AccessTokenDecoder;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace PasswordBasedAuthLogon
{
    internal class BIDSManager { 

        static RestInfo GetTokenInfo(Alius userInfo)
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

            if (decodedToken.Payload.ContainsKey(DataCenterClaim))
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

            Console.WriteLine("Please enter information about an Native application created\nwith grant_type:\"password refresh_token\"\n");

            RetrieveAuthenticationConfiguration(CloudCluster);

            HttpClient client = new HttpClient();
            Uri uri = new Uri(TokenEndpoint);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.BaseAddress = uri;
            string scope = "openid profile BarTenderServiceApi" + (" offline_access");
            var contentBodyList = new List<KeyValuePair<string, string>>();
            contentBodyList.Add(new KeyValuePair<string, string>("grant_type", "password"));
            contentBodyList.Add(new KeyValuePair<string, string>("username", userInfo.Email));
            contentBodyList.Add(new KeyValuePair<string, string>("password", userInfo.Password));
            contentBodyList.Add(new KeyValuePair<string, string>("client_id", ClientId));
            contentBodyList.Add(new KeyValuePair<string, string>("client_secret", ClientSecret));
            contentBodyList.Add(new KeyValuePair<string, string>("audience", "https://BarTenderCloudServiceApi"));
            contentBodyList.Add(new KeyValuePair<string, string>("scope", scope));

            string queryParam = BIDS ? $"?OrganizationDnsName={userInfo.OrganizationDNSName}" : "";
            var request = new HttpRequestMessage(HttpMethod.Post, queryParam) { Content = new FormUrlEncodedContent(contentBodyList) };
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
}