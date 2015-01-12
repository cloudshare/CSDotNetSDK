using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Web.Security;
using Newtonsoft.Json;

namespace CSAPI
{
    /// <summary>
    /// Provides raw functionality with CloudShare API.
    /// </summary>
    public class CSAPILowLevel
    {
        private const String ApiVersion = "v2";
        private const String BaseHostname = "use.cloudshare.com";

        private String _BaseURL;
        String _apiKey;
        String _apiId;

        public CSAPILowLevel(String apiKey, String apiId, String hostname = null)
        {
            _apiKey = apiKey;
            _apiId = apiId;
            _BaseURL = String.Format("https://{0}/Api/{1}", hostname ?? BaseHostname, ApiVersion);

        }

        /// <summary>
        /// Synchronous method, calls 'commandCategory/commandName' with parameters Params.
        /// </summary>
        /// <param name="commandCategory">The command category</param>
        /// <param name="commandName">The command's name</param>
        /// <param name="Params">The command's parameters</param>
        /// <exception cref="ApiException">Thrown when API response is not 200</exception>
        /// <returns>The API response body</returns>
        public String CallCSAPI(String commandCategory, String commandName, Dictionary<String, String> Params)
        {
            if (Params == null) throw new ArgumentNullException("Params");

            var apiResponse = CallURL(GenerateApiUrl(commandCategory, commandName, Params));

            if (apiResponse.status_code != "0x20000")
                throw new ApiException(apiResponse);

            // re-serialize data object
            return JsonConvert.SerializeObject(apiResponse.data);
        }

        /// <summary>
        /// Synchronous method, calls 'commandCategory/commandName' without parameters.
        /// </summary>
        /// <param name="commandCategory">The command category</param>
        /// <param name="commandName">The command's name</param>
        /// <exception cref="ApiException">Thrown when API response is not 200</exception>
        /// <returns>The API response body</returns>
        public String CallCSAPI(String commandCategory, String commandName)
        {
            return CallCSAPI(commandCategory, commandName, new Dictionary<string, string>());
        }

        /// <summary>
        /// Asynchronous method, calls 'commandCategory/commandName' with parameters Params.
        /// </summary>
        /// <param name="commandCategory">The command category</param>
        /// <param name="commandName">The command's name</param>
        /// <param name="Params">The command's parameters</param>
        /// <exception cref="ApiException">Thrown when API response is not 200</exception>
        /// <returns>The API response body</returns>
        public async Task<String> CallCSAPIAsync(String commandCategory, String commandName, Dictionary<String, String> Params)
        {
            if (Params == null) throw new ArgumentNullException("Params");

            var apiResponse = await CallURLAsync(GenerateApiUrl(commandCategory, commandName, Params));
            
            if (apiResponse.status_code != "0x20000")
                throw new ApiException(apiResponse);

            return JsonConvert.SerializeObject(apiResponse.data);
        }

        /// <summary>
        /// Synchronous call of the Ping API command to verify credentials and host connection.
        /// </summary>
        /// <returns>True when command is successful (API credentials) are ok. False otherwise.</returns>
        public bool CheckKeys()
        {
            var result = CallCSAPI("ApiTest", "Ping", new Dictionary<String, String>());

#if DEBUG
            if (result != null)
                Console.WriteLine(result);
#endif

            return result != null;
        }

        /// <summary>
        /// Asynchronous call of the Ping API command to verify credentials and host connection.
        /// </summary>
        /// <returns>True when command is successful (API credentials (Key and ID)) are ok. False otherwise.</returns>
        public async Task<bool> CheckKeysAsync()
        {
            var result = await CallCSAPIAsync("ApiTest", "Ping", new Dictionary<String, String>());

            if (result != null)
                Console.WriteLine(result);

            return result != null;
        }

        #region UtilityMethods

        private String GenerateApiUrl(String commandCategory, String commandName, Dictionary<String, String> Params)
        {
            String queryParams = "";
            String stringToSha = _apiKey + (ApiVersion != "v1" ? commandName.ToLower() : ""); // in version v2 we also add the commandName

            // clone dictionary
            var paramsDictionary = new Dictionary<String, String>(Params);

            // calculate timestamp
            var epochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            String timeStamp = ((int)((DateTime.UtcNow - epochDate).TotalSeconds)).ToString(CultureInfo.InvariantCulture);

            paramsDictionary["timestamp"] = timeStamp;
            paramsDictionary["UserApiId"] = _apiId;
            if (ApiVersion != "v1")
                paramsDictionary["token"] = Membership.GeneratePassword(10, 0);

            // sort all parameters in case-insensitive ascending order
            var paramNameList = new List<String>(paramsDictionary.Keys);
            foreach (var paramName in paramNameList.OrderBy(x => x.ToLower()))
            {
                if (paramName.ToLower() == "hmac")
                    continue;

                String value = paramsDictionary[paramName];
                stringToSha += paramName.ToLower() + value;

                if (queryParams != "")
                    queryParams += '&';

                queryParams += paramName + '=' + ParamValueEncode(value);
            }
            return String.Format("{0}/{1}/{2}?{3}&HMAC={4}", _BaseURL, commandCategory, commandName, queryParams, CalcSha(stringToSha));
        }

        private static string ParamValueEncode(string value)
        {
            var t = HttpUtility.UrlEncode(value);
            return t != null ? t.Replace("+", "%20") : "";
        }

        public static String ByteArrayToString(byte[] byteArray)
        {
            var sOutput = new StringBuilder(byteArray.Length);

            foreach (var byteItem in byteArray)
            {
                sOutput.Append(byteItem.ToString("x2"));
            }
            return sOutput.ToString();
        }

        private static String CalcSha(String stringToCompute)
        {
            var shaProvider = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            var hash = shaProvider.ComputeHash(Encoding.UTF8.GetBytes(stringToCompute));
            return ByteArrayToString(hash);
        }

        private static ApiResponse CallURL(String urlAddress)
        {
            using (var client = new HttpClient())
            {
                Task<HttpResponseMessage> httpResp;
                Task<string> httpContent;
                try
                {
                    httpResp = client.GetAsync(new Uri(urlAddress));
                    httpResp.Wait();

                    httpContent = httpResp.Result.Content.ReadAsStringAsync();
                    httpContent.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }

                return CreateApiResponseFromHttpResponse(httpResp.Result.StatusCode, httpContent.Result);
            }
        }

        private static async Task<ApiResponse> CallURLAsync(String urlAddress)
        {
            using (var client = new HttpClient())
            {
                var httpResp = await client.GetAsync(new Uri(urlAddress));
                var httpContent = await httpResp.Content.ReadAsStringAsync();

                return CreateApiResponseFromHttpResponse(httpResp.StatusCode, httpContent);
            }
        } 

        private static ApiResponse CreateApiResponseFromHttpResponse(HttpStatusCode statusCode, string content)
        {
            ApiResponse apiResponse;

            try
            {
                apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
            }
            catch (JsonSerializationException)
            {
                throw new ApiException(String.Format("Failed to deserialize API response:\n{0}", content), statusCode);
            }

            if (apiResponse != null && apiResponse.status_code != null)
            {
                return apiResponse;
            }

            throw new ApiException(String.Format("Got bad API response:\n{0}", content), statusCode);
        }

        #endregion
    }
}




