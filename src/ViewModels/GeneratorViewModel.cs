using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer.ViewModels;

internal class GeneratorViewModel : INotifyPropertyChanged
{
    private readonly IProcessEventSource eventSource;
    private readonly int processIndex;
    private readonly int generatorIndex;
    private GeneratorInfo generatorInfo;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<GeneratorRunViewModel> Executions { get; } = [];

    public GeneratorViewModel(IProcessEventSource eventSource, int processIndex, int generatorIndex)
    {
        this.eventSource = eventSource;
        this.processIndex = processIndex;
        this.generatorIndex = generatorIndex;

        this.eventSource.ProcessInfoUpdated += (o, e) => UpdateGeneratorInfo();
        UpdateGeneratorInfo();
    }

    [MemberNotNull(nameof(generatorInfo))]
    public void UpdateGeneratorInfo()
    {
        generatorInfo = eventSource.ProcessInfo[processIndex].generators[generatorIndex];
        for (int i = Executions.Count; i < generatorInfo.executions.Count; i++)
        {
            Executions.Add(new GeneratorRunViewModel($" Run {i}", generatorInfo.executions[i], generatorInfo, eventSource.ProcessInfo[processIndex]));
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalExecutions)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AverageExecutionTime)));
    }

    public string Name { get => generatorInfo.name; }
    
    public int TotalExecutions { get => generatorInfo.executions.Count; }

    public double AverageExecutionTime { get => generatorInfo.executions.Select(e => e.executionTime.TotalMilliseconds).Average();} 
}