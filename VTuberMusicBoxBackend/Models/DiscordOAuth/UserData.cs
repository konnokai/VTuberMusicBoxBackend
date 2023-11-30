#nullable disable

using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.DiscordOAuth
{
    public class UserData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("banner")]
        public string Banner { get; set; }
    }
}
