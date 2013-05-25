using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Bonobo.Git.Server.Extensions
{
    public static class TypeExtensions
    {
        public static string GetDisplayValue(this Type type, string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName)) throw new ArgumentException("propertyName");

            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo == null) 
                throw new InvalidOperationException("Type with this property does not exists");
            
            var displayAttribute = propertyInfo.GetCustomAttributes(true).FirstOrDefault(i => i.GetType().IsAssignableFrom(typeof(DisplayAttribute))) as DisplayAttribute;

            if (displayAttribute != null)
            {
                return displayAttribute.GetName();
            }

            return propertyName;
        }
    }
}