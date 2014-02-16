using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

public class UnityFilterAttributeFilterProvider : FilterAttributeFilterProvider
{
    private readonly IUnityContainer _container;

    public UnityFilterAttributeFilterProvider(IUnityContainer container)
    {
        _container = container;
    }

    protected override IEnumerable<FilterAttribute> GetControllerAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
    {
        var attributes = base.GetControllerAttributes(controllerContext, actionDescriptor).ToList();
        foreach (var attribute in attributes)
        {
            _container.BuildUp(attribute.GetType(), attribute);
        }

        return attributes;
    }

    protected override IEnumerable<FilterAttribute> GetActionAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
    {
        var attributes = base.GetActionAttributes(controllerContext, actionDescriptor).ToList();
        foreach (var attribute in attributes)
        {
            _container.BuildUp(attribute.GetType(), attribute);
        }

        return attributes;
    }
}