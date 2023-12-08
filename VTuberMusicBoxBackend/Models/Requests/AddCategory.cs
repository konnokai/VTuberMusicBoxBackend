using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class AddCategory
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("position")]
        public ushort Position { get; set; } = 0;
    }
}
