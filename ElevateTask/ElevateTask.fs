// Learn more about F# at http://fsharp.net

namespace MSBuild.Tekla.Tasks.ElevateTask
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
open System.Reflection
open System.Security.Principal

type ElevateTask() as this =
    inherit Task()
    let logger : TaskLoggingHelper = new TaskLoggingHelper(this)
    let executor : CommandExecutor = new CommandExecutor(logger, int64(1500000))
    let ElevateExec = "Elevate.exe"
    let SchedulleRegister = "SchedulleTaskRegister.exe"
    let IsAdministrator() = 
        let identity = WindowsIdentity.GetCurrent()
        if (identity <> null) then
            let principal = new WindowsPrincipal(identity)
            principal.IsInRole(WindowsBuiltInRole.Administrator)
        else
            false

    /// path for Cppcheck executable, default expects cppcheck in path
    member val Command = "" with get, set
    member val WorkingDirectory = "" with get, set

    member val IgnoreExitCode = false with get, set

    member x.generateCommandLineArgs =
        let builder = new CommandLineBuilder()

        builder.AppendSwitch(x.Command)
        builder.ToString()

    member x.generateCommandLineArgsForAdmin =
        let builder = new CommandLineBuilder()

        let elements = x.Command.Split(' ')

        let mutable isFirst = true
        for token in elements do
            if isFirst then
                isFirst <- false
            else
                builder.AppendSwitch(token)

        builder.ToString()

    member x.RunElevate executor =

        let assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        let elevate = Path.Combine(assemblyFolder, ElevateExec)

        if File.Exists(elevate) && Directory.Exists(x.WorkingDirectory) then
            
            let mutable returncode = 0

            if not(IsAdministrator()) then
                logger.LogMessage("[" + x.WorkingDirectory + "]: " + elevate + " -wait " + x.generateCommandLineArgs)
                returncode <- (executor :> ICommandExecutor).ExecuteCommandWait(elevate, "-wait " + x.generateCommandLineArgs, Map.empty, x.WorkingDirectory)
                logger.LogMessage("CMD: " + x.generateCommandLineArgs + " Ret Code: " + (sprintf "%i" returncode))
            else
                logger.LogMessage("Run Command : " + x.generateCommandLineArgsForAdmin)
                returncode <- (executor :> ICommandExecutor).ExecuteCommandWait(x.Command.Split(' ').[0], x.generateCommandLineArgsForAdmin, Map.empty, x.WorkingDirectory)
                logger.LogMessage("CMD: " + x.Command.Split(' ').[0] + " " + x.generateCommandLineArgsForAdmin + " Ret Code: " + (sprintf "%i" returncode))

            if returncode <> 0 then
                if x.IgnoreExitCode then
                    logger.LogWarning("Failed Ret Code: " + (sprintf "%i" returncode))
                else
                    logger.LogError("Failed Ret Code: " + (sprintf "%i" returncode))

            returncode = 0
        else
            logger.LogError("Elevate Not Found In: " + assemblyFolder + " or WorkingDir is invalid: " + x.WorkingDirectory)
            false

    override x.Execute() =
        x.RunElevate executor

    interface ICancelableTask with
        member this.Cancel() =
            (executor :> ICommandExecutor).CancelExecution |> ignore
            ()
