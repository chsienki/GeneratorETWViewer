using System;
using System.Collections.Generic;
using System.Linq;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels;

internal class GeneratorRunViewModel(string name, GeneratorRun executionInfo, GeneratorInfo generatorInfo, ProcessInfo processInfo)
{
    public GeneratorInfo GeneratorInfo { get; } = generatorInfo;

    public ProcessInfo ProcessInfo { get; } = processInfo;

    public GeneratorRun ExecutionInfo { get; } = executionInfo;

    public double ExecutionTime { get => ExecutionInfo.executionTime.TotalMilliseconds; }

    public DateTime StartTime { get => ExecutionInfo.startTime; }

    public (TimeSpan StartTime, TimeSpan ExecutionTime) RelativeTimeRange { get => (ExecutionInfo.relativeStartTime, ExecutionInfo.executionTime); }

    public string Name { get => name; }

    public List<string> ChangedInputs { get => ExecutionInfo.transforms.Where(t => t.IsInputNode() && !t.IsCached()).Select(t => t.name).Distinct().ToList(); }

    public int ChangedOutputCount { get => ExecutionInfo.transforms.Where(t => t.IsOutputNode() && !t.IsCached()).Count(); }

    public bool HadNewOutputs { get => ChangedOutputCount > 0; }

}