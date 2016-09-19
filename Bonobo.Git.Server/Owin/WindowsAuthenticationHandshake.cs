using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Bonobo.Git.Server.Owin.Windows
{
    internal class WindowsAuthenticationHandshake : IDisposable
    {
        public AuthenticationProperties AuthenticationProperties { get; set; }
        public string AuthenticatedUsername { get; private set; }

        private const int ISC_REQ_REPLAY_DETECT = 0x00000004;
        private const int ISC_REQ_SEQUENCE_DETECT = 0x00000008;
        private const int ISC_REQ_CONFIDENTIALITY = 0x00000010;
        private const int ISC_REQ_CONNECTION = 0x00000800;
        private const int MaximumTokenSize = 12288;
        private const int SecurityCredentialsInbound = 1;
        private const int StandardContextAttributes = ISC_REQ_CONFIDENTIALITY | ISC_REQ_REPLAY_DETECT | ISC_REQ_SEQUENCE_DETECT | ISC_REQ_CONNECTION;
        private const int SecurityNativeDataRepresentation = 0x10;
        private const int IntermediateResult = 0x90312;

        private SecurityHandle credentials;
        private SecurityHandle context;
        private bool disposed = false;

        public bool TryAcquireServerChallenge(WindowsAuthenticationToken message)
        {
            bool result = false;

            SecurityBufferDesciption clientToken = new SecurityBufferDesciption(message.Data);
            SecurityBufferDesciption serverToken = new SecurityBufferDesciption(MaximumTokenSize);

            try
            {
                SecurityInteger lifetime = new SecurityInteger(0);
                uint contextAttributes;

                if (NativeMethods.AcquireCredentialsHandle(null, "NTLM", SecurityCredentialsInbound, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, ref credentials, ref lifetime) == 0)
                {
                    if (NativeMethods.AcceptSecurityContext(ref credentials, IntPtr.Zero, ref clientToken, StandardContextAttributes, SecurityNativeDataRepresentation, out context, out serverToken, out contextAttributes, out lifetime) == IntermediateResult)
                    {
                        result = true;
                    }
                }
            }
            finally
            {
                message.Data = serverToken.GetBytes();
                clientToken.Dispose();
                serverToken.Dispose();
            }

            return result;
        }

        public bool IsClientResponseValid(WindowsAuthenticationToken token)
        {
            bool result = false;

            SecurityBufferDesciption clientToken = new SecurityBufferDesciption(token.Data);
            SecurityBufferDesciption serverToken = new SecurityBufferDesciption(MaximumTokenSize);
            IntPtr securityContextHandle = IntPtr.Zero;

            try
            {
                uint contextAttributes;
                var lifetime = new SecurityInteger(0);

                if (NativeMethods.AcceptSecurityContext(ref credentials, ref context, ref clientToken, StandardContextAttributes, SecurityNativeDataRepresentation, out context, out serverToken, out contextAttributes, out lifetime) == 0)
                {
                    if (NativeMethods.QuerySecurityContextToken(ref context, ref securityContextHandle) == 0)
                    {
                        using (WindowsIdentity identity = new WindowsIdentity(securityContextHandle))
                        { 
                            if (identity != null)
                            {
                                AuthenticatedUsername = identity.Name;
                                result = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                clientToken.Dispose();
                serverToken.Dispose();
                NativeMethods.CloseHandle(securityContextHandle);
                credentials.Reset();
                context.Reset();
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                context.Reset();
                credentials.Reset();
                disposed = true;
            }
        }

        ~WindowsAuthenticationHandshake()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public WindowsAuthenticationHandshake()
        {
            credentials = new SecurityHandle(0);
            context = new SecurityHandle(0);
        }
    }
}