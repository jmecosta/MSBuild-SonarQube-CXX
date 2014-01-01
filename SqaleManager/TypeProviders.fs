namespace SqaleManager

open FSharp.Data
open System.Xml.Linq

type CxxProjectDefinition = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<sqaleManager xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="cxx-model-project.xsd">
    <rules>
    <rule key="AssignmentAddressToInteger">
        <name>Assigning an address value to an integer (int/long/etc.) type is not portable</name>
        <requirement>undefined</requirement>
        <remediationFactorVal>0.0</remediationFactorVal>
        <remediationFactorUnit>undefined</remediationFactorUnit>
        <remediationFunction>undefined</remediationFunction>
        <severity>MINOR</severity>
        <repo>cppcheck</repo>
        <description>
      Assigning an address value to an integer (int/long/etc.) type is
      not portable.
    </description>
    </rule>
    <rule key="AssignmentIntegerToAddress">
        <name>Assigning an integer (int/long/etc) to a pointer is not portable</name>
        <requirement>undefined</requirement>
        <remediationFactorVal>0.0</remediationFactorVal>
        <remediationFactorUnit>undefined</remediationFactorUnit>
        <remediationFunction>undefined</remediationFunction>
        <severity>MINOR</severity>
        <repo>cppcheck</repo>
        <description>
      Assigning an integer (int/long/etc) to a pointer is not portable.
    </description>
    </rule>
    </rules>
</sqaleManager>""" >


type ProfileDefinition = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<profile>
  <name>Sonar way</name>
 <language>c++</language>
  <rules>
    <rule>
      <repositoryKey>cppcheck</repositoryKey>
      <key>AssignmentAddressToInteger</key>
      <priority>MINOR</priority>
    </rule>
    <rule>
      <repositoryKey>cppcheck</repositoryKey>
      <key>AssignmentIntegerToAddress</key>
      <priority>MINOR</priority>
    </rule>
  </rules>
</profile>""">

type RulesXmlNewType = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<rules>
    <rule key="cpplint.readability/nolint-0">
        <name><![CDATA[ Unknown NOLINT error category: %s  % category]]></name>
        <configKey><![CDATA[cpplint.readability/nolint-0@CPP_LINT]]></configKey>
        <category name="readability" />
        <description><![CDATA[  Unknown NOLINT error category: %s  % category ]]></description>
    </rule>
    <rule key="cpplint.readability/fn_size-0">
        <name><![CDATA[ Small and focused functions are preferred:   %s has %d non-comment lines   (error triggered by exceeding %d lines).  % (self.current_function, self.lines_in_function, trigger)]]></name>
        <configKey><![CDATA[cpplint.readability/fn_size-0@CPP_LINT]]></configKey>
        <category name="readability" />
        <description><![CDATA[  Small and focused functions are preferred:   %s has %d non-comment lines   (error triggered by exceeding %d lines).  % (self.current_function, self.lines_in_function, trigger) ]]></description>
    </rule>
</rules>""">

type RulesXmlOldType = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<rules>
  <rule>
    <key>AssignmentAddressToInteger</key>
    <configkey>AssignmentAddressToInteger</configkey>
    <name>Assigning an address value to an integer (int/long/etc.) type is not portable</name>
    <description>
      Assigning an address value to an integer (int/long/etc.) type is
      not portable.
    </description>
  </rule>
  <rule>
    <key>AssignmentIntegerToAddress</key>
    <configkey>AssignmentIntegerToAddress</configkey>
    <category>Maintainability</category>
    <name>Assigning an integer (int/long/etc) to a pointer is not portable</name>
    <description>
      Assigning an integer (int/long/etc) to a pointer is not portable.
    </description>
  </rule>
 </rules>""">   

type SqaleModelType = XmlProvider<"""<?xml version="1.0"?>
<sqale>
    <chc>
        <key>PORTABILITY</key>
        <name>Portability</name>
        <chc>
            <key>COMPILER_RELATED_PORTABILITY</key>
            <name>Compiler related portability</name>
            <chc>
                <rule-repo>gendarme</rule-repo>
                <rule-key>DoNotPrefixValuesWithEnumNameRule</rule-key>
                <prop>
                    <key>remediationFactor</key>
                    <val>0.03d</val>
                    <txt>d</txt>
                </prop>
                <prop>
                    <key>remediationFunction</key>
                    <txt>linear</txt>
                </prop>
            </chc>
            <chc>
                <rule-repo>gendarme</rule-repo>
                <rule-key>DoNotPrefixValuesWithEnumNameRule1</rule-key>
                <prop>
                    <key>remediationFactor</key>
                    <val>0.03d</val>
                    <txt>d</txt>
                </prop>
                <prop>
                    <key>remediationFunction</key>
                    <txt>linear</txt>
                </prop>
            </chc>                          
        </chc>
        <chc>
            <key>HARDWARE_RELATED_PORTABILITY</key>
            <name>Hardware related portability</name>
        </chc>
        <chc>
            <key>LANGUAGE_RELATED_PORTABILITY</key>
            <name>Language related portability</name>
        </chc>
        <chc>
            <key>OS_RELATED_PORTABILITY</key>
            <name>OS related portability</name>
        </chc>
        <chc>
            <key>SOFTWARE_RELATED_PORTABILITY</key>
            <name>Software related portability</name>
        </chc>
        <chc>
            <key>TIME_ZONE_RELATED_PORTABILITY</key>
            <name>Time zone related portability</name>
        </chc>
    </chc>
    <chc>
        <key>PORTABILITY</key>
        <name>Portability</name>
        <chc>
            <key>COMPILER_RELATED_PORTABILITY</key>
            <name>Compiler related portability</name>
        </chc>
    </chc>
</sqale>""">


