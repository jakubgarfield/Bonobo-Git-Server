using System;
using System.Web;

namespace TSharp.Core.Mvc
{
    public class DefaultSmartCaptcha : ISmartCaptcha
    {
        private static readonly string Key_LoginErrorCount = "Error_" + typeof(DefaultSmartCaptcha).FullName;
        private readonly Func<HttpContextBase, int> _maxAttemptLogonFail = x => 1;


        public DefaultSmartCaptcha(Func<HttpContextBase, int> maxAttemptLogonFail)
        {
            if (maxAttemptLogonFail == null)
                throw new ArgumentNullException("maxAttemptLogonFail");
            _maxAttemptLogonFail = maxAttemptLogonFail;
        }

        public bool Enable(HttpContextBase context)
        {
            var helper = context.Session[Key_LoginErrorCount] as CountHelper;
            if (helper != null)
                return helper.Count >= _maxAttemptLogonFail(context);
            return false;
        }


        public static void IncreaseLoginFail(HttpContextBase context)
        {
            var helper = context.Session[Key_LoginErrorCount] as CountHelper;
            if (helper == null)
                context.Session[Key_LoginErrorCount] = helper = new CountHelper();
            helper.Increase();
        }

        public static void LoginSuccess(HttpContextBase context)
        {
            var helper = context.Session[Key_LoginErrorCount] as CountHelper;
            if (helper != null)
                helper.Reset();
        }

        private class CountHelper
        {
            public CountHelper()
            {
                Count = 0;
            }

            public int Count { get; private set; }

            public CountHelper Increase()
            {
                Count += 1;
                return this;
            }

            public CountHelper Reset()
            {
                Count = 0;
                return this;
            }
        }
    }
}