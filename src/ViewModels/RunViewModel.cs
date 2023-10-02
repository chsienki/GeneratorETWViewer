using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorETWViewer.ViewModels
{
    internal class RunViewModel
    {
        public string Name { get; }

        public ICollection<GeneratorRunViewModel> Executions { get; }

    }
}
