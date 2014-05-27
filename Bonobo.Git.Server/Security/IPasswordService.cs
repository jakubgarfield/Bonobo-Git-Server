namespace Bonobo.Git.Server.Security
{
    public interface IPasswordService
    {
        string GetSaltedHash(string password, string salt);
        bool ComparePassword(string givenPassword, string knownSalt, string knownHash);
    }
}