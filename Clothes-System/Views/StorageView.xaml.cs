using Application.Services;
using Clothes_System.ViewModels;
using Domain.Entities;
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
    /// Interaction logic for StorageView.xaml
    /// </summary>
    public partial class StorageView : Page
    {
        private readonly StorageService _storageService;
        private readonly MaterialService _materialService;

        public StorageView(StorageService storageService, MaterialService materialService)
        {
            InitializeComponent();
            _storageService = storageService;
            _materialService = materialService;
            Loaded += async (s, e) => await LoadStorage();
        }

        private async Task LoadStorage()
        {
            var data = await _storageService.GetAllStorageAsync();
            DataContext = new { StorageItems = data.ToList() };
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var material = new Material
                {
                    Name = "Cotton Fabric",
                    Quantity = 10,
                    Price = 500
                };

                await _materialService.AddMaterialAsync(material);
                await LoadStorage();
                MessageBox.Show("Material added successfully!");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _storageService.RemoveFromStorageAsync("Cotton Fabric", 5);
                await LoadStorage();
                MessageBox.Show("Removed 5 Cotton Fabric.");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void Reload_Click(object sender, RoutedEventArgs e)
        {
            await LoadStorage();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the MenuPage
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }
}

