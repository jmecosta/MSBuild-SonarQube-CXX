namespace SqaleManager

open System
open System.IO
open System.Reflection
open System.Runtime.Serialization.Formatters.Binary
open System.Security
open System.Web
open System.Xml
open System.Xml.Linq
open SonarRestService

type SqaleManager() = 
    let SqaleDefaultModelDefinitionPath = "defaultmodel.xml"

    let EncodeStringAsXml(str : string) = SecurityElement.Escape(str).Replace("‘", "&#8216;").Replace("’", "&#8217;").Replace("–", "&#8211;").Replace("—", "&#8212;").Replace("„", "&#8222;").Replace("‟", "&#8223;")

    member x.GetDefaultSqaleModel() = 
        let assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
        let asm = Assembly.GetExecutingAssembly()
        use stream = asm.GetManifestResourceStream("defaultmodel.xml")
        let xmldoc = XDocument.Load(stream).ToString()
        let model = new SqaleModel()
        model.LoadSqaleModelFromString(xmldoc)
        model

    member x.ParseSqaleModelFromXmlFile(file : string) =
        let model = new SqaleModel()
        model.LoadSqaleModelFromFile(file)
        model

    member x.GetRepositoriesInModel(model : SqaleModel) =
        let mutable repos : string list = []

        for rule in model.GetProfile().Rules do
            try
                Array.find (fun elem -> elem.Equals(rule.repo)) (List.toArray repos) |> ignore
            with
            | :? System.Collections.Generic.KeyNotFoundException -> repos <- repos @ [rule.repo]

        repos

    member x.WriteProfileToFile(model : SqaleModel, repo : string, fileName : string) =
        let mutable rules : Rule list = []

        for ruleinprofile in model.GetProfile().Rules do
            if ruleinprofile.repo.Equals(repo) then
                rules <- rules @ [ruleinprofile]

        x.CreateQualityProfile(fileName, rules)

    member x.CreateQualityProfile(file : string, rules : Rule list) =

        let addLine (line:string) =                  
            use wr = new StreamWriter(file, true)
            wr.WriteLine(line)

        let writeXmlRule(rule : Rule) =
            addLine(sprintf """    <rule key="%s">""" rule.key)
            addLine(sprintf """        <name><![CDATA[%s]]></name>""" rule.name)
            addLine(sprintf """        <configKey><![CDATA[%s]]></configKey>""" rule.configKey)
            addLine(sprintf """        <category name="%s" />""" rule.category)
            addLine(sprintf """        <description><![CDATA[  %s  ]]></description>""" rule.description)
            addLine(sprintf """    </rule>""")

        addLine(sprintf """<?xml version="1.0" encoding="ASCII"?>""")
        addLine(sprintf """<rules>""")

        for rule in rules do
            writeXmlRule rule

        addLine(sprintf """</rules>""")       

    member x.AddAProfileFromFileToSqaleModel(repo : string, model : SqaleModel, fileToRead : string) =
        let addLine (line:string, fileToWrite:string) =                  
            use wr = new StreamWriter(fileToWrite, true)
            wr.WriteLine(line)

        try
            let profile = RulesXmlOldType.Parse(File.ReadAllText(fileToRead))

            for rule in profile.GetRules() do
                let createdRule = new Rule()
                createdRule.repo <- repo
                try
                    createdRule.configKey <- rule.Configkey
                with
                | ex -> ()
                try
                    createdRule.category <- rule.Category.Value                   
                with
                | ex -> ()
                createdRule.description <- rule.Description
                createdRule.name <- rule.Name
                createdRule.key <- rule.Key
                model.CreateRuleInProfile(createdRule) |> ignore
        with
        | ex ->
            let profile = RulesXmlNewType.Parse(File.ReadAllText(fileToRead))

            for rule in profile.GetRules() do
                let createdRule = new Rule()
                createdRule.repo <- repo
                createdRule.configKey <- rule.ConfigKey.Replace("![CDATA[", "").Replace("]]", "").Trim()
                try
                    createdRule.category <- rule.Category.Name
                with
                | ex -> ()
                createdRule.description <- rule.Description.Replace("![CDATA[", "").Replace("]]", "").Trim()
                createdRule.name <- rule.Name.Replace("![CDATA[", "").Replace("]]", "").Trim()
                createdRule.key <- rule.Key
                model.CreateRuleInProfile(createdRule) |> ignore

    member x.WriteCharacteristicsFromScaleModelToFile(model : SqaleModel, fileToWrite : string) =
        let addLine (line:string, fileToWrite:string) =                  
            use wr = new StreamWriter(fileToWrite, true)
            wr.WriteLine(line)

        if File.Exists(fileToWrite) then
            File.Delete(fileToWrite)

        addLine("""<?xml version="1.0"?>""", fileToWrite)
        addLine("""<sqale>""", fileToWrite)
        for char in model.GetCharacteristics() do
            addLine(sprintf """    <chc>""", fileToWrite)
            addLine(sprintf """    <key>%s</key>""" char.key, fileToWrite)
            addLine(sprintf """    <name>%s</name>""" char.name, fileToWrite)
            for subchar in char.GetSubChars do
                addLine(sprintf """        <chc>""", fileToWrite)
                addLine(sprintf """            <key>%s</key>""" subchar.key, fileToWrite)
                addLine(sprintf """            <name>%s</name>""" subchar.name, fileToWrite)
                addLine(sprintf """        </chc>""", fileToWrite)

            addLine(sprintf """    </chc>""", fileToWrite)

        addLine("""</sqale>""", fileToWrite)          
        addLine("""""", fileToWrite)
    
    member x.WriteSqaleModelToFile(model : SqaleModel, fileToWrite : string) =
        let addLine (line:string, fileToWrite:string) =                  
            use wr = new StreamWriter(fileToWrite, true)
            wr.WriteLine(line)

        if File.Exists(fileToWrite) then
            File.Delete(fileToWrite)

        let writePropToFile(key : string, value : string, txt : string, file : string) = 
            addLine(sprintf """                <prop>""", file)
            addLine(sprintf """                    <key>%s</key>""" key, file)
            if not(String.IsNullOrEmpty(value)) then
                addLine(sprintf """                    <val>%s</val>""" value, file)
            if not(String.IsNullOrEmpty(txt)) then
                addLine(sprintf """                    <txt>%s</txt>""" txt, file)

            addLine(sprintf """                </prop>""", file)

        let writeRulesChcToFile (charName : string, subcharName : string, file : string) = 
            for rule in model.GetProfile().Rules do
                if rule.category.Equals(charName) && rule.subcategory.Equals(subcharName) then
                    addLine(sprintf """            <chc>""", fileToWrite)
                    addLine(sprintf """                <rule-repo>%s</rule-repo>""" rule.repo, file)
                    addLine(sprintf """                <rule-key>%s</rule-key>""" rule.key, file)
                    writePropToFile("remediationFunction", "", rule.remediationFunction, file)
                    writePropToFile("remediationFactor", rule.remediationFactorVal, rule.remediationFactorTxt, file)

                    if not(rule.remediationFunction.Equals(Constants.RemediationFunction.CONSTANT_ISSUE)) then
                        if String.IsNullOrEmpty(rule.remediationOffsetVal) then                 
                            writePropToFile("offset", "0.0", "d", file)
                        else
                            writePropToFile("offset", rule.remediationOffsetVal, rule.remediationOffsetTxt, file)

                    addLine(sprintf """            </chc>""", fileToWrite)
           
        addLine("""<?xml version="1.0"?>""", fileToWrite)
        addLine("""<sqale>""", fileToWrite)
        for char in model.GetCharacteristics() do
            addLine(sprintf """    <chc>""", fileToWrite)
            addLine(sprintf """    <key>%s</key>""" char.key, fileToWrite)
            addLine(sprintf """    <name>%s</name>""" char.name, fileToWrite)
            for subchar in char.GetSubChars do
                addLine(sprintf """        <chc>""", fileToWrite)
                addLine(sprintf """            <key>%s</key>""" subchar.key, fileToWrite)
                addLine(sprintf """            <name>%s</name>""" subchar.name, fileToWrite)
                writeRulesChcToFile(char.key, subchar.key, fileToWrite)
                addLine(sprintf """        </chc>""", fileToWrite)
                

            addLine(sprintf """    </chc>""", fileToWrite)

        addLine("""</sqale>""", fileToWrite)          
        addLine("""""", fileToWrite)                    

    member x.SaveSqaleModelToDsk(model : SqaleModel, fileToWrite : string) =
        let WriteToBytes obj = 
            let formatter = new BinaryFormatter()
            use writeStream = new StreamWriter(fileToWrite, true)
            formatter.Serialize(writeStream.BaseStream, obj)
            writeStream.Flush
        WriteToBytes model

    member x.CombineWithDefaultProfileDefinition(model : SqaleModel, file : string) = 
        let profile = ProfileDefinition.Parse(File.ReadAllText(file))
        model.language <- profile.Language
        model.profileName <- profile.Name
        for rule in profile.Rules.GetRules() do
            try
                let ruletoUpdate = model.GetProfile().IsRulePresent(rule.Key)
                ruletoUpdate.severity <- rule.Priority
                ruletoUpdate.repo <- rule.RepositoryKey
            with
            | ex -> ()
            

    member x.SaveSqaleModelToDskAsXml(model : SqaleModel, fileToWrite : string) =
        let addLine (line:string, fileToWrite:string) =                  
            use wr = new StreamWriter(fileToWrite, true)
            wr.WriteLine(line)

        if File.Exists(fileToWrite) then
            File.Delete(fileToWrite)

        addLine(sprintf """<?xml version="1.0" encoding="ASCII"?>""", fileToWrite)
        addLine(sprintf """<sqaleManager xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="cxx-model-project.xsd">""", fileToWrite)
        //for char in model.GetCharacteristics() do
        //    addLine(sprintf """    <characteristic>""", fileToWrite)
        //    addLine(sprintf """        <key>%s</key>""" char.key, fileToWrite)
        //    addLine(sprintf """        <name>%s</name>""" char.name, fileToWrite)            
        //    for subchar in char.GetSubChars do
        //        addLine(sprintf """        <subcaracteristic>""", fileToWrite)
        //        addLine(sprintf """            <key>%s</key>""" subchar.key, fileToWrite)
        //        addLine(sprintf """            <name>%s</name>""" subchar.name, fileToWrite)                
         //       addLine(sprintf """        </subcaracteristic>""", fileToWrite)
         //   addLine(sprintf """    </characteristic>""", fileToWrite)
        
        addLine(sprintf """    <rules>""", fileToWrite)
        for rule in model.GetProfile().Rules do
            addLine(sprintf """    <rule key="%s">""" rule.key, fileToWrite)           
            addLine(sprintf """        <name>%s</name>""" (EncodeStringAsXml(rule.name)), fileToWrite)
            if String.IsNullOrEmpty(rule.subcategory) then
                addLine(sprintf """        <requirement>undefined</requirement>""", fileToWrite)
            else
                addLine(sprintf """        <requirement>%s</requirement>""" rule.subcategory, fileToWrite)
            if String.IsNullOrEmpty(rule.remediationFactorVal) then
                addLine(sprintf """        <remediationFactorVal>0.0</remediationFactorVal>""", fileToWrite)
            else
                addLine(sprintf """        <remediationFactorVal>%s</remediationFactorVal>""" rule.remediationFactorVal, fileToWrite)

            if String.IsNullOrEmpty(rule.remediationFactorTxt) then
                addLine(sprintf """        <remediationFactorUnit>undefined</remediationFactorUnit>""", fileToWrite)
            else 
                addLine(sprintf """        <remediationFactorUnit>%s</remediationFactorUnit>""" rule.remediationFactorTxt, fileToWrite)

            if String.IsNullOrEmpty(rule.remediationFunction) then
                addLine(sprintf """        <remediationFunction>undefined</remediationFunction>""", fileToWrite)
            else
                addLine(sprintf """        <remediationFunction>%s</remediationFunction>""" rule.remediationFunction, fileToWrite)

            if String.IsNullOrEmpty(rule.remediationOffsetVal) then
                addLine(sprintf """        <remediationOffsetVal>0.0</remediationOffsetVal>""", fileToWrite)
            else
                addLine(sprintf """        <remediationOffsetVal>%s</remediationOffsetVal>""" rule.remediationFunction, fileToWrite)

            if String.IsNullOrEmpty(rule.remediationOffsetTxt) then
                addLine(sprintf """        <remediationOffsetUnit>undefined</remediationOffsetUnit>""", fileToWrite)
            else
                addLine(sprintf """        <remediationOffsetUnit>%s</remediationOffsetUnit>""" rule.remediationFunction, fileToWrite)

            if String.IsNullOrEmpty(rule.severity) then
                addLine(sprintf """        <severity>undefined</severity>""", fileToWrite)
            else
                addLine(sprintf """        <severity>%s</severity>""" rule.severity, fileToWrite)

            addLine(sprintf """        <repo>%s</repo>""" rule.repo, fileToWrite)
            addLine(sprintf """        <description>%s</description>""" (EncodeStringAsXml(rule.description)), fileToWrite)            
            addLine(sprintf """    </rule>""", fileToWrite)
        addLine(sprintf """    </rules>""", fileToWrite)
        addLine(sprintf """</sqaleManager>""", fileToWrite)

    member x.GetCategoryFromSubcategoryKey(model : SqaleModel, requirement : string) = 
        let chars = model.GetCharacteristics()
        let mutable key = ""
                         
        for char in chars do
            try
                let subkey = char.IsSubCharPresent(requirement)
                key <- char.key
            with
            | ex -> ()

        key

    member x.ReadSqaleModelToDskAsXml(fileToRead : string) =

        let model = x.GetDefaultSqaleModel()

        let dskmodel = CxxProjectDefinition.Parse(File.ReadAllText(fileToRead))

        for item in dskmodel.Rules.GetRules() do
            let rule = new Rule()
            rule.key <- item.Key
            rule.name <- item.Name
            rule.repo <- item.Repo
            rule.configKey <- item.Name + "@" + item.Repo
            rule.description <- item.Description
            rule.category <- x.GetCategoryFromSubcategoryKey(model, item.Requirement)
            rule.subcategory <- item.Requirement
            rule.remediationFactorVal <- item.RemediationFactorVal.ToString()
            rule.remediationFactorTxt <- item.RemediationFactorUnit
            rule.remediationFunction <- item.RemediationFunction
            rule.remediationOffsetTxt <- item.RemediationOffsetUnit
            rule.remediationOffsetVal <- item.RemediationOffsetVal.ToString()
            rule.severity <- item.Severity
            model.CreateRuleInProfile(rule) |> ignore
                       
        model                 

    member x.AddProfileDefinitionFromServerToModel(model : SqaleModel, language : string, profile : string, conectionConf : ExtensionTypes.ConnectionConfiguration) =
        let service = SonarRestService(new JsonSonarConnector()) 
        let profile = (service :> ISonarRestService).GetEnabledRulesInProfile(conectionConf , language, profile)
        let rules = (service :> ISonarRestService).GetRules(conectionConf , language)

        for rule in profile.[0].Rules do
            let createdRule = new Rule()
            createdRule.repo <- rule.Repo            
            createdRule.key <- rule.Key
            createdRule.severity <- rule.Severity

            for ruledef in rules do
                if ruledef.Key.EndsWith(rule.Key, true, Globalization.CultureInfo.InvariantCulture) then
                    createdRule.description <- ruledef.Description
                    createdRule.name <- ruledef.Name
                    createdRule.configKey <- ruledef.ConfigKey                    

            model.CreateRuleInProfile(createdRule) |> ignore

        ()

    member x.MergeSqaleDataModels(sourceModel : SqaleModel, externalModel : SqaleModel) = 
        for rule in externalModel.GetProfile().Rules do
            if not(rule.category.Equals("undefined")) then
                try
                    let ruleinModel = sourceModel.GetProfile().Rules |> List.find (fun elem -> elem.key.Equals(rule.key))
                    ruleinModel.category <- rule.category
                    ruleinModel.subcategory <- rule.subcategory
                    ruleinModel.remediationFactorTxt <- rule.remediationFactorTxt
                    ruleinModel.remediationFactorVal <- rule.remediationFactorVal
                    ruleinModel.remediationFunction <- rule.remediationFunction
                with
                | ex -> ()

    member x.LoadSqaleModelFromDsk(fileToRead : string) =
        let ReadFromBytes(file : string)  = 
            let formatter = new BinaryFormatter()
            use readStream = new StreamReader(file)
            let obj = formatter.Deserialize(readStream.BaseStream)
            unbox obj
        let model:SqaleModel = ReadFromBytes fileToRead
        model

