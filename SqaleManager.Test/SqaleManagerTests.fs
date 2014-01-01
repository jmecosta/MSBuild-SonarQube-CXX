namespace SqaleManager.Test

open NUnit.Framework
open FsUnit
open SqaleManager
open Foq
open FSharp.Data
open System.Xml.Linq
open System.IO

type RootConfigurationPropsChecksTests() = 
    let rulesinFile = "fxcop-profile.xml"

    [<SetUp>]
    member test.``SetUp`` () = 
        if File.Exists(rulesinFile) then
            File.Delete(rulesinFile)

    [<TearDown>]
    member test.``tearDown`` () = 
        if File.Exists(rulesinFile) then
            File.Delete(rulesinFile)
            
    [<Test>]
    member test.``It Creates a Default Model`` () = 
        let manager = new SqaleManager()
        let def = manager.GetDefaultSqaleModel()
        def.GetCharacteristics().Length |> should equal 8
        def.GetProfile().Rules.Length |> should equal 0

    [<Test>]
    member test.``Should Load Profile into Model With New Format`` () = 
        let manager = new SqaleManager()
        let def = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("intel", def, "samples/intel-profile.xml")
        def.GetProfile().Rules.Length |> should equal 22
        def.GetProfile().Rules.[2].key |> should equal "intelXe.CrossThreadStackAccess"
        def.GetProfile().Rules.[2].name |> should equal "Cross-thread Stack Access"
        def.GetProfile().Rules.[2].repo |> should equal "intel"
        def.GetProfile().Rules.[2].category |> should equal "Reliability"
        def.GetProfile().Rules.[2].configKey |> should equal "intelXe.CrossThreadStackAccess@INTEL"
        def.GetProfile().Rules.[2].description |> should equal "Occurs when a thread accesses a different thread's stack."

    [<Test>]
    member test.``Should Load Profile into Model With Old Format`` () = 
        let manager = new SqaleManager()
        let def = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("cppcheck", def, "samples/cppcheck.xml")
        def.GetProfile().Rules.Length |> should equal 305

    [<Test>]
    member test.``Should Load Model From CSharp Xml File`` () = 
        let manager = new SqaleManager()
        let model = manager.ParseSqaleModelFromXmlFile("samples/CSharpSqaleModel.xml")
        model.GetCharacteristics().Length |> should equal 8
        model.GetProfile().Rules.Length |> should equal 617
        model.GetProfile().Rules.[0].category |> should equal "PORTABILITY"
        model.GetProfile().Rules.[0].subcategory |> should equal "COMPILER_RELATED_PORTABILITY"
        model.GetProfile().Rules.[0].repo |> should equal "common-c++"
        model.GetProfile().Rules.[0].key |> should equal "InsufficientBranchCoverage"
        model.GetProfile().Rules.[0].remediationFunction |> should equal "linear"
        model.GetProfile().Rules.[0].remediationFactorTxt |> should equal "d"
        model.GetProfile().Rules.[0].remediationFactorVal |> should equal "0.375"


    [<Test>]
    member test.``Should Get Correct Number Of Repositories From Model`` () = 
        let manager = new SqaleManager()
        let model = manager.ParseSqaleModelFromXmlFile("samples/CSharpSqaleModel.xml")
        manager.GetRepositoriesInModel(model).Length |> should equal 6
    
    [<Test>]
    member test.``Should Create Xml Profile`` () = 
        let manager = new SqaleManager()
        let model = manager.ParseSqaleModelFromXmlFile("samples/CSharpSqaleModel.xml")

        manager.WriteProfileToFile(model, "fxcop", rulesinFile)
        let managerNew = new SqaleManager()
        let newModel = managerNew.GetDefaultSqaleModel()
        managerNew.AddAProfileFromFileToSqaleModel("fxcop", newModel, rulesinFile)
        newModel.GetProfile().Rules.Length |> should equal 240
        newModel.GetProfile().Rules.[2].key |> should equal "EnumStorageShouldBeInt32"
        newModel.GetProfile().Rules.[2].name |> should equal ""
        newModel.GetProfile().Rules.[2].repo |> should equal "fxcop"
        newModel.GetProfile().Rules.[2].category |> should equal "PORTABILITY"
        newModel.GetProfile().Rules.[2].configKey |> should equal "EnumStorageShouldBeInt32@fxcop"
        newModel.GetProfile().Rules.[2].description |> should equal ""

    [<Test>]
    member test.``Should Create Write A Sqale Model To Xml Correctly And Read It`` () = 
        let manager = new SqaleManager()
        let def = manager.GetDefaultSqaleModel()

        let rule = new Rule()
        rule.key <- "RuleKey"
        rule.name <- "Rule Name"
        rule.configKey <- "Rule Name@Example"
        rule.description <- "this is description"
        rule.category <- "MAINTAINABILITY"
        rule.subcategory <- "READABILITY"
        rule.remediationFactorVal <- "10.0"
        rule.remediationFactorTxt <- "mn"
        rule.remediationFunction <- "constant_resource"
        rule.severity <- "MINOR"
        rule.repo <- "Example"
        
        def.CreateRuleInProfile(rule) |> ignore
        manager.WriteSqaleModelToFile(def, rulesinFile)

        let model = manager.ParseSqaleModelFromXmlFile(rulesinFile)
        model.GetProfile().Rules.Length |> should equal 1
        model.GetProfile().Rules.[0].key |> should equal "RuleKey"
        model.GetProfile().Rules.[0].name |> should equal ""
        model.GetProfile().Rules.[0].configKey |> should equal "RuleKey@Example"
        model.GetProfile().Rules.[0].description |> should equal ""
        model.GetProfile().Rules.[0].category |> should equal "MAINTAINABILITY"
        model.GetProfile().Rules.[0].subcategory |> should equal "READABILITY"
        model.GetProfile().Rules.[0].remediationFactorVal |> should equal "10.0"
        model.GetProfile().Rules.[0].remediationFactorTxt |> should equal "mn"
        model.GetProfile().Rules.[0].remediationFunction |> should equal "constant_resource"
        model.GetProfile().Rules.[0].severity |> should equal ""
        model.GetProfile().Rules.[0].repo |> should equal "Example"

    [<Test>]
    member test.``Should Serialize the model Correctly And Read It`` () = 
        let manager = new SqaleManager()
        let def = manager.GetDefaultSqaleModel()

        let rule = new Rule()
        rule.key <- "RuleKey"
        rule.name <- "Rule Name"
        rule.configKey <- "Rule Name@Example"
        rule.description <- "this is description"
        rule.category <- "Maintainability"
        rule.subcategory <- "Readability"
        rule.remediationFactorVal <- "10.0"
        rule.remediationFactorTxt <- "mn"
        rule.remediationFunction <- "constant_resource"
        rule.severity <- "MINOR"
        rule.repo <- "Example"
        
        def.CreateRuleInProfile(rule) |> ignore
        manager.SaveSqaleModelToDsk(def, rulesinFile) |> ignore

        let model = manager.LoadSqaleModelFromDsk(rulesinFile)
        model.GetProfile().Rules.Length |> should equal 1
        model.GetProfile().Rules.[0].key |> should equal "RuleKey"
        model.GetProfile().Rules.[0].name |> should equal "Rule Name"
        model.GetProfile().Rules.[0].configKey |> should equal "Rule Name@Example"
        model.GetProfile().Rules.[0].description |> should equal "this is description"
        model.GetProfile().Rules.[0].category |> should equal "Maintainability"
        model.GetProfile().Rules.[0].subcategory |> should equal "Readability"
        model.GetProfile().Rules.[0].remediationFactorVal |> should equal "10.0"
        model.GetProfile().Rules.[0].remediationFactorTxt |> should equal "mn"
        model.GetProfile().Rules.[0].remediationFunction |> should equal "constant_resource"
        model.GetProfile().Rules.[0].severity |> should equal "MINOR"
        model.GetProfile().Rules.[0].repo |> should equal "Example"

    [<Test>]
    member test.``Read A ProfileDefinition`` () = 
        let manager = new SqaleManager()
        let model = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("cppcheck", model, "samples/cppcheck.xml")
        manager.CombineWithDefaultProfileDefinition(model, "samples/default-profile.xml")

        model.GetProfile().Rules.Length |> should equal 305
        model.GetProfile().Rules.[0].severity |> should equal "MINOR"
        

    [<Test>]
    member test.``Should Save Model As XML`` () = 
        let manager = new SqaleManager()
        let model = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("cppcheck", model, "samples/cppcheck.xml")
        manager.AddAProfileFromFileToSqaleModel("pclint", model, "samples/pclint.xml")
        manager.AddAProfileFromFileToSqaleModel("rats", model, "samples/rats.xml")
        manager.AddAProfileFromFileToSqaleModel("vera++", model, "samples/vera++.xml")
        manager.AddAProfileFromFileToSqaleModel("valgrind", model, "samples/valgrind.xml")
        manager.AddAProfileFromFileToSqaleModel("compiler", model, "samples/compiler.xml")
        manager.CombineWithDefaultProfileDefinition(model, "samples/default-profile.xml")

        manager.SaveSqaleModelToDskAsXml(model, "cxx-model-project.xml")
        manager.WriteSqaleModelToFile(model, "cxx-model.xml")

    //[<Test>]
    member test.``Read Cxx Project`` () = 
        let manager = new SqaleManager()
        let model = manager.ReadSqaleModelToDskAsXml("cxx-model-project.xml")
        model.GetCharacteristics().Length |> should equal 8
        manager.WriteSqaleModelToFile(model, "cxx-model.xml")

    //[<Test>]
    member test.``Get C++ Profile`` () = 
        let manager = new SqaleManager()
        let model = manager.GetDefaultSqaleModel()
        manager.AddProfileDefinitionFromServerToModel(model, "c++", "DefaultTeklaC++", new ExtensionTypes.ConnectionConfiguration("http://sonar", "jocs1", "jocs1"))
        manager.SaveSqaleModelToDskAsXml(model, "cxx-model-project-updated.xml")
        ()

    //[<Test>]
    member test.``Read A Project and Merge Info From Another Project`` () = 
        let manager = new SqaleManager()
        let model = manager.ReadSqaleModelToDskAsXml("cxx-model-project.xml")
        let modelToMerge = manager.ReadSqaleModelToDskAsXml("cppcheck-model-project.xml")
        manager.MergeSqaleDataModels(model, modelToMerge)
        manager.WriteSqaleModelToFile(model, "cxx-model-combined.xml")
        manager.SaveSqaleModelToDskAsXml(model, "cxx-model-project-combined.xml")
        ()




