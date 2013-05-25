using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update
{
    public class UpdateScriptRepository
    {
        public IEnumerable<IUpdateScript> Scripts { get; private set; }

        /// <summary>
        /// Creates the list of scripts that should be executed on app start. Ordering matters!
        /// </summary>
        public UpdateScriptRepository()
        {            
            Scripts = new List<IUpdateScript>
            {
                new InitialCreateScript(),
                new InsertDefaultData(),
            };
        }
    }
}