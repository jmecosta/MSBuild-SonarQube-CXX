using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegisterTask
{
    using System.Text.RegularExpressions;

    using Microsoft.Win32.TaskScheduler;

    class Program
    {
        static void Main(string[] args)
        {

            TaskService ts = new TaskService();
            var tasks = ts.FindAllTasks(new Regex(string.Empty));
            var task = ts.GetTask("Elevate");
            TaskDefinition td = ts.NewTask();
            td.Principal.RunLevel = TaskRunLevel.Highest;
            //td.Triggers.AddNew(TaskTriggerType.YourDesiredSchedule);
            td.Triggers.AddNew(TaskTriggerType.Registration);
            //td.Actions.Add(new ExecAction("Path Of your Application File", null));
            td.Actions.Add(new ExecAction(@"E:\StructuresSrc\Kit\bin\gacutil.exe", "/nologo /u \"Tekla.Logging, Version=99.1\""));
            ts.RootFolder.RegisterTaskDefinition("GatUtil", td);
            td.Actions.Add(new ExecAction(@"E:\StructuresSrc\MSBuild\MSBuild\MSBuildTasks\Elevate.exe", "E:\\StructuresSrc\\Kit\\bin\\gacutil.exe /nologo /u \"Tekla.Logging, Version=99.1\""));
            ts.RootFolder.RegisterTaskDefinition("Elevate", td);
            ts.RootFolder.DeleteTask("Elevate", false);
        }
    }
}
