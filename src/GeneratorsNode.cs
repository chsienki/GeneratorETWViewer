using GeneratorETWViewer.Command;
using GeneratorETWViewer.Models;
using Microsoft.Diagnostics.Tracing.Analysis.GC;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers.JScript;
using Microsoft.Diagnostics.Tracing.Stacks;
using PerfView;
using PerfViewExtensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace GeneratorETWViewer
{
    internal class GeneratorsNode : PerfViewDirectory
    {
        private readonly string dataFile;

        public GeneratorsNode(string dataFile, bool hasExecution)
            : base("")
        {
            this.Name = "Generator Info";
            this.IsExpanded = true;
            this.m_Children = new List<PerfViewTreeItem>() { };
            this.dataFile = dataFile;
        }

        public override void Close()
        {
        }

        public override void Open(Window parentWindow, StatusBar worker, Action doAfter = null)
        {
            var etlFile = CommandEnvironment.OpenETLFile(dataFile);
            if (!etlFile.Events.EventNames.Any(n => n.Contains(Commands.CodeAnalysisEtwName)))
            {
                this.Children.Add(new NoGeneratorInfo());
            }
            else
            {
                // we should always have timing info
                this.Children.Add(new TimingNode(etlFile.Events));

                // TODO: check for executions and add a node if so
                this.Children.Add(new ExecutionNode(etlFile.TraceLog));

                OpenCPUStacksCommand.DataFile = etlFile;
            }

        }

        internal class NoGeneratorInfo : PerfViewTreeItem
        {
            public NoGeneratorInfo()
            {
                this.Name = "No Generator Info Detected";
            }

            public override void Close() { }

            public override void Open(Window parentWindow, StatusBar worker, Action doAfter = null) { }
        }

        internal class TimingNode : PerfViewTreeItem
        {
            private readonly Events events;

            public TimingNode(Events events)
            {
                this.Name = "Timing Info";
                this.events = events;
            }

            public override void Close()
            {
            }

            public override void Open(Window parentWindow, StatusBar worker, Action? doAfter = null)
            {
                // oh, but we're going to use the stacks view, so we just need to mutate the events into a stack 


                var s = new Stacks(new MyStackSource());
                CommandEnvironment.OpenStackViewer(s);
            }
        }

        internal class ExecutionNode : PerfViewTreeItem
        {
            private readonly TraceLog traceLog;

            public ExecutionNode(TraceLog traceLog)
            {
                this.Name = "Execution Info";
                this.traceLog = traceLog;
            }

            public override void Close()
            {
            }

            public override void Open(Window parentWindow, StatusBar worker, Action? doAfter = null)
            {
                ETLFileEventSource? results = null;
                worker.StartWork("Loading generator events", () =>
                {
                    results = ETLFileEventSource.ExtractGeneratorDriverRuns(traceLog, s => worker.Log(s));
                    worker.EndWork(() => { });
                },
                () =>
                {
                    var vm = new ViewModels.TraceViewModel(results!);
                    var view = new GeneratorEvents() { DataContext = vm };
                    view.Show();
                });

            }
        }
    }

    public class MyStackSource : StackSource
    {
        //private StackSourceInterner _interner;

        public MyStackSource()
        {
            //_interner = new StackSourceInterner(10, 10, 1);

        }

        public override bool OnlyManagedCodeStacks { get => true; }

        public override int CallStackIndexLimit { get; } = 10; // number of callstacks
        public override int CallFrameIndexLimit { get; } = 10; // total number of frames whithin all callstacks

        public override int SampleIndexLimit => 10;

        public override void ForEach(Action<StackSourceSample> callback)
        {
            // ROOT
            //callback(new StackSourceSample(this)
            //{
            //    StackIndex = (StackSourceCallStackIndex)0,
            //    Count = 1,
            //    Metric = 0,
            //});

            for (int i = 0, k = 0; i < CallStackIndexLimit; i++)
            {
                //  for (int j = 0; j <= i; j++, k++)
                //{
                callback(new StackSourceSample(this)
                {
                    StackIndex = (StackSourceCallStackIndex)(i),
                    SampleIndex = (StackSourceSampleIndex)k,
                    Count = 1,
                    Metric = 1,
                    //Scenario = i switch
                    //{
                    //    0 => 0,
                    //    1 => 1,
                    //    2 => 2,
                    //    3 => 3,
                    //    4 => 1,
                    //    5 => 1,
                    //    6 => 2,
                    //    7 => 2,
                    //    8 => 3,
                    //    9 => 3,
                    //    _ => 4
                    //}
                });
                //}

            }
        }

        public override StackSourceCallStackIndex GetCallerIndex(StackSourceCallStackIndex callStackIndex)
        {
            return (StackSourceCallStackIndex)((int)callStackIndex switch
            {
                0 => -1, // ROOT
                1 or 2 or 3 => 0,
                4 or 5 => 1,
                6 or 7 => 2,
                8 or 9 => 3,
                _ => throw new NotImplementedException()
            });

            //return (StackSourceCallStackIndex)((int)callStackIndex - 1);
            // return (StackSourceCallStackIndex)(callStackIndex == 0 ? -1 : 0);
            //if((int)callStackIndex >= CallStackIndexLimit)
            //{
            //    return StackSourceCallStackIndex.Start;
            //}
            //return _interner.GetCallerIndex(callStackIndex);
        }

        public override StackSourceFrameIndex GetFrameIndex(StackSourceCallStackIndex callStackIndex)
        {
            if (callStackIndex == 0)
                return 0;

            return (StackSourceFrameIndex)(callStackIndex);
            // return (StackSourceFrameIndex)((int)(callStackIndex - 1) % (CallFrameIndexLimit - 1));
            //if ((int)callStackIndex >= CallStackIndexLimit)
            //{
            //    return StackSourceFrameIndex.Unknown;
            //}
            //return _interner.GetFrameIndex(callStackIndex);
        }
        //public override int ScenarioCount => 4;

        public override bool IsGraphSource => true;

        public override void GetReferences(StackSourceSampleIndex nodeIndex, RefDirection direction, Action<StackSourceSampleIndex> callback)
        {
            base.GetReferences(nodeIndex, direction, callback);
        }

        public override string GetFrameName(StackSourceFrameIndex frameIndex, bool verboseName)
        {
            return (int)frameIndex switch
            {
                0 => "Root",
                1 => "R_A",
                2 => "R_B",
                3 => "R_C",
                4 => "R_A_A1",
                5 => "R_A_A2",
                6 => "R_B_B1",
                7 => "R_B_B2",
                8 => "R_C_C1",
                9 => "R_C_C2",
                _ => "?"
            };


            //if(frameIndex >= StackSourceFrameIndex.Start)
            //{
            //return "Name: " + (int)frameIndex;
            //}
            //else
            //{
            //    return frameIndex.ToString();
            //}
        }
    }
}
