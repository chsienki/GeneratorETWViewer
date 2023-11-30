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

        private ObservableCollection<object> currentView = [];

        private readonly IProcessEventSource eventSource;
        
        private readonly int processIndex;

        private bool isGeneratorMode = true;

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
            if(isGeneratorMode)
            {
                var generators = processInfo.generators.OrderBy(g => g.name).ToList();
                for (int i = currentView.Count; i < generators.Count; i++)
                {
                    currentView.Add(new GeneratorViewModel(eventSource, processIndex, i));
                }
            }
            else
            {
                var runs = processInfo.generators.SelectMany(g => g.executions).GroupBy(e => e.driverRun).ToList();
                for (int i = currentView.Count; i < runs.Count; i++)
                {
                    currentView.Add(new DriverRunViewModel(runs[i].Key, processInfo.generators, processInfo));
                }
            }
        }

        internal void SwitchViews()
        {
            isGeneratorMode = !isGeneratorMode;
            currentView.Clear();
            UpdateViewModels();
        }

        public string Name { get => processInfo.Name; }

        public IEnumerable<object> Children { get => currentView; }
    }
}
