using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GeneratorETWViewer.Command;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class DetailedExecutionViewModel : INotifyPropertyChanged
    {

        public DetailedExecutionViewModel(GeneratorRun executionInfo, GeneratorInfo generatorInfo, ProcessInfo processInfo)
        {
            this.executionInfo = executionInfo;
            this.generatorInfo = generatorInfo;
            this.processInfo = processInfo;
            this.SelectTransformCommand = new DelegateCommand(o => SelectedModel = (TransformViewModel)o, (o) => SelectedModel != null);
        }

        public string ProcessName { get => processInfo.Name; }

        public string GeneratorName { get => generatorInfo.name; }

        public List<Transform> Runs { get => executionInfo.transforms; }

        public List<TransformViewModel>? Inputs { get; } //= executionInfo.transforms.Select(t => new TransformViewModel(t)).ToList(); 

        public List<TransformViewModel> Outputs { get => BuildOutputs(); }

        private TransformViewModel? _selectedModel;
        private readonly GeneratorRun executionInfo;
        private readonly GeneratorInfo generatorInfo;
        private readonly ProcessInfo processInfo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TransformViewModel? SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectTransformCommand { get; }

        public List<TransformViewModel> BuildOutputs()
        {
            // start from the outputs
            var outputs = executionInfo.transforms.Where(t => t.IsOutputNode());
            List<TransformViewModel> outputVms = new List<TransformViewModel>();
            foreach (var output in outputs)
            {
                outputVms.Add(getViewModel(output));
            }
            return outputVms;

            TransformViewModel getViewModel(Transform transform)
            {
                // find the transform that matches the input
                var input1 = getVmForInput(transform.input1);
                var input2 = getVmForInput(transform.input2);

                var vm = new TransformViewModel(transform, [input1, input2], []);

                return vm;
            }

            TransformViewModel? getVmForInput(Table? table)
            {
                if (table is null || table.id == -2)
                {
                    return null;
                }

                var transform = findTransformForTable(table);
                if (transform is not null)
                {
                    return getViewModel(transform);
                }
                return null;
            }

            Transform? findTransformForTable(Table table)
            {
                // TODO: the transform that produced this table might not be a part of this run?
                // TODO: how do we handle it having the same one twice? That's because it can be the empty table, right? So we can't accurately trace it back to the transform that created it. 
                // TODO: should we just include the owning transform in the table info?
                return executionInfo.transforms.FirstOrDefault(t => t.newTable == table);
            }
        }
        
        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
