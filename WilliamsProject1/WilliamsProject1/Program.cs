using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace WilliamsProject1
{
    class Program
    {
        static void Main(string[] args)
        {
            // hardcode directory for now
            // TODO: change start and end calcs to a stopwatch
            var singleCounter = new SingleThreadCounter();
            var start = DateTime.Now;
            singleCounter.Run("C:\\Program Files (x86)\\Battle.net");
            var end = DateTime.Now;

            var multiCounter = new SingleThreadCounter();
            var multiStart = DateTime.Now;
            multiCounter.RunParallel("C:\\Program Files (x86)\\Battle.net");
            var multiEnd = DateTime.Now;

            Console.WriteLine("Single: " + (end - start) + "s");
            Console.WriteLine("Single: " + singleCounter.FolderCount + " folders");
            Console.WriteLine("Single: " + singleCounter.FileCount + " files");
            Console.WriteLine("Single: " + singleCounter.ByteCount + " bytes");

            Console.WriteLine("Parallel: " + (multiEnd - multiStart) + "s");
            Console.WriteLine("Parallel: " + multiCounter.FolderCount + " folders");
            Console.WriteLine("Parallel: " + multiCounter.FileCount + " files");
            Console.WriteLine("Parallel: " + multiCounter.ByteCount + " bytes");

            // test for comparison
            var dirs = Directory.GetDirectories("C:\\Program Files (x86)\\Battle.net", "*", SearchOption.AllDirectories);
            Console.WriteLine("Test: " + dirs.Length + " folders");
        }
    }

    public class SingleThreadCounter
    {

        // counter variables
        private int folderCount = 0;
        private int fileCount = 0;
        private long byteCount = 0;

        // getters for printing in main
        public int FolderCount { get { return folderCount; } }
        public int FileCount { get { return fileCount; } }
        public long ByteCount { get { return byteCount; } }


        public void Run(string root)
        {

            var dir = new DirectoryInfo(root);
            DirectoryInfo[] subDirectories;

            try
            {
                 subDirectories = dir.GetDirectories();
            }
            catch (UnauthorizedAccessException e)
            {
                return;
            }

            foreach (var d in subDirectories)
            {
                // TODO: if access is denied on the top level it kicks you out, fix
                // this isn't the case with a parallel loop since the parallel loop
                // will handle each directory seperately
                folderCount++;
                Run(d.FullName);

                FileInfo[] files;

                try
                {
                    files = dir.GetFiles();
                }
                catch (UnauthorizedAccessException e)
                {
                    return;
                }

                foreach (var f in files)
                {
                    fileCount++;
                    byteCount += f.Length;
                }
            }
            
        }

        public void RunParallel(string root)
        {
            var dir = new DirectoryInfo(root);

            DirectoryInfo[] subDirectories;
            try
            {
                subDirectories = dir.GetDirectories();
            }
            catch (UnauthorizedAccessException e)
            {
                return;
            }

            Parallel.ForEach(subDirectories, d => {

                // TODO: need to put a lock or two in here to prevent deadlock
                folderCount++;
                RunParallel(d.FullName);
                FileInfo[] files;

                try
                {
                    files = dir.GetFiles();
                }
                catch (UnauthorizedAccessException e)
                {
                    return;
                }

                Parallel.ForEach(files, f =>
                    {
                        fileCount++;
                        byteCount += f.Length;
                    });
                });
            }
        }
    }
