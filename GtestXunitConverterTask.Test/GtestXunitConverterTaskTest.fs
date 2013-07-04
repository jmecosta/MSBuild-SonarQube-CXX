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
open MSBuild.Tekla.Tasks.GtestXunitConverter
open Foq

type GtestXuniConverterPathTest() =

    [<TearDown>]
    member test.TearDown() =
        for filename in Directory.GetFiles(".", @"xunit-result-*.xml", SearchOption.AllDirectories) do
            File.Delete(filename)

    [<Test>]
    member test.``Should Parse Correctly Xunit Report`` () = 
        let task = GtestXunitConverterTask()
        task.SolutionPathToAnalyse <- "./testdata/solutionsfile.sln"
        task.TestSuffix <- "_test.cpp;_tests.cpp"
        task.GtestXMLReportFile <- "./testdata/xunit-report.xml"
        task.GtestXunitConverterOutputPath <- "./testdata/"
        task.Execute() |> should be True
       
        let files = Directory.GetFiles("./testdata/", "xunit-result-*.xml")
        files.Length |> should equal 2
        File.ReadAllText(files.[0]).Contains("file1_test.cpp") |> should be True
        File.ReadAllText(files.[1]).Contains("file2_tests.cpp") |> should be True

    [<Test>]
    member test.``Should Parse Correctly Xunit Report with *.xml`` () = 
        let task = GtestXunitConverterTask()
        task.SolutionPathToAnalyse <- "./testdata/solutionsfile.sln"
        task.TestSuffix <- "_test.cpp;_tests.cpp"
        task.GtestXMLReportFile <- "./testdata/*.xml"
        task.GtestXunitConverterOutputPath <- "./testdata/"
        task.Execute() |> should be True
       
        let files = Directory.GetFiles("./testdata/", "xunit-result-*.xml")
        files.Length |> should equal 2
        File.ReadAllText(files.[0]).Contains("file1_test.cpp") |> should be True
        File.ReadAllText(files.[1]).Contains("file2_tests.cpp") |> should be True


