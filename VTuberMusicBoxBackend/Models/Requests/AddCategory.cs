using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class AddCategory
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public ushort Position { get; set; }
    }
}
