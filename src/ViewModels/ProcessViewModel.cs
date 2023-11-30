using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Controls;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class ProcessViewModel : INotifyPropertyChanged
    {
        private ProcessInfo processInfo;

        private List<GeneratorViewModel> generatorViewModels = [];

        //private Lazy<List<object>> driverRunViewModels;

        private ObservableCollection<object> currentView = [];
        private readonly IProcessEventSource eventSource;
        private readonly int processIndex;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProcessViewModel(IProcessEventSource eventSource, int processIndex)
        {
            this.eventSource = eventSource;
            this.processIndex = processIndex;

            this.eventSource.ProcessInfoUpdated += (o, e) => UpdateViewModels();
            UpdateViewModels();
        }

        [MemberNotNull(nameof(processInfo))]
        private void UpdateViewModels()
        {
            processInfo = eventSource.ProcessInfo[processIndex];
            var generators = processInfo.generators.OrderBy(g => g.name).ToList();
            
            for (int i = currentView.Count; i < generators.Count; i++)
            {
                currentView.Add(new GeneratorViewModel(generators[i], processInfo));
            }

            //generatorViewModels = new(() => processInfo.generators.Select(gi => new GeneratorViewModel(gi, processInfo)).OrderBy(gvm => gvm.Name).Cast<object>().ToList());
            //driverRunViewModels = new(() => processInfo.generators
            //    .SelectMany(g => g.executions)
            //    .GroupBy(e => e.driverRun)
            //    .Select(g => (object)new DriverRunViewModel(g.Key, processInfo.generators, processInfo))
            //    .ToList());

        }

        internal void SwitchViews()
        {
           // currentView = (currentView == generatorViewModels.Value) ? driverRunViewModels.Value : generatorViewModels.Value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
        }

        public string Name { get => processInfo.Name; }

        public IEnumerable<object> Children { get => currentView; }
    }
}
