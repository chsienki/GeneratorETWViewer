using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
                (TimeSpan Start, TimeSpan Length) = ((TimeSpan, TimeSpan)) parameter;
                var cpuStacks = DataFile.CPUStacks();
                cpuStacks.Filter.StartTimeRelativeMSec = Start.TotalMilliseconds.ToString();
                cpuStacks.Filter.EndTimeRelativeMSec = (Start + Length).TotalMilliseconds.ToString();
                CommandEnvironment.OpenStackViewer(cpuStacks, (window) =>
                {
                });
            }
            

        }
    }
}
