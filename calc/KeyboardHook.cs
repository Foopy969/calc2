using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static calc.NativeMethods;

namespace calc
{
    class KeyboardHook
    {
        public static void Start() => _hookID = SetHook(_proc);
        public static void Stop() => UnhookWindowsHookEx(_hookID);

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if ((Keys)vkCode == Form1.k_AimBotToggleKey)
                {
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        Form1.b_KeyboardDown = !Form1.b_KeyboardDown;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
    }
}
