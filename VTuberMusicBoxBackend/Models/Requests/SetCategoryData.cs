using Newtonsoft.Json;

#nullable disable

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class SetCategoryData
    {
        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("video_and_position")]
        public Dictionary<string, ushort> VideoAndPosition { get; set; }
    }
}
