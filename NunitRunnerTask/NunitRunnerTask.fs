// Learn more about F# at http://fsharp.net

namespace MSBuild.Tekla.Tasks.NunitRunnerTask
#if INTERACTIVE
#r "Microsoft.Build.Framework.dll";;
#r "Microsoft.Build.Utilities.v4.0.dll";;
#endif

open System
open System.Diagnostics
open Microsoft.Build
open Microsoft.Build.Framework
open Microsoft.Build.Logging
open Microsoft.Build.Utilities
open Microsoft.Win32
open MSBuild.Tekla.Tasks.MsbuildTaskUtils
open Microsoft.FSharp.Collections
open MSBuild.Tekla.Tasks.Executor
open System.Xml.Linq
open FSharp.Data
open System.IO
open System.Threading

type KillUtils() = 

    member x.KillRunningProcesses killRunners (logger : TaskLoggingHelper) killCoverageTool =
    
        let KillProccessById Id =
            try
                Process.GetProcessById(Id).Kill()
            with
            | ex -> ()

        let LogMessage message (logger : TaskLoggingHelper) = 
            let datetime = DateTime.Now.ToString()
            if not(logger = null) then
                logger.LogMessage(datetime + " : " + message)
            else
                System.Console.WriteLine(datetime + " : " + message)
     
        LogMessage "Ensure Spanwned Process are Disposed - TS related" logger

        for name in System.Diagnostics.Process.GetProcessesByName("Floatie") do
            LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
            KillProccessById name.Id
        for name in System.Diagnostics.Process.GetProcessesByName("steel1") do
            LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
            KillProccessById name.Id
        for name in System.Diagnostics.Process.GetProcessesByName("steel2") do
            LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
            KillProccessById name.Id
        for name in System.Diagnostics.Process.GetProcessesByName("steel3") do
            LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
            KillProccessById name.Id
        for name in System.Diagnostics.Process.GetProcessesByName("Concrete1") do
            LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
            KillProccessById name.Id
        for name in System.Diagnostics.Process.GetProcessesByName("concrete1") do
            LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
            KillProccessById name.Id
        for name in System.Diagnostics.Process.GetProcessesByName("TeklaStructures") do
            LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
            KillProccessById name.Id

        if killCoverageTool then
            for name in System.Diagnostics.Process.GetProcessesByName("OpenCover.Console") do
                LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
                KillProccessById name.Id

        if killRunners then
            LogMessage "Ensure Spanwned Process are Disposed - Test Runner related" logger
            System.Console.WriteLine("Killing Running Processes")
            for name in System.Diagnostics.Process.GetProcessesByName("nunit-agent-x86") do
                LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
                KillProccessById name.Id
            for name in System.Diagnostics.Process.GetProcessesByName("nunit-agent") do
                LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
                KillProccessById name.Id
            for name in System.Diagnostics.Process.GetProcessesByName("nunit-console") do
                LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
                KillProccessById name.Id
            for name in System.Diagnostics.Process.GetProcessesByName("nunit-console-x86") do
                LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
                KillProccessById name.Id
            for name in System.Diagnostics.Process.GetProcessesByName("gallio.echo") do
                LogMessage (sprintf "Kill Process %s %i" name.ProcessName name.Id) logger
                KillProccessById name.Id

type NunitRunnerTask() as this =
    inherit Task()    
    (* Hard-coded settings *)
    let _gallioExec = "Gallio.Echo.exe"
    let _openCoverExec = "OpenCover.Console.exe"
    let _nunitExec = "nunit-console.exe"
    let _nunitExecx86 = "nunit-console-x86.exe"
    let _testrunner = "Gallio"
    let _processor = "x86"
    let _producecoverage = "false"
    let _log : TaskLoggingHelper = new TaskLoggingHelper(this)
    let executor : CommandExecutor = new CommandExecutor(_log, int64(600000000))
    let getTemporaryDirectory =
        let dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName())
        let info = System.IO.Directory.CreateDirectory(dir)
        info.FullName

    do
        ()

    /// Arguments
    member val stopWatch = Stopwatch.StartNew()
    member val TaskEnd = false with get, set     

    /// path for FxCop executable, default expects FxCop in path
    member val GallioPath = _gallioExec with get, set
    member val OpenCoverPath = _openCoverExec with get, set
    member val NunitPath = _nunitExec with get, set
    member val Processor = _processor with get, set
    member val TestRunner = _testrunner with get, set
    member val GallioTestFilter = "" with get, set
    member val NunitTestFilter = "" with get, set
    member val UseIcarus = false with get, set 

    // output properties
    member val OutputReportPaths = "" with get, set
    member val CoverageReportFormat = "coverage-report" with get, set
    member val UnitTestReportFormat = "gallio-report" with get, set
    member val GallioRunnerType = "IsolatedProcess" with get, set
    member val AttachDebugger = "false" with get, set
    member val BreakBuildOnFailedTests = "false" with get, set 

    member val Tries = 3 with get, set
    member val TimeoutOrException = false with get, set
    member val TestEntriesProblem = false with get, set

    member val ProduceCoverage = _producecoverage with get, set
    member val TeklaStructuresExecPath = "" with get, set 

    member val AssembliesToTest = "" with get, set

    /// Verify Gallio Properties - 2
    member x.verifyGallioExecProperties =
        lazy(
            if String.IsNullOrWhiteSpace(x.GallioPath) then _log.LogError("GallioPath Cannot Be Empty, Remove to use from Path")
            elif System.IO.Path.IsPathRooted(x.GallioPath) then 
                if not(System.IO.File.Exists(x.GallioPath)) then _log.LogError(sprintf "GallioPath: %s Cannot Be Found on System, Set Path Correctly" x.GallioPath)
            elif not(Utils().ExistsOnPath(x.GallioPath)) then _log.LogError(sprintf "GallioPath: %s Cannot Be Found on PATH, Set PATH variable Correctly" x.GallioPath)
            )

    /// Verify OpenCover Properties - 2
    member x.verifyOpenCoverExecProperties =
        lazy(
            if String.IsNullOrWhiteSpace(x.OpenCoverPath) then _log.LogError("OpenCoverPath Cannot Be Empty, Remove to use from Path")
            elif System.IO.Path.IsPathRooted(x.OpenCoverPath) then 
                if not(System.IO.File.Exists(x.OpenCoverPath)) then _log.LogError(sprintf "OpenCoverPath: %s Cannot Be Found on System, Set Path Correctly" x.OpenCoverPath)
            elif not(Utils().ExistsOnPath(x.OpenCoverPath)) then _log.LogError(sprintf "OpenCoverPath: %s Cannot Be Found on PATH, Set PATH variable Correctly" x.OpenCoverPath)
            )

    member x.generateCommandLineArgsForCoverageWithNunit =
        let builder = new CommandLineBuilder()

        let GetDebugger = 
            if x.AttachDebugger.Equals("true") then
                "/debug "
            else
                ""

        let GetAssemblies = 
            let mutable stringtests = ""
            // target assemblies
            if not(String.IsNullOrWhiteSpace(x.AssembliesToTest)) then
                let dirs = x.AssembliesToTest.Split(";".ToCharArray())

                for dir in dirs do
                    stringtests <- stringtests + dir + " "
            stringtests.TrimEnd()

        let getUnitPath = 
            if x.Processor.Equals("x86") then
                x.NunitPath + "\\" + _nunitExecx86
            else
                x.NunitPath + "\\" + _nunitExec

        // misc options
        builder.AppendSwitch("-register:user")
        builder.AppendSwitch("-target:" + getUnitPath)
        builder.AppendSwitch("-targetdir:" + x.OutputReportPaths)
        builder.AppendSwitch("-targetargs:\"/nologo" +
                             " /result=" + x.OutputReportPaths + "\\" + x.UnitTestReportFormat + 
                             " " + x.NunitTestFilter + 
                             " /labels /noshadow " +
                             GetAssemblies + "\"") 

        builder.AppendSwitch("-mergebyhash")
        builder.AppendSwitch("-output:" + x.OutputReportPaths + "\\" + x.CoverageReportFormat)

        builder.ToString()

    member x.generateCommandLineArgsForCoverageWithGallio =
        let builder = new CommandLineBuilder()

        let GetDebugger = 
            if x.AttachDebugger.Equals("true") then
                "/debug "
            else
                ""

        let GetAssemblies = 
            let mutable stringtests = ""
            // target assemblies
            if not(String.IsNullOrWhiteSpace(x.AssembliesToTest)) then
                let dirs = x.AssembliesToTest.Split(";".ToCharArray())

                for dir in dirs do
                    if not(String.IsNullOrEmpty(dir.Trim())) then
                        stringtests <- stringtests + dir + " "

            stringtests.TrimEnd()

        let GetFilter =
            if not(String.IsNullOrEmpty(x.GallioTestFilter)) then
                " /f:\"\"" + x.GallioTestFilter + "\"\" "
            else
                String.Empty

        // misc options
        builder.AppendSwitch("-register:user")
        builder.AppendSwitch("-target:" + x.GallioPath)
        builder.AppendSwitch("-targetdir:" + x.OutputReportPaths)
        builder.AppendSwitch("-targetargs:\"/r:" + x.GallioRunnerType + 
                             " /report-directory:" + x.OutputReportPaths +
                             " /report-name-format:" + x.UnitTestReportFormat +
                             " /report-type:Xml " +
                             GetFilter + 
                             GetDebugger +
                             GetAssemblies + "\"") 

        builder.AppendSwitch("-mergebyhash")
        builder.AppendSwitch("-output:" + x.OutputReportPaths + "\\" + x.CoverageReportFormat + ".xml")

        builder.ToString()

    member x.generateCommandLineArgsForNunit =
        let builder = new CommandLineBuilder()
        
        let GetAssemblies = 
            let mutable stringtests = ""
            // target assemblies
            if not(String.IsNullOrWhiteSpace(x.AssembliesToTest)) then
                let dirs = x.AssembliesToTest.Split(";".ToCharArray())

                for dir in dirs do
                    stringtests <- stringtests + dir + " "
            stringtests

        // misc options
        builder.AppendSwitch("/nologo")
        builder.AppendSwitch("/labels")
        builder.AppendSwitch("/noshadow")
        builder.AppendSwitch(x.NunitTestFilter)
        builder.AppendSwitch("/result=" + x.OutputReportPaths + "\\" + x.UnitTestReportFormat)
        builder.AppendSwitch(GetAssemblies)

        builder.ToString()

    member x.generateCommandLineArgsForGallio =
        let builder = new CommandLineBuilder()

        let GetDebugger = 
            if x.AttachDebugger.Equals("true") then
                "/debug "
            else
                ""

        let GetAssemblies = 
            let mutable stringtests = ""
            // target assemblies
            if not(String.IsNullOrWhiteSpace(x.AssembliesToTest)) then
                let dirs = x.AssembliesToTest.Split(";".ToCharArray())

                for dir in dirs do
                    stringtests <- stringtests + dir + " "
            stringtests

        let GetFilter =
            if not(String.IsNullOrEmpty(x.GallioTestFilter)) then
                " /f:\"" + x.GallioTestFilter + "\" "
            else
                String.Empty

        // misc options
        builder.AppendSwitch("/r:" + x.GallioRunnerType)
        builder.AppendSwitch("/report-directory:" + x.OutputReportPaths)
        builder.AppendSwitch("/report-name-format:" + x.UnitTestReportFormat)
        builder.AppendSwitch("/report-type:Xml")
        builder.AppendSwitch(GetFilter)
        builder.AppendSwitch(GetDebugger)
        builder.AppendSwitch(GetAssemblies)

        builder.ToString()

    member x.generateCommandLineArgsForGallioIcarus =
        let builder = new CommandLineBuilder()

        let GetAssemblies = 
            let mutable stringtests = ""
            // target assemblies
            if not(String.IsNullOrWhiteSpace(x.AssembliesToTest)) then
                let dirs = x.AssembliesToTest.Split(";".ToCharArray())

                for dir in dirs do
                    stringtests <- stringtests + dir + " "
            stringtests

        // misc options
        builder.AppendSwitch(GetAssemblies)
        builder.ToString()

    member x.ExecuteRunner executor execcmd cmdLineArgs =
        // set environment
        let env = Map.ofList [("TEKLA_COP_SOLUTION_PATH", "")]
        let returncode = (executor :> ICommandExecutor).ExecuteCommand(execcmd, cmdLineArgs, env, x.ProcessOutputDataReceived, x.ProcessOutputDataReceived, Directory.GetParent(execcmd).ToString())
        let mutable returndata = true
        if returncode > 0 then
            returndata <- false
        returndata 

    member x.RunTests =

        let getUnitPath = 
            if x.Processor.Equals("x86") then
                x.NunitPath + "\\" + _nunitExecx86
            else
                x.NunitPath + "\\" + _nunitExec

        if x.ProduceCoverage.Equals("true") then
            if x.TestRunner.Equals("Gallio") then
                _log.LogMessage(sprintf "Gallio test runner Command: %s %s" x.OpenCoverPath x.generateCommandLineArgsForCoverageWithGallio)
                x.ExecuteRunner executor x.OpenCoverPath x.generateCommandLineArgsForCoverageWithGallio
            else
                _log.LogMessage(sprintf "NUnit test runner Command: %s %s" x.OpenCoverPath x.generateCommandLineArgsForCoverageWithNunit)
                x.ExecuteRunner executor x.OpenCoverPath x.generateCommandLineArgsForCoverageWithNunit
        else
            if x.TestRunner.Equals("Gallio") then
                
                if x.UseIcarus then
                    let path = Path.Combine(Path.GetDirectoryName(x.GallioPath), "Gallio.Icarus.exe")
                    _log.LogMessage(sprintf "Gallio Icarus Command: %s %s" path x.generateCommandLineArgsForGallio)
                    x.ExecuteRunner executor path x.generateCommandLineArgsForGallioIcarus
                else
                    _log.LogMessage(sprintf "Gallio test runner Command: %s %s" x.GallioPath x.generateCommandLineArgsForGallio)
                    x.ExecuteRunner executor x.GallioPath x.generateCommandLineArgsForGallio
            else
                _log.LogMessage(sprintf "NUnit test runner Command: %s %s" getUnitPath x.generateCommandLineArgsForNunit)
                x.ExecuteRunner executor getUnitPath x.generateCommandLineArgsForNunit  

    member x.ProcessOutputDataReceived(e : DataReceivedEventArgs) =
        
        if not(String.IsNullOrWhiteSpace(e.Data)) &&  x.Tries > 0 then
            this.stopWatch.Restart()
            let datetime = DateTime.Now.ToString()
            _log.LogMessage(sprintf "%s : %s" datetime e.Data)

            if e.Data.Contains("'Tekla.Structures.ModelInternal.DelegateProxy' threw an exception.") 
            || e.Data.Contains("System.Runtime.Remoting.RemotingException: Failed to connect to an IPC") then
                _log.LogWarning(e.Data)
                _log.LogMessage("Reset And Retry with a different test runner")
                //if x.TestRunner = "Gallio" then
                //    x.TestRunner <- "Nunit"

                x.Tries <- x.Tries - 1
                Thread.Sleep(5000)
                if x.Tries > 0 then
                    KillUtils().KillRunningProcesses true _log true
                    x.RunTests |> ignore
                else
                    KillUtils().KillRunningProcesses true _log false
                    _log.LogError("Number of Exceptions Exceed, cannot recover")
                    x.TimeoutOrException <- true

            if e.Data.Contains("Cannot find file") || e.Data.Contains("Could not find file") || e.Data.Contains("File type not known:") then
                _log.LogError("Test Assembly Was Not Found: Check Configuration" + e.Data)
                x.TestEntriesProblem <- true
                

            if e.Data.Contains("0 run, 0 passed, 0 failed, 0 inconclusive, 0 skipped") then
                _log.LogError("No Tests Have Run, Check Configuration: " + e.Data)
                x.TestEntriesProblem <- true

            if e.Data.Contains("[failed] Namespace") then
                if x.BreakBuildOnFailedTests.Equals("true") then
                    _log.LogError("Test Failed: " + e.Data)
                else
                    _log.LogWarning("Test Failed: " + e.Data)

            if e.Data.Contains("Tests run:") then
                if x.BreakBuildOnFailedTests.Equals("false") then
                    if not(e.Data.Contains("Errors: 0, Failures: 0,")) then
                        _log.LogWarning("There are errors or failed test: " + e.Data)
                else
                    if not(e.Data.Contains("Errors: 0, Failures: 0,")) then
                        _log.LogError("There are errors or failed test: " + e.Data)
                Thread.Sleep(5000)
                KillUtils().KillRunningProcesses true _log false

            if e.Data.Contains("Disposing the test runner.") then
                Thread.Sleep(5000)
                KillUtils().KillRunningProcesses true _log false
        ()

    member x.TimerControl(timeout : int64) =
        async {
          
          _log.LogMessage(sprintf "Nunit Start %d" this.stopWatch.ElapsedMilliseconds)

          while this.stopWatch.ElapsedMilliseconds < timeout do
            Thread.Sleep(1000)
          
          if not(this.TaskEnd) then
            _log.LogError(sprintf "Nunit Timeout Timeout %d" this.stopWatch.ElapsedMilliseconds)
          
          x.TimeoutOrException <- true

          if not(x.UseIcarus) then
            KillUtils().KillRunningProcesses true _log false
          
        }

    override x.Execute() =
        let tempDir = getTemporaryDirectory
        if String.IsNullOrEmpty(x.OutputReportPaths) then
            x.OutputReportPaths <- tempDir

        let mutable result = true

        if String.IsNullOrWhiteSpace(x.AssembliesToTest) then
            _log.LogWarning("No test assemblies defined, not running tests")
        else 
            this.verifyOpenCoverExecProperties.Force()

            result <- not(_log.HasLoggedErrors)

            if result && not(String.IsNullOrWhiteSpace(x.AssembliesToTest)) then

                if File.Exists(x.TeklaStructuresExecPath + "\\TeklaStructures.exe.crash.log") then
                    File.Delete(x.TeklaStructuresExecPath + "\\TeklaStructures.exe.crash.log")

                this.stopWatch.Restart()
                Async.Start(this.TimerControl(int64(600000)))

                result <- x.RunTests
                if not(x.BreakBuildOnFailedTests.Equals("true")) then
                    if not(result) then
                        _log.LogMessage("Changing Status of Runner to Ok, break build is off")
                        result <- true
                if not(x.TeklaStructuresExecPath.Equals("")) then
                    KillUtils().KillRunningProcesses false _log false
                         
                if x.TimeoutOrException then
                    result <- false

                if File.Exists(x.TeklaStructuresExecPath + "\\TeklaStructures.exe.crash.log") then
                    _log.LogWarning("Found CrashLog")
                    let allLines = File.ReadAllLines(x.TeklaStructuresExecPath + "\\TeklaStructures.exe.crash.log")
                    for line in allLines do
                        _log.LogMessage("Crash Log Entry : " + line)
                        if line.Contains("Exception EXCEPTION_ACCESS_VIOLATION in [unknown]") then
                            _log.LogWarning("Potential ACCESS_VIOLATION : " + line)
                        elif line.Contains("Exception EXCEPTION_ACCESS_VIOLATION in") then
                            _log.LogError("ACCESS_VIOLATION : " + line)
                            result <- false

                if File.Exists(Path.Combine(x.TeklaStructuresExecPath,"TeklaStructures.ini")) then
                    File.Delete(Path.Combine(x.TeklaStructuresExecPath,"TeklaStructures.ini"))

            if tempDir.Equals(x.OutputReportPaths) then
                Directory.Delete(x.OutputReportPaths, true)

            if not(result) then
                _log.LogError("Nunit Task Runner Failed")
            if x.TestEntriesProblem then
                result <- false

        result

    interface ICancelableTask with
        member this.Cancel() =
            KillUtils().KillRunningProcesses true null true
            Environment.Exit(0)
            ()
