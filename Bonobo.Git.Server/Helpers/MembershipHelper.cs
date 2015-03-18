using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace Bonobo.Git.Server.Helpers
{
    public class MembershipHelper
    {
        public static string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            if (!string.IsNullOrWhiteSpace(appUrl)) appUrl += "/";

            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }

        public static bool SendForgotPasswordEmail(User user, string token)
        {
            bool result = true;
            try
            {
                var passwordLink = MembershipHelper.GetBaseUrl() +
                  "Home/ResetPassword?digest=" +
                  HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(token));

                var email = new MailMessage();

                //email.From = new MailAddress("admin@domain.com");
                email.To.Add(new MailAddress(user.Email));

                email.Subject =  Resources.Email_PasswordReset_Title;
                email.IsBodyHtml = true;

                email.Body = Resources.Email_PasswordReset_Body +
                           "<a href='" + passwordLink + "'>" + passwordLink + "</a>";

                SmtpClient smtpClient = new SmtpClient();

                if (ConfigurationManager.AppSettings["smtpHost"] != null) { 
                    smtpClient.Host = ConfigurationManager.AppSettings["smtpHost"];
                }

                int port = 25;
                int.TryParse(ConfigurationManager.AppSettings["smtpPort"], out port);
                smtpClient.Port = port;

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["smtpUsername"]) && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["smtpPassword"])) {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["smtpUsername"], ConfigurationManager.AppSettings["smtpPassword"]); 
                }

                email.From = new MailAddress(ConfigurationManager.AppSettings["smtpSenderAddress"]);

                smtpClient.Send(email);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}