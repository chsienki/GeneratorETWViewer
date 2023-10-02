using System;
using System.Collections.Generic;
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
        private readonly List<ProcessViewModel> processes;

        public TraceViewModel(List<Models.ProcessInfo> processInfo)
        {
            this.processes = processInfo.Select(pi => new ProcessViewModel(pi)).ToList();
        }

        public ICollection<ProcessViewModel> Processes
        {
            get => processes;
        }

        public DelegateCommand SwitchViewsCommand => new DelegateCommand((o) => { foreach (var p in processes) { p.SwitchViews(); } });

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