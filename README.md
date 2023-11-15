# GeneratorETWViewer

Generator ETW Viewer is a [PerfView](https://github.com/microsoft/perfview) plug in that helps diagnose potential performance issues caused by Roslyn IIncrementalGenerators.

## Capturing a trace for analysis

By default a trace captured with PerfView will not have any generator information in it. You will need to enable the `Microsoft-CodeAnalysis-General` provider in the `Additional Providers` box. This is hidden by default, and you will need to expand the `Advanced Options` section to see it.

## Building + Installing

Due to the way PerfView plugins are implemented a copy of PerfView is needed to build the plugin. In order to simplify building, this repo includes a copy of perfview. You will need to run the included perfview at least once before building (PerfView extracts the assemblies the plugin needs to reference to a temporary directory on first run).

PerfView is a Windows-only .NET Framework application, so building must be performed via desktop MSBuild. 

From a Visual Studio Developer Prompt:

```cmd

# clone
git clone https://github.com/chsienki/GeneratorETWViewer/
cd GeneratorETWViewer

# execute perfview once
./perfview/PerfView.exe

# build
msbuild
```

This will produce `src\bin\Debug\net472\GeneratorETWViewer.dll`

### Option A: Use the included perfview

As part of build, the plugin is 'installed' into the local copy of PerfView. If you open this copy of perfview after building, the extension will be loaded. You should see a 'Generator Info' node on a trace after opening it. 

### Option B: Install into your own copy of perfview

PerfView loads plugins from a folder called `PerfViewExtensions` located next to the `PerfView.exe` application. Create the folder next to the exe you want to load the plugin into and copy `GeneratorETWViewer.dll` there.

PerfView plugins are not autotmatically invoked after being loaded, and you have to issue a command to do so. To make this easier PerfView supports a 'startup file' that contains a list of commands to execute on startup.

In the `PerfViewExtensions` folder create a file called `PerfViewStartup` (no file extension). Add the following line and save the file:

`OnFileOpen .etl GeneratorETWViewer.Open`

Now, when opening your copy of PerfView the plugin should be loaded and activated.

