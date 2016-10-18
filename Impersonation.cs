using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Web.Hosting;

namespace Impersonation
{
       public class Impersonator : IDisposable
        {
            private WindowsImpersonationContext _impersonatedContext;

            public Impersonator(string userName, string password, string domain)
            {
                IntPtr logonToken = IntPtr.Zero;
                IntPtr logonTokenDuplicate = IntPtr.Zero;
                try
                {
                    if (Win32API.LogonUser(userName,
                                           domain,
                                           password,
                                           LogonType.LOGON32_LOGON_INTERACTIVE,
                                           LogonProvider.LOGON32_PROVIDER_DEFAULT,
                                           out logonToken))
                    {
#if DEBUG
                        _impersonatedContext = WindowsIdentity.Impersonate(logonToken); //for Win8
#else
                    if (Win32API.DuplicateToken(logonToken,
                                                SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                                                out logonTokenDuplicate))
                    {
                        _impersonatedContext = WindowsIdentity.Impersonate(logonTokenDuplicate);
                    }
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
#endif
                    }
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally
                {
                    if (logonToken != IntPtr.Zero)
                        Win32API.CloseHandle(logonToken);

                    if (logonTokenDuplicate != IntPtr.Zero)
                        Win32API.CloseHandle(logonTokenDuplicate);
                }
            }

            public void Dispose()
            {
                if (this._impersonatedContext != null)
                    this._impersonatedContext.Undo();
                this._impersonatedContext = null;
            }
        }

        /* Usage:
            using (new WebImpersonator(userName, password, domain))
            {
                ... ... ... 
            }
         * */
        public class WebImpersonator : IDisposable
        {
            private IDisposable _WebImpersonatedContext;

            public WebImpersonator(string userName, string password, string domain)
            {
                IntPtr logonToken = IntPtr.Zero;
                IntPtr logonTokenDuplicate = IntPtr.Zero;
                try
                {
                    if (Win32API.LogonUser(userName,
                                           domain,
                                           password,
                                           LogonType.LOGON32_LOGON_INTERACTIVE,
                                           LogonProvider.LOGON32_PROVIDER_DEFAULT,
                                           out logonToken))
                    {
                        if (Win32API.DuplicateToken(logonToken,
                                                    SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                                                    out logonTokenDuplicate))
                        {
                            _WebImpersonatedContext = HostingEnvironment.Impersonate(logonTokenDuplicate);
                        }
                        else
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally
                {
                    if (logonToken != IntPtr.Zero)
                        Win32API.CloseHandle(logonToken);

                    if (logonTokenDuplicate != IntPtr.Zero)
                        Win32API.CloseHandle(logonTokenDuplicate);
                }
            }

            public void Dispose()
            {
                this._WebImpersonatedContext.Dispose();
            }
        }

        public class Impersonation
        {
            public static IntPtr GetUserToken(string userName, string password, string domain)
            {
                IntPtr userToken = IntPtr.Zero;
                IntPtr logonToken = IntPtr.Zero;
                IntPtr impersonateToken = IntPtr.Zero;
                try
                {
                    if (Win32API.LogonUser(userName,
                                           domain,
                                           password,
                                           LogonType.LOGON32_LOGON_INTERACTIVE,
                                           LogonProvider.LOGON32_PROVIDER_DEFAULT,
                                           out logonToken))
                    {
                        if (Win32API.DuplicateToken(logonToken,
                                                    SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                                                    out impersonateToken))
                        {
                            userToken = impersonateToken;
                        }
                        else
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally
                {
                    if (logonToken != IntPtr.Zero)
                        Win32API.CloseHandle(logonToken);

                    //if (impersonateToken != IntPtr.Zero) // shouldn't be closed yet
                    //    Win32API.CloseHandle(impersonateToken);
                }
                return userToken;
            }

            public static void CloseToken(IntPtr token)
            {
                if (token != null)
                    if (token != IntPtr.Zero)
                        Win32API.CloseHandle(token);
                token = IntPtr.Zero;
            }
        }

        public static class Win32API
        {

            [DllImport("kernel32.dll")]
            public static extern uint FormatMessage(uint flags, IntPtr source, uint messageId, uint langId, StringBuilder buffer, uint size, string[] arguments);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool LogonUser(
                [MarshalAs(UnmanagedType.LPWStr)] string lpszUsername,
                [MarshalAs(UnmanagedType.LPWStr)] string lpszDomain,
                [MarshalAs(UnmanagedType.LPWStr)] string lpszPassword,
                LogonType dwLogonType,
                LogonProvider dwLogonProvider,
                out IntPtr phToken
            );

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public extern static bool DuplicateToken(
                IntPtr ExistingTokenHandle,
                SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                out IntPtr DuplicateTokenHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
                                                IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
                                                uint dwDesiredAccess, bool bInheritHandle, uint dwOptions);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public extern static bool CloseHandle(IntPtr handle);
        }

        public enum LogonType : int
        {
            // This logon type is intended for users who will be interactively 
            // using the computer
            LOGON32_LOGON_INTERACTIVE = 2,

            // This logon type is intended for high performance servers to 
            // authenticate plaintext passwords
            LOGON32_LOGON_NETWORK = 3,

            // This logon type is intended for batch servers
            LOGON32_LOGON_BATCH = 4,

            // Indicates a service-type logon
            LOGON32_LOGON_SERVICE = 5,

            // This logon type is for GINA DLLs that log on users who will be 
            // interactively using the computer       
            LOGON32_LOGON_UNLOCK = 7,

            // This logon type preserves the name and password in the 
            // authentication package
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,

            // This logon type allows the caller to clone its current token 
            // and specify new credentials for outbound connections.        
            LOGON32_LOGON_NEW_CREDENTIALS = 9
        }

        public enum LogonProvider : int
        {
            // Use the standard logon provider for the system        
            LOGON32_PROVIDER_DEFAULT = 0,
            // Use the negotiate logon provider
            LOGON32_PROVIDER_WINNT50 = 1,
            // Use the NTLM logon provider
            LOGON32_PROVIDER_WINNT40 = 2
        }

        public enum SECURITY_IMPERSONATION_LEVEL : int
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3
        }
}
