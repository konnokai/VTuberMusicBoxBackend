using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.Database
{
    public class Track : DbEntity
    {
        [JsonIgnore]
        public string DiscordUserId { get; set; } = string.Empty;

        [JsonProperty("videoId")]
        public string VideoId { get; set; } = "";

        [JsonProperty("startAt")]
        public ushort StartAt { get; set; } = 0;

        [JsonProperty("endAt")]
        public ushort EndAt { get; set; } = 0;
    }
}
