using Bonobo.Git.Server.Data.Mapping;
using System;
using System.Data.Entity;
using System.IO;
using System.Web;
using Bonobo.Git.Server.Data.Update;

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


        public static void RunAutomaticUpdate()
        {
            using (var ctx = new BonoboGitServerContext())
            using (var connection = ctx.Database.Connection)
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                foreach (var item in new UpdateScriptRepository().Scripts)
                {
                    if (!String.IsNullOrEmpty(item.Precondition))
                    {
                        command.CommandText = item.Precondition;
                        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                        {
                            return;
                        }
                    }

                    command.CommandText = item.Command;
                    command.ExecuteNonQuery();
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
