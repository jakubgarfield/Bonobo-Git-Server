using System;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Bonobo.Git.Server.Data.Update
{
    public class AutomaticUpdater
    {
        public void Run()
        {
            UpdateDatabase();
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

            foreach (var item in UpdateScriptRepository.GetScriptsBySqlProviderName(ctx.Database.Connection.GetType().Name))
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
                ctxAdapter.ObjectContext.ExecuteStoreCommand(item.Command);

                // the current pattern does not cut it anymore for adding the guid column
                new AddGuidColumn(ctx);
            }
        }
    }
}