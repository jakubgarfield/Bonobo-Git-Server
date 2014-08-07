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

        private void UpdateDatabase()
        {
            using (var ctx = new BonoboGitServerContext())
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
                        catch(Exception)
                        {
                            // consider failures in pre-conditions as an indication that
                            // store ecommand should be executed
                        }
                    }
                    ctxAdapter.ObjectContext.ExecuteStoreCommand(item.Command);
                }
            }
        }
    }
}