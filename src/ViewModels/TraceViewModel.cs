using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GeneratorETWViewer.Command;
using GeneratorETWViewer.Models;
using GeneratorETWViewer.ViewModels;

namespace GeneratorETWViewer.ViewModels
{
    internal class TraceViewModel : INotifyPropertyChanged
    {
        private readonly IProcessEventSource processEventSource;

        public TraceViewModel(IProcessEventSource processEventSource)
        {
            this.processEventSource = processEventSource;
            this.processEventSource.ProcessInfoUpdated += (o, e) => UpdateProcessList();
            UpdateProcessList();
        }

        public ObservableCollection<ProcessViewModel> Processes { get; } = [];

        private void UpdateProcessList()
        {
            for (int i = Processes.Count; i < processEventSource.ProcessInfo.Count; i++)
            {
                Processes.Add(new ProcessViewModel(processEventSource, i));
            }
        }

        public DelegateCommand SwitchViewsCommand => new DelegateCommand((o) => { foreach (var p in Processes) { p.SwitchViews(); } });

        public DelegateCommand ClearResultsCommand => new DelegateCommand((o) => processEventSource.Clear());

        public bool CanClear => processEventSource.SupportsClear;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RunOnUiThread(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _ = dispatcher.InvokeAsync(action);
            }
        }
    }
}