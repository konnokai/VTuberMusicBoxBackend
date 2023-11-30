using System.ComponentModel.DataAnnotations;

namespace VTuberMusicBoxBackend.Configs
{
    public class JwtConfig
    {
        [Required]
        public string Secret { get; set; } = "";

        private static string GenRandomKey()
        {
            var characters = "ABCDEF_GHIJKLMNOPQRSTUVWXYZ@abcdefghijklmnopqrstuvwx-yz0123456789";
            var Charsarr = new char[128];
            var random = new Random();

            for (int i = 0; i < Charsarr.Length; i++)
            {
                Charsarr[i] = characters[random.Next(characters.Length)];
            }

            var resultString = new string(Charsarr);
            return resultString;
        }
    }
}
