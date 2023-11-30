#nullable disable
using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Configs;

public class DiscordConfig
{
    [Required]
    public string ClientId { get; set; } = "";
    [Required]
    public string ClientSecret { get; set; } = "";
    [Required]
    public string RedirectURI { get; set; } = "http://localhost";
}