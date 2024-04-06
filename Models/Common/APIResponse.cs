
using System.Text.Json;

namespace BlueCollarEngine.API.Models.Common
{
    public class APIResponse
    {
        public APIResponse(int statusCode, string message = null, object data = null)
        {
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
            Data = data;
        }
        public string Message { get; }
        public object Data { get; set; }
      
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 200:
                    return "Success";
                case 400:
                    return "Bad request found";
                case 401:
                    return "User not authorize";
                case 404:
                    return "Resource not found";
                case 405:
                    return "Method not allowed";
                case 500:
                    return "An unhandled error occurred";
                case 502:
                    return "Bad Gateway";
                case 503:
                    return "Service Unavailable";
                default:
                    return null;
            }
        }
    }
}
