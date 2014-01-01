namespace SqaleManager

module Constants = 

    module Severity = 
        [<Literal>]
        let BLOCKER = "BLOCKER"
        [<Literal>]
        let CRITICAL = "CRITICAL"
        [<Literal>]
        let MAJOR = "MAJOR"
        [<Literal>]
        let MINOR = "MAJOR"
        [<Literal>]
        let INFO = "INFO"

    module RemediationFunction = 
        [<Literal>]
        let LINEAR = "linear"
        [<Literal>]
        let LINEAR_OFFSET = "linear_offset"
        [<Literal>]
        let CONSTANT_RESOURCE = "constant_resource"

    module RemediationUnit = 
        [<Literal>]
        let MINUTE = "mn"
        [<Literal>]
        let HOUR = "h"
        [<Literal>]
        let DAY = "d"

    module Category = 
        [<Literal>]
        let PORTABILITY = "PORTABILITY"
        [<Literal>]
        let MAINTAINABILITY = "MAINTAINABILITY"
        [<Literal>]
        let SECURITY = "SECURITY"
        [<Literal>]
        let EFFICIENCY = "EFFICIENCY"
        [<Literal>]
        let CHANGEABILITY = "CHANGEABILITY"
        [<Literal>]
        let RELIABILITY = "RELIABILITY"
        [<Literal>]
        let TESTABILITY = "TESTABILITY"
        [<Literal>]
        let REUSABILITY = "REUSABILITY"

    module SubCategory = 
        [<Literal>]
        let MODULARITY = "MODULARITY"
        [<Literal>]
        let TRANSPORTABILITY = "TRANSPORTABILITY"
        [<Literal>]
        let UNIT_TESTABILITY = "UNIT_TESTABILITY"
        [<Literal>]
        let UNIT_TESTS = "UNIT_TESTS"
        [<Literal>]
        let SYNCHRONIZATION_RELIABILITY = "SYNCHRONIZATION_RELIABILITY"
        [<Literal>]
        let INSTRUCTION_RELIABILITY = "INSTRUCTION_RELIABILITY"
        [<Literal>]
        let FAULT_TOLERANCE = "FAULT_TOLERANCE"
        [<Literal>]
        let EXCEPTION_HANDLING = "EXCEPTION_HANDLING"
        [<Literal>]
        let DATA_RELIABILITY = "DATA_RELIABILITY"
        [<Literal>]
        let ARCHITECTURE_RELIABILITY = "ARCHITECTURE_RELIABILITY"
        [<Literal>]
        let LOGIC_CHANGEABILITY = "LOGIC_CHANGEABILITY"
        [<Literal>]
        let DATA_CHANGEABILITY = "DATA_CHANGEABILITY"
        [<Literal>]
        let ARCHITECTURE_CHANGEABILITY = "ARCHITECTURE_CHANGEABILITY"
        [<Literal>]
        let CPU_EFFICIENCY = "CPU_EFFICIENCY"
        [<Literal>]
        let MEMORY_EFFICIENCY = "MEMORY_EFFICIENCY"
        [<Literal>]
        let SECURITY_FEATURES = "SECURITY_FEATURES"
        [<Literal>]
        let INPUT_VALIDATION_AND_REPRESENTATION = "INPUT_VALIDATION_AND_REPRESENTATION"
        [<Literal>]
        let ERRORS = "ERRORS"
        [<Literal>]
        let API_ABUSE = "API_ABUSE"
        [<Literal>]
        let UNDERSTANDABILITY = "UNDERSTANDABILITY"
        [<Literal>]
        let READABILITY = "READABILITY"
        [<Literal>]
        let TIME_ZONE_RELATED_PORTABILITY = "TIME_ZONE_RELATED_PORTABILITY"
        [<Literal>]
        let SOFTWARE_RELATED_PORTABILITY = "SOFTWARE_RELATED_PORTABILITY"
        [<Literal>]
        let OS_RELATED_PORTABILITY = "OS_RELATED_PORTABILITY"
        [<Literal>]
        let LANGUAGE_RELATED_PORTABILITY = "LANGUAGE_RELATED_PORTABILITY"
        [<Literal>]
        let HARDWARE_RELATED_PORTABILITY = "HARDWARE_RELATED_PORTABILITY"
        [<Literal>]
        let COMPILER_RELATED_PORTABILITY = "COMPILER_RELATED_PORTABILITY"
