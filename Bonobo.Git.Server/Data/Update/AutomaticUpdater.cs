using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Data.Update
{
    public class AutomaticUpdater
    {
        private readonly IMembershipService _membershipService = DependencyResolver.Current.GetService<IMembershipService>();


        public void Run()
        {
            UpdateDatabase();
        }

        private void UpdateDatabase()
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
                            continue;
                        }
                    }

                    command.CommandText = item.Command;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}