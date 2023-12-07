using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Models.Database
{
    public class User
    {
        [Key]
        public string DiscordId { get; set; } = string.Empty;
        public DateTime? DateAdded { get; set; } = DateTime.Now;
        public List<Track> TrackList { get; set; } = new List<Track>();
        public List<Category> CategoryList { get; set; } = new List<Category>();
    }
}
