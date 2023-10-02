using System;
using System.Collections.Generic;
using System.Linq;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels;

internal class GeneratorViewModel(Models.GeneratorInfo generatorInfo, Models.ProcessInfo processInfo)
{
    public string Name { get => generatorInfo.name; }
    
    public int TotalExecutions { get => generatorInfo.executions.Count; }

    public double AverageExecutionTime { get => generatorInfo.executions.Select(e => e.executionTime.TotalMilliseconds).Average();} 

    public List<GeneratorRunViewModel> Executions { get => generatorInfo.executions.Select((e, i) => new GeneratorRunViewModel($" Run {i}", e, generatorInfo, processInfo)).ToList(); }

    internal ProcessInfo ProcessInfo => processInfo;
}