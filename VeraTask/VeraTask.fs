// Learn more about F# at http://fsharp.net

namespace MSBuild.Tekla.Tasks.Vera
#if INTERACTIVE
#r "Microsoft.Build.Framework.dll";;
#r "Microsoft.Build.Utilities.v4.0.dll";;
#endif

open FSharp.Data
open System
open System.IO
open System.Xml.Linq
open System.Diagnostics
open Microsoft.Build
open Microsoft.Build.Framework
open Microsoft.Build.Logging
open Microsoft.Build.Utilities
open Microsoft.Win32
open MSBuild.Tekla.Tasks.MsbuildTaskUtils
open MSBuild.Tekla.Tasks.Executor

type VeraErrorX(filename:string, line:string, severity:string, message:string, source:string) =
    member val filename = filename
    member val line = line
    member val severity = severity
    member val message = message
    member val source = source

type VeraTask() as this =
    inherit Task()
    let logger : TaskLoggingHelper = new TaskLoggingHelper(this)
    let executor : CommandExecutor = new CommandExecutor(logger, int64(1500000))
    let _VeraExec = "Vera.exe"

    member val totalViolations : int = 0 with get, set
    member val buildok : bool = true with get, set
    member val counter : int = 0 with get, set
    member val ToOutputData : string list = [] with get, set

    /// Solution Path, Required
    [<Required>]
    member val SolutionPathToAnalyse = "" with get, set

    /// Optional result target file. Must be unique to each test run.
    [<Required>]
    member val VeraOutputPath = "" with get, set

    /// path for Vera executable, default expects Vera in path
    member val VeraOutputType = "vs7" with get, set
    member val VeraPath = _VeraExec with get, set
    member val VeraOptions = "" with get, set
    member val VeraIgnores = "" with get, set
    member val PathReplacementStrings = "" with get, set

    /// Verify Xml output
    member x.VerifyOutput(logger  : TaskLoggingHelper) =
        lazy(
            if String.IsNullOrWhiteSpace(x.VeraOutputType) then logger.LogError("Output Type cannot be empty, use vs7 or xml")
            elif not("xml" = x.VeraOutputType) && not("vs7" = x.VeraOutputType) then logger.LogError("Output Type Invalid, use vs7 or xml")
            elif "xml" = x.VeraOutputType && String.IsNullOrWhiteSpace(x.VeraOutputType) then logger.LogError("VeraOutputType: Output Report Path should be defined for xml reporting")
            )

    /// Verify Vera is found on Path
    member x.verifyVeraExecProperties(logger  : TaskLoggingHelper) =
        lazy(
            if String.IsNullOrWhiteSpace(x.VeraPath) then logger.LogError("VeraPath Cannot Be Empty, Remove to use from Path")
            elif System.IO.Path.IsPathRooted(x.VeraPath) then 
                if not(System.IO.File.Exists(x.VeraPath)) then logger.LogError(sprintf "VeraPath: %s Cannot Be Found on System, Set Path Correctly" x.VeraPath)
            elif not(Utils().ExistsOnPath(x.VeraPath)) then logger.LogError(sprintf "VeraPath: %s Cannot Be Found on PATH, Set PATH variable Correctly" x.VeraPath)
            )

    member x.generateCommandLineArgs(fileToAnalyse : string)=
        let builder = new CommandLineBuilder()

        // options
        if not(String.IsNullOrWhiteSpace(x.VeraOptions)) then
            let values = x.VeraOptions.Split(";".ToCharArray())
            for value in values do
                builder.AppendSwitch(value)

        builder.AppendSwitch(fileToAnalyse)

        builder.ToString()

    member x.ExecuteVera executor filepath ouputFilePath =
        // set environment
        let mutable env = Map.ofList []
        if Environment.GetEnvironmentVariable("VERA_ROOT") = null then
            let pathVera = Path.Combine(Directory.GetParent(Directory.GetParent(x.VeraPath).ToString()).ToString(), "lib\\vera++")
            env <- Map.ofList [("VERA_ROOT", pathVera)]

        let mutable tries = 3
        let mutable returncode = 1

        while tries > 0  && returncode > 0 do
            (executor :> ICommandExecutor).ResetData()
            returncode <- (executor :> ICommandExecutor).ExecuteCommand(x.VeraPath, x.generateCommandLineArgs(filepath), env)
            if not((executor :> ICommandExecutor).GetErrorCode = ReturnCode.Ok) || returncode > 0 then
                tries <- tries - 1
                if this.BuildEngine = null then
                    Console.WriteLine("Vera: Try Failed")
                    (executor :> ICommandExecutor).GetStdError |> fun s -> for i in s do Console.WriteLine(i)
                else
                    logger.LogWarning("Vera: Try Failed: Error")
                    (executor :> ICommandExecutor).GetStdError |> fun s -> for i in s do logger.LogWarning(i)

                /// and a newline to the end of the file # https://bitbucket.org/verateam/vera/issue/34/last-line-comment-produces-an-error
                use wr = new StreamWriter(filepath, true)
                wr.WriteLine("")

        if tries = 0 then
            if this.BuildEngine = null then
                Console.WriteLine("Vera: Number of tries exceeded")
                (executor :> ICommandExecutor).GetStdError |> fun s -> for i in s do Console.WriteLine(i)
            else
                logger.LogError("Vera: Number of tries exceeded")
                (executor :> ICommandExecutor).GetStdError |> fun s -> for i in s do logger.LogError(i)

            (executor :> ICommandExecutor).GetStdOut |> fun s -> for i in s do  if this.BuildEngine = null then Console.WriteLine(i) else logger.LogError(i)
            this.buildok <- false
            false
        else
            let getVeraWarningFromLine(line:string) =
                let linerelative = line.Replace(Directory.GetParent(x.SolutionPathToAnalyse).ToString(), "")
                let elems = linerelative.Split(':')
                let file = elems.[0]
                let line = elems.[1]
                let ruleid = elems.[2].Trim().Split('(').[1].Split(')').[0]
                let message = elems.[2].Split(')').[1].Trim()
                VeraErrorX(file, line, "warning", message, ruleid)

            let addLine (line:string) =                  
                use wr = new StreamWriter(ouputFilePath, true)
                wr.WriteLine(line)

            if not(x.VeraOutputType = "vs7") then
                let parentdir = Directory.GetParent(ouputFilePath).ToString()
                if File.Exists(ouputFilePath) then File.Delete(ouputFilePath)
                if not(Directory.Exists(parentdir)) then Directory.CreateDirectory(parentdir) |> ignore

                let writeError(line:string) =
                    let veraelement = getVeraWarningFromLine(line)
                    let message = Utils().EscapeString(veraelement.message)
                    let error = sprintf """<error line="%s" severity="%s" message="%s" source="%s"/>""" veraelement.line veraelement.severity message veraelement.source
                    addLine(error)
                    this.totalViolations <- this.totalViolations + 1
                
                addLine("""<?xml version="1.0" encoding="UTF-8"?>""")
                addLine("""<checkstyle version="5.0">""")
                let fileNameLine = sprintf """<file name="%s">""" filepath
                addLine(fileNameLine)
                (executor :> ICommandExecutor).GetStdError |> Seq.iter (fun x -> writeError(x))
                addLine("""</file>""")
                addLine("""</checkstyle>""")
            else
                let WriteToVsOUtput(line:string)=
                    this.totalViolations <- this.totalViolations + 1
                    let veraelement = getVeraWarningFromLine(line)
                    if this.BuildEngine = null then
                        let data = sprintf "%s : %s : %s : %s" filepath veraelement.line veraelement.severity (veraelement.message  + " " + veraelement.source)
                        Console.WriteLine(data);
                    else
                        logger.LogWarning("", veraelement.severity, "", filepath, Convert.ToInt32(veraelement.line), 0, 0, 0, veraelement.message)

                let lines = (executor :> ICommandExecutor).GetStdError

                if lines <> List.Empty then
                    lines |> Seq.iter (fun x -> WriteToVsOUtput(x))

            true

    override x.Execute() =

        this.verifyVeraExecProperties(logger).Force()
        this.VerifyOutput(logger).Force()

        let mutable result = not(logger.HasLoggedErrors)
        if result then
            let stopWatchTotal = Stopwatch.StartNew()
            let solutionHelper = new VSSolutionUtils()
            let projectHelper = new VSProjectUtils()

            if Directory.Exists(x.VeraOutputPath) then
                for filename in Directory.GetFiles(x.VeraOutputPath, @"vera++-result-*.xml") do
                    File.Delete(filename)
            else
                Directory.CreateDirectory(x.VeraOutputPath) |> ignore

            let iterateOverFiles (file : string) (projectPath : string) =
                let ignoreFiles = x.VeraIgnores.Split(";".ToCharArray())
                let projectPathDir = Directory.GetParent(projectPath).ToString()
                let mutable skip = false

                if not(file.Contains(Directory.GetParent(x.SolutionPathToAnalyse).ToString())) then
                    skip <- true

                for ignore in ignoreFiles do
                    let pathignore = Path.Combine(Directory.GetParent(x.SolutionPathToAnalyse).ToString(), ignore.Trim())
                    if Path.GetFullPath(file) = Path.GetFullPath(pathignore) then skip <- true

                if not(skip) then
                    let index = sprintf "%i" this.counter 
                    let xml_file = Path.Combine(x.VeraOutputPath, String.Concat(String.Concat("vera++-result-", index),".xml"))
                    let arguments = x.generateCommandLineArgs(file)
                    logger.LogMessage(sprintf "Vera++Command: %s %s" x.VeraPath arguments)
                    x.ExecuteVera executor file xml_file |> ignore
                    this.counter <- this.counter + 1
                ()

            let iterateOverProjectFiles(projectFile : ProjectFiles) = 
                projectHelper.GetCompilationFiles(projectFile.path, "", x.PathReplacementStrings)  |> Seq.iter (fun x -> iterateOverFiles x projectFile.path)

            solutionHelper.GetProjectFilesFromSolutions(x.SolutionPathToAnalyse) |> Seq.iter (fun x -> iterateOverProjectFiles x)

            logger.LogMessage(sprintf "Total Violations: %u" this.totalViolations)
            logger.LogMessage(sprintf "Vera End: %u ms" stopWatchTotal.ElapsedMilliseconds)

        if result && this.buildok then
            true
        else
            false

    interface ICancelableTask with
        member this.Cancel() =
            (executor :> ICommandExecutor).CancelExecution |> ignore
            Environment.Exit(0)
            ()
