using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsApiInteropService
{
    using System.Runtime.InteropServices;

    using WindowsInput;

    /// <summary>
    /// The interop with native.
    /// </summary>
    public class InteropWithNative
    {
        /// <summary>
        /// The find window.
        /// </summary>
        /// <param name="lpClassName">
        /// The lp class name.
        /// </param>
        /// <param name="lpWindowName">
        /// The lp window name.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("User32.dll")]
        public static extern Int32 FindWindow(String lpClassName, String lpWindowName);

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="hWnd">
        /// The h wnd.
        /// </param>
        /// <param name="msg">
        /// The msg.
        /// </param>
        /// <param name="wParam">
        /// The w param.
        /// </param>
        /// <param name="lParam">
        /// The l param.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(int hWnd, int msg, int wParam, IntPtr lParam);

        /// <summary>
        /// The find window ex.
        /// </summary>
        /// <param name="parentHandle">
        /// The parent handle.
        /// </param>
        /// <param name="childAfter">
        /// The child after.
        /// </param>
        /// <param name="className">
        /// The class name.
        /// </param>
        /// <param name="windowTitle">
        /// The window title.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        /// <summary>
        /// The set foreground window.
        /// </summary>
        /// <param name="hwnd">
        /// The hwnd.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetForegroundWindow(IntPtr hwnd);


        public bool ShowWindow(String lpClassName, String lpWindowName)
        {
            var hwnd = FindWindow(lpClassName, lpWindowName);
            if (hwnd == 0)
            {
                return false;
            }

            SetForegroundWindow((IntPtr)hwnd);
            return true;
        }

        /// <summary>
        /// The post ctrl c.
        /// </summary>
        /// <param name="lpClassName">
        /// The lp Class Name.
        /// </param>
        /// <param name="lpWindowName">
        /// The lp Window Name.
        /// </param>
        public bool PostCtrlC(String lpClassName, String lpWindowName)
        {
            if (!string.IsNullOrEmpty(lpWindowName))
            {
                var ptr = FindWindow(lpClassName, lpWindowName);

                if (ptr == 0)
                {
                    return false;
                }

                while (ptr != 0)
                {
                    SetForegroundWindow((IntPtr)ptr);
                    //System.Threading.Thread.Sleep(1000);
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.CANCEL);
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, new[] { VirtualKeyCode.VK_K, VirtualKeyCode.VK_C });
                    ptr = FindWindow(lpClassName, lpWindowName);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
