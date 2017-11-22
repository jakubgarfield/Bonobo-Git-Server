using System.IO;

namespace Bonobo.Git.Server.Git
{
    public class ConfigurationBasedRepositoryLocator : IGitRepositoryLocator
    {
        private readonly string repositoryBasePath;

        public ConfigurationBasedRepositoryLocator(string repositoryBasePath)
        {
            this.repositoryBasePath = repositoryBasePath;
        }

        public DirectoryInfo GetRepositoryDirectoryPath(string repository)
        {
            return new DirectoryInfo(Path.Combine(repositoryBasePath, repository));
        }
    }
}