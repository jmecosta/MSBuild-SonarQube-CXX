namespace BuildAllExtension.Test
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;

    using Microsoft.FSharp.Collections;

    using MSBuild.Tekla.Tasks.Executor;

    using NFluent;

    using NUnit.Framework;

    using InteropWithNative = WindowsApiInteropService.InteropWithNative;

    [TestFixture]
    public class TestInteropServices
    {
        public static FSharpMap<string, string> ConvertCsMapToFSharpMap(Dictionary<string, string> data)
        {
            var map = new FSharpMap<string, string>(new List<Tuple<string, string>>());
            foreach (var elem in data)
            {
                map = map.Add(elem.Key, elem.Value);
            }

            return map;
        }

        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void GetUserConfigurationIsOkAfterAuthentication()
        {
        }
    }
}
