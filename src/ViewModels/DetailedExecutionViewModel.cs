using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class DetailedExecutionViewModel(GeneratorRun executionInfo, GeneratorInfo generatorInfo, ProcessInfo processInfo)
    {
        public string ProcessName { get; } = processInfo.Name;

        public string GeneratorName { get; } = generatorInfo.name;

        public List<Transform> Runs { get; } = executionInfo.transforms;
    }
}
