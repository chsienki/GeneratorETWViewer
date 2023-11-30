using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneratorETWViewer.Models;
using Microsoft.Diagnostics.Tracing;

namespace GeneratorETWViewer
{
    internal class EventProcessor
    {
        public List<ProcessInfo> ProcessInfo { get => [.. generatorTimingInfo.Values]; }

        public event EventHandler? OnProcessInfoUpdated;

        private static readonly Table missingTable = new(-2, "", "<missing>");

        private readonly Dictionary<int, int> executionIds = [];
        private readonly Dictionary<int, Models.ProcessInfo> generatorTimingInfo = [];
        private readonly Dictionary<(int processId, int threadId), List<Transform>> currentExecutions = [];
        private readonly Dictionary<(int processId, int threadId), int> currentRunId = [];

        public void ProcessEvent(TraceEvent e)
        {
            if (e.ProviderName == Commands.CodeAnalysisEtwName)
            {
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
                        currentExecutions[(e.ProcessID, e.ThreadID)] = [];
                        break;
                    case "SingleGeneratorRunTime/Stop":
                        RecordGeneratorExecution(e);
                        break;
                    case "BuildStateTable":
                        RecordStateTable(e);
                        break;
                }

                OnProcessInfoUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        void EnsureProcessSlot(int processID, string processName = "")
        {
            if (!generatorTimingInfo.ContainsKey(processID))
            {
                generatorTimingInfo[processID] = new Models.ProcessInfo(getProcessName(processID, processName), new List<GeneratorInfo>(), new Dictionary<int, Table>(), 1);
            }
            if (!executionIds.ContainsKey(processID))
            {
                executionIds[processID] = 0;
            }

            static string getProcessName(int processID, string processName)
            {
                if (!string.IsNullOrWhiteSpace(processName))
                {
                    return processName;
                }

                processName = "Process";
                foreach (var proc in Process.GetProcesses())
                {
                    if (proc.Id == processID)
                    {
                        processName = proc.ProcessName;
                        break;
                    }
                }
                return $"{processName} ({processID})";
            }

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

            Table? input2Table = null;
            if (input2Id != -1 && !tables.TryGetValue(input2Id, out input2Table))
            {
                input2Table = missingTable;
            }

            var transform = new Transform(
                    (int)data.PayloadByName("nodeHashCode"),
                    (string)data.PayloadByName("name"),
                    previousTable,
                    newTable,
                    input1Table,
                    input2Table
                    );

            currentExecutions[(data.ProcessID, data.ThreadID)].Add(transform);
        }
    }
}
