using Application.Services;
using Clothes_System.ViewModels;
using Domain.Entities;
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

        private void NotesTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is Model model)
            {
                var modelService = App.ServiceProvider.GetRequiredService<ModelService>();
                var notesWindow = new ModelNotesWindow(model, modelService);
                notesWindow.ShowDialog();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedModel = (sender as DataGrid)?.SelectedItem as Model;
            if (selectedModel != null)
            {
                // Optional: Handle selection if needed
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var dataGrid = sender as DataGrid;
                var editedModel = e.Row.Item as Model;
                var viewModel = DataContext as ModelViewModel;

                if (editedModel != null && viewModel != null)
                {
                    // Get the updated value from the editing control
                    if (e.EditingElement is TextBox textBox)
                    {
                        var column = e.Column as DataGridTextColumn;
                        if (column != null)
                        {
                            var binding = column.Binding as System.Windows.Data.Binding;
                            if (binding != null)
                            {
                                var propertyName = binding.Path.Path;

                                // Update the model property
                                switch (propertyName)
                                {
                                    case "Name":
                                        editedModel.Name = textBox.Text;
                                        break;
                                    case "Type":
                                        editedModel.Type = textBox.Text;
                                        break;
                                    case "Code":
                                        editedModel.Code = textBox.Text;
                                        break;
                                    case "Cost":
                                        if (decimal.TryParse(textBox.Text, out decimal cost))
                                            editedModel.Cost = cost;
                                        break;
                                    case "Metrag":
                                        if (int.TryParse(textBox.Text, out int metrag))
                                            editedModel.Metrag = metrag;
                                        break;
                                    case "MakingPrice":
                                        if (decimal.TryParse(textBox.Text, out decimal makingPrice))
                                            editedModel.MakingPrice = makingPrice;
                                        break;
                                    case "SellPrice":
                                        if (decimal.TryParse(textBox.Text, out decimal sellPrice))
                                            editedModel.SellPrice = sellPrice;
                                        break;
                                }
                            }
                        }
                    }

                    // Call the update command
                    viewModel.UpdateModelCommand.Execute(editedModel);
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var main = (MainWindow)System.Windows.Application.Current.MainWindow;
            main.Navigate(new MenuPage());
        }
    }
}