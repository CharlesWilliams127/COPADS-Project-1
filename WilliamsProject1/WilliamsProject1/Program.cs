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
            try
            {
                // switch on command line args
                switch (args[1])
                {
                    case "-s": // single thread
                        CountSequential(args[2]);
                        break;
                    case "-p": // parallel threads
                        CountParallel(args[2]);
                        break;
                    case "-b": // both
                        CountParallel(args[2]);
                        CountSequential(args[2]);
                        break;
                    default:
                        DisplayHelp();
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("There was an insufficient number of arguments provided.");
                DisplayHelp();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("The directory specified could not be found.");
                DisplayHelp();
            }
        }

        static void CountSequential(string root)
        {
            var du = new DiskUsageCounter();
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            du.RunSequential(root);
            stopWatch.Stop();

            Console.WriteLine("Sequential Calculated in: {0}s", stopWatch.Elapsed.ToString("s\\.fffffff"));
            Console.WriteLine("{0} folders, {1} files, {2} bytes\n", du.FolderCount.ToString("N0"), 
                du.FileCount.ToString("N0"), 
                du.ByteCount.ToString("N0"));
        }

        static void CountParallel(string root)
        {
            var du = new DiskUsageCounter();
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            du.RunParallel(root);
            stopWatch.Stop();

            Console.WriteLine("Sequential Calculated in: {0}s", stopWatch.Elapsed.ToString("s\\.fffffff"));
            Console.WriteLine("{0} folders, {1} files, {2} bytes\n", du.FolderCount.ToString("N0"), 
                du.FileCount.ToString("N0"), 
                du.ByteCount.ToString("N0"));
        }

        static void DisplayHelp()
        {
            Console.Write("Usage: du [-s] [-p] [-b] <path>\n" +
                            "Summarize disk usage of the set of FILES, recursively for directories.\n\n" +
                            "You MUST specify one of the parameters, -s, -p, or -b\n" +
                            "-s\tRun in single threaded mode\n" +
                            "-p\tRun in parallel mode (uses all available processors)\n" +
                            "-b\tRun in both parallel and single threaded mode.\n" +
                            "  \tRuns parallel followed by sequential mode\n");
        }
    }

    public class DiskUsageCounter
    {

        public static Object myLock = new Object();

        // counter variables
        private int folderCount = 0;
        private int fileCount = 0;
        private long byteCount = 0;

        // getters for printing in main
        public int FolderCount { get { return folderCount; } }
        public int FileCount { get { return fileCount; } }
        public long ByteCount { get { return byteCount; } }


        public void RunSequential(string root)
        {
            var dir = new DirectoryInfo(root);
            DirectoryInfo[] subDirectories;

            try
            {
                 subDirectories = dir.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            foreach (var d in subDirectories)
            {
                folderCount++;
                RunSequential(d.FullName);

                FileInfo[] files;

                try
                {
                    files = dir.GetFiles();
                }
                catch (UnauthorizedAccessException)
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
            catch (UnauthorizedAccessException)
            {
                return;
            }

            Parallel.ForEach(subDirectories, d => 
            {
                lock (myLock)
                {
                    folderCount++;
                }
                RunParallel(d.FullName);
                FileInfo[] files;

                try
                {
                    files = dir.GetFiles();
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }

                Parallel.ForEach(files, f =>
                {
                    lock (myLock)
                    {
                        fileCount++;
                        byteCount += f.Length;
                    }
                });
            });
        }
    }
}
