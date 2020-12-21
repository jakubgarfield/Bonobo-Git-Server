﻿using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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