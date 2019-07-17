namespace Bonobo.Git.Server.Git.Models
{
    public class BatchApiErrorResponse
    {
        public string Message { get; set; }
        public string Documentation_Url => "https://github.com/git-lfs/git-lfs/blob/master/docs/api/batch.md";
        public string Request_Id { get; set; }
    }
}
