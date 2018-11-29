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
		private AddHooks hooks = new AddHooks();
		private DispatcherTimer _timer;
		private bool _canClose = false;
		public TextBlock TimeDisplay1 {
			get => TimeDisplay;
			set => TimeDisplay = value;
		}

        public MainWindow() {
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
		    {
				TimeDisplay1.Text = DateTime.Now.ToString("HH:mm:ss") +
									Environment.NewLine +
									DateTime.Now.ToString("MM/dd/yyyy");
			}, Dispatcher);
			Closing += OnClosingWindow;
            InitializeComponent();
		}

		private void DisableButtons() {
			try {
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\");
				regKey.SetValue("NoWinKeys", "1");
				regKey.Close();
            }
			catch (Exception e) {
				MessageBox.Show(e.ToString());
			}
		}

		private void EnableButtons() {
			try
			{
				string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";
				RegistryKey rk = Registry.CurrentUser;
				RegistryKey sk1 = rk.OpenSubKey(subKey);
				if (sk1 != null)
					rk.DeleteSubKeyTree(subKey);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
        }

        private void OnClosingWindow(object sender, CancelEventArgs e) {
			if (_canClose) {
				hooks.ShowStartMenu();
				hooks.EnableCTRLALTDEL();
				e.Cancel = false;
			}
			else {
				e.Cancel = true;
			}
		}

		private void DelTaskMngr_OnClick(object sender, RoutedEventArgs e) {
			hooks.KillCtrlAltDelete();
		}

		private void EnableTaskMngr_OnClick(object sender, RoutedEventArgs e) {
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

		private void SetAutoRunValue(bool value) {
			RegistryKey regKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
			regKey?.SetValue("MyApp", Assembly.GetExecutingAssembly().Location);
			regKey?.Close();
        }

		private void DisableWinKeys_OnClick(object sender, RoutedEventArgs e) {
			DisableButtons();
		}

		private void EnableWinKeys_OnClick(object sender, RoutedEventArgs e) {
			EnableButtons();
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
	}
}
