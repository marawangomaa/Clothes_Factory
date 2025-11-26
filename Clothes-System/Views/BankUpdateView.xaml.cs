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
    /// Interaction logic for BankUpdateView.xaml
    /// </summary>
    public partial class BankUpdateView : Page
    {
        private readonly BankViewModel _viewModel;

        public BankUpdateView(BankViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;

            Loaded += async (s, e) => await LoadBankData();
        }

        private async Task LoadBankData()
        {
            if (_viewModel.LoadBankCommand.CanExecute(null))
                _viewModel.LoadBankCommand.Execute(null);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }

}
