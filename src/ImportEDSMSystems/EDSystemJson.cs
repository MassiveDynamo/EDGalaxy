using System.Text.Json.Serialization;

namespace ImportEdsmSystems
{
    [Serializable]
    public class EDSystemJson
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("id64")]
        public long? Id64 { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("coords")] 
        public CoordsJson? Coords { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        internal static IEnumerable<string> GetPropertyNames()
        {
            List<string> names = new()
            {
                "Id",
                "Id64",
                "Name",
                "X",
                "Y",
                "Z",
                "Date"
            };
            return names.AsEnumerable();
        }
    }

    [Serializable]
    public class CoordsJson
    {
        [JsonPropertyName("x")]
        public double X { get; set; }
        [JsonPropertyName("y")]
        public double Y { get; set; }
        [JsonPropertyName("z")]
        public double Z { get; set; }
    }
}