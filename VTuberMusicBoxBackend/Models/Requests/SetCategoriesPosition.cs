#nullable disable

using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class SetCategoriesPosition
    {
        [Required]
        public Dictionary<string, ushort> GuidAndPosition { get; set; }
    }
}
