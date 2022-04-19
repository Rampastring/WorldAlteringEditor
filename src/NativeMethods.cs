using System;
using System.Runtime.InteropServices;

namespace TSMapEditor
{
    public static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        private const int LOGPIXELSX = 88;

        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        private const int LOGPIXELSY = 90;


        public static int GetScreenDPI()
        {
            // This function currently only works on Windows
            IntPtr hdc = GetDC(IntPtr.Zero);
            return GetDeviceCaps(hdc, LOGPIXELSX);
        }
    }
}
