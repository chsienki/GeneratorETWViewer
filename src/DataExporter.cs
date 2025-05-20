using System.Diagnostics;
using System.IO;

namespace GeneratorETWViewer
{
    internal class DataExporter
    {
        public static void ExportAndOpenFile(IProcessEventSource eventSource)
        {
            var file = Path.GetTempFileName();
            using (var stream = new StreamWriter(new FileStream(file, FileMode.Open, FileAccess.Write)))
            {

                // write the header
                stream.WriteLine("process,generator_name,generator_assembly,run_id,start_time,execution_time_ms");

                foreach (var process in eventSource.ProcessInfo)
                {
                    foreach (var generator in process.generators)
                    {
                        foreach (var execution in generator.executions)
                        {
                            stream.WriteLine($"{process.Name},{generator.name},{generator.assembly},{execution.driverRun},{execution.startTime:HH:mm:ss.fff},{execution.executionTime.TotalMilliseconds}");
                        }
                    }
                }
            }

            // rename to csv
            var csvFile = Path.ChangeExtension(file, "csv");
            File.Move(file, csvFile);

            // Open the file in Excel
            Process.Start("excel.exe", csvFile);
        }
    }
}
