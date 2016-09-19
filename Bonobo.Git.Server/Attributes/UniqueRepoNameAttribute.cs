using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.App_GlobalResources;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Attributes
{
    public class UniqueRepoNameAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null)
            {
                return new ValidationResult("empty repo name?");
            }

            IRepositoryRepository RepositoryRepository = DependencyResolver.Current.GetService<IRepositoryRepository>();
            if (RepositoryRepository.NameIsUnique(value.ToString(), ((RepositoryDetailModel)context.ObjectInstance).Id))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(Resources.Validation_Duplicate_Name);
        }
    }
}