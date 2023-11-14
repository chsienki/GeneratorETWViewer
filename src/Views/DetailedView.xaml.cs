﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private void DataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            var dg = (DataGrid)sender;
            foreach(var column in dg.Columns)
            {
                column.Width = 300;
            }
        }
    }
}
