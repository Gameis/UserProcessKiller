using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window {
		private AddHooks hooks = new AddHooks();
		private DispatcherTimer _timer;

		public TextBlock TimeDisplay1 {
			get => TimeDisplay;
			set => TimeDisplay = value;
		}
        public MainWindow() {
			InitializeComponent();
			_timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
			{
			   TimeDisplay1.Text = DateTime.Now.ToString("HH:mm:ss") + 
								   Environment.NewLine + 
								   DateTime.Now.ToString("MM/dd/yyyy");
			}, Dispatcher);
			Closing += OnClosingWindow;
		}

		private const uint SPI_GETFILTERKEYS = 0x0032;
		private const uint SPI_SETFILTERKEYS = 0x0033;
		private const uint SPI_GETTOGGLEKEYS = 0x0034;
		private const uint SPI_SETTOGGLEKEYS = 0x0035;
		private const uint SPI_GETSTICKYKEYS = 0x003A;
		private const uint SPI_SETSTICKYKEYS = 0x003B;

		private static bool      StartupAccessibilitySet = false;
		private static SKEY      StartupStickyKeys;
		private static SKEY      StartupToggleKeys;
		private static FILTERKEY StartupFilterKeys;

		private const uint SKF_STICKYKEYSON  = 0x00000001;
		private const uint TKF_TOGGLEKEYSON  = 0x00000001;
		private const uint SKF_CONFIRMHOTKEY = 0x00000008;
		private const uint SKF_HOTKEYACTIVE  = 0x00000004;
		private const uint TKF_CONFIRMHOTKEY = 0x00000008;
		private const uint TKF_HOTKEYACTIVE  = 0x00000004;
		private const uint FKF_CONFIRMHOTKEY = 0x00000008;
		private const uint FKF_HOTKEYACTIVE  = 0x00000004;
		private const uint FKF_FILTERKEYSON  = 0x00000001;

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

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SKEY
		{
			public uint cbSize;
			public uint dwFlags;
		}

        private static uint SKEYSize = sizeof(uint) * 2;
		private static uint FKEYSize = sizeof(uint) * 6;
		[DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = false)]
		private static extern bool SystemParametersInfo(uint          action, uint param,
														ref FILTERKEY vparam, uint init);

        public static void AllowAccessibilityShortcutKeys(bool bAllowKeys)
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


        private void OnClosingWindow(object sender, CancelEventArgs e) {
			hooks.ShowStartMenu();
			hooks.EnableCTRLALTDEL();
		}

		private void DelCtrlAltDel_OnClick(object sender, RoutedEventArgs e) {
			hooks.KillCtrlAltDelete();
		}

		private void EnableCtrlAltDel_OnClick(object sender, RoutedEventArgs e) {
			hooks.EnableCTRLALTDEL();
		}

		private void DelStartMenu_OnClick(object sender, RoutedEventArgs e) {
			try {
				hooks.KillStartMenu();
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		private void EnableStartMenu_OnClick(object sender, RoutedEventArgs e) {
			try
			{
				hooks.ShowStartMenu();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
        }

		private void SomeMethod_OnClick(object sender, RoutedEventArgs e) {
			hooks.SomeMethod();
        }

		private void MainWindow_OnKeyDown(object sender, KeyEventArgs e) {
			throw new NotImplementedException();
		}
	}
}
