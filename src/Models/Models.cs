using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorETWViewer.Models
{
    record Table(int id, string content, string type);
    record Transform(int nodeHashCode, string name, Table previousTable, Table newTable, Table input1, Table? input2);
    record GeneratorRun(int driverRun, DateTime startTime, TimeSpan relativeStartTime, TimeSpan executionTime, List<Transform> transforms);
    record GeneratorInfo(string name, string assembly, List<GeneratorRun> executions);
    //record DriverRun(int id, List<GeneratorInfo> generators);
    record ProcessInfo(string Name, List<GeneratorInfo> generators, Dictionary<int, Table> tables, int totalExecutions);
}
