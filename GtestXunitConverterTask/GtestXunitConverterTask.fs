// Learn more about F# at http://fsharp.net

namespace MSBuild.Tekla.Tasks.GtestXunitConverter
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

type GtestXmlReport = XmlProvider<"""<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="43" failures="0" disabled="2" errors="0" timestamp="2013-06-29T09:23:30" time="0.348" name="AllTests">
  <testsuite name="MD_GetMac" tests="1" failures="0" disabled="0" errors="0" time="0">
    <testcase name="ReturnsMacAddress" status="notrun" time="0" classname="MD_GetMac" /> 
  </testsuite>
  <testsuite name="CM_toolFileSystemTest" tests="34" failures="0" disabled="0" errors="0" time="0.128">
    <testcase name="DISABLED_TT80570_TestgeoSolveExtremaSpatialRelation_5" status="notrun" time="0" classname="CM_geoExtremaTest" />
    <testcase name="DISABLED_TT80570_TestgeoSolveExtremaSpatialRelation_6" status="notrun" time="0" classname="CM_geoExtremaTest" />
    <testcase name="testWLowerCase" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testLowerCase" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGets_FilesWithSuffixFromSubFolder" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFilesFromSubFolderWhenMainDirIsCached" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemDoesNotReturnDuplicateFileNames" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemDoesNotCareAboutExcludeList" status="run" time="0.004" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGets_FilesWithSuffixFromSubFolderAfterAddingFiles" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsThreeFilesAfterClearingCache" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="DoesNotReturnFiles_FromNonExistentFolder" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="GetsFilesFromMainFolder_WithJustMainDirectory" status="run" time="0.006" classname="CM_toolFileSystemTest" />
    <testcase name="GetsFilesFromMainFolder_WhenSubFolderDoesNotExist" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="GetsFileFullPath_WhenSubFolderDoesNotExist" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFilesFromMainFolderIfSubFolderDoesNotExist_WithDifferentCase" status="run" time="0.004" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFilesByPrefixAndSuffix_DoesNotLoseCaseOfFilenameCharacters" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFilesByPrefixAndSuffix" status="run" time="0.009" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFilesByPrefix" status="run" time="0.012" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFilesByPrefixAndSuffix_WithDifferentCase" status="run" time="0.004" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFilesByPrefixInDifferentCase" status="run" time="0.006" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFileFullPath" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFileFullPath_WithExactNameOnly" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFileFullPathFromSubFolder" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFileFullPath_NonExistingSubFolder" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFileFullPath_FileNotInCache" status="run" time="0.004" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFileFullPath_FileInCacheWasRemoved" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testFileSystemGetsFileFullPath_FileWithSameNameWasCreatedInHigherPriorityFolder" status="run" time="0.004" classname="CM_toolFileSystemTest" />
    <testcase name="testClearDirectoryCache_AllowsNewlyCreatedFilesToBeFetched" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testClearDirectoryCache_CanEmptyDirectory" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testFileExists_FileExists" status="run" time="0.003" classname="CM_toolFileSystemTest" />
    <testcase name="testFileExists_FileDoesNotExist" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testInsertInCache_FilesExistInFolder" status="run" time="0.009" classname="CM_toolFileSystemTest" />
    <testcase name="testInsertInCache_EmptyFolder" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="testInsertInCache_ExistingFile" status="run" time="0.008" classname="CM_toolFileSystemTest" />
    <testcase name="testInsertInCache_FileDoesNotExist" status="run" time="0.002" classname="CM_toolFileSystemTest" />
    <testcase name="GetFileFullPath_ShouldNotReturnNullDString_c" status="run" time="0.003" classname="CM_toolFileSystemTest" />
  </testsuite>
  <testsuite name="CM_toolFileSystemJapaneseTest" tests="6" failures="0" disabled="0" errors="0" time="0.021">
    <testcase name="testFileSystemGets_FilesWithSuffixFromSubFolder" status="run" time="0.004" classname="CM_toolFileSystemJapaneseTest" />
    <testcase name="testFileSystem_GetFullPathTo" status="run" time="0.004" classname="CM_toolFileSystemJapaneseTest" />
    <testcase name="testTransform" status="run" time="0.003" classname="CM_toolFileSystemJapaneseTest" />
    <testcase name="testFileSystemGetsAllFilesFromAllFolders" status="run" time="0.004" classname="CM_toolFileSystemJapaneseTest" />
    <testcase name="testFileSystemGetsAllFilesFromSubFolder" status="run" time="0.003" classname="CM_toolFileSystemJapaneseTest" />
    <testcase name="testFileSystemGetsAllFilesFromSubFolder_FailsWithNonJapaneseLocale" status="run" time="0.003" classname="CM_toolFileSystemJapaneseTest" />
  </testsuite>
  <testsuite name="CM_GetMac" tests="1" failures="0" disabled="0" errors="0" time="0.001">
    <testcase name="ReturnsMacAddress" status="run" time="0.001" classname="CM_GetMac" />
  </testsuite>
  </testsuites>""">

type TestCase(name:string, status:string, time:string, classname:string) = 
    member val name = name
    member val status = status
    member val time = time
    member val classname = classname

type TestSuite(suitename:string, tests:string, disabled:string, errors:string, time:string) =
    member val suitename = suitename
    member val tests = tests
    member val disabled = disabled
    member val errors = errors
    member val time = time

    member val testcases : TestCase list = [] with get, set

    member this.AddTestCase(case : TestCase) =
        this.testcases <- this.testcases @ [case]

type GtestXunitConverterTask() as this =
    inherit Task()
    let logger : TaskLoggingHelper = new TaskLoggingHelper(this)

    member val buildok : bool = true with get, set
    member val counterFile : int = 0 with get, set
    member val ToOutputData : string list = [] with get, set
    member val testFiles : string list = [] with get, set

    /// Solution Path, Required
    [<Required>]
    member val SolutionPathToAnalyse = "" with get, set

    /// Output Path, Required
    [<Required>]
    member val GtestXunitConverterOutputPath = "" with get, set

    /// Input report file Path, Required
    [<Required>]
    member val GtestXMLReportFile = "" with get, set

    /// path for GtestXunitConverter executable, default expects GtestXunitConverter in path
    member val TestSuffix = "" with get, set

    /// path replacement strings
    member val PathReplacementStrings = "" with get, set

/// path replacement strings
    member val SkipSearchForFileLocation = false with get, set

    member x.ParseXunitReport filePath = 
        let xunitReport = GtestXmlReport.Parse(File.ReadAllText(filePath))

        let mutable xmloutputcontent = ""

        let addLine (line:string, ouputFilePath) =                  
            use wr = new StreamWriter(ouputFilePath, true)
            wr.WriteLine(line)

        let getFileNameFromListOfFile className =
            let mutable filefound = false
            let mutable fileout = ""
            for file in this.testFiles do
                if not(filefound) then
                    if File.Exists(file) then
                        let lines = File.ReadAllText(file)
                        let lineswithoutspaces = lines.Replace(" ", "")
                        if lineswithoutspaces.Contains("TEST_F(" + className) || lineswithoutspaces.Contains("TEST(" + className) then
                            filefound <- true
                            fileout <- file

            fileout
                    
        for testSuite in xunitReport.GetTestsuites() do
            let mutable skipCase = true
            for testcase in testSuite.GetTestcases() do
                if testcase.Status.Equals("run") then
                    skipCase <- false                

            if not(skipCase) then
                let xml_file = Path.Combine(x.GtestXunitConverterOutputPath, String.Concat(String.Concat("xunit-result-", this.counterFile),".xml"))
                this.counterFile <- this.counterFile + 1
                addLine("""<?xml version="1.0" encoding="UTF-8"?>""", xml_file)
                let mutable fileName = ""
                if not(x.SkipSearchForFileLocation) then
                    fileName <- getFileNameFromListOfFile(testSuite.Name)                   

                let suitestr = sprintf """<testsuite name="%s" tests="%i" failures="%i" disabled="%i" errors="%i" time="%f" filename="%s">""" testSuite.Name testSuite.Tests testSuite.Failures testSuite.Disabled testSuite.Errors testSuite.Time fileName
                addLine(suitestr, xml_file)
                for testcase in testSuite.GetTestcases() do                          
                    let casestr = sprintf """   <testcase name="%s" status="%s" time="%f" classname="%s" />""" testcase.Name testcase.Status testcase.Time testcase.Classname
                    addLine(casestr, xml_file)
                addLine("""</testsuite>""", xml_file)                 
        ()

    override x.Execute() =

        let mutable result = not(logger.HasLoggedErrors)
        if result then
            let stopWatchTotal = Stopwatch.StartNew()
            let solutionHelper = new VSSolutionUtils()
            let projectHelper = new VSProjectUtils()

            if Directory.Exists(x.GtestXunitConverterOutputPath) then
                for filename in Directory.GetFiles(x.GtestXunitConverterOutputPath, @"xunit-result-*.xml", SearchOption.AllDirectories) do
                    File.Delete(filename)
            else
                Directory.CreateDirectory(x.GtestXunitConverterOutputPath) |> ignore

            let iterateOverProjectFiles(projectFile : ProjectFiles) = 
                this.testFiles <- this.testFiles @ projectHelper.GetCppCompilationFiles(projectFile.path, x.TestSuffix, x.PathReplacementStrings)

            solutionHelper.GetProjectFilesFromSolutions(x.SolutionPathToAnalyse) |> Seq.iter (fun x -> iterateOverProjectFiles x)

            for repfile in Directory.GetFiles(Directory.GetParent(this.GtestXMLReportFile).ToString(), Path.GetFileName(this.GtestXMLReportFile)) do
                this.ParseXunitReport repfile

            if this.BuildEngine = null then
                System.Console.WriteLine(sprintf "GtestXunitConverter End: %u ms" stopWatchTotal.ElapsedMilliseconds)
            else
                logger.LogMessage(sprintf "GtestXunitConverter End: %u ms" stopWatchTotal.ElapsedMilliseconds)

        if result && this.buildok then
            true
        else
            false

    interface ICancelableTask with
        member this.Cancel() =
            Environment.Exit(0)
            ()
