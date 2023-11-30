using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer
{
    internal interface IProcessEventSource
    {
        public List<ProcessInfo> ProcessInfo { get; }

        public event EventHandler OnProcessInfoUpdated;
    }
}
