﻿module CppCheckTaskTest

open System.IO
open Microsoft.Build
open Microsoft.Build.Framework
open Microsoft.Build.Logging
open Microsoft.Build.Utilities

open NUnit.Framework
open FsUnit
open Foq

open MSBuild.Tekla.Tasks.Executor
open MSBuild.Tekla.Tasks.CppCheck

type CppCheckTaskTest() = 
    let tempFile = Path.Combine(Directory.GetParent(Path.GetTempFileName()).ToString(), "MSBuildTaskTests")
    let mockLogger = Mock<TaskLoggingHelper>().Create()

    [<TearDown>]
    member test.TearDown() =
        if File.Exists(tempFile) then
            File.Delete(tempFile)

    [<Test>]
    member test.``Run return 0 when run ok with vs with no lines`` () = 
        let task = CppCheckTask()
        task.CppCheckOutputPath <- tempFile
        task.CppCheckOutputType <- "vs7"
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetStdError @>).Returns([])
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Create()

        task.ExecuteCppCheck mockExecutor "foo bar" |> should be True

    [<Test>]
    member test.``Run return 0 when run ok with vs`` () = 
        let task = CppCheckTask()
        task.CppCheckOutputPath <- tempFile
        task.CppCheckOutputType <- "vs7"
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetStdError @>).Returns(["<?xml version=\"1.0\" encoding=\"UTF-8\"?>"; "<results>"; """<error file="E:\TSSRC\Core\Common\common\Common.cpp" line="4" id="missingInclude" severity="style" msg="Include file: &quot;cancel_proto.h&quot; not found."/>""";"</results>"])
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Create()

        task.ExecuteCppCheck mockExecutor "foo bar" |> should be True


    [<Test>]
    member test.``Run return bigger than 0 when failed`` () = 
        let task = new CppCheckTask()
        task.CppCheckOutputType <- "xml-version-1"
        task.CppCheckOutputPath <- tempFile
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(1)
                .Setup(fun x -> <@ x.GetStdOut @>).Returns([])
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.NokAppSpecific)
                .Create()

        task.ExecuteCppCheck mockExecutor "foo bar" |> should be False

    [<Test>]
    member test.``Runs Ok with Xml data version 1`` () = 
        let task = CppCheckTask()
        task.CppCheckOutputType <- "xml-version-1"
        task.CppCheckOutputPath <- tempFile
        task.SolutionPathToAnalyse <- Directory.GetParent(tempFile).ToString()
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Setup(fun x -> <@ x.GetStdError @>).Returns(["<?xml version=\"1.0\" encoding=\"UTF-8\"?>"; "<results>"; "<error file=\"Analysis.cpp\" line=\"36\" id=\"unusedFunction\" severity=\"style\" msg=\"The function used\"/>"; "</results>"])
                .Create()

        task.ExecuteCppCheck mockExecutor "foo bar" |> should be True

    [<Test>]
    member test.``Runs Ok with Xml data version 2`` () = 
        let task = CppCheckTask()
        task.CppCheckOutputType <- "xml-version-2"
        task.CppCheckOutputPath <- tempFile
        task.SolutionPathToAnalyse <- Directory.GetParent(tempFile).ToString()
        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Setup(fun x -> <@ x.GetStdError @>).Returns(["<?xml version=\"1.0\" encoding=\"UTF-8\"?>"; "<results>"; "<error file=\"Analysis.cpp\" line=\"36\" id=\"unusedFunction\" severity=\"style\" msg=\"The function used\"/>"; "</results>"])
                .Create()

        task.ExecuteCppCheck mockExecutor "foo bar" |> should be True

    [<Test>]
    member test.``Run CppCheckCommand No violations Reported`` () = 
        let task = CppCheckTask()
        task.CppCheckDefines <- "CppCheck"
        task.CppCheckOutputPath <- tempFile

        let mockExecutor =
            Mock<ICommandExecutor>()
                .Setup(fun x -> <@ x.ExecuteCommand(any(), any(), any()) @>).Returns(0)
                .Setup(fun x -> <@ x.GetErrorCode @>).Returns(ReturnCode.Ok)
                .Setup(fun x -> <@ x.GetStdError @>).Returns(["<?xml version=\"1.0\" encoding=\"UTF-8\"?>"; "<results>"; "</results>"])
                .Create()

        task.ExecuteCppCheck mockExecutor "foo bar" |> should be True

    [<Test>]
    member test.``Non Existent CppCheck Should Report Error to log`` () = 
        let Task = new CppCheckTask()
        Task.CppCheckPath <- "Non_Exixtent_CppCheckPath.exe"
        (fun () -> Task.verifyCppCheckExecProperties(mockLogger).Force() |> ignore) |> should throw typeof<System.InvalidOperationException>
