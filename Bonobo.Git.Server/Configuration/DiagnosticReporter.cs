using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;

namespace Bonobo.Git.Server.Configuration
{
    /// <summary>
    /// This class can produce a textual diagnostic report of Bonobo's configuration
    /// The idea is to give a one-shot collection of everything which might be needed to help diagnose problems people are having
    /// It's written to be incredible defensive about all the checks it does, so that if things are misconfigured
    /// we can still get a complete report
    /// </summary>
    public class DiagnosticReporter
    {
        private readonly StringBuilder _report = new StringBuilder();
        private readonly UserConfiguration _userConfig = UserConfiguration.Current;

        public string GetVerificationReport()
        {
            RunReport();
            return _report.ToString();
        }

        private void RunReport()
        {
            DumpAppSettings();
            CheckUserConfigurationFile();
            CheckRepositoryDirectory();
            CheckGitSettings();
            CheckFederatedAuth();
            CheckADMembership();
            CheckInternalMembership();
            ExceptionLog();
        }

        
        private void DumpAppSettings()
        {
            _report.AppendLine("Web.Config AppSettings");
            foreach (string key in ConfigurationManager.AppSettings)
            {
                QuotedReport("AppSettings."+key, ConfigurationManager.AppSettings[key]);
            }
        }

        private void CheckUserConfigurationFile()
        {
            _report.AppendLine("User Configuration:");
            var configFile = MapPath(AppSetting("UserConfiguration"));
            QuotedReport("User config file", configFile);
            SafelyReport("User config readable", () => !String.IsNullOrEmpty(File.ReadAllText(configFile)));
            SafelyReport("User config saveable", () =>
            {
                UserConfiguration.Current.Save();
                return true;
            });
            ReportDirectoryStatus("User config folder", Path.GetDirectoryName(configFile));

        }

        private void CheckRepositoryDirectory()
        {
            _report.AppendLine("Repo Directory");
            QuotedReport("Configured repo path", _userConfig.RepositoryPath);
            QuotedReport("Effective repo path", _userConfig.Repositories);
            ReportDirectoryStatus("Repo dir", _userConfig.Repositories);
        }

        private void CheckGitSettings()
        {
            _report.AppendLine("Git Exe");
            var gitPath = MapPath(AppSetting("GitPath"));
            QuotedReport("Git path", gitPath);
            SafelyReport("Git.exe exists", () => File.Exists(gitPath));
        }

        private void CheckFederatedAuth()
        {
            _report.AppendLine("Federated Authentication");
            if (AppSetting("AuthenticationProvider") == "Federation")
            {
                SafelyReport("Metadata available", () =>
                {
                    WebClient client = new WebClient();
                    var metadata = client.DownloadString(AppSetting("FederationMetadataAddress"));
                    return !String.IsNullOrWhiteSpace(metadata);
                });

            }
            else
            {
                Report("Not Enabled", "");
            }
        }

        private void CheckADMembership()
        {
            _report.AppendLine("Active Directory");

            if (AppSetting("MembershipService") == "ActiveDirectory")
            {
                SafelyReport("Backend folder exists", () => Directory.Exists(MapPath(AppSetting("ActiveDirectoryBackendPath"))));
                ReportDirectoryStatus("Backend folder", MapPath(AppSetting("ActiveDirectoryBackendPath")));

                var ad = ADBackend.Instance;
                SafelyReport("User count", () => ad.Users.Count());

                _report.AppendLine("AD Teams");
                SafelyRun(() =>
                {
                    foreach (var item in ad.Teams)
                    {
                        var thisTeam = item;
                        SafelyReport(item.Name, () => thisTeam.Members.Length + " members");
                    }
                });
                _report.AppendLine("AD Roles");
                SafelyRun(() =>
                {
                    foreach (var item in ad.Roles)
                    {
                        var thisRole = item;
                        SafelyReport(item.Name, () => thisRole.Members.Length + " members");
                    }
                });
            }
            else
            {
                Report("Not Enabled");
            }
        }

        private void ReportDirectoryStatus(string text, string directory)
        {
            var sb = new StringBuilder();
            if (Directory.Exists(directory))
            {
                sb.AppendFormat("Exists, {0} files, {1} entries, ", 
                    Directory.GetFiles(directory).Length,
                    Directory.GetFileSystemEntries(directory).Length
                    );
                sb.Append(DirectoryIsWritable(directory) ? "writeable" : "NOT WRITEABLE");
            }
            else
            {
                sb.Append("Doesn't exist");
            }
            Report(text, sb.ToString());
        }

        private bool DirectoryIsWritable(string directory)
        {
            string probeFile = Path.Combine(directory, "Probe.txt");
            try
            {
                File.WriteAllBytes(probeFile, new byte[16]);
                return true;
            }
            catch (Exception ex)
            {
                Report("Exception probing dir " + directory, ex.Message);
                return false;
            }
            finally
            {
                try
                {
                    File.Delete(probeFile);
                }
                catch
                {
                    // We deliberately ignore these exceptions, we don't care
                }
            }
        }

        private void CheckInternalMembership()
        {
            _report.AppendLine("Internal Membership");

            if (AppSetting("MembershipService") == "Internal")
            {
                SafelyReport("User count", () => new EFMembershipService { CreateContext = () => new BonoboGitServerContext() }.GetAllUsers().Count);
            }
            else
            {
                Report("Not Enabled");
            }
        }

        /// <summary>
        /// Append the last 10K of the exception log to the report
        /// </summary>
        private void ExceptionLog()
        {
            _report.AppendLine("**********************************************************************************");
            _report.AppendLine("Exception Log");
            SafelyRun(() =>
            {
                var nameFormat = MvcApplication.GetLogFileNameFormat();
                var todayLogFileName = nameFormat.Replace("{Date}", DateTime.Now.ToString("yyyyMMdd"));
                SafelyReport("LogFileName: ", () => todayLogFileName);
                var chunkSize = 10000;
                var length = new FileInfo(todayLogFileName).Length;
                Report("Log File total length", length);

                var startingPoint = Math.Max(0, length - chunkSize);
                Report("Starting log dump from ", startingPoint);

                using (var logText = File.Open(todayLogFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    logText.Seek(startingPoint, SeekOrigin.Begin);
                    var reader = new StreamReader(logText);
                    _report.AppendLine(reader.ReadToEnd());
                }
            });
        }

        private void SafelyRun(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Report("Diag error", FormatException(ex));
            }
        }

        private void SafelyReport(string tag, Func<object> func)
        {
            try
            {
                object result = func();
                if (result is bool)
                {
                    if ((bool)result)
                    {
                        Report(tag, "OK");
                    }
                    else
                    {
                        Report(tag, "FAIL");
                    }
                }
                else
                {
                    Report(tag, result.ToString());
                }
            }
            catch (Exception ex)
            {
                Report(tag, FormatException(ex));
            }
        }

        private string MapPath(string path)
        {
            return Path.IsPathRooted(path) ? path : HostingEnvironment.MapPath(path);
        }

        private string AppSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        private static string FormatException(Exception ex)
        {
            return "EXCEPTION: " + ex.ToString().Replace("\r\n", "***");
        }

        private void Report(string tag, object value = null)
        {
            if (value != null)
            {
                _report.AppendFormat("--{0}: {1}" + Environment.NewLine, tag, value);
            }
            else
            {
                _report.AppendLine("--" + tag);
            }
        }

        private void QuotedReport(string tag, object value)
        {
            Report(tag, "'"+value+"'");
        }
    }
}

