using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update.Sqlite
{
    public class AddRepoPushColumn : IUpdateScript
    {
        public string Command
        {
            get
            {
                return "ALTER TABLE Repository ADD COLUMN [AllowAnonymousPush] INTEGER DEFAULT(3)";
            }
        }

        public string Precondition
        {
            get
            {
                return "SELECT Count([AllowAnonymousPush]) = -1 FROM Repository";
            }
        }

        public void CodeAction(BonoboGitServerContext context) { }

    }
}