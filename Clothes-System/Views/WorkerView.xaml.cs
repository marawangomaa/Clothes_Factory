using Application.Services;
using Clothes_System.Helpers;
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
    /// Interaction logic for WorkerView.xaml
    /// </summary>
    public partial class WorkerView : Page
    {
        private readonly WorkerViewModel _viewModel;

        public WorkerView(WorkerService workerService)
        {
            InitializeComponent();
            _viewModel = new WorkerViewModel(workerService);
            DataContext = _viewModel;

            Loaded += async (s, e) =>
            {
                if (_viewModel.LoadWorkersCommand is AsyncRelayCommand asyncCmd)
                    await asyncCmd.ExecuteAsync();
            };

        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the MenuPage
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }
}
