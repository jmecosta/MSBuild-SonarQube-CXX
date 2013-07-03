﻿// Learn more about F# at http://fsharp.net

namespace MSBuild.Tekla.Tasks
#if INTERACTIVE
#r "Microsoft.Build.Framework.dll";;
#r "Microsoft.Build.Utilities.v4.0.dll";;
#endif
open System
open System.IO
open System.Diagnostics
open Microsoft.Build
open Microsoft.Build.Framework
open Microsoft.Build.Logging
open Microsoft.Build.Utilities
open Microsoft.Win32
open System.Threading

type GenericTaskExecuter() =
    let shellExecute program args env =
        let startInfo = ProcessStartInfo(FileName = program,
                                       Arguments = args,
                                       UseShellExecute = false,
                                       RedirectStandardOutput = true)
        env |> List.map startInfo.EnvironmentVariables.Add |> ignore
        Process.Start(startInfo)

    member this.ExecuteProgram(logger, program, args, env, timouthandler : Stopwatch) =
        let proc = shellExecute program args env
        let rec printProcessOutputs (reader:System.IO.StreamReader) =
            timouthandler.Restart()
            let line = reader.ReadLine()
            if not (String.IsNullOrEmpty(line)) then logger(line)
            if (proc.StandardOutput.EndOfStream) then ()
            else printProcessOutputs reader
        printProcessOutputs proc.StandardOutput
        proc.WaitForExit()
        proc

type IntelInspectorTask() as this =
    inherit Task()
    (* Hard-coded settings *)
    let _inspectorExecutable = "inspxe-cl"
    let _inspectorExecutableMC = "inspxe-runmc"
    let _log : TaskLoggingHelper = new TaskLoggingHelper(this)

    let getTemporaryDirectory =
        let dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName())
        let info = System.IO.Directory.CreateDirectory(dir)
        info.FullName
    
    do
        ()

    member val proc = new Process()  with get, set
    member val stopWatch = Stopwatch.StartNew()     
    
    /// Specifies how many stack frames are captured in leak analysis
    member val StackDepth = 8 with get, set

    /// Whether to include resource leaks into reports or not (defaults to false)
    member val ResourceLeaks = false with get, set

    member val TaskEnd = false with get, set
        
    /// Specifies the analysis type, default is "Detect Leaks" -> mi1
    member val AnalysisType = "mi1" with get, set

    /// Runtime mode. Valid options are native, mixed or managed. Defaults to native.
    member val RuntimeMode = "auto" with get, set

    /// Optional result target file. Must be unique to each test run.
    member val ResultFile = "" with get, set

    /// Environment variables for executable
    member val ExecutableEnvironment = "" with get, set

    /// Path to suppression file.
    member val SuppressionFile = null with get, set

    /// Whether to print the report in build output
    member val PrintReport = false with get, set

    /// Specifies the executable to check for memory leaks
    [<Required>]
    member val ExecutableToRun = "" with get, set

    [<Required>]
    member val PathToIntelInspector = "C:\Program Files (x86)\Intel\Inspector XE 2013" with get, set

    member val AppWorkDir = "" with get, set
        
    /// Specifies the executable command line parameters
    member val ExecutableParameters : string = null with get, set

    member x.getInspectorFullPath =
        if this.PathToIntelInspector = "" then
            _inspectorExecutable
        else
            Path.Combine(Path.Combine(this.PathToIntelInspector, "bin32"), _inspectorExecutable)

    /// Parses the given environment variable string: "foo=bar;baz=fob" etc.. to (string*string)list
    member x.getEnvironment =
        if System.String.IsNullOrWhiteSpace(x.ExecutableEnvironment) then List.empty
        else
            let splitString (f:string) =
                let keyValue = f.Split("=".ToCharArray())
                (keyValue.[0], keyValue.[1])
            x.ExecutableEnvironment.Split(";".ToCharArray()) |> Array.map(splitString) |> List.ofArray

    /// Check that Intel Inspector is installed or report a failure
    /// Check that ResultDirectory exists or is created
    /// Check that ResultDirectory is specified if PrintReport is true
    member x.verifyProperties =
        lazy(
            if not(String.IsNullOrWhiteSpace(x.SuppressionFile)) && not(System.IO.File.Exists(x.SuppressionFile)) then
                _log.LogError(sprintf "Suppression file does not exist %s" x.SuppressionFile))

    member x.generateCommandLine (temp:string) =
        let getBoolean booleanValue = if booleanValue then "true" else "false"

        let builder = new CommandLineBuilder()
        builder.AppendSwitchIfNotNull("--collect ", x.AnalysisType)
        builder.AppendSwitchIfNotNull("-knob stack-depth=", (sprintf "%d" x.StackDepth))
        builder.AppendSwitchIfNotNull("-mrte-mode=", x.RuntimeMode)
        builder.AppendSwitchIfNotNull("-result-dir=", temp)
        builder.AppendSwitchIfNotNull("-suppression-file=", x.SuppressionFile)
        builder.AppendSwitch("-knob enable-memory-growth-detection=true")
        builder.AppendSwitch("-verbose")
        builder.AppendSwitch(sprintf "-knob resources=%s" (getBoolean x.ResourceLeaks))
        builder.AppendSwitchIfNotNull("-- ", x.ExecutableToRun)
        builder.AppendSwitchUnquotedIfNotNull(" ", x.ExecutableParameters)
        builder.ToString()

    member x.generateCommandLineForReport (temp:string) =
        let builder = new CommandLineBuilder()
        builder.AppendSwitch("--report problems")
        builder.AppendSwitchIfNotNull("-result-dir=", temp)
        builder.ToString()

    member x.formatOutputCode code =
        if code = 1 then "Usage error"
        else if code = 2 then "Internal error"
        else if code = 4 then "Application returned a non-zero exit code"
        else if code = 8 then "At least one new problem detected"
        else if code = 12 then "Application returned a non-zero exit code and at least one new problem detected"
        else failwith "Unexpected error code"

    member x.doPrintReportAboutProblems (te:GenericTaskExecuter) env temp =
        te.ExecuteProgram(_log.LogWarning,
            x.getInspectorFullPath,
            x.generateCommandLineForReport temp,
            env, this.stopWatch) |> ignore
        ()

    member x.memoryCheck cmdLine env temp =
        let te = GenericTaskExecuter()
        this.proc <- te.ExecuteProgram(_log.LogMessage, x.getInspectorFullPath, cmdLine, env, this.stopWatch)
        if not(this.proc.ExitCode = 0) then
            if x.PrintReport then x.doPrintReportAboutProblems te env temp
            _log.LogWarning(sprintf "Process exitec with code %d: %s" this.proc.ExitCode (x.formatOutputCode this.proc.ExitCode))
        ()


    member x.exportCsv tempDir =
        let temporaryFileName = Path.GetTempFileName()
        use writer = System.IO.File.AppendText(temporaryFileName)
        let cmd = (sprintf "-report problems -format=csv -result-dir=%s" tempDir)
        this.proc <- GenericTaskExecuter().ExecuteProgram(writer.WriteLine, x.getInspectorFullPath, cmd, List.empty, this.stopWatch)
        this.proc.WaitForExit()
        writer.Close()
        if not(this.proc.ExitCode = 0) then
            _log.LogMessage(sprintf "Process exitec with code %d: %s" this.proc.ExitCode (x.formatOutputCode this.proc.ExitCode))

        // covert csv to xml file supported by sonar
        let ErrorCategories = ["cross-thread stack access";
  "data race";
  "deadlock";
  "gdi resource leak";
  "incorrect memcpy call";
  "invalid deallocation";
  "invalid memory access";
  "invalid partial memory access";
  "kernel resource leak";
  "lock hierarchy violation";
  "memory growth";
  "memory leak";
  "memory not deallocated";
  "mismatched allocation/deallocation";
  "missing allocation";
  "thread exit information";
  "thread start information";
  "unhandled application exception";
  "uninitialized memory access";
  "uninitialized partial memory access"]

        let ErrorCategoriesKeys = ["CrossThreadStackAccess";
  "DataRace";
  "DeadLock";
  "GDIResourceLeak";
  "IncorrectMemcpyCall";
  "InvalidDeallocation";
  "InvalidMemoryAccess";
  "InvalidPartialMemoryAccess";
  "KernelResourceLeak";
  "LockHierarchyViolation";
  "MemoryGrowth";
  "MemoryLeak";
  "MemoryNotDeallocated";
  "MismatchedAllocation/Deallocation";
  "MissingAllocation";
  "ThreadExitInformation";
  "ThreadStartInformation";
  "UnhandledApplicationException";
  "UninitializedMemoryAccess";
  "UninitializedPartialMemoryAccess"]

        let addLine (line:string) =                  
            use wr = new StreamWriter(x.ResultFile, true)
            wr.WriteLine(line)

        try System.IO.File.Delete(x.ResultFile) with _ -> ()
        let parentName = Directory.GetParent(x.ResultFile).ToString()
        if not(Directory.Exists(parentName)) then Directory.CreateDirectory(parentName) |> ignore

        addLine("""<?xml version="1.0" encoding="UTF-8"?>""")
        addLine("""<results>""")

        let fileLines = File.ReadAllLines(temporaryFileName)

        for line in fileLines do
            let elems = line.Split(',')
            if elems.Length > 5 then
                let source = elems.[0].Replace("\"","")
                let mutable line = elems.[1].Replace("\"","")
                try
                    if Convert.ToInt32(line) > 1000000 then line <- "0"
                with
                | ex -> line <- "0"
                let severity = elems.[2].Replace("\"","")
                let id = elems.[3].Replace("\"","") + "." + elems.[4].Replace("\"","")
                let typeid = elems.[5].Replace("\"","").Trim()
                let description = elems.[5].Replace("\"","").Trim()
                let functionid = elems.[5].Replace("\"","").Trim()
                let moduleid = elems.[5].Replace("\"","").Trim()
                let msg = "[" + x.ExecutableToRun + "] " + typeid + " : " + description

                let mutable foundrule = false
                let mutable index = 0
                for i in 1 .. ErrorCategories.Length-1 do
                    if typeid.ToLower() = ErrorCategories.[i] then
                        index <- i
                        foundrule <- true

                if foundrule then
                    let errormsg = sprintf """<error file="%s" line="%s" id="intelXe.%s" severity="%s" msg="%s"/>""" source line ErrorCategoriesKeys.[index] severity msg
                    addLine(errormsg)
                else
                    let errormsg = sprintf """<error file="%s" line="%s" id="intelXe.%s" severity="%s" msg="%s"/>""" source line severity severity ("Rule Not Found In Sonar Profile: " + msg)
                    addLine(errormsg)

        addLine("""</results>""")
        try System.IO.File.Delete(temporaryFileName) with _ -> ()         
        ()

    member this.TimerControl(timeout : int64) =
        async {
          
          while this.stopWatch.ElapsedMilliseconds < timeout do
            Thread.Sleep(1000)
          
          if not(this.TaskEnd) then
            _log.LogMessage(sprintf "Intel Timeout %d" this.stopWatch.ElapsedMilliseconds)

            for name in System.Diagnostics.Process.GetProcessesByName(_inspectorExecutable) do
                Process.GetProcessById(name.Id).Kill()
            for name in System.Diagnostics.Process.GetProcessesByName(_inspectorExecutableMC) do
                Process.GetProcessById(name.Id).Kill()
            for name in System.Diagnostics.Process.GetProcessesByName(Path.GetFileName(this.ExecutableToRun)) do
                Process.GetProcessById(name.Id).Kill()

            Environment.Exit(0)
        }

    override x.Execute() =
        let tempDir = getTemporaryDirectory
        this.verifyProperties.Force()
        let result = not(_log.HasLoggedErrors)
        if result then
            let cmdLine = x.generateCommandLine tempDir
            _log.LogCommandLine(sprintf "%s %s" x.getInspectorFullPath cmdLine)
            this.stopWatch.Restart()
            Async.Start(this.TimerControl(int64(900000)))
            x.memoryCheck cmdLine x.getEnvironment tempDir
            this.TaskEnd <- true
            if not(System.String.IsNullOrWhiteSpace x.ResultFile) then
                x.exportCsv tempDir
        result
        
    interface ICancelableTask with
        member this.Cancel() =
            Environment.Exit(0)
            ()      
