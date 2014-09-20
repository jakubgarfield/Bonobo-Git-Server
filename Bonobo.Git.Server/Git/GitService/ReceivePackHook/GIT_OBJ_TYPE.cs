using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public enum GIT_OBJ_TYPE
    {
        OBJ_COMMIT = 1,
        OBJ_TREE = 2,
        OBJ_BLOB = 3,
        OBJ_TAG = 4,
        OBJ_OFS_DELTA = 6,
        OBJ_REF_DELTA = 7
    }
}