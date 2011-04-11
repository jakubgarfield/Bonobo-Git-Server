using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public class FileResult : ActionResult
    {
        private string _data;
        private string _name;

        public FileResult(string data, string name)
        {
            _data = data;
            _name = name;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            if (!String.IsNullOrEmpty(_name))
            {
                context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + this._name);
            }

            context.HttpContext.Response.Write(_data);
        }
    }
}