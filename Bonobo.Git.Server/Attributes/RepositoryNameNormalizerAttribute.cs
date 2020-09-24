using Bonobo.Git.Server.Data;
using System.Web.Mvc;
using Unity;

namespace Bonobo.Git.Server
{
    /// <summary>
    /// Applied to a Controller or Action, this attribute will ensure that a repo name has its case corrected to match that in the database
    /// If the name doesn't match anything in the database, then it's returned unchanged
    /// The name of the action parameter to be corrected is passed as a constructor parameter
    /// Normalising the name of the repos at this early stage means that later stages do not have to be concerned about trying 
    /// to do case-insensitive lookups in databases
    /// </summary>
    public class RepositoryNameNormalizerAttribute : ActionFilterAttribute
    {
        private readonly string _repositoryNameParameterName;

        public RepositoryNameNormalizerAttribute(string repositoryNameParameterName)
        {
            _repositoryNameParameterName = repositoryNameParameterName;
        }

        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            object incomingRepositoryNameParameter;
            if (filterContext.ActionParameters.TryGetValue(_repositoryNameParameterName, out incomingRepositoryNameParameter))
            {
                var incomingRepositoryName = (string)incomingRepositoryNameParameter;
                var normalizedName = Repository.NormalizeRepositoryName(incomingRepositoryName, RepositoryRepository);
                if (normalizedName != incomingRepositoryName)
                {
                    // We've had to correct the incoming repository name
                    filterContext.ActionParameters[_repositoryNameParameterName] = normalizedName;
                }
            }
        }
    }
}
