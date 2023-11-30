﻿using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Models
{
    public class DbEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime? DateAdded { get; set; } = DateTime.Now;
    }
}
