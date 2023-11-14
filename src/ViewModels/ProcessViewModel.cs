using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class ProcessViewModel : INotifyPropertyChanged
    {
        private ProcessInfo processInfo;

        private Lazy<List<object>> generatorViewModels;

        private Lazy<List<object>> driverRunViewModels;

        private List<object> currentView;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProcessViewModel(Models.ProcessInfo processInfo)
        {
            this.processInfo = processInfo;

            this.generatorViewModels = new(() => processInfo.generators.Select(gi => new GeneratorViewModel(gi, processInfo)).OrderBy(gvm => gvm.Name).Cast<object>().ToList());
            this.driverRunViewModels = new(() => processInfo.generators
                .SelectMany(g => g.executions)
                .GroupBy(e => e.driverRun)
                .Select(g => (object)new DriverRunViewModel(g.Key, processInfo.generators, processInfo))
                .ToList());

            this.currentView = generatorViewModels.Value;
        }

        internal void SwitchViews()
        {
            currentView = (currentView == generatorViewModels.Value) ? driverRunViewModels.Value : generatorViewModels.Value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
        }

        public string Name { get => processInfo.Name; }

        public IEnumerable<object> Children { get => currentView; }
    }
}
