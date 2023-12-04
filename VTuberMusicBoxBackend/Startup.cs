using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System.Text;
using VTuberMusicBoxBackend.Configs;
using VTuberMusicBoxBackend.Models.Database;

namespace VTuberMusicBoxBackend
{
    public class Startup
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.UseMemberCasing();
            });

            try
            {
                RedisConnection.Init(Configuration.GetConnectionString("RedisConnection")!);
                Utility.Redis = RedisConnection.Instance.ConnectionMultiplexer;
                Utility.RedisDb = Utility.Redis.GetDatabase(2);

                _logger.Info("Redis已連線");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Redis連線錯誤，請確認伺服器是否已開啟\r\n");
                return;
            }

            // https://medium.com/selectprogram/asp-net-core%E4%BD%BF%E7%94%A8jwt%E9%A9%97%E8%AD%89-1b0609e6e8e3
            //將"class JwtConfig"中的"Secret"賦值為"appsettings.json"中的"JwtConfig"
            services.Configure<JwtConfig>(Configuration.GetSection(nameof(JwtConfig)));

            //設定key
            var key = Encoding.ASCII.GetBytes(Configuration.GetValue<string>($"{nameof(JwtConfig)}:{nameof(JwtConfig.Secret)}"));

            TokenValidationParameters tokenValidationParams = new()
            {
                RequireExpirationTime = false,
                ValidateIssuer = false,
                ValidateAudience = false,

                //驗證IssuerSigningKey
                ValidateIssuerSigningKey = true,
                //以JwtConfig:Secret為Key，做為Jwt加密
                IssuerSigningKey = new SymmetricSecurityKey(key),

                //驗證時效
                ValidateLifetime = true,

                //設定token的過期時間可以以秒來計算，當token的過期時間低於五分鐘時使用。
                ClockSkew = TimeSpan.Zero
            };

            //註冊tokenValidationParams，後續可以注入使用。
            services.AddSingleton(tokenValidationParams);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = tokenValidationParams;
            });

            services.AddDbContext<MainDbContext>(options =>
            {
                string? connectionString = Configuration.GetConnectionString("MySQLConnection");
                if (connectionString == null)
                    throw new NullReferenceException(nameof(connectionString));

                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            services.Configure<DiscordConfig>(Configuration.GetSection(nameof(DiscordConfig)));

            var hostUri = new Uri(Configuration.GetValue<string>($"{nameof(DiscordConfig)}:{nameof(DiscordConfig.RedirectURI)}"));
            services.AddCors(options =>
            {
                options.AddPolicy(name: "allowGET", builder =>
                {
                    builder.WithOrigins($"{hostUri.Scheme}://{hostUri.Authority}")
                           .WithMethods("GET")
                           .WithHeaders("Content-Type");
                });
                options.AddPolicy(name: "allowPOST", builder =>
                {
                    builder.WithOrigins($"{hostUri.Scheme}://{hostUri.Authority}")
                           .WithMethods("POST")
                           .WithHeaders("Content-Type");
                });
            });

            services.AddAuthentication("Bearer");

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MainDbContext dbContext)
        {
            app.UseMiddleware<Middleware.LogMiddleware>();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            dbContext.Database.EnsureCreated();

            _logger.Info("初始化完成");
        }
    }
}
