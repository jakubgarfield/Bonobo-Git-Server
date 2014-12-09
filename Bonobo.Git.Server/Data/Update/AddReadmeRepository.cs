using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update
{
    public class AddReadmeRepository : IUpdateScript
    {
        public string Command
        {
            get
            {
                return @"
                    
                ALTER TABLE Repository ADD COLUMN Readme VarChar(20000) NULL
";
            }
        }

        public string Precondition
        {
            get
            {
                return "SELECT Count(Readme) = -1 FROM [Repository]";
            }
        }
    }
}