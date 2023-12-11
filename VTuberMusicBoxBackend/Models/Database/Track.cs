using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.Database
{
    public class Track : DbEntity
    {
        [JsonIgnore]
        public string DiscordUserId { get; set; } = string.Empty;

        [JsonProperty("guid")]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();

        [JsonProperty("videoId")]
        public string VideoId { get; set; } = "";

        [JsonProperty("videoTitle")]
        public string VideoTitle { get; set; } = "";

        [JsonProperty("startAt")]
        public ushort StartAt { get; set; } = 0;

        [JsonProperty("endAt")]
        public ushort EndAt { get; set; } = 0;

        [JsonProperty("trackTitle")]
        public string TrackTitle { get; set; } = "";

        [JsonProperty("artist")]
        public string Artist { get; set; } = "";

        [JsonProperty("unplayable")]
        public bool Unplayable { get; set; } = false;
    }
}
