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
                Console.WriteLine("Directory {0}\n", args[1]);
                // switch on command line args
                switch (args[0])
                {
                    case "-s": // single thread
                        CountSequential(new DirectoryInfo(args[1]));
                        break;
                    case "-p": // parallel threads
                        CountParallel(new DirectoryInfo(args[1]));
                        break;
                    case "-b": // both
                        CountParallel(new DirectoryInfo(args[1]));
                        CountSequential(new DirectoryInfo(args[1]));
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

        // Helper method for calling sequential count from main
        static void CountSequential(DirectoryInfo dir)
        {
            var du = new DiskUsageCounter();
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            du.RunSequential(dir);
            stopWatch.Stop();

            Console.WriteLine("Sequential Calculated in: {0}s", stopWatch.Elapsed.ToString("s\\.fffffff"));
            Console.WriteLine("{0} folders, {1} files, {2} bytes\n", du.FolderCount.ToString("N0"),
                du.FileCount.ToString("N0"),
                du.ByteCount.ToString("N0"));
        }

        // Helper method for calling parallel count from main
        static void CountParallel(DirectoryInfo dir)
        {
            var du = new DiskUsageCounter();
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            du.RunParallel(dir);
            stopWatch.Stop();

            Console.WriteLine("Parallel Calculated in: {0}s", stopWatch.Elapsed.ToString("s\\.fffffff"));
            Console.WriteLine("{0} folders, {1} files, {2} bytes\n", du.FolderCount.ToString("N0"),
                du.FileCount.ToString("N0"),
                du.ByteCount.ToString("N0"));
        }

        // Helper method for displaying command prompt help
        static void DisplayHelp()
        {
            Console.Write("Usage: dotnet run -- [-s] [-p] [-b] <path>\n" +
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
        // this object will be locked to prevent deadlock
        public static Object myLock = new Object();

        // counter variables
        private int folderCount = 0;
        private int fileCount = 0;
        private long byteCount = 0;

        // getters for printing in main
        public int FolderCount { get { return folderCount; } }
        public int FileCount { get { return fileCount; } }
        public long ByteCount { get { return byteCount; } }

        // will count files and directories, starting from the root
        // recursively, and sequentially, starting with files and
        // then calling itself from each subdirectory
        public void RunSequential(DirectoryInfo dir)
        {
            DirectoryInfo[] subDirectories;
            FileInfo[] files;

            try
            {
                subDirectories = dir.GetDirectories();
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

            foreach (var d in subDirectories)
            {
                folderCount++;
                RunSequential(d);
            }

        }

        // will count files and directories, starting from the root
        // recursively, and in parallel, starting with files and
        // then calling itself from each subdirectory
        public void RunParallel(DirectoryInfo dir)
        {
            DirectoryInfo[] subDirectories;
            FileInfo[] files;

            try
            {
                subDirectories = dir.GetDirectories();
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

            Parallel.ForEach(subDirectories, d =>
            {
                lock (myLock)
                {
                    folderCount++;
                }
                RunParallel(d);
            });
        }
    }
}
