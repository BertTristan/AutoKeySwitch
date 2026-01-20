using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AutoKeySwitch.App.Services
{
    public static class AppMonitor
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize);

        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        /// <summary>
        /// Detects the foreground app and retrieves its info
        /// </summary>
        /// <returns>Tuple containing the app name and full path</returns>
        public static (string appName, string appPath) DetectForegroundApp()
        {
            try
            {
                // Get handle of foreground window
                IntPtr hwnd = GetForegroundWindow();

                if (hwnd == IntPtr.Zero)
                {
                    return ("", "");
                }

                // Get process ID of foreground window
                _ = GetWindowThreadProcessId(hwnd, out uint processId);

                // Get process information
                using Process process = Process.GetProcessById((int)processId);
                string appName = process.ProcessName + ".exe";
                string appPath = GetProcessPath(processId);

                return (appName, appPath);
            }
            catch
            {
                return ("", "");
            }
        }

        private static string GetProcessPath(uint processId)
        {
            IntPtr hProcess = IntPtr.Zero;

            try
            {
                // Open process with limited query permission
                hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);

                if (hProcess == IntPtr.Zero)
                {
                    return "";
                }

                // Query full process path
                StringBuilder path = new(1024);
                uint size = (uint)path.Capacity;

                if (QueryFullProcessImageName(hProcess, 0, path, ref size))
                {
                    return path.ToString();
                }

                return "";
            }
            catch
            {
                return "";
            }
            finally
            {
                if (hProcess != IntPtr.Zero)
                {
                    CloseHandle(hProcess);
                }
            }
        }
    }
}