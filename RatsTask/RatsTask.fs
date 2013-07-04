﻿// Learn more about F# at http://fsharp.net

namespace MSBuild.Tekla.Tasks.Rats
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
open System.Xml
open Microsoft.Build.Evaluation
open Microsoft.Build.Framework
open Microsoft.Build.Logging
open Microsoft.Build.Utilities
open Microsoft.Win32
open MSBuild.Tekla.Tasks.MsbuildTaskUtils
open MSBuild.Tekla.Tasks.Executor

type RatsError = XmlProvider<"""<?xml version="1.0"?><rats_output>
<stats>
<dbcount lang="perl">33</dbcount>
<dbcount lang="ruby">46</dbcount>
<dbcount lang="python">62</dbcount>
<dbcount lang="c">334</dbcount>
<dbcount lang="php">55</dbcount>
</stats>
<analyzed>E:\TSSRC\Core\Common\libtools\test\tool_base64_test.cpp</analyzed>
<vulnerability>
  <severity>High</severity>
  <type>fixed size global buffer</type>
  <message>
    Extra care should be taken to ensure that character arrays that are
    allocated on the stack are used safely.  They are prime targets for
    buffer overflow attacks.
  </message>
  <file>
    <name>E:\TSSRC\Core\Common\libtools\test\tool_base64_test.cpp</name>
    <line>114</line>
    <line>115</line>
    <line>116</line>
  </file>
</vulnerability>
<vulnerability>
  <severity>Medium</severity>
  <type>srand</type>
  <message>
    Standard random number generators should not be used to 
generate randomness used for security reasons.  For security sensitive 
randomness a crytographic randomness generator that provides sufficient
entropy should be used.
  </message>
  <file>
    <name>E:\TSSRC\Core\Common\libtools\test\tool_base64_test.cpp</name>
    <line>121</line>
  </file>
</vulnerability>
<timing>
<total_lines>141</total_lines>
<total_time>0.000000</total_time>
<lines_per_second>-2147483648</lines_per_second>
</timing>
</rats_output>""">

type RatsTask() as this =
    inherit Task()
    let logger : TaskLoggingHelper = new TaskLoggingHelper(this)
    let executor : CommandExecutor = new CommandExecutor(logger, int64(1500000))
    let _ratsExec = "rats.exe"

    member val buildok : bool = true with get, set
    member val counter : int = 0 with get, set
    member val ToOutputData : string list = [] with get, set
    member val totalViolations : int = 0 with get, set

    /// Solution Path, Required
    [<Required>]
    member val SolutionPathToAnalyse = "" with get, set

    /// Optional result target file. Must be unique to each test run.
    [<Required>]
    member val RatsOutputPath = "" with get, set

    /// path for Rats executable, default expects Rats in path
    member val RatsOutputType = "vs7" with get, set
    member val RatsPath = _ratsExec with get, set
    member val PathReplacementStrings = "" with get, set
    member val RatsOptions = "" with get, set
    member val RatsIgnores = "" with get, set

    /// Verify Xml output
    member x.VerifyOutput(logger  : TaskLoggingHelper) =
        lazy(
            if String.IsNullOrWhiteSpace(x.RatsOutputType) then logger.LogError("Output Type cannot be empty, use vs7 or xml")
            elif not("xml" = x.RatsOutputType) && not("vs7" = x.RatsOutputType) then logger.LogError("Output Type Invalid, use vs7 or xml")
            elif "xml" = x.RatsOutputType && String.IsNullOrWhiteSpace(x.RatsOutputType) then logger.LogError("RatsOutputType: Output Report Path should be defined for xml reporting")
            )

    /// Verify Rats is found on Path
    member x.verifyRatsExecProperties(logger  : TaskLoggingHelper) =
        lazy(
            if String.IsNullOrWhiteSpace(x.RatsPath) then logger.LogError("RatsPath Cannot Be Empty, Remove to use from Path")
            elif System.IO.Path.IsPathRooted(x.RatsPath) then 
                if not(System.IO.File.Exists(x.RatsPath)) then logger.LogError(sprintf "RatsPath: %s Cannot Be Found on System, Set Path Correctly" x.RatsPath)
            elif not(Utils().ExistsOnPath(x.RatsPath)) then logger.LogError(sprintf "RatsPath: %s Cannot Be Found on PATH, Set PATH variable Correctly" x.RatsPath)
            )

    member x.generateCommandLineArgs(fileToAnalyse : string)=
        let builder = new CommandLineBuilder()

        builder.AppendSwitch("--xml")

        // options
        if not(String.IsNullOrWhiteSpace(x.RatsOptions)) then
            let values = x.RatsOptions.Split(";".ToCharArray())
            for value in values do
                builder.AppendSwitch(value.Trim())

        builder.AppendSwitch(fileToAnalyse)

        builder.ToString()

    member x.ExecuteRats executor cmdLineArgs ouputFilePath =
        // set environment
        let env = Map.ofList [("RATS_INPUT", x.RatsPath)]

        let mutable tries = 3
        let mutable returncode = 1

        while tries > 0  && returncode > 0 do
            (executor :> ICommandExecutor).ResetData()
            returncode <- (executor :> ICommandExecutor).ExecuteCommand(x.RatsPath, cmdLineArgs, env)
            if not((executor :> ICommandExecutor).GetErrorCode = ReturnCode.Ok) || returncode > 0 then
                tries <- tries - 1

        if tries = 0 then
            this.buildok <- false
            if this.BuildEngine = null then
                Console.WriteLine("Rats: Number of tries exceeded")
                (executor :> ICommandExecutor).GetStdError |> fun s -> for i in s do Console.WriteLine(i)
            else
                logger.LogError("Rats: Number of tries exceeded")
                (executor :> ICommandExecutor).GetStdError |> fun s -> for i in s do logger.LogError(i)

            (executor :> ICommandExecutor).GetStdOut |> fun s -> for i in s do  if this.BuildEngine = null then Console.WriteLine(i) else logger.LogError(i)
            false
        else
            if not(x.RatsOutputType = "vs7") then
                let parentdir = Directory.GetParent(ouputFilePath).ToString()
                if File.Exists(ouputFilePath) then File.Delete(ouputFilePath)
                if not(Directory.Exists(parentdir)) then Directory.CreateDirectory(parentdir) |> ignore

                let addLine (line:string) =
                    use wr = new StreamWriter(ouputFilePath, true)
                    wr.WriteLine(line)
                    this.totalViolations <- this.totalViolations + 1

                (executor :> ICommandExecutor).GetStdOut |> Seq.iter (fun x -> addLine(x))
            else
                let lines = (executor :> ICommandExecutor).GetStdOut
                if lines <> List.Empty then
                    let datastr = lines |> List.reduce (+)

                    try
                        let errorsFile = RatsError.Parse(datastr)
                        for vulnerability in errorsFile.GetVulnerabilities() do
                            for line in vulnerability.File.GetLines() do
                                if this.BuildEngine = null then
                                    let data = sprintf "%s : %i : %s : %s" vulnerability.File.Name line vulnerability.Severity vulnerability.Message
                                    Console.WriteLine(data);
                                else
                                    logger.LogWarning("", vulnerability.Severity, "", vulnerability.File.Name, line, 0, 0, 0, vulnerability.Message)
                    with
                    | ex -> printfn "Exception! %s " (ex.Message)
            true

    override x.Execute() =

        this.verifyRatsExecProperties(logger).Force()
        this.VerifyOutput(logger).Force()

        let mutable result = not(logger.HasLoggedErrors)
        if result then
            let stopWatchTotal = Stopwatch.StartNew()
            let solutionHelper = new VSSolutionUtils()
            let projectHelper = new VSProjectUtils()

            if Directory.Exists(x.RatsOutputPath) then
                for filename in Directory.GetFiles(x.RatsOutputPath, @"rats-result-*.xml") do
                    File.Delete(filename)
            else
                Directory.CreateDirectory(x.RatsOutputPath) |> ignore

            let iterateOverFiles (file : string) = 
                let index = sprintf "%i" this.counter 
                let xml_file = Path.Combine(x.RatsOutputPath, String.Concat(String.Concat("rats-result-", index),".xml"))

                let arguments = x.generateCommandLineArgs(file)
                if file.Contains(Directory.GetParent(x.SolutionPathToAnalyse).ToString()) then
                    logger.LogMessage(sprintf "RatsCopCommand: %s %s" x.RatsPath arguments)
                    x.ExecuteRats executor arguments xml_file |> ignore
                    this.counter <- this.counter + 1
                ()

            let iterateOverProjectFiles(projectFile : ProjectFiles) = 
                projectHelper.GetCppCompilationFiles(projectFile.path, "", x.PathReplacementStrings)  |> Seq.iter (fun x -> iterateOverFiles x)

            solutionHelper.GetProjectFilesFromSolutions(x.SolutionPathToAnalyse) |> Seq.iter (fun x -> iterateOverProjectFiles x)

            logger.LogMessage(sprintf "Total Violations: %u" this.totalViolations)
            logger.LogMessage(sprintf "Rats End: %u ms" stopWatchTotal.ElapsedMilliseconds)

        if result && this.buildok then
            true
        else
            false

    interface ICancelableTask with
        member this.Cancel() =
            (executor :> ICommandExecutor).CancelExecution |> ignore
            Environment.Exit(0)
            ()
