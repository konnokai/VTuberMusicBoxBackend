#nullable disable

using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class SetCategoryTrack
    {
        [Required]
        public string Guid { get; set; }

        [Required]
        public Dictionary<string, ushort> TrackGuidAndPosition { get; set; }
    }
}
