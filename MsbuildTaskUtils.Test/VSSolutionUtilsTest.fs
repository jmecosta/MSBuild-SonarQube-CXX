namespace MsbuildTaskUtils.Test

open NUnit.Framework
open FsUnit
open MSBuild.Tekla.Tasks.MsbuildTaskUtils
open System.IO

type VSSolutionUtilsTest() = 

    [<Test>]
    member test.``Should Read the Corrent Number of source files in project`` () = 
        let vsproject = new VSProjectUtils()
        let projects = vsproject.GetCompilationFiles("testdata\\project1\\project1.vcxproj", "", "")

        projects.Length |> should equal 4
        let mutable expectedPath = Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "source1.cpp")
        projects.[0] |> should equal expectedPath
        expectedPath <- Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "source2.cpp")
        projects.[1] |> should equal expectedPath
        expectedPath <- Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "header1.hpp")
        projects.[2] |> should equal expectedPath
        expectedPath <- Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "header2.hpp")
        projects.[3] |> should equal expectedPath

    [<Test>]
    member test.``Should Read the Corrent Number of source files in project give a include search string`` () = 
        let vsproject = new VSProjectUtils()
        let projects = vsproject.GetCompilationFiles("testdata\\project1\\project1.vcxproj", "ce1.cpp;er1.hpp", "")

        projects.Length |> should equal 2
        let mutable expectedPath = Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "source1.cpp")
        projects.[0] |> should equal expectedPath
        expectedPath <- Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "header1.hpp")
        projects.[1] |> should equal expectedPath

    [<Test>]
    member test.``Should Read the Corrent Number of source files in project give a include search string without case sensitive`` () = 
        let vsproject = new VSProjectUtils()
        let projects = vsproject.GetCompilationFiles("testdata\\project1\\project1.vcxproj", "Ce1.cpp;eR1.hpp", "")

        projects.Length |> should equal 2
        let mutable expectedPath = Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "source1.cpp")
        projects.[0] |> should equal expectedPath
        expectedPath <- Path.Combine(Directory.GetParent("testdata\\project1\\project1.vcxproj").ToString(), "header1.hpp")
        projects.[1] |> should equal expectedPath

    [<Test>]
    member test.``Should Read the Correct Number of Projects From Solution`` () = 
        let vssolution = new VSSolutionUtils()
        let projects = vssolution.GetProjectFilesFromSolutions("testdata\\solutionsfile.sln")
        projects.Length |> should equal 1
        projects.[0].name |> should equal "project1"        
