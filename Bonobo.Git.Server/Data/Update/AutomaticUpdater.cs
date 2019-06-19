using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data.Update.ADBackendUpdate;
using System;
using Bonobo.Git.Server.Extensions;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bonobo.Git.Server.Data.Update
{
    public class AutomaticUpdater
    {
        public void Run(BonoboGitServerContext context, IAuthenticationProvider authenticationProvider, IHostingEnvironment hostingEnvironment)
        {
            if (AuthenticationSettings.MembershipService.ToLowerInvariant() == "activedirectory")
            {
                Pre600UpdateTo600.UpdateADBackend(hostingEnvironment);
            }
            else
            {
                RunWithContext(context, authenticationProvider);
            }
        }

        public void RunWithContext(BonoboGitServerContext context, IAuthenticationProvider authenticationProvider)
        {
            DoUpdate(context, authenticationProvider);
        }

        private void DoUpdate(BonoboGitServerContext ctx, IAuthenticationProvider authenticationProvider)
        {
            foreach (var item in UpdateScriptRepository.GetScriptsBySqlProviderName(ctx.Database.ProviderName, authenticationProvider))
            {
                if (!string.IsNullOrEmpty(item.Precondition))
                {
                    try
                    {
                        var preConditionResult = Convert.ToInt32(ctx.Database.ExecuteScalar(item.Precondition));
                        if (preConditionResult == 0)
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
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