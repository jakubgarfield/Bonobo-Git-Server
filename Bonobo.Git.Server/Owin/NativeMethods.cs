using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace Bonobo.Git.Server.Owin.Windows
{
    enum SecurityBufferType
    {
        SECBUFFER_VERSION = 0,
        SECBUFFER_EMPTY = 0,
        SECBUFFER_DATA = 1,
        SECBUFFER_TOKEN = 2
    }

    struct SecurityBufferWrapper
    {
        public byte[] Buffer;
        public SecurityBufferType BufferType;

        public SecurityBufferWrapper(byte[] buffer, SecurityBufferType bufferType)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("Buffer cannot be null or zero length");
            }

            Buffer = buffer;
            BufferType = bufferType;
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityHandle
    {
        public IntPtr LowPart;
        public IntPtr HighPart;

        public SecurityHandle(int dummy)
        {
            LowPart = IntPtr.Zero;
            HighPart = IntPtr.Zero;
        }

        public void Reset()
        {
            LowPart = IntPtr.Zero;
            HighPart = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityInteger
    {
        public uint LowPart;
        public int HighPart;

        public SecurityInteger(int dummy)
        {
            LowPart = 0;
            HighPart = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityBuffer : IDisposable
    {
        public int cbBuffer;
        public int cbBufferType;
        public IntPtr pvBuffer;

        public SecurityBuffer(int bufferSize)
        {
            cbBuffer = bufferSize;
            cbBufferType = (int)SecurityBufferType.SECBUFFER_TOKEN;
            pvBuffer = Marshal.AllocHGlobal(bufferSize);
        }

        public SecurityBuffer(byte[] secBufferBytes)
        {
            cbBuffer = secBufferBytes.Length;
            cbBufferType = (int)SecurityBufferType.SECBUFFER_TOKEN;
            pvBuffer = Marshal.AllocHGlobal(cbBuffer);
            Marshal.Copy(secBufferBytes, 0, pvBuffer, cbBuffer);
        }

        public SecurityBuffer(byte[] secBufferBytes, SecurityBufferType bufferType)
        {
            cbBuffer = secBufferBytes.Length;
            cbBufferType = (int)bufferType;
            pvBuffer = Marshal.AllocHGlobal(cbBuffer);
            Marshal.Copy(secBufferBytes, 0, pvBuffer, cbBuffer);
        }

        public void Dispose()
        {
            if (pvBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pvBuffer);
                pvBuffer = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityBufferDesciption : IDisposable
    {

        public int ulVersion;
        public int cBuffers;
        public IntPtr pBuffers; //Point to SecBuffer

        public SecurityBufferDesciption(int bufferSize)
        {
            ulVersion = (int)SecurityBufferType.SECBUFFER_VERSION;
            cBuffers = 1;
            SecurityBuffer ThisSecBuffer = new SecurityBuffer(bufferSize);
            pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(ThisSecBuffer));
            Marshal.StructureToPtr(ThisSecBuffer, pBuffers, false);
        }

        public SecurityBufferDesciption(byte[] secBufferBytes)
        {
            ulVersion = (int)SecurityBufferType.SECBUFFER_VERSION;
            cBuffers = 1;
            SecurityBuffer ThisSecBuffer = new SecurityBuffer(secBufferBytes);
            pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(ThisSecBuffer));
            Marshal.StructureToPtr(ThisSecBuffer, pBuffers, false);
        }

        public void Dispose()
        {
            if (pBuffers != IntPtr.Zero)
            {
                if (cBuffers == 1)
                {
                    SecurityBuffer ThisSecBuffer = (SecurityBuffer)Marshal.PtrToStructure(pBuffers, typeof(SecurityBuffer));
                    ThisSecBuffer.Dispose();
                }
                else
                {
                    for (int Index = 0; Index < cBuffers; Index++)
                    {
                        int CurrentOffset = Index * Marshal.SizeOf(typeof(Buffer));
                        IntPtr SecBufferpvBuffer = Marshal.ReadIntPtr(pBuffers, CurrentOffset + Marshal.SizeOf(typeof(int)) + Marshal.SizeOf(typeof(int)));
                        Marshal.FreeHGlobal(SecBufferpvBuffer);
                    }
                }

                Marshal.FreeHGlobal(pBuffers);
                pBuffers = IntPtr.Zero;
            }
        }

        public byte[] GetBytes()
        {
            byte[] Buffer = null;

            if (pBuffers == IntPtr.Zero)
            {
                throw new InvalidOperationException("SecurityBufferDesciption instance already disposed");
            }

            if (cBuffers == 1)
            {
                SecurityBuffer ThisSecBuffer = (SecurityBuffer)Marshal.PtrToStructure(pBuffers, typeof(SecurityBuffer));

                if (ThisSecBuffer.cbBuffer > 0)
                {
                    Buffer = new byte[ThisSecBuffer.cbBuffer];
                    Marshal.Copy(ThisSecBuffer.pvBuffer, Buffer, 0, ThisSecBuffer.cbBuffer);
                }
            }
            else
            {
                int BytesToAllocate = 0;

                for (int Index = 0; Index < cBuffers; Index++)
                {
                    int CurrentOffset = Index * Marshal.SizeOf(typeof(Buffer));
                    BytesToAllocate += Marshal.ReadInt32(pBuffers, CurrentOffset);
                }

                Buffer = new byte[BytesToAllocate];

                for (int Index = 0, BufferIndex = 0; Index < cBuffers; Index++)
                {
                    int CurrentOffset = Index * Marshal.SizeOf(typeof(Buffer));
                    int BytesToCopy = Marshal.ReadInt32(pBuffers, CurrentOffset);
                    IntPtr SecBufferpvBuffer = Marshal.ReadIntPtr(pBuffers, CurrentOffset + Marshal.SizeOf(typeof(int)) + Marshal.SizeOf(typeof(int)));
                    Marshal.Copy(SecBufferpvBuffer, Buffer, BufferIndex, BytesToCopy);
                    BufferIndex += BytesToCopy;
                }
            }

            return (Buffer);
        }
    }

    internal class NativeMethods
    {
        [DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int AcquireCredentialsHandle(
            string pszPrincipal,
            string pszPackage,
            int fCredentialUse,
            IntPtr PAuthenticationID,
            IntPtr pAuthData,
            int pGetKeyFn,
            IntPtr pvGetKeyArgument,
            ref SecurityHandle phCredential,
            ref SecurityInteger ptsExpiry);

        [DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int AcceptSecurityContext(ref SecurityHandle phCredential,
            IntPtr phContext,
            ref SecurityBufferDesciption pInput,
            uint fContextReq,
            uint TargetDataRep,
            out SecurityHandle phNewContext,
            out SecurityBufferDesciption pOutput,
            out uint pfContextAttr,
            out SecurityInteger ptsTimeStamp
            );

        [DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int AcceptSecurityContext(ref SecurityHandle phCredential,
            ref SecurityHandle phContext,
            ref SecurityBufferDesciption pInput,
            uint fContextReq,
            uint TargetDataRep,
            out SecurityHandle phNewContext,
            out SecurityBufferDesciption pOutput,
            out uint pfContextAttr,
            out SecurityInteger ptsTimeStamp
            );

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int QuerySecurityContextToken(
            ref SecurityHandle phContext,
            ref IntPtr phToken
            );

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
    }
}