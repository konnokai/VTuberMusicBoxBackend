﻿using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class AddTrack
    {
        [Required]
        public string VideoId { get; set; } = "";
        [Required]
        public string VideoTitle { get; set; } = "";
        [Required]
        public ushort StartAt { get; set; } = 0;
        [Required]
        public ushort EndAt { get; set; } = 0;
        public string TrackTitle { get; set; } = "";
        public string Artist { get; set; } = "";
    }
}
