﻿using System.ComponentModel.DataAnnotations;

#nullable disable

namespace VTuberMusicBoxBackend.Models.Requests
{
    public class DeleteTrack
    {
        [Required]
        public string Guid { get; set; }
    }
}
