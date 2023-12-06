using Newtonsoft.Json;

#nullable disable

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class SetTrackCategorie
    {
        [JsonProperty("categorie_guid")]
        public string CategorieGuId { get; set; }

        [JsonProperty("video_id")]
        public string VideoId { get; set; }

        [JsonProperty("position")]
        public ushort Position { get; set; }
    }
}
