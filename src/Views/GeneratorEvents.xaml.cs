using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GeneratorETWViewer.ViewModels;

namespace GeneratorETWViewer;

public partial class GeneratorEvents : Window
{
    public GeneratorEvents()
    {
        InitializeComponent();
    }

    internal GeneratorEvents(TraceViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
