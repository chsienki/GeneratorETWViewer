// See https://aka.ms/new-console-template for more information

using Microsoft.Diagnostics.Tracing.Etlx;
using static System.Runtime.InteropServices.JavaScript.JSType;

const string EtlxFile = @"C:\Users\chsienki\Documents\Completion-PerfMetrics-2.merged.etl\Completion-PerfMetrics-2.merged.etlx";
TraceLog traceLog = new TraceLog(EtlxFile);

bool inDriverStart = false;
bool inGeneratorStart = false;
int currentThread = 0;
int currentEx = -1;
string currentGenerator = "";

//var list = traceLog.Events.Where((e) => e.ProviderName == "Microsoft-CodeAnalysis-General").OrderBy((e) => e.TimeStampRelativeMSec).ToList();

//for (int i = 0; i < list.Count; i++)
//{
//    Microsoft.Diagnostics.Tracing.TraceEvent? e = list[i];
foreach(var e in traceLog.Events) 
{ 
    if (e.EventName == "GeneratorDriverRunTime/Start")
    {
        inDriverStart = true;
        currentThread = e.ThreadID;
        currentEx++;
    }
    else if (e.EventName == "GeneratorDriverRunTime/Stop")
    {
        inDriverStart = false;
    }
    else if (e.EventName == "SingleGeneratorRunTime/Start")
    {
        inGeneratorStart = true;
        currentGenerator = e.PayloadByName("generatorName").ToString();
    }
    else if (e.EventName == "SingleGeneratorRunTime/Stop")
    {
        inGeneratorStart = false;
    }
    else if (e.EventName == "BuildStateTable")
    {
        if (((string)e.PayloadByName("newTableContent")).Contains("C"))
        {
            Console.WriteLine("Found cached");
        }
        //if (!inDriverStart || !inGeneratorStart)
        //{
        //    // yep we're on a different thread (!)
        //    Console.WriteLine("Unexpected");
        //}
    }
}