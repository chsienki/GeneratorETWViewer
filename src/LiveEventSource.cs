using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GeneratorETWViewer.Models;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace GeneratorETWViewer
{
    internal class LiveEventSource : IProcessEventSource, IDisposable
    {
        public event EventHandler? ProcessInfoUpdated;

        private readonly TraceEventSession traceEventSession;

        private EventProcessor processor;

        public List<ProcessInfo> ProcessInfo { get => processor.ProcessInfo; }

        public bool SupportsClear => false; // suprisingly complicated :/

        public LiveEventSource()
        {
            if (!TraceEventSession.IsElevated() ?? false)
            {
                throw new InvalidOperationException("Must run as admin");
            }

            processor = new EventProcessor();
            processor.OnProcessInfoUpdated += (o, e) => RunOnUiThread(() => ProcessInfoUpdated?.Invoke(o, e));
            traceEventSession = new TraceEventSession("Microsoft-CodeAnalysis-Generators-Trace-Session");
            traceEventSession.Source.Dynamic.AddCallbackForProviderEvents(ShouldAccept, processor.ProcessEvent);
            traceEventSession.EnableProvider("Microsoft-CodeAnalysis-General", providerLevel: TraceEventLevel.Verbose);

            Task.Run(traceEventSession.Source.Process);
        }


        public void Clear() => processor.Clear();

        private EventFilterResponse ShouldAccept(string providerName, string eventName)
        {
            if(providerName != "Microsoft-CodeAnalysis-General")
            {
                return EventFilterResponse.RejectProvider;
            }

            // just assume we want all MS-CA-General events
            return EventFilterResponse.AcceptEvent;
        }

        private void RunOnUiThread(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _ = dispatcher.InvokeAsync(action);
            }
        }

        public void Dispose()
        {
            traceEventSession.Source.StopProcessing();
            traceEventSession.Dispose();
        }
    }
}
