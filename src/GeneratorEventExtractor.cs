using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GeneratorETWViewer.Models;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers.ApplicationServer;
using PerfView;
using System.Windows.Markup;
using System.Drawing.Text;
using Microsoft.Diagnostics.Tracing.AutomatedAnalysis;
using System.Threading;
using ETLStackBrowse;

namespace GeneratorETWViewer
{
    internal class GeneratorEventExtractor
    {
        public static List<Models.ProcessInfo> ExtractGeneratorDriverRuns(TraceLog traceLog, Action<string> onProgress = null)
        {
            var processor = new EventProcessor();

            foreach (var e in traceLog.Events)
            {
                if (e.ProviderName == Commands.CodeAnalysisEtwName)
                {
                    processor.ProcessEvent(e);
                }
            }

            return processor.ProcessInfo;
        }

        private record GeneratorDriverRun(int executionId, DateTime startTime, List<GeneratorRun> executions);
    }
}
