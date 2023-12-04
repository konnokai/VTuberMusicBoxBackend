namespace VTuberMusicBoxBackend.Models.Database
{
    public class Category : DbEntity
    {
        public string Name { get; set; } = "";

        /// <summary>
        /// string = VideoId, int = Position
        /// </summary>
        public Dictionary<string, ushort> VideoIdList { get; set; } = new Dictionary<string, ushort>();
    }
}
