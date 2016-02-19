using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
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

                try
                {
                    ctxAdapter.ObjectContext.ExecuteStoreCommand(item.Command);
                }
                catch(Exception ex)
                {
                    Trace.TraceError("Exception while processing upgrade script {0}\r\n{1}", item.Command, ex);
                    throw;
                }
            }
            // the current pattern does not cut it anymore for adding the guid column

            if (connectiontype.Equals("SQLiteConnection"))
            {
                new Sqlite.AddGuidColumn(ctx);
            }
            else
            {
                new SqlServer.AddGuidColumn(ctx);
            }

        }
    }
}