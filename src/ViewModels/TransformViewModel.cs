using System.Collections.Generic;
using System.Linq;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class TransformViewModel
    {
        private Transform transform;
        private readonly List<TransformViewModel> inputs;
        private readonly List<Transform> outputs;

        public TransformViewModel(Transform t, List<TransformViewModel?> inputs, List<Transform> outputs)
        {
            this.transform = t;
            this.inputs = inputs.Where(i => i is not null).Select(i => i!).ToList();
            this.outputs = outputs;
        }

        public string Name { get => transform.name; }

        public string Type { get => transform.newTable.type; }

        public bool IsCached { get => transform.IsCached(); }

        public string PreviousStates { get => transform.previousTable.content; }

        public string NewStates { get => transform.newTable.content; }

        public List<TransformViewModel> Inputs { get => inputs; }

        public List<TransformViewModel> Outputs { get; set; }

    }
}