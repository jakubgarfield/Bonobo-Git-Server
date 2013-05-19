using Bonobo.Git.Server.Data.Mapping;
using System;
using System.Data.Entity;
using System.IO;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    public partial class BonoboGitServerContext : DbContext
    {
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }


        static BonoboGitServerContext()
        {
            Database.SetInitializer<BonoboGitServerContext>(null);
        }

        public BonoboGitServerContext()
            : base("Name=BonoboGitServerContext")
        {
        }

        
        public static void CreateDatabaseIfNotExists()
        {
            using (var ctx = new BonoboGitServerContext())
            {
                if (ctx.Database.Connection.GetType().Name == "SQLiteConnection") // Don't use 'ctx.Database.Connection is SQLiteConnection', it make reference to SQLite assembly cause loading error in IIS.                
                {
                    ctx.Database.Exists();

                    using (var conn = ctx.Database.Connection)
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('UserTeam_Member', 'UserRole_InRole', 'UserRepository_Permission', 'UserRepository_Administrator', 'TeamRepository_Permission', 'User', 'Team', 'Role', 'Repository')";
                        var ret = "" + cmd.ExecuteScalar();
                        if (ret != "9")
                        {
                            // HttpRuntime.AppDomainAppPath is better than HttpContext.Current.Server.MapPath
                            var sql = File.ReadAllText(Path.Combine(HttpRuntime.AppDomainAppPath, @"App_LocalResources\Create.sql"));

                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                }
            }
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new RepositoryMap());
            modelBuilder.Configurations.Add(new RoleMap());
            modelBuilder.Configurations.Add(new TeamMap());
            modelBuilder.Configurations.Add(new UserMap());
        }
    }
}
