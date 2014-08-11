using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    public interface IReceivePackRepository
    {
        void Add(ParsedReceivePack receivePack);
        IEnumerable<ParsedReceivePack> All();
        void Delete(string packId);        
    }
}