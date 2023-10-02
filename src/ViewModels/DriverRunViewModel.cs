using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class DriverRunViewModel(int count, List<GeneratorInfo> generators, ProcessInfo processInfo)
    {
        public string Name { get => $"Run {count}"; }

        public ICollection<GeneratorRunViewModel> Executions { get => generators
                .Select(generator => (generator, generator.executions.SingleOrDefault(e => e.driverRun == count)))
                .Where(p => p.Item2 is not null)
                .Select(p => new GeneratorRunViewModel(p.generator.name, p.Item2, p.generator, processInfo))
                .ToList(); 
        }
    }
}
