using System.ComponentModel.DataAnnotations;

#nullable disable

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class DeleteCategory
    {
        [Required]
        public string Guid { get; set; }
    }
}
