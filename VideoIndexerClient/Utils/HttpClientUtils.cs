using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;

namespace VideoIndexer.Utils
{
    public static class HttpClientUtils
    {
        public static string CreateQueryString(this IDictionary<string, string> parameters)
        {
            var queryParameters = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
            {
                queryParameters[parameter.Key] = parameter.Value;
            }
            return queryParameters.ToString();
        }

        public static async Task VerifyStatusAsync(this HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode)
        {
            if (response.StatusCode != expectedStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Unexpected status code: {response.StatusCode}. Response body: {responseBody}");
            }
        }
    }
}
