using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class AddCategorie
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("position")]
        public ushort Position { get; set; } = 0;
    }
}
