using Application.Services;
using Clothes_System.ViewModels;
using Domain.Entities;
using Domain.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Clothes_System.Views
{
    public partial class ScissorView : Page
    {
        public ScissorView()
        {
            InitializeComponent();

            var scissorService = new ScissorService(App.GetService<IRepository<Scissor>>());
            var modelService = new ModelService(
                App.GetService<IRepository<Model>>(),
                App.GetService<IRepository<Storage>>(),
                App.GetService<StorageService>());

            DataContext = new ScissorViewModel(scissorService, modelService);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the MenuPage
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }
}
