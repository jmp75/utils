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
                    var match = candidateAsmFiles.Where((x => x.Value.GetName().Name == refAss.Name));
                    var matchingFilename = match.FirstOrDefault();
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

    }
}
