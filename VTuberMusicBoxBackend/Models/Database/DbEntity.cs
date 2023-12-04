using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Models.Database
{
    public class DbEntity
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public DateTime? DateAdded { get; set; } = DateTime.Now;
    }
}
