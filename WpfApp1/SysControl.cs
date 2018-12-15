using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace WpfApp1
{
    public class SysControl
    {
        [DllImport("user32", EntryPoint = "SetWindowsHookExA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, int hMod, int dwThreadId);

        [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern int UnhookWindowsHookEx(int hHook);

		private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);


        [DllImport("user32", EntryPoint = "CallNextHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

		private const int WH_KEYBOARD_LL = 13;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr LoadLibrary(string lpFileName);

        /*code needed to disable start menu*/
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = false)]
        private static extern bool SystemParametersInfo(uint action, uint param,
            ref SKEY vparam, uint init);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = false)]
        private static extern bool SystemParametersInfo(uint action, uint param,
            ref FILTERKEY vparam, uint init);

        private const uint SPI_GETFILTERKEYS = 0x0032;
        private const uint SPI_SETFILTERKEYS = 0x0033;
        private const uint SPI_GETTOGGLEKEYS = 0x0034;
        private const uint SPI_SETTOGGLEKEYS = 0x0035;
        private const uint SPI_GETSTICKYKEYS = 0x003A;
        private const uint SPI_SETSTICKYKEYS = 0x003B;

        private static bool StartupAccessibilitySet = false;
        private static SKEY StartupStickyKeys;
        private static SKEY StartupToggleKeys;
        private static FILTERKEY StartupFilterKeys;

        private const uint SKF_STICKYKEYSON = 0x00000001;
        private const uint TKF_TOGGLEKEYSON = 0x00000001;
        private const uint SKF_CONFIRMHOTKEY = 0x00000008;
        private const uint SKF_HOTKEYACTIVE = 0x00000004;
        private const uint TKF_CONFIRMHOTKEY = 0x00000008;
        private const uint TKF_HOTKEYACTIVE = 0x00000004;
        private const uint FKF_CONFIRMHOTKEY = 0x00000008;
        private const uint FKF_HOTKEYACTIVE = 0x00000004;
        private const uint FKF_FILTERKEYSON = 0x00000001;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SKEY
        {
            public uint cbSize;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct FILTERKEY
        {
            public uint cbSize;
            public uint dwFlags;
            public uint iWaitMSec;
            public uint iDelayMSec;
            public uint iRepeatMSec;
            public uint iBounceMSec;
        }

        private static uint SKEYSize = sizeof(uint) * 2;
        private static uint FKEYSize = sizeof(uint) * 6;

        public bool Hooked = false;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

		private Reboot _rebootController = new Reboot();

        private static int _intLlKey;

		private int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            bool blnEat = false;

            switch (wParam)
            {
                case 256:
                case 257:
                case 260:
                case 261:
                    //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key, ...
					blnEat = ((lParam.vkCode == 9) && (lParam.flags == 32)) ||
							 ((lParam.vkCode == 27) && (lParam.flags == 32))||
							 ((lParam.vkCode == 27) && (lParam.flags == 0)) ||
							 ((lParam.vkCode == 91) && (lParam.flags == 1)) ||
							 ((lParam.vkCode == 92) && (lParam.flags == 1)) ||
							 ((lParam.vkCode == 73) && (lParam.flags == 0));
                    break;
            }

            if (blnEat && Hooked)
            {
                return 1;
            }
            else
            {
                return CallNextHookEx(0, nCode, wParam, ref lParam);
            }
        }


        public void KillStartMenu()
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_HIDE);
        }

		private static LowLevelKeyboardProcDelegate _lowLevelKeyboardProcDelegate;
        public void SetLowLevelProcToLib()
        {
            var inst = LoadLibrary("user32.dll").ToInt32();
			_lowLevelKeyboardProcDelegate = LowLevelKeyboardProc;
            _intLlKey = SetWindowsHookEx(WH_KEYBOARD_LL, _lowLevelKeyboardProcDelegate, inst, 0);
        }

        public void KillTaskMngr()
        {
            RegistryKey regkey;
            string keyValueInt = "1";
            string subKey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
            regkey = Registry.CurrentUser.CreateSubKey(subKey);
            regkey.SetValue("DisableTaskMgr", keyValueInt);
            regkey.Close();
        }

        public void ShowStartMenu()
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_SHOW);
        }
        public void EnableTaskMngr()
        {
		    string subKey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
		    RegistryKey rk = Registry.CurrentUser;
		    rk.DeleteSubKeyTree(subKey);
        }

		public void OnClose() {
			UnhookWindowsHookEx(_intLlKey);
        }

		public void SetAutoRunValue()
		{
			RegistryKey regKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
			regKey.SetValue("MyApp", Assembly.GetExecutingAssembly().Location);
			regKey.Close();
		}

		public void BlockScreen(ref Window window) {
			window.Topmost = true;
		}

		public void UnBlocScreen(ref Window window) {
			window.Topmost = false;
		}

		public void HardReboot() {
			_rebootController.Halt(true, true);
        }

		public void SoftReboot() {
			_rebootController.Halt(true, false);
        }

		public void SoftShutdown() {
			_rebootController.Halt(false, false);
        }

		public void HardShutdown() {
			_rebootController.Halt(false, true);
		}

        public void AllowAccessibilityShortcutKeys(bool bAllowKeys)
        {
            if (!StartupAccessibilitySet)
            {
                StartupStickyKeys.cbSize = SKEYSize;
                StartupToggleKeys.cbSize = SKEYSize;
                StartupFilterKeys.cbSize = FKEYSize;
                SystemParametersInfo(SPI_GETSTICKYKEYS, SKEYSize, ref StartupStickyKeys, 0);
                SystemParametersInfo(SPI_GETTOGGLEKEYS, SKEYSize, ref StartupToggleKeys, 0);
                SystemParametersInfo(SPI_GETFILTERKEYS, FKEYSize, ref StartupFilterKeys, 0);
                StartupAccessibilitySet = true;
            }

            if (bAllowKeys)
            {
                // Restore StickyKeys/etc to original state and enable Windows key 
                SystemParametersInfo(SPI_SETSTICKYKEYS, SKEYSize, ref StartupStickyKeys, 0);
                SystemParametersInfo(SPI_SETTOGGLEKEYS, SKEYSize, ref StartupToggleKeys, 0);
                SystemParametersInfo(SPI_SETFILTERKEYS, FKEYSize, ref StartupFilterKeys, 0);
            }
            else
            {
                // Disable StickyKeys/etc shortcuts but if the accessibility feature is on,  
                // then leave the settings alone as its probably being usefully used 
                SKEY skOff = StartupStickyKeys;
                if ((skOff.dwFlags & SKF_STICKYKEYSON) == 0)
                {
                    // Disable the hotkey and the confirmation 
                    skOff.dwFlags &= ~SKF_HOTKEYACTIVE;
                    skOff.dwFlags &= ~SKF_CONFIRMHOTKEY;
                    SystemParametersInfo(SPI_SETSTICKYKEYS, SKEYSize, ref skOff, 0);
                }
                SKEY tkOff = StartupToggleKeys;
                if ((tkOff.dwFlags & TKF_TOGGLEKEYSON) == 0)
                {
                    // Disable the hotkey and the confirmation 
                    tkOff.dwFlags &= ~TKF_HOTKEYACTIVE;
                    tkOff.dwFlags &= ~TKF_CONFIRMHOTKEY;
                    SystemParametersInfo(SPI_SETTOGGLEKEYS, SKEYSize, ref tkOff, 0);
                }

                FILTERKEY fkOff = StartupFilterKeys;
                if ((fkOff.dwFlags & FKF_FILTERKEYSON) == 0)
                {
                    // Disable the hotkey and the confirmation 
                    fkOff.dwFlags &= ~FKF_HOTKEYACTIVE;
                    fkOff.dwFlags &= ~FKF_CONFIRMHOTKEY;
                    SystemParametersInfo(SPI_SETFILTERKEYS, FKEYSize, ref fkOff, 0);
                }
            }
        }
    }
}
