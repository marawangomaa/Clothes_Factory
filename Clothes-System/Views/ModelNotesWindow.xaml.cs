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
using System.Windows.Shapes;

namespace Clothes_System.Views
{
    /// <summary>
    /// Interaction logic for ModelNotesWindow.xaml
    /// </summary>
    public partial class ModelNotesWindow : Window
    {
        public ModelNotesWindow(Model model, ModelService modelService)
        {
            InitializeComponent(); // This was missing!
            var viewModel = new ModelNotesViewModel(model, modelService);
            viewModel.CloseRequested += () => this.Close();
            DataContext = viewModel;
        }
    }
}
