using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utils.Lib.BuildSupport
{
    public class AssemblyDiscover
    {
        public static void CopyFiles(IEnumerable<string> toCopy, string destinationDir)
        {
            foreach (var file in toCopy)
            {
                if (File.Exists(file))
                    File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            }
        }

        public static string[] GetPdbFiles(List<string> toCopy)
        {
            var pdbFiles = Array.ConvertAll(toCopy.ToArray(),
                                            (x =>
                                             Path.Combine(Path.GetDirectoryName(x), Path.GetFileNameWithoutExtension(x) + ".pdb")));
            return pdbFiles;
        }

        public static List<string> GetFilesToCopy(string[] files, Dictionary<string, Assembly> candidateAsmFiles)
        {
            var toCopy = new List<string>(files);
            //            var allFiles = new List<string>( );
            foreach (var file in files)
            {
                AddReferencedAssemblies(file, candidateAsmFiles, toCopy);
            }
            return toCopy;
        }

        public static Dictionary<string, Assembly> GetCandidateAssemblies(string[] candidateRefFiles)
        {
            //var assemblies = Array.ConvertAll( candidateRefFiles, ( x => Assembly.LoadFrom( x ) ) );
            var candidateAsmFiles = new Dictionary<string, Assembly>();
            for (int i = 0; i < candidateRefFiles.Length; i++)
            {
                if (!candidateAsmFiles.ContainsKey(candidateRefFiles[i]))
                {
                    try
                    {
                        candidateAsmFiles.Add(candidateRefFiles[i], Assembly.LoadFrom(candidateRefFiles[i]));
                    }
                    catch
                    {
                    }
                }
            }
            return candidateAsmFiles;
        }

        public static string[] GetCandidateRefFiles(string directory, params string[] findRefDir)
        {
            var candidateRefFiles = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly).ToList();
            for (int i = 0; i < findRefDir.Length; i++)
                candidateRefFiles.AddRange(Directory.GetFiles(findRefDir[i], "*.dll", SearchOption.TopDirectoryOnly));

            return candidateRefFiles.ToArray();
        }

        public static string[] FilterOut(string[] filepaths, params string[] stringsToFilterOut)
        {
            string[] res = filepaths;
            for (int j = 0; j < stringsToFilterOut.Length; j++)
            {
                res = oneFilterOut(res, stringsToFilterOut[j]);
            }
            return res.ToArray();
        }

        private static string[] oneFilterOut(string[] filepaths, string stringToFilterOut)
        {
            return filepaths.Where((x => (x.Contains(stringToFilterOut) == false))).ToArray();
        }

        public static void AddReferencedAssemblies(string file, Dictionary<string, Assembly> candidateAsmFiles, List<string> toCopy)
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                var referencedAssemblies = assembly.GetReferencedAssemblies();
                foreach (var refAss in referencedAssemblies)
                {
                    var matchingFilename = GetMatchingName(candidateAsmFiles, refAss);
                    if (matchingFilename.Key != null && !toCopy.Contains(matchingFilename.Key))
                    {
                        toCopy.Add(matchingFilename.Key);
                        AddReferencedAssemblies(matchingFilename.Key, candidateAsmFiles, toCopy);
                    }
                }
            }
            catch
            {
                // Most likely, a native DLL.
            }
        }

        private static KeyValuePair<string, Assembly> GetMatchingName(Dictionary<string, Assembly> candidateAsmFiles, AssemblyName refAss)
        {
            var match = candidateAsmFiles.Where((x => x.Value.GetName().Name == refAss.Name));
            var matchingFilename = match.FirstOrDefault();
            return matchingFilename;
        }

        public static Dictionary<string, string[]> FindDependencies(string[] mainAssmbFiles,
                                                                    Dictionary<string, Assembly> candidateAsmFiles)
        {
            var allFileDepends = ToFilePathsDependencies(candidateAsmFiles);
            return FindDependencies(mainAssmbFiles, allFileDepends);
        }

        public static Dictionary<string, string[]> FindDependencies(string[] mainAssmbFiles, Dictionary<string, string[]> allFileDepends)
        {
            var allref = new Dictionary<string, string[]>();
            var result = new Dictionary<string, string[]>();
            for (int i = 0; i < mainAssmbFiles.Length; i++)
            {
                RecursiveDepFind(allref, mainAssmbFiles[i], allFileDepends);
            }
            allref = DiffPackages(allref, mainAssmbFiles);
            for (int i = 0; i < mainAssmbFiles.Length; i++)
            {
                result[mainAssmbFiles[i]] = allref[mainAssmbFiles[i]];
            }
            return result;
        }

        public static Dictionary<string, string[]> DiffPackages(Dictionary<string, string[]> allref, string[] packageKeys)
        {
            var result = new Dictionary<string, string[]>();
            foreach (var k in allref.Keys)
            {
                result[k] = DiffPackages(allref[k], allref, packageKeys);
            }
            return result;
        }

        private static string[] DiffPackages(string[] allDeps, Dictionary<string, string[]> allref, string[] packageKeys)
        {
            string[] result = (string[])allDeps.Clone();
            foreach (var dep in allDeps)
            {
                if (packageKeys.Contains(dep))
                {
                    var removed = allref[dep].ToList();
                    removed.Add(dep);
                    result = SetDiff(result, removed);
                }
            }
            return result;
        }

        private static string[] SetDiff(string[] result, IEnumerable<string> removed)
        {
            return result.Where(x => !removed.Contains(x)).ToArray();
        }

        public static Dictionary<string, string[]> ToFilePathsDependencies(Dictionary<string, Assembly> candidateAsmFiles)
        {
            var assmbRef = new Dictionary<string, string[]>();
            foreach (var candidateAsmFile in candidateAsmFiles)
            {
                var referencedAssemblies = candidateAsmFile.Value.GetReferencedAssemblies();
                var deps = new List<string>();
                foreach (var refAss in referencedAssemblies)
                {
                    var matchingFilename = GetMatchingName(candidateAsmFiles, refAss);
                    if(matchingFilename.Key != null)
                       deps.Add(matchingFilename.Key);
                }
                assmbRef.Add(candidateAsmFile.Key, deps.ToArray());
            }
            return assmbRef;
        }

        public static List<string> RecursiveDepFind(Dictionary<string, string[]> result, string file, Dictionary<string, string[]> assmbReferences)
        {
            // Note that this probably does not cater for circular dependencies. May be an issue for e.g. IKVM. Let it be... StackOverflow
            if (result.ContainsKey(file))
                return result[file].ToList();
            if (!assmbReferences.ContainsKey(file))
                return new List<string>();
            var depFiles = assmbReferences[file];
            var deps = new List<string>(depFiles);
            foreach (var depFile in depFiles)
            {
                deps = deps.Union(RecursiveDepFind(result, depFile, assmbReferences)).ToList();
            }
            deps = deps.Intersect(assmbReferences.Keys.ToArray()).ToList();
            result[file] = deps.ToArray();
            return deps;
        }

        private static void RecursiveDepFindOutdated(Dictionary<string, string[]> result, string file, List<string> deps)
        {
            if (result.ContainsKey(file)) // all the dependencies of file has been found and flattened
                foreach (var s in result[file])
                {
                    RecursiveDepFindOutdated(result, s, deps);
                    if (!deps.Contains(s))
                        deps.Add(s);
                }
            if (!deps.Contains(file))
                deps.Add(file);
        }
    }
}
