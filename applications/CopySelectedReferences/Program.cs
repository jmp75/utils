using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Utils.Lib.BuildSupport;

namespace Utils.App.CopySelectedReferences
{
    /// <summary>
    /// A program that was designed to find/copy the subset of assemblies and their dependencies from one or more folders.
    /// </summary>
    class Program
    {
        static void Main( string[] args )
        {
            if (args.Length < 4 || args.Contains("-h"))
            {
                Console.WriteLine(@"Copy one or more assemblies and their dependencies to a destination folder");
                Console.WriteLine(@"CopySelectedReferences directory fileWildCards copyto findRefDir1 [findRefDir2 ...]");
                return;
            }
            string directory = args[0];
            string fileWildCards = args[1];
            string copyto = args[2];
            var findRefDir = new string[args.Length-3];
            Array.Copy(args, 3, findRefDir, 0, findRefDir.Length); 

            var files = Directory.GetFiles( directory, fileWildCards, SearchOption.TopDirectoryOnly );

            var candidateRefFiles = AssemblyDiscover.GetCandidateRefFiles(directory, findRefDir);
            candidateRefFiles = AssemblyDiscover.FilterOut(candidateRefFiles, "Direct"); // some DirectX assembly caused problems with TIME
            var candidateAsmFiles = AssemblyDiscover.GetCandidateAssemblies(candidateRefFiles);
            var toCopy = AssemblyDiscover.GetFilesToCopy(files, candidateAsmFiles);
            var pdbFiles = AssemblyDiscover.GetPdbFiles(toCopy);

            AssemblyDiscover.CopyFiles(toCopy, copyto);
            AssemblyDiscover.CopyFiles(pdbFiles, copyto);
        }
    }
}
