using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TSharp.Core.Mvc
{
    public interface ISmartCaptcha
    {
        bool Enable(HttpContextBase context);
    }

    public class DefaultSmartCaptcha : ISmartCaptcha
    {
        private readonly Func<HttpContextBase, int> _maxAttemptLogonFail = x => 1;

        private class CountHelper
        {
            private int count = 0;

            public int Count
            {
                get
                {
                    return count;
                }
            }

            public CountHelper Increase()
            {
                count += 1;
                return this;
            }
            public CountHelper Reset()
            {
                count = 0;
                return this;
            }
        }

        private static string Key_LoginErrorCount = "Error_" + typeof(DefaultSmartCaptcha).FullName;


        public DefaultSmartCaptcha(Func<HttpContextBase, int> maxAttemptLogonFail)
        {
            if (maxAttemptLogonFail == null)
                throw new ArgumentNullException(nameof(maxAttemptLogonFail));
            _maxAttemptLogonFail = maxAttemptLogonFail;
        }
        

        public static void IncreaseLoginFail(HttpContextBase context)
        {
            var helper = context.Session[Key_LoginErrorCount] as CountHelper;
            if (helper == null)
            {
                context.Session[Key_LoginErrorCount] = helper = new CountHelper();
            }
            helper.Increase();
        }
        public static void LoginSuccess(HttpContextBase context)
        {
            var helper = context.Session[Key_LoginErrorCount] as CountHelper;
            if (helper != null)
            {
                helper.Reset();
            }
        }
        public bool Enable(HttpContextBase context)
        {
            var helper = context.Session[Key_LoginErrorCount] as CountHelper;
            if (helper != null)
            {
                return helper.Count >= _maxAttemptLogonFail(context);
            }
            return false;
        }
    }
}