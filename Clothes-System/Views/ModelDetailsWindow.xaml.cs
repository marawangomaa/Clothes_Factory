using Application.Services;
using Clothes_System.ViewModels;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Shapes;

namespace Clothes_System.Views
{
    /// <summary>
    /// Interaction logic for ModelDetailsWindow.xaml
    /// </summary>
    public partial class ModelDetailsWindow : Window
    {
        public ModelDetailsWindow()
        {
            InitializeComponent();
        }

        public ModelDetailsWindow(Model model)
        {
            InitializeComponent();
            var modelService = App.ServiceProvider.GetRequiredService<ModelService>();
            DataContext = new ModelDetailsViewModel(model, modelService);
        }
    }
}
