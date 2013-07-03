namespace VeraTask.Test


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
open MSBuild.Tekla.Tasks.Vera
open Foq
open MSBuild.Tekla.Tasks.Executor


type VeraTest() =
    let tempFile = "out.xml"
    let mockLogger = Mock<TaskLoggingHelper>().Create()

    [<TearDown>]
    member test.TearDown() =
        if File.Exists(tempFile) then
            File.Delete(tempFile)

    [<Test>]
    member test.``Run return 0 when run ok with vs with empty lines`` () = 
        let task = VeraTask()
        task.VeraOutputType <- "vs7"
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetStdError @>).Returns([])
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Create()

        task.ExecuteVera mockExecutor "foo bar" "out.xml" |> should be True

    [<Test>]
    member test.``Run return 0 when run ok with xml`` () = 
        let data = ["E:\SRC\Project\file.cpp:18: (T002) reserved name used for macro (incorrect use of underscore)";
                "E:\SRC\Project\file.cpp:83: (T008) keyword 'if' not followed by a single space";
                "E:\SRC\Project\file.cpp:88: (T008) keyword 'if' not followed by a single space";
                "E:\SRC\Project\file.cpp:93: (T008) keyword 'if' not followed by a single space";
                "E:\SRC\Project\file.cpp:202: (L003) trailing empty line(s)";]
        let task = VeraTask()
        task.VeraOutputType <- "xml"
        task.SolutionPathToAnalyse <- "E:\\SRC\\Project\\test.sln"
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetStdError @>).Returns(data)
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Create()

        task.ExecuteVera mockExecutor "filepath" tempFile |> should be True
        File.Exists(tempFile) |> should be True
        File.ReadAllLines(tempFile).Length |> should equal 10
