using Bonobo.Git.Server.App_GlobalResources;
using System;
using System.Net.Mail;
using Bonobo.Git.Server.Models;
using Serilog;

namespace Bonobo.Git.Server.Helpers
{
    public static class MembershipHelper
    {
        public static bool SendForgotPasswordEmail(UserModel user, string passwordResetUrl)
        {
            bool result = true;
            try
            {
                var email = new MailMessage();

                //email.From = new MailAddress("admin@domain.com");
                email.To.Add(new MailAddress(user.Email));

                email.Subject =  Resources.Email_PasswordReset_Title;
                email.IsBodyHtml = true;

                email.Body = Resources.Email_PasswordReset_Body +
                           "<a href='" + passwordResetUrl + "'>" + passwordResetUrl + "</a>";

                SmtpClient smtpClient = new SmtpClient();

                smtpClient.Send(email);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Caught exception sending password reset email ");
                result = false;
            }
            return result;
        }
    }
}