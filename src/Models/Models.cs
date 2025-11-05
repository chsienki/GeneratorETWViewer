using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorETWViewer.Models
{
    record Table(int id, string content, string type);
    record Transform(int nodeHashCode, string name, EventTime time, Table previousTable, Table newTable, Table input1, Table? input2);
    record GeneratorRun(int driverRun, string projectName, EventTime time, List<Transform> transforms);
    record GeneratorInfo(string name, string assembly, List<GeneratorRun> executions);
    //record DriverRun(int id, List<GeneratorInfo> generators);
    record ProcessInfo(string Name, List<GeneratorInfo> generators, Dictionary<int, Table> tables, int totalExecutions);
    record EventTime(DateTime start, DateTime end, TimeSpan relativeStart, TimeSpan relativeEnd, TimeSpan duration);
}
