﻿using System.ComponentModel.DataAnnotations;

namespace Bonobo.Git.Server
{
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute() :
            base(@"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,63}$")
        {
        }
    }
}
