using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using GeneratorETWViewer;
using Microsoft.Diagnostics.Tracing.Etlx;
using PerfView;
using GeneratorETWViewer.ViewModels;
using static Vanara.PInvoke.Shell32;
using GeneratorETWViewer.Views;

namespace OopHost
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string EtlFile = "PerfViewData_Executions.etl";

        //private const string EtlFile = "DebugCombine.etl";

        //private const string EtlFile = @"C:\Users\chsienki\Documents\Completion-PerfMetrics-2.merged.etl\Completion-PerfMetrics-2.merged.etl";
        public App()
        {
            ProgressDialog progressDialog = new ProgressDialog();
            IProgressDialog iProgressDialog = (IProgressDialog)progressDialog;

            //iProgressDialog.Timer(PDTIMER.PDTIMER_RESUME); 
            //iProgressDialog.SetTitle("Reading ETL File");
            //iProgressDialog.StartProgressDialog(dwFlags: PROGDLG.PROGDLG_MARQUEEPROGRESS);
            
            var etlxFile = Path.ChangeExtension(EtlFile, "etlx");
            if (!File.Exists(etlxFile))
            {
                //iProgressDialog.SetLine(1, "Extracting ETL File", false);
                //iProgressDialog.SetLine(2, $"Converting file {EtlFile}", false);
                TraceLog.CreateFromEventTraceLogFile(EtlFile, etlxFile);
            }

            //iProgressDialog.SetLine(1, "Converting Events", false);
            //var results = ETLFileEventSource.ExtractGeneratorDriverRuns(new TraceLog(etlxFile), (s) => { }/* iProgressDialog.SetLine(2, s, false)*/);
            var results = new LiveEventSource();
            //iProgressDialog.StopProgressDialog();

            GeneratorEvents view = new(new(results));
            //DetailedView view = new(new(results[0].generators[4].executions[1], results[0].generators[4], results[0]));
            //DetailedView view = new(new(results[0].generators[0].executions[0], results[0].generators[0], results[0]));

            view.Show();
            view.Activate();

            view.Closed += (o, e) =>
            {
                results.Dispose();
            };

           // results.Dispose();
            

            //DetailedExecutionViewModel vm = new DetailedExecutionViewModel(results[0].generators[1].executions[0], results[0].generators[1], results[0]);
            //DetailedView view = new DetailedView(vm);
            //view.Show();
            //view.Activate();
        }
    }
}


