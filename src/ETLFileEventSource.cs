using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing.Etlx;

namespace GeneratorETWViewer
{
    internal class ETLFileEventSource : IProcessEventSource
    {
        private ETLFileEventSource(List<Models.ProcessInfo> processInfo)
        {
            this.ProcessInfo = processInfo;
        }

        public List<Models.ProcessInfo> ProcessInfo { get; }

        public event EventHandler? ProcessInfoUpdated;

        public bool SupportsClear => false;

        public void Clear() => throw new NotImplementedException();

        public static ETLFileEventSource ExtractGeneratorDriverRuns(TraceLog traceLog, Action<string> onProgress = null)
        {
            var processor = new EventProcessor();

            foreach (var e in traceLog.Events)
            {
                if (e.ProviderName == Commands.CodeAnalysisEtwName)
                {
                    processor.ProcessEvent(e);
                }
            }

            return new ETLFileEventSource(processor.ProcessInfo);
        }
    }
}
