using Bonobo.Git.Server.DAL.Mapping;
using System;
using System.Data.Entity;
using System.IO;
using System.Web;

namespace Bonobo.Git.Server.DAL
{
    public partial class BonoboGitServerContext : DbContext
    {
        static BonoboGitServerContext()
        {
            Database.SetInitializer<BonoboGitServerContext>(null);
        }

        public BonoboGitServerContext()
            : base("Name=BonoboGitServerContext")
        {
        }

        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new RepositoryMap());
            modelBuilder.Configurations.Add(new RoleMap());
            modelBuilder.Configurations.Add(new TeamMap());
            modelBuilder.Configurations.Add(new UserMap());
        }

        //Not a good way, how improve it?
        public static void CreateDatabaseIfNotExists()
        {
            using (var ctx = new BonoboGitServerContext())
            {
                // Don't use 'ctx.Database.Connection is SQLiteConnection', it make reference to SQLite assembly cause loading error in IIS.
                if (ctx.Database.Connection.GetType().Name == "SQLiteConnection")
                {
                    var sql = File.ReadAllText(HttpContext.Current.Server.MapPath(@"~\App_LocalResources\Create.sql"));

                    /*
                     * After this, a SQLite db file to be created if not exists.
                     * Otherwish, nothing to do.
                     * Generally, this method is to check the database exists or not but to be not create a db file.
                     * I'm not sure wheather there are bug or not. -- Aimeast
                     */
                    ctx.Database.Exists();

                    using (var conn = ctx.Database.Connection)
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('UserTeam_Member', 'UserRole_InRole', 'UserRepository_Permission', 'UserRepository_Administrator', 'TeamRepository_Permission', 'User', 'Team', 'Role', 'Repository')";
                        var ret = "" + cmd.ExecuteScalar();
                        if (ret != "9")
                        {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                }
            }
        }
    }
}
