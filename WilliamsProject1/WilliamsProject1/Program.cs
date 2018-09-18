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
            var singleCounter = new SingleThreadCounter();
            var start = DateTime.Now;
            singleCounter.Run("C:\\");
            var end = DateTime.Now;
            Console.WriteLine(end - start + "s");
            Console.WriteLine(singleCounter.FolderCount + " folders");
            Console.WriteLine(singleCounter.FileCount + " files");
            Console.WriteLine(singleCounter.ByteCount + " bytes");
        }
    }

    public class SingleThreadCounter
    {

        // counter variables
        private int folderCount = 0;
        private int fileCount = 0;
        private long byteCount = 0;

        // getters for printing in main
        public int FolderCount {get {return folderCount; } }
        public int FileCount { get { return fileCount; } }
        public long ByteCount { get { return byteCount; } }


        public void Run(string root)
        {
            
            var dir = new DirectoryInfo(root);

            try
            {
                foreach (var d in dir.GetDirectories())
                {
                    
                    folderCount++;
                    Console.WriteLine(d.FullName);
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
    }
}
