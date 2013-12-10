using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Utils.Lib.BuildSupport.Tests
{
    [TestFixture]
    public class TestNuGetBuilder
    {
        [Test]
        public void TestPackageDependencyDetection()
        {

            var d = new Dictionary<string, string[]>();
            d.Add(@"c:\fakepath\a.dll", new[]{@"c:\tmp\ref\SysBlah.dll",@"c:\tmp\ref\SysBlah.Core.dll"});
            d.Add(@"c:\fakepath\b.dll", new[]{@"c:\tmp\ref\SysBlah.dll",@"c:\tmp\ref\SysBlah.Core.dll", @"c:\fakepath\a.dll"});
            d.Add(@"c:\fakepath\c.dll", new[]{@"c:\tmp\ref\SysBlah.dll",@"c:\tmp\ref\SysBlah.Core.dll", @"c:\tmp\ref\SysBlah.XML.dll", @"c:\fakepath\b.dll"});
            d.Add(@"c:\fakepath\d.dll", new[]{@"c:\tmp\ref\SysBlah.dll",@"c:\tmp\ref\SysBlah.Core.dll", @"c:\fakepath\c.dll"});
            d.Add(@"c:\fakepath\e.dll", new[]{@"c:\tmp\ref\SysBlah.dll",@"c:\tmp\ref\SysBlah.Core.dll", @"c:\fakepath\f.dll"});
            d.Add(@"c:\fakepath\f.dll", new[] { @"c:\tmp\ref\SysBlah.dll", @"c:\tmp\ref\SysBlah.Core.dll" });
            d.Add(@"c:\tmp\ref\SysBlah.XML.dll", new[] { @"c:\tmp\ref\mscorlibBlah.dll", @"c:\tmp\ref\SysBlah.dll" });
            d.Add(@"c:\tmp\ref\SysBlah.dll", new string[] { @"System" });
            d.Add(@"c:\tmp\ref\mscorlibBlah.dll", new string[] { @"System" });
            d.Add(@"c:\tmp\ref\SysBlah.Core.dll", new[] { @"c:\tmp\ref\SysBlah.dll" });

            var mainAssemblies = new[]
                {
                    @"c:\fakepath\a.dll",
                    @"c:\fakepath\c.dll"
                };

            var pkgDefinitions = AssemblyDiscover.FindDependencies(mainAssemblies, d);

            AssertSameSet(
                new[]
                {
                    @"c:\fakepath\a.dll",
                    @"c:\fakepath\c.dll"
                },
                pkgDefinitions.Keys.ToArray()
            );
            AssertSameSet(
                new[] { @"c:\tmp\ref\SysBlah.dll", @"c:\tmp\ref\SysBlah.Core.dll" },
                pkgDefinitions[@"c:\fakepath\a.dll"]);
            AssertSameSet(
                new[] { @"c:\tmp\ref\SysBlah.XML.dll", @"c:\fakepath\b.dll", @"c:\tmp\ref\mscorlibBlah.dll" },
                pkgDefinitions[@"c:\fakepath\c.dll"]);
        }

        private void AssertSameSet(string[] expected, string[] actual)
        {
            Array.Sort(expected);
            Array.Sort(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}
