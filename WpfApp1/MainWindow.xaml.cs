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
		private SysControl _hooks = new SysControl();
		
		private DispatcherTimer _timer;
		private bool _canClose = false;
		
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
				TimeDisplay1.ToolTip = TimeDisplay1.Text;
			}, Dispatcher);
			Closing += OnClosingWindow;
			_hooks.SetLowLevelProcToLib();
            _hooks.AllowAccessibilityShortcutKeys(false);
			_hooks.Hooked = false;
		}

        private void OnClosingWindow(object sender, CancelEventArgs e) {
			e.Cancel = !_canClose;
			if (_canClose) {
				_hooks.AllowAccessibilityShortcutKeys(true);
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
			_hooks.SoftReboot();
		}

		private void HardReboot_OnClick(object sender, RoutedEventArgs e) {
			_hooks.HardReboot();
		}

		private void SoftPoweroff_OnClick(object sender, RoutedEventArgs e) {
			_hooks.SoftShutdown();
		}

		private void HardPoweroff_OnClick(object sender, RoutedEventArgs e) {
			_hooks.HardShutdown();
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
