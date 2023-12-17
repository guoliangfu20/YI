using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YI.Core.Const;
using YI.Core.Extensions;

namespace YI.Core.Configuration
{
    public static class AppSetting
    {
        public static IConfiguration Configuration { get; private set; }


        public static string DbConnectionString
        {
            get { return _connection.DbConnectionString; }
        }

        public static Secret Secret { get; private set; }

        public static CreateMember CreateMember { get; private set; }

        public static ModifyMember ModifyMember { get; private set; }


        public static string TokenHeaderName = "Authorization";

        /// <summary>
        /// Actions权限过滤
        /// </summary>
        public static GlobalFilter GlobalFilter { get; set; }

        public static KafkaModel Kafka { get; set; }


        private static DbConnection _connection;

        /// <summary>
        /// JWT有效期(分钟=默认120)
        /// </summary>
        public static int ExpMinutes { get; private set; } = 120;

        public static string CurrentPath { get; private set; } = null;
        public static string DownLoadPath { get { return CurrentPath + "\\Download\\"; } }


        public static void Init(IServiceCollection services, IConfiguration configuration)
        {
            Configuration = configuration;
            // 注册配置
            //services.Configure<DbConnection>(configuration.GetSection("Connection"));
            //services.Configure<DbConnection>(configuration.GetSection("Connection"));
            //services.Configure<CreateMember>(configuration.GetSection("CreateMember"));
            //services.Configure<ModifyMember>(configuration.GetSection("ModifyMember"));
            //services.Configure<GlobalFilter>(configuration.GetSection("GlobalFilter"));
            //services.Configure<Kafka>(configuration.GetSection("Kafka"));


            var provider = services.BuildServiceProvider();
            //IWebHostEnvironment environment = provider.GetRequiredService<IWebHostEnvironment>();
            //CurrentPath = Path.Combine(environment.ContentRootPath, "").ReplacePath();


            Secret = provider.GetRequiredService<IOptions<Secret>>().Value;

            //设置修改或删除时需要设置为默认用户信息的字段
            CreateMember = provider.GetRequiredService<IOptions<CreateMember>>().Value ?? new CreateMember();
            ModifyMember = provider.GetRequiredService<IOptions<ModifyMember>>().Value ?? new ModifyMember();

            // 权限
            GlobalFilter = provider.GetRequiredService<IOptions<GlobalFilter>>().Value ?? new GlobalFilter();
            GlobalFilter.Actions = GlobalFilter.Actions ?? new string[0];

            // kafka 配置
            Kafka = provider.GetRequiredService<IOptions<KafkaModel>>().Value ?? new KafkaModel();


            _connection = provider.GetRequiredService<IOptions<DbConnection>>().Value;

            ExpMinutes = (configuration["ExpMinutes"] ?? "120").GetInt();


            // DB
            DBType.Name = _connection.DBType;
            if (string.IsNullOrEmpty(_connection.DbConnectionString))
                throw new System.Exception("未配置好数据库默认连接");

            try
            {
                _connection.DbConnectionString = _connection.DbConnectionString.DecryptDES(Secret.DB);
            }
            catch { }


            // redis 连接
            if (!string.IsNullOrEmpty(_connection.RedisConnectionString))
            {
                try
                {
                    _connection.RedisConnectionString = _connection.RedisConnectionString.DecryptDES(Secret.Redis);
                }
                catch { }
            }
        }


        /// <summary>
        /// 多个节点name格式 ：["key:key1"]
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetSettingString(string key)
        {
            return Configuration[key];
        }

        /// <summary>
        /// 多个节点,通过.GetSection("key")["key1"]获取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IConfigurationSection GetSection(string key)
        {
            return Configuration.GetSection(key);
        }

    }
}
