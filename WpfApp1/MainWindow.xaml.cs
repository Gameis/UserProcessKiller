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

		private ServerController _serverController = new ServerController();
		
		private DispatcherTimer _timer;
		private bool _canClose = false;

		private string token;
		private PCInfoType[] pcs;
		private string pcToken;

		private bool isUse = false;
		private long end = -100;

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

			_serverController.Logged += OnLogin;
			_serverController.PCsInfoReceived += OnPCReceived;
			_serverController.PCConnected += OnConnected;
			_serverController.Updated += OnUpdate;

			Closing += OnClosingWindow;
			_hooks.SetLowLevelProcToLib();
            _hooks.AllowAccessibilityShortcutKeys(false);
			_hooks.Hooked = false;
		}

		private void OnUpdate(object sender, UpdateEventArgs e) {
			Console.WriteLine($"before: {{\"isUse\":\"{isUse}\",\"end\":\"{end}\"}}");
			isUse = e.isUse;
			end = e.end;
			Console.WriteLine($"after: {{\"isUse\":\"{isUse}\",\"end\":\"{end}\"}}");
        }

		private void OnConnected(object sender, ConnectedEventArgs e) {
			pcToken = e.token;
		}

		private void OnPCReceived(object sender, PCsInfoEventArgs e) {
			pcs = e.PCsCotainer.pc;
		}

		private void OnLogin(object sender, LoginEventArgs e) {
			token = e.token;
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

		private void SwitchTaskMgr_OnClick(object sender, RoutedEventArgs e) {
			try {
				if (((Button)sender).Content.ToString() == "Disale Task Manager")
				{
					_hooks.KillTaskMngr();
					((Button) sender).Content = "Enable Task manager";
				}
				else {
					_hooks.EnableTaskMngr();
					((Button) sender).Content = "Disale Task Manager";

				}
            }
			catch (Exception ex) {
				MessageBox.Show(ex.ToString());
			}
		}


		private async void SendLoginReq_OnClick(object sender, RoutedEventArgs e) {
			await _serverController.LoginPost("log", "pass");
			Console.WriteLine($"token: {token}");
		}

		private async void GetPCsReq_OnClick(object sender, RoutedEventArgs e) {
            await _serverController.GetPCPost(token);
		}

		private void ShowPCs_OnClick(object sender, RoutedEventArgs e) {
			string temp = "";
			foreach (var pc in pcs) {
				temp += $"name: {pc.name} | id: {pc.id} | type: {pc.type}" + Environment.NewLine;
			}

			MessageBox.Show(temp);
		}

		private async void AddPC_OnClick(object sender, RoutedEventArgs e) {
			AddPC adder = new AddPC();
			adder.ShowDialog();
			await _serverController.AddPCPost(token, adder.nameVal, adder.typeVal);
		}

		private async void RandConnect_OnClick(object sender, RoutedEventArgs e) {
			await _serverController.ConnectPost(token,pcs[1].id);
		}

		private  void Update_OnClick(object sender, RoutedEventArgs e) {
			_serverController.SetPCToken(pcToken);
			_serverController.SetUpdateTimeOut(10000);
			_serverController.UpdateStart();
		}
	}
}
