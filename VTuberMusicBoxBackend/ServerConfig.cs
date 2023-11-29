using Newtonsoft.Json;
using NLog;

#nullable disable

public class ServerConfig
{
    public string DiscordClientId { get; set; } = "";
    public string DiscordClientSecret { get; set; } = "";
    public string RedirectURI { get; set; } = "http://localhost";
    public string RedisOption { get; set; } = "127.0.0.1,syncTimeout=3000";
    public string TokenKey { get; set; } = "";

    private readonly Logger _logger = LogManager.GetLogger("Conf");

    public void InitServerConfig()
    {
        try { File.WriteAllText("server_config_example.json", JsonConvert.SerializeObject(new ServerConfig(), Formatting.Indented)); } catch { }
        if (!File.Exists("server_config.json"))
        {
            _logger.Error($"server_config.json遺失，請依照 {Path.GetFullPath("server_config_example.json")} 內的格式填入正確的數值");
            if (!Console.IsInputRedirected)
                Console.ReadKey();
            Environment.Exit(3);
        }

        var config = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText("server_config.json"));

        try
        {
            if (string.IsNullOrWhiteSpace(config.DiscordClientId))
            {
                _logger.Error("DiscordToken遺失，請輸入至server_config.json後重開伺服器");
                if (!Console.IsInputRedirected)
                    Console.ReadKey();
                Environment.Exit(3);
            }

            if (string.IsNullOrWhiteSpace(config.DiscordClientSecret))
            {
                _logger.Error("DiscordToken遺失，請輸入至server_config.json後重開伺服器");
                if (!Console.IsInputRedirected)
                    Console.ReadKey();
                Environment.Exit(3);
            }

            if (string.IsNullOrWhiteSpace(config.RedirectURI))
            {
                _logger.Error("RedirectURI遺失，請輸入至server_config.json後重開伺服器");
                if (!Console.IsInputRedirected)
                    Console.ReadKey();
                Environment.Exit(3);
            }

            if (string.IsNullOrWhiteSpace(config.RedisOption))
            {
                _logger.Error("RedisOption遺失，請輸入至server_config.json後重開伺服器");
                if (!Console.IsInputRedirected)
                    Console.ReadKey();
                Environment.Exit(3);
            }

            DiscordClientId = config.DiscordClientId;
            DiscordClientSecret = config.DiscordClientSecret;
            RedirectURI = config.RedirectURI;
            RedisOption = config.RedisOption;
            TokenKey = config.TokenKey;

            if (string.IsNullOrWhiteSpace(config.TokenKey) || string.IsNullOrWhiteSpace(TokenKey))
            {
                _logger.Warn($"{nameof(TokenKey)}遺失，將重新建立隨機亂數");

                TokenKey = GenRandomKey();

                try { File.WriteAllText("server_config.json", JsonConvert.SerializeObject(this, Formatting.Indented)); }
                catch (Exception ex)
                {
                    _logger.Error($"設定檔保存失敗: {ex}");
                    _logger.Error($"請手動將此字串填入設定檔中的 \"{nameof(TokenKey)}\" 欄位: {TokenKey}");
                    Environment.Exit(3);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
            throw;
        }
    }

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