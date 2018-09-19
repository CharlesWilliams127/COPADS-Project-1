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
            singleCounter.Run("D:\\");
            var end = DateTime.Now;

            var multiCounter = new SingleThreadCounter();
            var multiStart = DateTime.Now;
            multiCounter.RunParallel("D:\\");
            var multiEnd = DateTime.Now;

            Console.WriteLine("Single: " + (end - start) + "s");
            Console.WriteLine("Single: " + singleCounter.FolderCount + " folders");
            Console.WriteLine("Single: " + singleCounter.FileCount + " files");
            Console.WriteLine("Single: " + singleCounter.ByteCount + " bytes");

            Console.WriteLine("Parallel: " + (multiEnd - multiStart) + "s");
            Console.WriteLine("Parallel: " + multiCounter.FolderCount + " folders");
            Console.WriteLine("Parallel: " + multiCounter.FileCount + " files");
            Console.WriteLine("Parallel: " + multiCounter.ByteCount + " bytes");
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

            try
            {
                foreach (var d in dir.GetDirectories())
                {
                    // TODO: if access is denied on the top level it kicks you out, fix
                    // this isn't the case with a parallel loop since the parallel loop
                    // will handle each directory seperately
                    folderCount++;
                    Run(d.FullName);

                    foreach (var f in d.GetFiles())
                    {
                        fileCount++;
                        byteCount += f.Length;
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void RunParallel(string root)
        {
            var dir = new DirectoryInfo(root);

                Parallel.ForEach(dir.GetDirectories(), d => {
                    try
                    {
                        // TODO: need to put a lock or two in here to prevent deadlock
                        folderCount++;
                        Run(d.FullName);

                        Parallel.ForEach(d.GetFiles(), f =>
                        {
                            fileCount++;
                            byteCount += f.Length;
                        });
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                });
            }
        }
    }
