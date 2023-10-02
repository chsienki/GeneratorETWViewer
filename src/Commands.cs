using GeneratorETWViewer.Command;
using Microsoft.Diagnostics.Tracing.Etlx;
using PerfView;
using PerfViewExtensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EventSource = EventSources.EventSource;

public class Commands : CommandEnvironment
{
    internal const string CodeAnalysisEtwName = "Microsoft-CodeAnalysis-General";

    public void Open(string dataFilePath)
    {
        var guiFile = PerfViewFile.Get(dataFilePath);
        if (guiFile != null && guiFile.Children is not null)
        {
            guiFile.Children.Add(new GeneratorETWViewer.GeneratorsNode(dataFilePath, false));
        }
    }

    public void TryGetGeneratorInfo(string dataFilePath, string viewName)
    {
        MessageBox.Show("Looking for info");
    }
}
