using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RESTStoragePolicy
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static string subscriptionId = "<YOUR INFO>";
        static string resourceGroupName = "<YOUR INFO>";
        static string accountName = "<YOUR INFO>";

        private static string token = "";

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            string CLIENT_ID = "<YOUR INFO>";
            string CLIENT_SECRET = "<YOUR INFO>";
            string TENANT_ID = "<YOUR INFO>";
            string token = await GetAccessToken(TENANT_ID, CLIENT_ID, CLIENT_SECRET);

            var authenticationContext = new AuthenticationContext($"https://login.windows.net/{TENANT_ID}");
            var clientCredentials = new ClientCredential(CLIENT_ID, CLIENT_SECRET);
            var authenticationResult = await authenticationContext.AcquireTokenAsync("https://management.core.windows.net/", clientCredentials);
            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken);

            // Update Storage policy
            DoPUTMgmtPolicyCreateOrUpdateAsyncREST(token).GetAwaiter().GetResult();

            Console.WriteLine("REST API Call Completed...Press any Key...");
            Console.ReadLine();
        }
        private static async Task DoPUTMgmtPolicyCreateOrUpdateAsyncREST(String token)
        {
            String uri = string.Format("https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Storage/storageAccounts/{2}/managementPolicies/default?api-version=2019-04-01", subscriptionId, resourceGroupName, accountName);

            var jsonText = File.ReadAllText("StoragePolicy.json");
            Byte[] requestPayload = Encoding.ASCII.GetBytes(jsonText);
            HttpContent content = new ByteArrayContent(requestPayload);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                HttpResponseMessage httpResponseMessage = await client.PutAsync(uri, content);
                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    String jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
                    string CleanJsonObject = JObject.Parse(jsonString).ToString();
                    string JsonContentFixed = CleanJsonObject.Replace(@"\", "");
                }
            }
            catch (Exception ex)
            {
            }

            string result = null;
        }
        private static async Task<string> GetAccessToken(string tenantName, string clientId, string clientSecret)
        {
            var authString = "https://login.microsoftonline.com/" + tenantName;
            var resourceUrl = "https://management.azure.com/";

            var authenticationContext = new AuthenticationContext(authString, false);
            var clientCred = new ClientCredential(clientId, clientSecret);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(resourceUrl, clientCred);
            var token = authenticationResult.AccessToken;

            return token;
        }
    }
}
