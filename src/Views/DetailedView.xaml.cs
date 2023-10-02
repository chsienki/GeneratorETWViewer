using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GeneratorETWViewer.ViewModels;

namespace GeneratorETWViewer.Views
{
    public partial class DetailedView : Window
    {
        public DetailedView()
        {
            InitializeComponent();
        }

        internal DetailedView(DetailedExecutionViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
