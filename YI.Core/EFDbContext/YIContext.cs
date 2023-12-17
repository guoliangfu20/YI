using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Enums;
using System.Reflection;
using YI.Entity.SystemModels;

namespace YI.Core.EFDbContext
{
    /// <summary>
    /// 上下文
    /// </summary>
    public class YIContext : DbContext//,IDependency
    {
        /// <summary>
        /// 数据库连接名
        /// </summary>
        public string _connection = null;

        public YIContext() : base() { }

        public YIContext(string connection) : base()
        {
            _connection = connection;
        }

        public YIContext(DbContextOptions<YIContext> options) : base(options)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                throw (ex.InnerException as Exception ?? ex);
            }
        }

        public override DbSet<TEntity> Set<TEntity>()
        {
            return base.Set<TEntity>();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = DBServerProvider.GetConnectionString();

            if (Const.DBType.Name == DbCurrentType.MySql.ToString())
            {
                optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 11)));
            }
            else if (Const.DBType.Name == DbCurrentType.PgSql.ToString())
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                optionsBuilder.UseSqlServer(connectionString);
            }

            //默认禁用实体跟踪
            optionsBuilder = optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Type type = null;

            //获取所有类库
            var compilationLibrary = DependencyContext.Default
                .RuntimeLibraries
                .Where(r => !r.Serviceable && r.Type != "package" && r.Type != "project");
            foreach (var _compilation in compilationLibrary)
            {
                AssemblyLoadContext.Default
                    .LoadFromAssemblyName(new AssemblyName(_compilation.Name))
                    .GetTypes()
                    .Where(x =>
                    x.GetTypeInfo().BaseType != null
                    && x.BaseType == typeof(BaseEntity)).ToList()
                    .ForEach(t =>
                    {
                        modelBuilder.Entity(t);
                    });

            }
            base.OnModelCreating(modelBuilder);
        }



        /// <summary>
        /// 设置追踪
        /// </summary>
        public bool QueryTracking
        {
            set
            {
                this.ChangeTracker.QueryTrackingBehavior =
                    value ? QueryTrackingBehavior.TrackAll
                    : QueryTrackingBehavior.NoTracking;

            }
        }
    }
}
