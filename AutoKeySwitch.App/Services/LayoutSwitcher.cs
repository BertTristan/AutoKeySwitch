using System.Runtime.InteropServices;
using Serilog;

namespace AutoKeySwitch.App.Services
{
    public static class LayoutSwitcher
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
        private const uint KLF_ACTIVATE = 0x00000001;
        private const int HWND_BROADCAST = 0xffff;

        /// <summary>
        /// Changes the keyboard layout globally across all applications
        /// </summary>
        /// <param name="layoutId">Layout identifier</param>
        public static void ChangeLayout(string layoutId)
        {
            try
            {
                string klid = GetKLID(layoutId);

                // Load Keyboard Layout
                IntPtr hkl = LoadKeyboardLayout(klid, KLF_ACTIVATE);

                if (hkl == IntPtr.Zero)
                {
                    Log.Warning("Failed to load layout: {LayoutId}", layoutId);
                    return;
                }

                // Broadcast to all windows
                PostMessage((IntPtr)HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);

                Log.Information("Layout changed: {LayoutId}", layoutId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Layout change error");
            }
        }

        /// <summary>
        /// Converts layout identifier to Windows KLID
        /// </summary>
        /// <param name="layoutId">Layout identifier</param>
        /// <returns>Windows keyboard layout identifier</returns>
        private static string GetKLID(string layoutId)
        {
            return layoutId switch
            {
                "fr-FR" => "0000040c",
                "en-US" => "00000409",
                "en-GB" => "00000809",
                _ => "0000040c"
            };
        }
    }
}