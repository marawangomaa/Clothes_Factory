using Application.Services;
using Clothes_System.ViewModels;
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

namespace Clothes_System.Views
{
    /// <summary>
    /// Interaction logic for BankView.xaml
    /// </summary>
    public partial class BankView : Page
    {
        public BankView(BankViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += (s, e) => viewModel.LoadBankCommand.Execute(null);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the MenuPage
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }
}
