using Clothes_System.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Clothes_System.Views
{
    public partial class ModelView : Page
    {
        public ModelView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<ModelViewModel>();
        }

        // Event handler for the DataGrid SelectionChanged event
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // You can handle the selection change here
            var selectedModel = (sender as DataGrid)?.SelectedItem as Domain.Entities.Model; // Replace with your actual model class
            if (selectedModel != null)
            {
                // Example: Show a message box with selected model name
                MessageBox.Show($"Selected Model: {selectedModel.Name}");
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the MenuPage
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }
}
