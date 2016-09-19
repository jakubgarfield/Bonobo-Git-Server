namespace Bonobo.Git.Server.Security
{
    public interface IPasswordService
    {
        string GetSaltedHash(string password, string salt);
        bool ComparePassword(string givenPassword, string userName, string knownSalt, string knownHash);
    }
}