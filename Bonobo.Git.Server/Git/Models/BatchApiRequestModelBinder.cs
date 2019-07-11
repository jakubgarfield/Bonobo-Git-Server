using System;
using System.Web.Mvc;
using Serilog;

namespace Bonobo.Git.Server.Git.Models
{
    public class BatchApiRequestModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            using (System.IO.Stream bodyInputStream = controllerContext.HttpContext.Request.GetBufferedInputStream())
            {
                using (System.IO.StreamReader bodyReader = new System.IO.StreamReader(bodyInputStream))
                {
                    string bodyText = bodyReader.ReadToEnd();
                    try
                    {
                        object result = Newtonsoft.Json.JsonConvert.DeserializeObject<BatchApiRequest>(bodyText);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Log.Information($"Error parsing request: {bodyText}");
                        Log.Information(ex, "");
                        return null;
                    }
                }
            }
        }
    }
}