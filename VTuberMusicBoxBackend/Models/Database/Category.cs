using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.Database
{
    public class Category : DbEntity
    {
        [JsonProperty("guid")]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("position")]
        public ushort Position { get; set; } = 0;

        /// <summary>
        /// string = VideoId, int = Position
        /// </summary>
        [JsonProperty("video_id_list")]
        public Dictionary<string, ushort> VideoIdList { get; set; } = new Dictionary<string, ushort>();
    }
}
