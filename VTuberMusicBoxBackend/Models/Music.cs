namespace VTuberMusicBoxBackend.Models
{
    public class Music : DbEntity
    {
        public string VideoId { get; set; } = "";
        public ushort StartAt { get; set; } = 0;
        public ushort EndAt { get; set; } = 0;
        public bool IsLike { get; set; } = false;
    }
}
