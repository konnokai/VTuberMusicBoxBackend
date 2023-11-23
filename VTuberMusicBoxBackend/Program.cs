using NLog;
using NLog.Web;
using System.Reflection;

#nullable disable

namespace VTuberMusicBoxBackend
{
    public class Program
    {
        public static string VERSION => GetLinkerTime(Assembly.GetEntryAssembly());
        public static void Main(string[] args)
        {
            var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                Utility.ServerConfig.InitServerConfig();
                logger.Info(VERSION + " 初始化中");

                try
                {
                    RedisConnection.Init(Utility.ServerConfig.RedisOption);
                    Utility.Redis = RedisConnection.Instance.ConnectionMultiplexer;
                    Utility.RedisDb = Utility.Redis.GetDatabase(2);

                    logger.Info("Redis已連線");
                }
                catch (Exception exception)
                {
                    logger.Error(exception, "Redis連線錯誤，請確認伺服器是否已開啟\r\n");
                    return;
                }

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception\r\n");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
                Utility.Redis.Dispose();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();
        }

        public static string GetLinkerTime(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    return value;
                }
            }
            return default;
        }
    }
}
