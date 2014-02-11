using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public class FileResult : ActionResult
    {
        private readonly string _data;
        private readonly string _name;

        public FileResult(string data, string name)
        {
            _data = data;
            _name = name;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            if (!String.IsNullOrEmpty(_name))
            {
                context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + _name);
            }

            context.HttpContext.Response.Write(_data);
        }
    }
}