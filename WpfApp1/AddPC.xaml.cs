using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для AddPC.xaml
    /// </summary>
    public partial class AddPC : Window {
		public string nameVal;
		public int typeVal;
        public AddPC()
        {
            InitializeComponent();
        }

		private void Accept_OnClick(object sender, RoutedEventArgs e) {
			nameVal = Name.Text;
			int.TryParse(Type.Text,out typeVal);
			this.DialogResult = true;
		}
	}
}
