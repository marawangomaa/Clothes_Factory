using Application.Services;
using Clothes_System.ViewModels;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Clothes_System.Views
{
    public partial class ModelDetailsWindow : Window
    {
        public ModelDetailsWindow(Model model)
        {
            InitializeComponent();
            var modelService = App.ServiceProvider.GetRequiredService<ModelService>();
            var viewModel = new ModelDetailsViewModel(model, modelService);

            // Subscribe to the close event
            viewModel.CloseRequested += () => this.Close();

            DataContext = viewModel;
        }

        public ModelDetailsWindow()
        {
            InitializeComponent();
        }
    }
}