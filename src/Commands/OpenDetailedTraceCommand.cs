using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GeneratorETWViewer.ViewModels;
using GeneratorETWViewer.Views;

namespace GeneratorETWViewer.Command
{
    internal class OpenDetailedTraceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => parameter is GeneratorRunViewModel;

        public void Execute(object parameter)
        {
            if (parameter is GeneratorRunViewModel run)
            {
                var vm = new DetailedExecutionViewModel(run.ExecutionInfo, run.GeneratorInfo, run.ProcessInfo);
                var view = new DetailedView(vm);
                view.Show();
            }
        }
    }
}
