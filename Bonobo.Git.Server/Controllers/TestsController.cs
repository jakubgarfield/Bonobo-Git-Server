using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Bonobo.Git.Server.App_GlobalResources;

namespace Bonobo.Git.Server.Controllers
{
    public class TestsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult TwitterBootstrap()
		{
			return View();
		}
	
	}
}
