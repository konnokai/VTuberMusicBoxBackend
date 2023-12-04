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

                _logger.Info("Redis�w�s�u");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Redis�s�u���~�A�нT�{���A���O�_�w�}��\r\n");
                return;
            }

            // https://medium.com/selectprogram/asp-net-core%E4%BD%BF%E7%94%A8jwt%E9%A9%97%E8%AD%89-1b0609e6e8e3
            //�N"class JwtConfig"����"Secret"��Ȭ�"appsettings.json"����"JwtConfig"
            services.Configure<JwtConfig>(Configuration.GetSection(nameof(JwtConfig)));

            //�]�wkey
            var key = Encoding.ASCII.GetBytes(Configuration.GetValue<string>($"{nameof(JwtConfig)}:{nameof(JwtConfig.Secret)}"));

            TokenValidationParameters tokenValidationParams = new()
            {
                RequireExpirationTime = false,
                ValidateIssuer = false,
                ValidateAudience = false,

                //����IssuerSigningKey
                ValidateIssuerSigningKey = true,
                //�HJwtConfig:Secret��Key�A����Jwt�[�K
                IssuerSigningKey = new SymmetricSecurityKey(key),

                //���Үɮ�
                ValidateLifetime = true,

                //�]�wtoken���L���ɶ��i�H�H��ӭp��A��token���L���ɶ��C�󤭤����ɨϥΡC
                ClockSkew = TimeSpan.Zero
            };

            //���UtokenValidationParams�A����i�H�`�J�ϥΡC
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

            _logger.Info("��l�Ƨ���");
        }
    }
}
