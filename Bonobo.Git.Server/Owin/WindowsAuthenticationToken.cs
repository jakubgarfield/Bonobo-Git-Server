using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Owin.Windows
{
    internal enum AuthenticationStage
    {
        Unauthenticated,
        Request,
        Challenge,
        Response
    }

    internal class WindowsAuthenticationToken
    {
        public byte[] Data { get; set; }

        public string Challenge
        {
            get
            {
                string result = null;

                if (AuthorizationStage == AuthenticationStage.Challenge)
                {
                    result = Convert.ToBase64String(Data);
                }

                return result;
            }
        }

        public AuthenticationStage AuthorizationStage
        {
            get
            {
                AuthenticationStage result = AuthenticationStage.Unauthenticated;

                if (Data != null && Data.Length > 8)
                {
                    switch (Data[8])
                    {
                        case 1:
                            result = AuthenticationStage.Request;
                            break;
                        case 2:
                            result = AuthenticationStage.Challenge;
                            break;
                        case 3:
                            result = AuthenticationStage.Response;
                            break;
                    }
                }

                return result;
            }
        }

        public static WindowsAuthenticationToken Create(string headerValue)
        {
            byte[] data = null;

            if (!string.IsNullOrEmpty(headerValue) && headerValue.StartsWith("NTLM "))
            {
                data = Convert.FromBase64String(headerValue.Substring(5));
            }

            return new WindowsAuthenticationToken(data);
        }

        private WindowsAuthenticationToken(byte[] data)
        {
            Data = data;
        }
    }
}