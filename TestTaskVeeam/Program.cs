// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace TestTaskVeeam
{
    class Program
    {
        public string logFilePath  // property
        {
            get { return logFilePath; }   // get method
            set { logFilePath = value; }  // set method
        }
        void Main(string[] args)
        {
            switch (args.Length)
            {
                case 4:
                    {
                        //periodic update
                        while (true)
                        {

                        string rootPath = args[0];
                        string replicaPath = args[1];
                        string synchInterval = args[2];
                        logFilePath = args[3]; 
                        //Check if root path exists
                        bool directoryExists = Directory.Exists(rootPath);
                        if (!directoryExists)
                        {
                            Console.WriteLine("Please write a valid directory for the rootPath");
                            break;
                        }
                        //Check if interval is int
                        try
                        {
                            int synchInt = Int32.Parse(synchInterval);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Please enter an interval in seconds");
                            break;
                        }
                        //check if log file exists
                        if (File.Exists(logFilePath)) 
                        {
                            Console.WriteLine("Please enter an existant log file path with .txt extension");
                            break;
                        }
                        Directory.CreateDirectory(replicaPath);
                        string[] differDocs = CompareTwoFoldersSubFolders(rootPath, replicaPath);
                        DeleteFolders(differDocs);
                        IEnumerable<System.IO.FileInfo> DifferFiles = CompareFoldersFiles(rootPath, replicaPath);
                        DeleteFiles(DifferFiles);
                        CopyFiles(rootPath, replicaPath);
                        Thread.Sleep(Int32.Parse(synchInterval) * 1000);
                        }
                    }
                    break;
                default: Console.WriteLine("Please enter between two and four arguments in this order 'rootPath' 'replicaPath' 'SynchInterval' 'LogFilePath' ");
                    break;  
            }
        }
        void CopyFiles(string pathOne, string pathTwo)
        {
            string[] files = Directory.GetFiles(pathOne);
            foreach (string file in files)
            {
                File.Copy(file, $"{pathTwo}{Path.GetFileName(file)}", true);
                Console.WriteLine($"{file} got copied to replica");
                WriteToLogFile($"{file} got copied to replica");
            }
        }
        void WriteToLogFile(string message)
        {
            using (StreamWriter outputFile = new StreamWriter(logFilePath, true))
            {
                outputFile.WriteLine(message);
            }
        }
         string[] CompareTwoFoldersSubFolders(string pathOne,string pathTwo)
        {
            var rootDirs = Directory.GetDirectories(pathOne, "*", SearchOption.AllDirectories);
            var replicaDirs = Directory.GetDirectories(pathTwo, "*", SearchOption.AllDirectories);
            string[] DifferArray = replicaDirs.Except(rootDirs).ToArray();
            return DifferArray;
        }
         void DeleteFolders(string[] listOfFoldersPaths)
        {
            foreach (string dir in listOfFoldersPaths)
            {
                Directory.Delete(dir, true);
                Console.WriteLine($"{dir} got deleted");
                WriteToLogFile($"{dir} got deleted");
            }
        }
         void DeleteFiles(IEnumerable<System.IO.FileInfo> filesList)
        {
            foreach (var file in filesList)
            {
                File.Delete(file.FullName);
                Console.WriteLine($"{file} got deleted");
                WriteToLogFile($"{file} got deleted");
            }
        }
         IEnumerable<System.IO.FileInfo> CompareFoldersFiles(string pathOne,string pathTwo)
        {
            System.IO.DirectoryInfo dir1 = new System.IO.DirectoryInfo(pathOne);
            System.IO.DirectoryInfo dir2 = new System.IO.DirectoryInfo(pathTwo);

            // Take a snapshot of the file system.  
            IEnumerable<System.IO.FileInfo> list1 = dir1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            IEnumerable<System.IO.FileInfo> list2 = dir2.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            //A custom file comparer defined below  
            FileCompare myFileCompare = new FileCompare();

            // Find the set difference between the two folders.  
            // For this example we only check one way.  
            var queryList1Only = (from file in list2
                                  select file).Except(list1, myFileCompare);
            return queryList1Only;

        }
    }
    // This implementation defines a very simple comparison  
    // between two FileInfo objects. It only compares the name  
    // of the files being compared and their length in bytes.  
    class FileCompare : System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
    {
        public FileCompare() { }

        public bool Equals(System.IO.FileInfo f1, System.IO.FileInfo f2)
        {
            return (f1.Name == f2.Name &&
                    f1.Length == f2.Length);
        }

        // Return a hash that reflects the comparison criteria. According to the
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must  
        // also be equal. Because equality as defined here is a simple value equality, not  
        // reference identity, it is possible that two or more objects will produce the same  
        // hash code.  
        public int GetHashCode(System.IO.FileInfo fi)
        {
            string s = $"{fi.Name}{fi.Length}";
            return s.GetHashCode();
        }
    }
}

