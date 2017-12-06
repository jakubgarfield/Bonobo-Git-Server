namespace Bonobo.Git.Server.Configuration
{
    public class AuthenticationSettings
    {
        public string MembershipService { get; set; }
        public string AuthenticationProvider { get; set; }
        public bool ImportWindowsAuthUsersAsAdmin { get; set; }
        public bool DemoModeActive { get; set; }
    }
}