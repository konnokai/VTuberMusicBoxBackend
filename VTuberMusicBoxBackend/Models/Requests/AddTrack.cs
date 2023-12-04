using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class AddTrack
    {
        [JsonProperty("video_id")]
        public string VideoId { get; set; } = "";
        [JsonProperty("start_at")]
        public ushort StartAt { get; set; } = 0;
        [JsonProperty("end_at")]
        public ushort EndAt { get; set; } = 0;
    }
}
