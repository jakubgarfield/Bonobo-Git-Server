using System;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data.Update.ADBackendUpdate;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bonobo.Git.Server.Data.Update
{
    public class AutomaticUpdater
    {
        public void Run(IServiceProvider serviceProvider, BonoboGitServerContext context, AuthenticationSettings authSettings)
        {
            if (string.Equals(authSettings.MembershipService, "activedirectory", StringComparison.OrdinalIgnoreCase))
            {
                var updater = serviceProvider.GetService<Pre600UpdateTo600>();
                updater.UpdateADBackend();
            }
            else
            {
                UpdateDatabase(serviceProvider, context);
            }
        }

        public void RunWithContext(IServiceProvider serviceProvider, BonoboGitServerContext context)
        {
            DoUpdate(serviceProvider, context);
        }

        private void UpdateDatabase(IServiceProvider serviceProvider, BonoboGitServerContext ctx)
        {
            //using (var ctx = new BonoboGitServerContext(databaseConnection))
            {
                DoUpdate(serviceProvider, ctx);
            }
        }

        private void DoUpdate(IServiceProvider serviceProvider, BonoboGitServerContext ctx)
        {
            var connectiontype = ctx.Database.ProviderName;//.Connection.GetType().Name;

            foreach (var item in UpdateScriptRepository.GetScriptsBySqlProviderName(connectiontype, serviceProvider))
            {
                if (!string.IsNullOrEmpty(item.Precondition))
                {
                    try
                    {
                        var preConditionResult = ctx.Database.GetDbConnection().ExecuteScalar<int>(item.Precondition);
                        if (preConditionResult == 0)
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        // consider failures in pre-conditions as an indication that
                        // store ecommand should be executed
                    }
                }

                if (!string.IsNullOrEmpty(item.Command))
                {
                    try
                    {
                        ctx.Database.ExecuteSqlCommand(item.Command);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Exception while processing upgrade script {0}", item.Command);
                        throw;
                    }
                }

                item.CodeAction(ctx);
            }
        }
    }
}