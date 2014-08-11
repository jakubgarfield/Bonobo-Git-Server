using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    public class EFReceivePackRepository : IReceivePackRepository
    {
        public void Add(Git.GitService.ReceivePackHook.ParsedReceivePack receivePack)
        {
            using (var db = new BonoboGitServerContext())
            {
                var packData = new ReceivePackData()
                {
                    Data = JsonConvert.SerializeObject(receivePack),
                    EntryTimestamp = receivePack.Timestamp,
                    PackId = receivePack.PackId
                };

                db.ReceivePackData.Add(packData);
                db.SaveChanges();
            }
        }

        public IEnumerable<Git.GitService.ReceivePackHook.ParsedReceivePack> All()
        {
            using (var db = new BonoboGitServerContext())
            {
                return (from rpd in db.ReceivePackData.ToList()
                        select JsonConvert.DeserializeObject<ParsedReceivePack>(rpd.Data)).ToList();
            }
        }

        public void Delete(string packId)
        {
            using (var db = new BonoboGitServerContext())
            {
                var packData = db.ReceivePackData.FirstOrDefault(rpd => rpd.PackId == packId);

                // don't fail if that packId entry doesn't exist
                if (packData != null)
                {
                    db.ReceivePackData.Remove(packData);
                    db.SaveChanges();
                }
            }
        }
    }
}