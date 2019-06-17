using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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


        public override void ExecuteResult(ActionContext context)
        {
            if (!string.IsNullOrEmpty(_name))
            {
                context.HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=" + _name);
            }

            context.HttpContext.Response.WriteAsync(_data).RunSynchronously();
        }
    }
}