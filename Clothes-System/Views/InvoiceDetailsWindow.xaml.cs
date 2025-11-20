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
    /// Interaction logic for InvoiceDetailsWindow.xaml
    /// </summary>
    public partial class InvoiceDetailsWindow : Window
    {
        public InvoiceDetailsWindow(InvoiceDisplayDto invoice)
        {
            InitializeComponent();
            DataContext = invoice; // bind the window to the selected invoice
        }
    }
}
