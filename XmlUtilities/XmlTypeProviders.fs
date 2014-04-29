// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlTypeProviders.fs" company="Trimble Navigation Limited">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@trimble.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. 
// You should have received a copy of the GNU General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------

module XmlUtilities

open System.Xml.Linq
open FSharp.Data
open System
open System.IO

open System.Reflection;
open System.Runtime.InteropServices;

type UserSettingsXml = XmlProvider<"""<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
        <PropertyGroup>
        <UserSourceDir>path</UserSourceDir> 
        <UserOutputDir>path</UserOutputDir>
        <UserTSExeDir>path</UserTSExeDir>
        <UserApplicationsDir>path</UserApplicationsDir>
        <UserSRCDir>path</UserSRCDir>
        <UserOBJDir>path</UserOBJDir>
        <UserBINDir>path</UserBINDir>
    </PropertyGroup>
  <PropertyGroup Condition="'$(COND)'=='version'">
	<UserSRCDir>path1</UserSRCDir>
	<UserOBJDir>path1</UserOBJDir> 
	<UserBINDir>path2</UserBINDir> 
  </PropertyGroup>
</Project>""">

type IXmlHelpersService = 
  abstract member GetUserSRCDir : string -> string
  abstract member GetUserBINDir : string -> string
  abstract member GetUserOBJDir : string -> string

type XmlHelpersService() = 
    interface IXmlHelpersService with
        member this.GetUserSRCDir(filepath : string) =
            let helper = UserSettingsXml.Parse(File.ReadAllText(filepath))
            let mutable value = ""
            for data in helper.GetPropertyGroups() do
                try
                    let condition = data.Condition
                    if not(obj.ReferenceEquals(data.Condition, null)) then
                        if condition.Value.Contains("'Work'") || condition.Value = "''"  || condition.Value = "" then
                            value <- data.UserSrcdIr
                    else
                        value <- data.UserSrcdIr                        
                with
                | ex -> value <- data.UserSrcdIr
            value

        member this.GetUserBINDir(filepath : string) =
            let helper = UserSettingsXml.Parse(File.ReadAllText(filepath))
            let mutable value = ""
            for data in helper.GetPropertyGroups() do
                try
                    let condition = data.Condition
                    if not(obj.ReferenceEquals(data.Condition, null)) then
                        if condition.Value.Contains("'Work'") || condition.Value = "''"  || condition.Value = "" then
                            value <- data.UserBindIr
                    else
                        value <- data.UserBindIr                        
                with
                | ex -> value <- data.UserBindIr
            value

        member this.GetUserOBJDir(filepath : string) =
            let helper = UserSettingsXml.Parse(File.ReadAllText(filepath))
            let mutable value = ""
            for data in helper.GetPropertyGroups() do
                try
                    let condition = data.Condition
                    if not(obj.ReferenceEquals(data.Condition, null)) then
                        if condition.Value.Contains("'Work'") || condition.Value = "''"  || condition.Value = "" then
                            value <- data.UserObjdIr
                    else
                        value <- data.UserObjdIr                        
                with
                | ex -> value <- data.UserObjdIr
            value