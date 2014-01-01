namespace MSBuild.Tekla.Tasks.Executor

open System
open System.IO
open System.Threading
open System.Diagnostics
open Microsoft.FSharp.Collections
open Microsoft.Build.Utilities

type ReturnCode =
   | Ok = 0
   | Timeout = 1
   | NokAppSpecific = 2

type ICommandExecutor = 
  abstract member GetStdOut : list<string>
  abstract member GetStdError : list<string>
  abstract member GetErrorCode : ReturnCode
  abstract member CancelExecution : ReturnCode
  abstract member ResetData : unit -> unit

  // no redirection of output
  abstract member ExecuteCommand : string * string * Map<string, string> -> int

  // with redirection of output
  abstract member ExecuteCommand : string * string * Map<string, string> * (DataReceivedEventArgs -> unit) * (DataReceivedEventArgs -> unit) * string -> int

type CommandExecutor(logger : TaskLoggingHelper, timeout : int64) =
    let addEnvironmentVariable (startInfo:ProcessStartInfo) a b = startInfo.EnvironmentVariables.Add(a, b)
    member val Logger = logger
    member val stopWatch = Stopwatch.StartNew()
    member val proc : Process  = new Process() with get, set
    member val output : string list = [] with get, set
    member val error : string list = [] with get, set
    member val returncode : ReturnCode = ReturnCode.Ok with get, set
    member val cancelSignal : bool = false with get, set

    member this.TimerControl() =
        async {
          while not this.cancelSignal do
            if this.stopWatch.ElapsedMilliseconds > timeout then

                if not(obj.ReferenceEquals(logger, null)) then
                    logger.LogError(sprintf "Expired Timer: %x " this.stopWatch.ElapsedMilliseconds)

                try
                    this.proc.Kill()
                with
                | ex -> ()
                this.returncode <- ReturnCode.Timeout
            Thread.Sleep(1000)
        }

    member this.ProcessErrorDataReceived(e : DataReceivedEventArgs) =
        this.stopWatch.Restart()
        if not(String.IsNullOrWhiteSpace(e.Data)) then
            this.error <- this.error @ [e.Data]
        ()

    member this.ProcessOutputDataReceived(e : DataReceivedEventArgs) =
        this.stopWatch.Restart()
        if not(String.IsNullOrWhiteSpace(e.Data)) then
            this.output <- this.output @ [e.Data]
        ()

    interface ICommandExecutor with
        member this.GetStdOut =
            this.output

        member this.GetStdError =
            this.error

        member this.GetErrorCode =
            this.returncode

        member this.CancelExecution =
            if this.proc.HasExited = false then
                try
                    this.proc.Kill()
                with
                | ex -> ()

            this.cancelSignal <- true
            ReturnCode.Ok

        member this.ResetData() =
            this.error <- []
            this.output <- []
            ()

        member this.ExecuteCommand(program, args, env) =
            let startInfo = ProcessStartInfo(FileName = program,
                                             Arguments = args,
                                             WindowStyle = ProcessWindowStyle.Normal,
                                             UseShellExecute = false,
                                             RedirectStandardOutput = true,
                                             RedirectStandardError = true,
                                             RedirectStandardInput = true,
                                             CreateNoWindow = true,
                                             WorkingDirectory = Path.GetDirectoryName(program))
            env |> Map.iter (addEnvironmentVariable startInfo)

            this.proc <- new Process(StartInfo = startInfo)
            this.proc.ErrorDataReceived.Add(this.ProcessErrorDataReceived)
            this.proc.OutputDataReceived.Add(this.ProcessOutputDataReceived)

            this.proc.EnableRaisingEvents <- true
            let ret = this.proc.Start()

            this.stopWatch.Restart()
            Async.Start(this.TimerControl());

            this.proc.BeginOutputReadLine()
            this.proc.BeginErrorReadLine()
            this.proc.WaitForExit()
            this.cancelSignal <- true
            this.proc.ExitCode


        member this.ExecuteCommand(program, args, env, outputHandler, errorHandler, workingDir) =               
            let startInfo = ProcessStartInfo(FileName = program,
                                             Arguments = args,
                                             WindowStyle = ProcessWindowStyle.Normal,
                                             UseShellExecute = false,
                                             RedirectStandardOutput = true,
                                             RedirectStandardError = true,
                                             RedirectStandardInput = true,
                                             CreateNoWindow = true,
                                             WorkingDirectory = workingDir)
            env |> Map.iter (addEnvironmentVariable startInfo)

            this.proc <- new Process(StartInfo = startInfo,
                                     EnableRaisingEvents = true)
            this.proc.OutputDataReceived.Add(outputHandler)
            this.proc.ErrorDataReceived.Add(errorHandler)
            let ret = this.proc.Start()

            this.stopWatch.Restart()
            Async.Start(this.TimerControl());

            this.proc.BeginOutputReadLine()
            this.proc.BeginErrorReadLine()
            this.proc.WaitForExit()
            this.cancelSignal <- true
            this.proc.ExitCode
