using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace WpfApp1
{
    public partial class MainWindow : Window {
		private AddHooks _hooks = new AddHooks();
		private Reboot _rebootController = new Reboot();
		private DispatcherTimer _timer;
		private bool _canClose = false;
		
		public TextBlock TimeDisplay1 {
			get => TimeDisplay;
			set => TimeDisplay = value;
		}


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
        /// <summary>
        /// False to stop the sticky keys popup.
        /// True to return to whatever the system has been set to.
        /// </summary>
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

        public MainWindow() {
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
		    {
				TimeDisplay1.Text = DateTime.Now.ToString("HH:mm:ss") +
									Environment.NewLine +
									DateTime.Now.ToString("MM/dd/yyyy");
				TimeDisplay1.ToolTip = TimeDisplay1.Text;
			}, Dispatcher);
			Closing += OnClosingWindow;
			_hooks.SomeMethod();
            InitializeComponent();
		}

        private void OnClosingWindow(object sender, CancelEventArgs e) {
			e.Cancel = !_canClose;
			if (_canClose) {
				_hooks.OnClose();
            }
		}

		private void DelStartMenu_OnClick(object sender, RoutedEventArgs e) {
			try {
				_hooks.KillStartMenu();
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		private void EnableStartMenu_OnClick(object sender, RoutedEventArgs e) {
			try
			{
				_hooks.ShowStartMenu();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
        }

		private void SwitchOnClose_OnClick(object sender, RoutedEventArgs e) {
			_canClose = !_canClose;
			try {
				(sender as Button).Content = _canClose ? "Disable Closing" : "Enable Closing";
            }
			catch (Exception exception) {
				MessageBox.Show(exception.ToString());
			}
			
		}

		private void SoftReboot_OnClick(object sender, RoutedEventArgs e) {
			_rebootController.Halt(true, false);
		}

		private void HardReboot_OnClick(object sender, RoutedEventArgs e) {
			_rebootController.Halt(true, true);
		}

		private void SoftPoweroff_OnClick(object sender, RoutedEventArgs e) {
			_rebootController.Halt(false, false);
		}

		private void HardPoweroff_OnClick(object sender, RoutedEventArgs e) {
			_rebootController.Halt(false, true);
		}

		private void SetAutorun_OnClick(object sender, RoutedEventArgs e) {
			try {
				_hooks.SetAutoRunValue();
            }
			catch (Exception ex) {
				MessageBox.Show(ex.ToString());
			}
		}

		private void Hook_OnClick(object sender, RoutedEventArgs e) {
			_hooks.Hooked = true;
        }

		private void Unhook_OnClick(object sender, RoutedEventArgs e) {
			_hooks.Hooked = false;
        }
	}
}
