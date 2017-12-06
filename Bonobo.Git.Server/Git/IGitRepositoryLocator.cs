using System.IO;

namespace Bonobo.Git.Server.Git
{
    public interface IGitRepositoryLocator
    {
        DirectoryInfo GetRepositoryDirectoryPath(string repository);
    }
}