using Application.Services;
using Clothes_System.Views;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clothes_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load the menu page as the default page
            MainFrame.Navigate(new MenuPage());
        }

        public void Navigate(Page page)
        {
            MainFrame.Navigate(page);
        }
    }

}

