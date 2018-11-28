using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
	}
}
