using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Toolkit.WPF;

namespace WinUI3Islands.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowsXamlHost_ChildChanged(object sender, EventArgs e)
        {
            var xamlHost = (WindowsXamlHost)sender;
            Microsoft.UI.Xaml.Controls.Page page = (Microsoft.UI.Xaml.Controls.Page)xamlHost.GetUwpInternalObject();
            if (page != null)
            {
                page.Background = (Microsoft.UI.Xaml.Media.Brush)Program.xamlApp.Resources["ApplicationPageBackgroundThemeBrush"];
                Microsoft.UI.Xaml.Controls.Button button = new();
                button.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                button.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
                button.Content = "Click Me!";

                page.Content = button;
            }
        }
    }
}
