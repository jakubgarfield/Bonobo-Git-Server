using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data.Update.ADBackendUpdate;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Serilog;

namespace Bonobo.Git.Server.Data.Update
{
    public class AutomaticUpdater
    {
        public void Run()
        {
            if (AuthenticationSettings.MembershipService.ToLowerInvariant() == "activedirectory")
            {
                Pre600UpdateTo600.UpdateADBackend();
            }
            else
            {
                UpdateDatabase();
            }
        }

        public void RunWithContext(BonoboGitServerContext context)
        {
            DoUpdate(context);
        }

        private void UpdateDatabase()
        {
            using (var ctx = new BonoboGitServerContext())
            {
                DoUpdate(ctx);
            }
        }

        private void DoUpdate(BonoboGitServerContext ctx)
        {
            IObjectContextAdapter ctxAdapter = ctx;
            var connectiontype = ctx.Database.Connection.GetType().Name;

            foreach (var item in UpdateScriptRepository.GetScriptsBySqlProviderName(connectiontype))
            {
                if (!string.IsNullOrEmpty(item.Precondition))
                {
                    try
                    {
                        var preConditionResult = ctxAdapter.ObjectContext.ExecuteStoreQuery<int>(item.Precondition).Single();
                        if (preConditionResult == 0)
                        {
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        // consider failures in pre-conditions as an indication that
                        // store ecommand should be executed
                    }
                }

                if (!string.IsNullOrEmpty(item.Command))
                {
                    try
                    {
                        ctxAdapter.ObjectContext.ExecuteStoreCommand(item.Command);
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