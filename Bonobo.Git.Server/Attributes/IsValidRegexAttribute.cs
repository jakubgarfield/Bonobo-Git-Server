using Bonobo.Git.Server.App_GlobalResources;
using System;
using System.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

namespace Bonobo.Git.Server.Attributes
{
    public class IsValidRegexAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            try{
                new Regex((string)value);
                return ValidationResult.Success;
            }catch(ArgumentException e){
                return new ValidationResult(string.Format(Resources.Validation_Invalid_Regex, e.Message));
            }
        }
    }
}