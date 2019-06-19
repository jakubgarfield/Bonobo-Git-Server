using Bonobo.Git.Server.Data;
using Microsoft.AspNetCore.Mvc.Filters;

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
        public IRepositoryRepository RepositoryRepository { get; set; }

        private readonly string _repositoryNameParameterName;

        public RepositoryNameNormalizerAttribute(IRepositoryRepository repositoryRepository, string repositoryNameParameterName)
        {
            RepositoryRepository = repositoryRepository;
            _repositoryNameParameterName = repositoryNameParameterName;
        }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            object incomingRepositoryNameParameter;
            if(filterContext.ActionArguments.TryGetValue(_repositoryNameParameterName, out incomingRepositoryNameParameter))
            {
                var incomingRepositoryName = (string)incomingRepositoryNameParameter;
                var normalizedName = Repository.NormalizeRepositoryName(incomingRepositoryName, RepositoryRepository);
                if (normalizedName != incomingRepositoryName)
                {
                    // We've had to correct the incoming repository name
                    filterContext.ActionArguments[_repositoryNameParameterName] = normalizedName;
                }
            }
        }
    }
}
