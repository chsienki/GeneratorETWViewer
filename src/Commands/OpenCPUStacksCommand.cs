using System;
using System.Windows.Input;
using GeneratorETWViewer.Models;
using PerfViewExtensibility;

namespace GeneratorETWViewer.Command
{
    internal class OpenCPUStacksCommand : ICommand
    {
        // TODO: should we move these off the command and into a dedicated static space somewhere?
        internal static ETLDataFile? DataFile { get; set; } 

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter) => DataFile is not null;

        public void Execute(object parameter)
        {
            if(DataFile is not null)
            {
                EventTime time = (EventTime)parameter;
                var cpuStacks = DataFile.CPUStacks(false);
                cpuStacks.Filter.StartTimeRelativeMSec = time.relativeStart.TotalMilliseconds.ToString();
                cpuStacks.Filter.EndTimeRelativeMSec = time.relativeEnd.TotalMilliseconds.ToString();
                cpuStacks.Filter.GroupRegExs = "";
                CommandEnvironment.OpenStackViewer(cpuStacks, (window) =>
                {
                });
            }
            

        }
    }
}
