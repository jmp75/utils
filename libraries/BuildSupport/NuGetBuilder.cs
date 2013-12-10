using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils.Lib.BuildSupport
{
    public class NuGetBuilder
    {
        public static void BuildPackages(string configfile)
        {
            var config = NuGetBuilderConfig.Load(configfile);
            Dictionary<string, string[]> pkgslibs = CreatePackages(config);
            var outdir = config.OuputFolder;
            CreateDir(outdir);
            foreach (var k in pkgslibs.Keys)
            {
                CreatePackageOnDisk(outdir, k, pkgslibs[k]);
            }
        }

        private static void CreatePackageOnDisk(string outdir, string mainAssemblyPath, string[] dependenciesPaths)
        {
            string pkgName = MakePkgId(mainAssemblyPath);

            // http://docs.nuget.org/docs/creating-packages/creating-and-publishing-a-package#From_a_convention_based_working_directory

            var pkgDir = Path.Combine(outdir, pkgName);
            var libDir = Path.Combine(pkgDir, "lib");
            CreateDir(libDir);
            AssemblyDiscover.CopyFiles(new[] { mainAssemblyPath }, libDir);
            AssemblyDiscover.CopyFiles(dependenciesPaths, libDir);


        }

        private static string MakePkgId(string mainAssemblyPath)
        {
            throw new NotImplementedException();
        }

        public static string CreateNuspec()
        {
            throw new NotImplementedException();
//<?xml version="1.0"?>
//<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
//  <metadata>
//    <id>MyPackageId</id>
//    <version>1.0</version>
//    <authors>philha</authors>
//    <owners>philha</owners>
//    <licenseUrl>http://LICENSE_URL_HERE_OR_DELETE_THIS_LINE</licenseUrl>
//    <projectUrl>http://PROJECT_URL_HERE_OR_DELETE_THIS_LINE</projectUrl>
//    <iconUrl>http://ICON_URL_HERE_OR_DELETE_THIS_LINE</iconUrl>
//    <requireLicenseAcceptance>false</requireLicenseAcceptance>
//    <description>Package description</description>
//    <tags>Tag1 Tag2</tags>
//    <dependencies>
//      <dependency id="SampleDependency" version="1.0" />
//    </dependencies>
//  </metadata>
//</package>
        }

        private static void CreateDir(string outdir)
        {
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);
        }

        private static Dictionary<string, string[]> CreatePackages(NuGetBuilderConfig config)
        {
            List<string> files = new List<string>();
            for (int i = 0; i < config.MainAssembliesFiles.Length; i++)
            {
                files.AddRange(Directory.GetFiles(config.HeadFolder, config.MainAssembliesFiles[i], SearchOption.TopDirectoryOnly));
            }

            var mainAssmbFiles = files.ToArray();

            var candidateRefFiles = AssemblyDiscover.GetCandidateRefFiles(config.HeadFolder, config.ReferencesFolders);
            candidateRefFiles = AssemblyDiscover.FilterOut(candidateRefFiles, config.FilterOutMatches); // some DirectX assembly caused problems with TIME
            var candidateAsmFiles = AssemblyDiscover.GetCandidateAssemblies(candidateRefFiles);

            Dictionary<string, string[]> pkgDependencies = AssemblyDiscover.FindDependencies(mainAssmbFiles, candidateAsmFiles);

            return MakePkgNames(pkgDependencies);
        }

        private static Dictionary<string, string[]> MakePkgNames(Dictionary<string, string[]> filesInPkgs)
        {
            throw new NotImplementedException();
        }
    }

    public class NuGetBuilderConfig
    {

        public static NuGetBuilderConfig Load(string filename)
        {
            return JsonIo.Load<NuGetBuilderConfig>(filename);
        }

        public static void Save(NuGetBuilderConfig config, string filename)
        {
            JsonIo.Save<NuGetBuilderConfig>(config, filename);
        }

        public string HeadFolder { get; set; }
        public string OuputFolder { get; set; }
        public string[] ReferencesFolders { get; set; }
        public string[] MainAssembliesFiles { get; set; }
        public string[] FilterOutMatches { get; set; }
    }
}
