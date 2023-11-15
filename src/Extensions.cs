using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ETLStackBrowse;
using GeneratorETWViewer.Models;

namespace GeneratorETWViewer
{
    internal static class Extensions
    {
        internal static bool IsInputNode(this Transform transform)
        {
            // we should probably expose MS.CA.WellKnownGeneratorInputs publicly for this (or track inputs in the telemetry directly)
            return transform.name == "Compilation"
                || transform.name == "CompilationOptions"
                || transform.name == "ParseOptions"
                || transform.name == "AdditionalTexts"
                || transform.name == "AnalyzerConfigOptions"
                || transform.name == "MetadataReferences";
        }

        internal static bool IsOutputNode(this Transform transform)
        {
            return transform.name == "SourceOutput"
                || transform.name == "ImplementationSourceOutput"
                || transform.name == "HostOutput";
        }

        internal static bool IsCached(this Transform transform)
        {
            // for most nodes we could just check that input == newtable, but that doesn't apply to input nodes at the moment

            return !transform.newTable.content.Contains("A")
                && !transform.newTable.content.Contains("M")
                && !transform.newTable.content.Contains("R");
        }
    }
}