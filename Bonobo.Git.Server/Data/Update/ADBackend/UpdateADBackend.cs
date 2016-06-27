using Bonobo.Git.Server.Data.Update.Pre600ADBackend;
using System.IO;
using Bonobo.Git.Server.Helpers;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using Bonobo.Git.Server.Configuration;
using System;
using System.Threading;
using System.Linq;

namespace Bonobo.Git.Server.Data.Update.ADBackendUpdate
{
    public class Pre600UpdateTo600
    {
        // Before 6.0.0 the mapping was done via the name property. After that the Guid is used.
        public static void UpdateADBackend()
        {
            // Make a copy of the current backendfolder if it exists, so we can use the modern models for saving
            // it all to the correct location directly
            var dir = PathEncoder.GetRootPath(ActiveDirectorySettings.BackendPath);
            var bkpdir = PathEncoder.GetRootPath(ActiveDirectorySettings.BackendPath + "_pre6.0.0");
            if (Directory.Exists(dir))
            {
                var mustConvert = false;
                foreach (var jsonfile in new DirectoryInfo(dir).EnumerateFiles("*.json", SearchOption.AllDirectories))
                {
                    // try to load with the modern models, if it succeeds we don't need to update
                    try
                    {
                        var hadFile = true;
                        switch (jsonfile.Directory.Name)
                        {
                            case "Roles":
                                {
                                    var x = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.RoleModel>(File.ReadAllText(jsonfile.FullName));
                                    if (x.Id == Guid.Empty)
                                    {
                                        mustConvert = true;
                                    }
                                    break;
                                }

                            case "Users":
                                {
                                    var x = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.UserModel>(File.ReadAllText(jsonfile.FullName));
                                    if (x.Id == Guid.Empty)
                                    {
                                        mustConvert = true;
                                    }
                                    break;
                                }

                            case "Teams":
                                {
                                    var x = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.TeamModel>(File.ReadAllText(jsonfile.FullName));
                                    if (x.Id == Guid.Empty)
                                    {
                                        mustConvert = true;
                                    }
                                    break;
                                }

                            case "Repos":
                                {
                                    var x = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.RepositoryModel>(File.ReadAllText(jsonfile.FullName));
                                    if (x.Id == Guid.Empty)
                                    {
                                        mustConvert = true;
                                    }
                                    break;
                                }

                            default:
                                // the user created some directory we don't know about...
                                hadFile = false;
                                continue;
                        }
                        if (hadFile)
                        {
                            break;
                        }
                    }
                    catch (Newtonsoft.Json.JsonSerializationException)
                    {
                        // We must convert...
                        mustConvert = true;
                        break;
                    }
                }
                if (!mustConvert)
                {
                    // It seems the directory is here but no json files exist.
                    // So there is nothing to update.
                    return;
                }

                if (!Directory.Exists(bkpdir))
                {
                    {
                        new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(dir, bkpdir);
                    }
                    // Make sure the Computer() instance has been disposed se we can delete the source directory
                    // If anyone knows a better way of doing this, please use it.
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (IOException) // System.IO.IOException: The directory is not empty
                    {
                        // Normally it's only the top most dir that fails to delete for some reason...
                        // If any json files are left we have a problem, if only folders are left
                        // we can let it update without any problems. Leaving the old json files
                        // means they would get loaded next run and crash the server.
                        var anyfile = new DirectoryInfo(dir).EnumerateFiles("*.json").FirstOrDefault();
                        if (anyfile != null)
                        {
                            throw;
                        }
                    }
                }

                // We must create one that will not automatically update the items while we update them
                ADBackend.ResetSingletonWithoutAutomaticUpdate();

                var newUsers = UpdateUsers(Path.Combine(bkpdir, "Users"));
                var newTeams = UpdateTeams(Path.Combine(bkpdir, "Teams"), newUsers);
                UpdateRoles(Path.Combine(bkpdir, "Roles"), newUsers);
                UpdateRepos(Path.Combine(bkpdir, "Repos"), newUsers, newTeams);

                // We are done, enable automatic update again.
                ADBackend.ResetSingletonWithAutomaticUpdate();
            }
        }

        private static void UpdateRepos(string dir, Dictionary<string, Models.UserModel> users, Dictionary<string, Models.TeamModel> teams)
        {
            var repos = Pre600Functions.LoadContent<Pre600RepositoryModel>(dir);
            foreach(var repoitem in repos)
            {
                var repo = repoitem.Value;
                var newrepo = new Models.RepositoryModel();
                newrepo.Name = repo.Name;
                newrepo.Group = repo.Group;
                newrepo.Description = repo.Description;
                newrepo.AnonymousAccess = repo.AnonymousAccess;
                newrepo.AuditPushUser = repo.AuditPushUser;
                newrepo.Logo = repo.Logo;
                newrepo.RemoveLogo = repo.RemoveLogo;

                var list = new List<Models.UserModel>();
                foreach (var user in repo.Users)
                {
                    list.Add(users[user]);
                }
                newrepo.Users = list.ToArray();

                list.Clear();
                foreach(var admins in repo.Administrators)
                {
                    list.Add(users[admins]);
                }
                newrepo.Administrators = list.ToArray();

                var newteams = new List<Models.TeamModel>();
                foreach(var team in repo.Teams)
                {
                    newteams.Add(teams[team]);
                }
                newrepo.Teams = newteams.ToArray();

                ADBackend.Instance.Repositories.Add(newrepo);
            }
        }

        private static void UpdateRoles(string dir, Dictionary<string, Models.UserModel> users)
        {
            var roles = Pre600Functions.LoadContent<Pre600RoleModel>(dir);
            foreach(var roleitem in roles)
            {
                var role = roleitem.Value;
                var newrole = new Models.RoleModel();
                newrole.Name = role.Name;

                newrole.Id = Guid.NewGuid();
                var members = new List<Guid>();
                foreach (var member in role.Members) {
                    members.Add(users[member].Id);
                }
            }
        }

        private static Dictionary<string, Models.TeamModel> UpdateTeams(string dir, Dictionary<string, Models.UserModel> users)
        {
            var teams = Pre600Functions.LoadContent<Pre600TeamModel>(dir);
            var newTeams = new Dictionary<string, Models.TeamModel>();
            foreach(var teamitem in teams)
            {
                var team = teamitem.Value;
                var newteam = new Models.TeamModel();
                newteam.Name = team.Name;
                newteam.Description = team.Description;

                newteam.Id = Guid.NewGuid();
                var members = new List<Models.UserModel>();
                foreach (var member in team.Members)
                {
                    members.Add(users[member]);
                }
                newteam.Members = members.ToArray();

                ADBackend.Instance.Teams.Add(newteam);
                newTeams[team.Name] = newteam;
            }
            return newTeams;
        }

        private static Dictionary<string, Models.UserModel> UpdateUsers(string dir)
        {
            var users = Pre600Functions.LoadContent<Pre600UserModel>(dir);
            var domains = new Dictionary<string, PrincipalContext>();
            var newUsers = new Dictionary<string, Models.UserModel>();
            foreach (var user in users)
            {
                var ou = user.Value;
                var u = new Models.UserModel();
                u.Email = ou.Email;
                u.GivenName = ou.GivenName;
                u.Surname = ou.Surname;
                u.Username = ou.Name;
                u.Id = Guid.NewGuid();

                PrincipalContext dc;
                var domain = u.Username.GetDomain();
                if (!domains.TryGetValue(domain, out dc))
                {
                    dc = new PrincipalContext(ContextType.Domain, domain);
                    domains[domain] = dc;
                }

                var domainuser = UserPrincipal.FindByIdentity(dc, u.Username);
                if (domainuser != null)
                {
                    u.Id = domainuser.Guid.Value;
                }

                ADBackend.Instance.Users.Add(u);
                newUsers[u.Username] = u;
            }
            return newUsers;
        }
    }
}
