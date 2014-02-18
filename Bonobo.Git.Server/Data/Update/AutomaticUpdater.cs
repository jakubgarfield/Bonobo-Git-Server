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
                        var preConditionResult = ctxAdapter.ObjectContext.ExecuteStoreQuery<int>(item.Precondition).Single();
                        if (preConditionResult == 0)
                        {
                            continue;
                        }
                    }
                    ctxAdapter.ObjectContext.ExecuteStoreCommand(item.Command);
                }
            }
        }
    }
}