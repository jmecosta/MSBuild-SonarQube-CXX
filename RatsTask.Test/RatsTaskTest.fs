namespace RatsTask.Test


#if INTERACTIVE
#r "Microsoft.Build.Framework.dll";;
#r "Microsoft.Build.Utilities.v4.0.dll";;
#endif
open System.IO
open NUnit.Framework
open FsUnit
open Microsoft.Build
open Microsoft.Build.Framework
open Microsoft.Build.Logging
open Microsoft.Build.Utilities
open MSBuild.Tekla.Tasks.Rats
open Foq
open MSBuild.Tekla.Tasks.Executor

type RatsTest() =
    let tempFile = "out.xml"
    let lines = List.ofArray(File.ReadAllLines("rats-report.txt"))
    let mockLogger = Mock<TaskLoggingHelper>().Create()

    [<TearDown>]
    member test.TearDown() =
        if File.Exists(tempFile) then
            File.Delete(tempFile)

    [<Test>]
    member test.``Run return 0 when run ok with vs with empty lines`` () = 
        let task = RatsTask()
        task.RatsOutputType <- "vs7"
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetStdOut @>).Returns([])
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Create()

        task.ExecuteRats mockExecutor "foo bar" "out.xml" |> should be True

    [<Test>]
    member test.``Run return 0 when run ok with vs with lines`` () = 
        let task = RatsTask()
        task.RatsOutputType <- "xml"
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetStdOut @>).Returns(lines)
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Create()

        task.ExecuteRats mockExecutor "foo bar" tempFile |> should be True
        File.Exists(tempFile) |> should be True
        File.ReadAllLines(tempFile).Length |> should equal 41
