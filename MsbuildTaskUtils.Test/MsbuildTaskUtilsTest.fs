namespace MsbuildTaskUtils.Test

open NUnit.Framework
open FsUnit
open MSBuild.Tekla.Tasks.MsbuildTaskUtils

type MsbuildTaskTest() = 

    [<Test>]
    member test.``Ask Non Existent Program Should Return False`` () = 
        let utils = new Utils()
        utils.ExistsOnPath("Askxkjjhazjsdjsz.exe") |> should be False

    [<Test>]
    member test.``Notepad should exist on path`` () = 
        Utils().ExistsOnPath("Notepad.exe") |> should be True


