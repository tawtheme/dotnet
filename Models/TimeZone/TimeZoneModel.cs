using Newtonsoft.Json;

namespace BlueCollarEngine.API.Models.TimeZone
{
    public class TimeZoneModel
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("tzCode")]
        public string TZCode { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("utc")]
        public string UTC { get; set; }
    }
}
