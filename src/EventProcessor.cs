using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
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
                        if (generatorTimingInfo.ContainsKey(e.ProcessID))
                        {
                            generatorTimingInfo[e.ProcessID] = generatorTimingInfo[e.ProcessID] with { totalExecutions = generatorTimingInfo[e.ProcessID].totalExecutions + 1 };
                        }
                        break;
                    case "SingleGeneratorRunTime/Start":
                        // start processing a new generator run
                        currentExecutions[(e.ProcessID, e.ThreadID)] = [GenerateStartPlaceholder(e)];
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

        private Transform GenerateStartPlaceholder(TraceEvent e)
            => new(-1, "GeneratorStart", ToEventTime(e, TimeSpan.Zero), missingTable, missingTable, missingTable, missingTable);

        public void Clear()
        {
            executionIds.Clear();
            generatorTimingInfo.Clear();
            currentExecutions.Clear();
            currentRunId.Clear();
        }

        void EnsureProcessSlot(int processID, string processName = "", string projectName = "")
        {
            if (!generatorTimingInfo.ContainsKey(processID))
            {
                generatorTimingInfo[processID] = new Models.ProcessInfo(getProcessName(processID, processName), [], [], 1);
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

        bool IsMissingExecutions(TraceEvent data)
        {
            return !currentExecutions.ContainsKey((data.ProcessID, data.ThreadID)) || !generatorTimingInfo.ContainsKey(data.ProcessID) || !currentRunId.ContainsKey((data.ProcessID, data.ThreadID));
        }

        void RecordGeneratorExecution(TraceEvent data)
        {
            if (IsMissingExecutions(data))
            {
                // we never saw a start event for this run, meaning it's incomplete. Just drop it.
                return;
            }

            var transforms = currentExecutions[(data.ProcessID, data.ThreadID)].Skip(1); // skip the start timing sentinal
            var runId = currentRunId[(data.ProcessID, data.ThreadID)];
            var processInfo = generatorTimingInfo[data.ProcessID];

            var generatorName = (string)data.PayloadByName("generatorName");
            var projectName = (string)(data.PayloadByName("projectName") ?? "<unknown project>");
            var assemblyPath = (string)data.PayloadByName("assemblyPath");
            var elapsedTime = TimeSpan.FromTicks((long)data.PayloadByName("elapsedTicks"));
            var eventTime = ToEventTime(data, elapsedTime);

            var info = processInfo.generators.SingleOrDefault(i => i.name == generatorName && i.assembly == assemblyPath);
            if (info is null)
            {
                info = new GeneratorInfo(generatorName, assemblyPath, []);
                processInfo.generators.Add(info);
            }

            info.executions.Add(new GeneratorRun(runId, projectName, eventTime, [.. transforms]));
        }

        void RecordStateTable(TraceEvent data)
        {
            if (IsMissingExecutions(data))
            {
                // we never saw a start event for this run, meaning it's incomplete. Just drop it.
                return;
            }

            var previousTransform = currentExecutions[(data.ProcessID, data.ThreadID)].Last();
            // calculate the time between the last event to give us a pretty good idea of how long this event took.
            var time = ToEventTime(data, FromMS(data.TimeStampRelativeMSec).Subtract(previousTransform.time.relativeEnd));


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
                newTable = AsCached(previousTable);
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
                    time,
                    previousTable,
                    newTable,
                    input1Table,
                    input2Table
                    );

            currentExecutions[(data.ProcessID, data.ThreadID)].Add(transform);
        }

        Table AsCached(Table table)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in table.content)
            {
                if (c is 'A' or 'C' or 'M')
                {
                    sb.Append('C');
                }
            }
            return table with { content = sb.ToString() };
        }

        TimeSpan FromMS(double ms) => TimeSpan.FromTicks((long)(ms * 10_000));

        EventTime ToEventTime(TraceEvent data, TimeSpan duration)
        {
            // data.Timestamp is the time that event was recorded, which is at the end of the actual work
            // we subtract the duration from the reported time to get the actual start time of the work that occurred
            return new EventTime(
                start: data.TimeStamp.Subtract(duration),
                end: data.TimeStamp,
                relativeStart: FromMS(data.TimeStampRelativeMSec).Subtract(duration),
                relativeEnd: FromMS(data.TimeStampRelativeMSec),
                duration);
        }
    }
}
