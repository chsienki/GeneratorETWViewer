using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using GeneratorETWViewer.Models;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers.ApplicationServer;
using PerfView;
using Microsoft.Diagnostics.Tracing;
using System.Windows.Markup;
using System.Drawing.Text;
using Microsoft.Diagnostics.Tracing.AutomatedAnalysis;
using System.Threading;

namespace GeneratorETWViewer
{
    internal class GeneratorEventExtractor
    {
        private static Table missingTable = new Table(-2, "", "<missing>");

        public static List<Models.ProcessInfo> ExtractGeneratorDriverRuns(TraceLog traceLog, Action<string> onProgress = null)
        {
            var executionIds = new Dictionary<int, int>();
            var generatorTimingInfo = new Dictionary<int, Models.ProcessInfo>();
            var currentExecutions = new Dictionary<(int processId, int threadId), List<Transform>>();
            var currentRunId = new Dictionary<(int processId, int threadId), int>();

            // Note that in an async operation the thread can change from underneath you. But we know that generators are single threaded and non-async.
            // So between a runStart and a runEnd we can guarantee that we're looking at the same operation

            foreach (var e in traceLog.Events)
            {
                if (e.ProviderName == Commands.CodeAnalysisEtwName)
                {
                    //if (e.EventName != "BuildStateTable")
                    //{
                    //    onProgress?.Invoke($"Parsing event: {e.EventName}");
                    //}

                    switch (e.EventName)
                    {
                        case "GeneratorDriverRunTime/Start":
                            EnsureProcessSlot(e.ProcessID, e.ProcessName);
                            currentRunId[(e.ProcessID, e.ThreadID)] = executionIds[e.ProcessID]++;
                            break;
                        case "GeneratorDriverRunTime/Stop":
                            EnsureProcessSlot(e.ProcessID, e.ProcessName);
                            generatorTimingInfo[e.ProcessID] = generatorTimingInfo[e.ProcessID] with { totalExecutions = generatorTimingInfo[e.ProcessID].totalExecutions + 1 };
                            break;
                        case "SingleGeneratorRunTime/Start":
                            // start processing a new generator run
                            currentExecutions[(e.ProcessID, e.ThreadID)] = new List<Transform>();
                            break;
                        case "SingleGeneratorRunTime/Stop":
                            RecordGeneratorExecution(e);
                            break;
                        case "BuildStateTable":
                            RecordStateTable(e);
                            break;
                    }
                }
            }
            return generatorTimingInfo.Values.ToList();

            void EnsureProcessSlot(int processID, string processName = "")
            {
                if (!generatorTimingInfo.ContainsKey(processID))
                {
                    generatorTimingInfo[processID] = new Models.ProcessInfo(processName, new List<GeneratorInfo>(), new Dictionary<int, Table>(), 1);
                }
                if (!executionIds.ContainsKey(processID))
                {
                    executionIds[processID] = 0;
                }
            }

            void EnsureRunSlot(int processID, int threadID)
            {
            }

            void RecordGeneratorExecution(TraceEvent data)
            {
                if (!currentExecutions.ContainsKey(((data.ProcessID, data.ThreadID))) || !generatorTimingInfo.ContainsKey(data.ProcessID))
                {
                    // we never saw a start event for this run, meaning it's incomplete. Just drop it.
                    return;
                }

                var transforms = currentExecutions[(data.ProcessID, data.ThreadID)];
                var runId = currentRunId[(data.ProcessID, data.ThreadID)];
                var processInfo = generatorTimingInfo[data.ProcessID];

                var generatorName = (string)data.PayloadByName("generatorName");
                var assemblyPath = (string)data.PayloadByName("assemblyPath");
                var elapsedTime = TimeSpan.FromTicks((long)data.PayloadByName("elapsedTicks"));
                var startTime = data.TimeStamp.Subtract(elapsedTime);
                var relativeStartTime = TimeSpan.FromMilliseconds(data.TimeStampRelativeMSec);

                var info = processInfo.generators.SingleOrDefault(i => i.name == generatorName && i.assembly == assemblyPath);
                if (info is null)
                {
                    info = new GeneratorInfo(generatorName, assemblyPath, new List<GeneratorRun>() { });
                    processInfo.generators.Add(info);
                }

                info.executions.Add(new GeneratorRun(runId, startTime, relativeStartTime, elapsedTime, new List<Transform>(transforms)));
            }

            void RecordStateTable(TraceEvent data)
            {
                var previousTableId = (int)data.PayloadByName("previousTable");
                var newTableId = (int)data.PayloadByName("newTable");
                var tableType = (string)data.PayloadByName("tableType");

                var tables = generatorTimingInfo[data.ProcessID].tables;

                if (!tables.TryGetValue(previousTableId, out var previousTable))
                {
                    previousTable = tables[previousTableId] = new Table(previousTableId, (string)data.PayloadByName("previousTableContent"), tableType); // might be the empty table // BUG; we only have a single empty table, the type will be wrong for some
                }

                if (!tables.TryGetValue(newTableId, out var newTable))
                {
                    newTable = tables[newTableId] = new Table(newTableId, (string)data.PayloadByName("newTableContent"), tableType); // might be the empty table 
                }

                if (newTable.id == -1)
                {
                    newTable = previousTable;
                }

                var input1Id = (int)data.PayloadByName("input1");
                var input2Id = (int)data.PayloadByName("input2");

                if (!tables.TryGetValue(input1Id, out var input1Table))
                {
                    input1Table = missingTable;
                }

                if (input2Id != -1 && !tables.TryGetValue(input1Id, out var input2Table))
                {
                    input2Table = missingTable;
                }
                else
                {
                    input2Table = null;
                }

                var transform = new Transform(
                        (int)data.PayloadValue(0),
                        (string)data.PayloadByName("name"),
                        previousTable,
                        newTable,
                        input1Table,
                        input2Table
                        );

                currentExecutions[(data.ProcessID, data.ThreadID)].Add(transform);
            }
        }

        private record GeneratorDriverRun(int executionId, DateTime startTime, List<GeneratorRun> executions);
    }
}
