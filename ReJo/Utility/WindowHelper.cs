using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReJo.Utility
{
    public class WindowHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        public static void BringWindowToFront(IntPtr handle)
        {
            // Rende visibile la finestra se minimizzata
            ShowWindowAsync(handle, SW_RESTORE);
            // Porta la finestra in primo piano
            SetForegroundWindow(handle);
        }
    }
}
