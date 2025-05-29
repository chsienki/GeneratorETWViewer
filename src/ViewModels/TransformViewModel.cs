using System;
using System.Collections.Generic;
using System.Linq;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels
{
    internal class TransformViewModel
    {
        private readonly Transform transform;
        private readonly List<TransformViewModel?> inputs;

        private readonly double durationInclusive;

        public TransformViewModel(Transform t, List<TransformViewModel?> inputs)
        {
            this.transform = t;
            this.inputs = [.. inputs.Where(i => i is not null).Select(i => i!)];
            this.durationInclusive = CalcualateInclusiveTime(inputs);
        }

        public string Name { get => transform.name; }

        public string Type { get => transform.newTable.type; }

        public bool IsCached { get => transform.IsCached(); }

        public string PreviousStates { get => transform.previousTable.content; }

        public string NewStates { get => transform.newTable.content; }

        public EventTime EventTime { get => transform.time; }

        public double DurationExclusive { get => transform.time.duration.TotalMilliseconds; }

        public double DurationInclusive { get => durationInclusive; }

        public TimeSpan Duration { get => transform.time.duration; }

        public List<TransformViewModel?> Inputs { get => inputs; }

        private double CalcualateInclusiveTime(List<TransformViewModel?> inputs)
        {
            // we have to filter out the duplicate inputs further up the tree, so we can't just recursively calculate inclusive time.
            //TODO: something here isn't quite right

            List<Transform> visitedModels = [];
            double totalTime = 0;

            VisitInputList(inputs);
            return totalTime + transform.time.duration.TotalMilliseconds;

            void VisitInputList(List<TransformViewModel?> children)
            {
                foreach (var model in children)
                {
                    if (model is not null && !visitedModels.Contains(model.transform))
                    {
                        visitedModels.Add(model.transform);
                        VisitInputList(model.inputs);
                    }
                    totalTime += model?.transform.time.duration.TotalMilliseconds ?? 0;
                }
            }
        }
    }
}