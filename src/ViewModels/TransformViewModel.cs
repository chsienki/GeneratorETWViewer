using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class TransformViewModel
    {
        private Transform transform;

        public TransformViewModel(Transform t)
        {
            this.transform = t;
        }

        public string Name { get => transform.name; }

    }
}