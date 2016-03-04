using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class AddRepoPushColumn : IUpdateScript
    {
        public string Command
        {
            get
            {
                return "ALTER TABLE Repository ADD [AllowAnonymousPush] Integer NOT NULL CONSTRAINT Def_Var_AAP Default 3";
            }
        }

        public string Precondition
        {
            get
            {
                return @"
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Repository' AND  COLUMN_NAME = 'AllowAnonymousPush')
                SELECT 0
            ELSE
                SELECT 1";
            }
        }

        public void CodeAction(BonoboGitServerContext context) { }

    }
}