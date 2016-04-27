using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonobo.Git.Server.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> AllResults { get; set; }

        public IEnumerable<T> Result { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
