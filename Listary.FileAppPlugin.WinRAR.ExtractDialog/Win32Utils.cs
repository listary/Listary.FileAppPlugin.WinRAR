using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Listary.FileAppPlugin.WinRAR.ExtractDialog
{
    internal static class Win32Utils
    {
        public const int BM_CLICK = 0xF5;

        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        private enum WM : uint
        {
            /// <summary>
            /// An application sends a WM_SETTEXT message to set the text of a window.
            /// </summary>
            SETTEXT = 0x000C,
            /// <summary>
            /// An application sends a WM_GETTEXT message to copy the text that corresponds to a window into a buffer provided by the caller.
            /// </summary>
            GETTEXT = 0x000D,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern long GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        public static string GetClassName(IntPtr hWnd)
        {
            var className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            return className.ToString();
        }

        public static string GetProcessPathFromHwnd(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            
            StringBuilder buffer = new StringBuilder(1024);
            IntPtr process = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, (int)pid);
            if (process != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(process, 0, buffer, ref size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    CloseHandle(process);
                }
            }
            
            return string.Empty;
        }
        
        public static string GetWindowText(this IFileAppPluginHost host, IntPtr hWnd)
        {
            var buffer = new byte[1000];
            if (host.SendMessage(hWnd, (uint)WM.GETTEXT, (IntPtr)buffer.Length, buffer) != IntPtr.Zero)
            {
                return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
            }
            return string.Empty;
        }

        public static bool SetWindowText(this IFileAppPluginHost host, IntPtr hWnd, string text)
        {
            // lParam should be a null-terminated string
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Encoding.Unicode.GetBytes(text));
            bytes.Add(0);
            IntPtr result = host.SendMessage(hWnd, (uint)WM.SETTEXT, IntPtr.Zero, bytes.ToArray());
            return result != IntPtr.Zero;
        }
    }
}
