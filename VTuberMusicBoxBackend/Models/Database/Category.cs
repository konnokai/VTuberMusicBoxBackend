﻿using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Models.Database
{
    public class Category : DbEntity
    {
        [JsonIgnore]
        public string DiscordUserId { get; set; } = string.Empty;

        [JsonProperty("guid")]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("position")]
        public ushort Position { get; set; } = 0;

        /// <summary>
        /// string = VideoId, int = Position
        /// </summary>
        [JsonProperty("videoIdList")]
        public Dictionary<string, ushort> VideoIdList { get; set; } = new();
    }
}
