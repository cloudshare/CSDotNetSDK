using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CSAPI
{
    /// <summary>
    /// Thrown when API response is not 200. 
    /// </summary>
    public class ApiException : ApplicationException
    {
        public ApiException(string message, HttpStatusCode httpStatusCode)
            : base(String.Format("ApiException (HTTP {0}):\n{1}", (int)httpStatusCode, message))
        {
            HttpStatusCode = httpStatusCode;
            ApiResponse = null;
        }

        public ApiException(ApiResponse apiResponse)
            : base(String.Format("ApiException (HTTP {0}):\n{1}: {2} {3}", 
                                 (int)GetHttpStatusCode(apiResponse),
                                 apiResponse.status_code, 
                                 apiResponse.status_text, 
                                 String.IsNullOrEmpty(apiResponse.status_additional_data) ? "" : " - " + apiResponse.status_additional_data))
        {
            HttpStatusCode = GetHttpStatusCode(apiResponse);

            ApiResponse = apiResponse;
        }

        /// <summary>
        /// The HTTP status code.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; private set; }

        /// <summary>
        /// The API response itself.
        /// </summary>
        public ApiResponse ApiResponse { get; private set; }

        private static HttpStatusCode GetHttpStatusCode(ApiResponse apiResponse)
        {
            if (apiResponse == null || apiResponse.status_code == null)
                return HttpStatusCode.InternalServerError;

            int intStatusCode;
            int.TryParse(apiResponse.status_code.Substring(2, 3), out intStatusCode);

            return (HttpStatusCode)(intStatusCode == 0 ? 500 : intStatusCode);
        }
    }
}
