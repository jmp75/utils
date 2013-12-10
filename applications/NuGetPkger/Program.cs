using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Lib.BuildSupport;

namespace NuGetPkger
{
    /// <summary>
    /// A program to automate the creation of packages from a set of assemblies with dependencies.
    /// </summary>
    /// <remarks>The main purpose is to propose something to handle the TIME assemblies</remarks>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var config = new NuGetBuilderConfig
                    {
                        FilterOutMatches = new[] {"Direct"},
                        HeadFolder = @"F:\src\csiro\time\Solutions\Output",
                        MainAssembliesFiles = new[] {"TIME.dll", "TIME.Tools.dll"},
                        OuputFolder = @"F:\tmp\testnuget",
                        ReferencesFolders = new[] {@"F:\src\csiro\time\Solutions\Output"}
                    };
                Console.WriteLine(JsonIo.Serialize(config));
                return;
            }
            NuGetBuilder.BuildPackages(args[0]);
        }
    }
}
