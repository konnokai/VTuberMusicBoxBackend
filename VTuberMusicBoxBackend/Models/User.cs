using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Models
{
    public class User
    {
        [Key]
        public string DiscordId { get; set; } = string.Empty;
        public DateTime? DateAdded { get; set; } = DateTime.Now;
        public List<Music> MusicList { get; set; } = new List<Music>();
        public List<Category> CategorieList { get; set; } = new List<Category>();
        public List<string> LikedMusicList { get; set; } = new List<string>();
    }
}
