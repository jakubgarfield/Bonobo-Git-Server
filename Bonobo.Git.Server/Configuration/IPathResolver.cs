namespace Bonobo.Git.Server.Configuration
{
    public interface IPathResolver
    {
        string ResolveWithConfiguration(string configKey);
        string Resolve(string path);
    }
}