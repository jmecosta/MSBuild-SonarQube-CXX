namespace SqaleManager

open FSharp.Data
open System.Xml.Linq
open System.IO

type Rule() =
    member val key = "" with get, set
    member val name = "" with get, set
    member val description = "" with get, set 
    member val configKey = "" with get, set
    member val category = "" with get, set
    member val subcategory = "" with get, set
    member val remediationFactorVal = "" with get, set
    member val remediationFactorTxt = "" with get, set
    member val remediationFunction = "" with get, set
    member val remediationOffsetVal = "" with get, set
    member val remediationOffsetTxt = "" with get, set
    member val severity = "" with get, set
    member val repo = "" with get, set
     
type Profile() = 
    let mutable rules : Rule list = []

    member x.Rules = rules

    member x.AddRule (rule : Rule) =
        rules <- rules @ [rule]
    
    member x.IsRulePresent(key : string) =              
        (List.toArray rules) |> Array.find (fun elem -> elem.key.Equals(key)) 

    member x.CreateRule(rule : Rule) =
        try
            x.IsRulePresent(rule.key)
        with
        | :? System.Collections.Generic.KeyNotFoundException ->         
            rules <- rules @ [rule]
            rule
        

type SubCharacteristics(key : string, name : string) = 
    member val key = key with get
    member val name = name with get

type Characteristic(key : string, name : string) =
    let mutable subchars : SubCharacteristics list = []
    member val key = key with get
    member val name = name with get
   
    member x.CreateSubChar(key : string, name : string) =
        try
            x.IsSubCharPresent(key : string)
        with
        | :? System.Collections.Generic.KeyNotFoundException ->         
            let newChar = new SubCharacteristics(key, name)
            subchars <- subchars @ [newChar]
            newChar

    member x.IsSubCharPresent(key : string) = 
        (List.toArray subchars) |> Array.find (fun elem -> elem.key.Equals(key))

    member this.GetSubChars = subchars
        

type SqaleModel() =
    let profile : Profile = new Profile()
    let mutable characteristics : Characteristic list = []

    member val profileName = "" with get, set
    member val language = "" with get, set
    member x.GetCharacteristics() = characteristics
    member x.GetProfile() = profile

    member x.IsCharPresent(key : string) =                    
        (List.toArray characteristics) |> Array.find (fun elem -> elem.key.Equals(key))

    member x.CreateAChar(key : string, name : string) =
        try
            x.IsCharPresent(key : string)
        with
        | :? System.Collections.Generic.KeyNotFoundException ->         
            let newChar = new Characteristic(key, name)
            characteristics <- characteristics @ [newChar]
            newChar
    
    member x.CreateRuleInProfile(rule : Rule) =
        profile.CreateRule(rule)
      
    member x.LoadSqaleModelFromString(str : string) =
        let sqale = SqaleModelType.Parse(str)

        for chk in sqale.GetChcs() do
            let char = x.CreateAChar(chk.Key, chk.Name)            

            for subchk in chk.GetChcs() do
                char.CreateSubChar(subchk.Key, subchk.Name) |> ignore

                for chc in subchk.GetChcs() do
                    let rule = new Rule()
                    rule.repo <- chc.RuleRepo
                    rule.key <- chc.RuleKey
                    rule.configKey <- chc.RuleKey + "@" + chc.RuleRepo

                    for prop in chc.GetProps() do
                        if prop.Key.Equals("remediationFactor") then
                            rule.category <- chk.Key
                            rule.subcategory <- subchk.Key
                            try
                                rule.remediationFactorVal <- prop.Val
                            with
                            | ex -> ()
                            try
                                rule.remediationFactorTxt <- prop.Txt
                            with
                            | ex -> ()

                        if prop.Key.Equals("remediationFunction") then
                            try
                                rule.remediationFunction <- prop.Txt
                            with
                            | ex -> ()

                    profile.CreateRule(rule) |> ignore

    member x.LoadSqaleModelFromFile(path : string) =
        let sqale = SqaleModelType.Parse(File.ReadAllText(path))

        for chk in sqale.GetChcs() do
            let char = x.CreateAChar(chk.Key, chk.Name)            

            for subchk in chk.GetChcs() do
                char.CreateSubChar(subchk.Key, subchk.Name) |> ignore

                for chc in subchk.GetChcs() do
                    let rule = new Rule()
                    rule.repo <- chc.RuleRepo
                    rule.key <- chc.RuleKey
                    rule.configKey <- chc.RuleKey + "@" + chc.RuleRepo

                    for prop in chc.GetProps() do
                        if prop.Key.Equals("remediationFactor") then
                            rule.category <- chk.Key
                            rule.subcategory <- subchk.Key
                            try
                                rule.remediationFactorVal <- prop.Val
                            with
                            | ex -> ()
                            try
                                rule.remediationFactorTxt <- prop.Txt
                            with
                            | ex -> ()

                        if prop.Key.Equals("remediationFunction") then
                            try
                                rule.remediationFunction <- prop.Txt
                            with
                            | ex -> ()

                    profile.CreateRule(rule) |> ignore


    

    



