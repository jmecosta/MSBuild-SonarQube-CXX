namespace NunitRunnerTask.Test


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
open MSBuild.Tekla.Tasks.NunitRunnerTask
open Foq
open System

type NunitRunnerTaskTest() =

    [<TearDown>]
    member test.TearDown() =
        for filename in Directory.GetFiles(".", @"xunit-result-*.xml", SearchOption.AllDirectories) do
            File.Delete(filename)

    [<Test>]
    member test.``If no assemblies to test than empty string`` () = 
        let task = NunitRunnerTask()
        let data = task.generateCommandLineArgsForGallioIcarus
        data |> should equal ""

    [<Test>]
    member test.``If relative path one than should be the ok`` () = 
        let task = NunitRunnerTask()
        task.AssembliesToTest <- "testdata\\file1_test.cpp"
        let data = task.generateCommandLineArgsForGallio       
        data |> should equal ("/r:IsolatedProcess /report-directory: /report-name-format:gallio-report /report-type:Xml " + Environment.CurrentDirectory + "\\testdata\\file1_test.cpp ")

    [<Test>]
    member test.``If relative path two than should be the ok`` () = 
        let task = NunitRunnerTask()
        task.AssembliesToTest <- "testdata\\file*_test.cpp"
        let data = task.generateCommandLineArgsForGallio       
        data |> should equal ("/r:IsolatedProcess /report-directory: /report-name-format:gallio-report /report-type:Xml " + Environment.CurrentDirectory + "\\testdata\\file1_test.cpp " + Environment.CurrentDirectory + "\\testdata\\file3_test.cpp ")

    [<Test>]
    member test.``If one absolute path than should be the ok`` () = 
        let task = NunitRunnerTask()
        task.AssembliesToTest <- Environment.CurrentDirectory + "\\testdata\\file1_test.cpp"
        let data = task.generateCommandLineArgsForGallio       
        data |> should equal ("/r:IsolatedProcess /report-directory: /report-name-format:gallio-report /report-type:Xml " + Environment.CurrentDirectory + "\\testdata\\file1_test.cpp ")

    [<Test>]
    member test.``If two absolute path than should be the ok`` () = 
        let task = NunitRunnerTask()
        task.AssembliesToTest <- Environment.CurrentDirectory + "\\testdata\\file1_test.cpp" + ";" + Environment.CurrentDirectory + "\\testdata\\file2_tests.cpp"
        let data = task.generateCommandLineArgsForGallio       
        data |> should equal ("/r:IsolatedProcess /report-directory: /report-name-format:gallio-report /report-type:Xml " + Environment.CurrentDirectory + "\\testdata\\file1_test.cpp " + Environment.CurrentDirectory + "\\testdata\\file2_tests.cpp ")

    [<Test>]
    member test.``If search multiple absolute path than should be the ok`` () = 
        let task = NunitRunnerTask()
        task.AssembliesToTest <- Environment.CurrentDirectory + "\\testdata\\file*_test*.cpp"
        let data = task.generateCommandLineArgsForGallio       
        data |> should equal ("/r:IsolatedProcess /report-directory: /report-name-format:gallio-report /report-type:Xml " + Environment.CurrentDirectory + "\\testdata\\file1_test.cpp " + Environment.CurrentDirectory + "\\testdata\\file2_tests.cpp "  + Environment.CurrentDirectory + "\\testdata\\file3_test.cpp ")

    [<Test>]
    member test.``If relative path multiple than should be the ok`` () = 
        let task = NunitRunnerTask()
        task.AssembliesToTest <- "testdata\\file*_test*.cpp"
        let data = task.generateCommandLineArgsForGallio       
        data |> should equal ("/r:IsolatedProcess /report-directory: /report-name-format:gallio-report /report-type:Xml " + Environment.CurrentDirectory + "\\testdata\\file1_test.cpp " + Environment.CurrentDirectory + "\\testdata\\file2_tests.cpp "  + Environment.CurrentDirectory + "\\testdata\\file3_test.cpp ")