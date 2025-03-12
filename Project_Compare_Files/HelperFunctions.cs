using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project_Compare_Files
{
    public class HelperFunctions
    {
        public string ShortenedPath(string path)
        {
            string[] tokens = null;
            string shortenedPath = "";

            try
            {
                tokens = path.Split('\\');

                if (tokens != null)
                {
                    for (int i = 0; i < tokens.Length - 1; i++)
                    {
                        shortenedPath += tokens[i] + '\\';
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return shortenedPath.TrimEnd('\\');
        }

        public SortedDictionary<string, FileInfoHolder> CheckForChanges(string pathToChangesFile)
        {
            SortedDictionary<string, FileInfoHolder> sortedFiles = new SortedDictionary<string, FileInfoHolder>();

            try
            {
                if (File.Exists(pathToChangesFile))
                {
                    string[] lines = File.ReadAllLines(pathToChangesFile);

                    for (int i = 0; i < lines.Length - 1; i += 2)
                    {
                        var fih = new FileInfoHolder("", DateTime.Parse(lines[i + 1]).ToUniversalTime());
                        sortedFiles.Add(lines[i], fih);
                    }
                }
                else
                {
                    Console.WriteLine($"Cannot find: {pathToChangesFile} | Moving to collect directories and files.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return sortedFiles;
        }

        public List<string> GetAllDirectories(string startingDirectory)
        {
            List<string> allDirectories =
                Directory.GetDirectories(startingDirectory)
                .Where(dir => !dir.Contains("GitHub"))
                .ToList();

            try
            {
                for (int i = 0; i < allDirectories.Count; i++)
                {
                    try
                    {
                        allDirectories.AddRange(Directory.GetDirectories(allDirectories[i]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return allDirectories;
        }

        public SortedDictionary<string, FileInfoHolder> GetAllFiles(List<string> allDirectories)
        {
            SortedDictionary<string, FileInfoHolder> allSortedFiles = new SortedDictionary<string, FileInfoHolder>();
            ConcurrentBag<FileInfoHolder> bagOfAllFiles = new ConcurrentBag<FileInfoHolder>();

            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

            Parallel.ForEach(allDirectories, options, directory =>
            {
                ConcurrentGetFiles(directory, bagOfAllFiles);
            });

            foreach (var fih in bagOfAllFiles)
            {
                if(!allSortedFiles.ContainsKey(fih.FullFilePath))
                {
                    allSortedFiles.Add(fih.FullFilePath, fih);
                }
            }
            
            return allSortedFiles;
        }

        public void ConcurrentGetFiles(string directory, ConcurrentBag<FileInfoHolder> bagOfAllFiles)
        {
            List<string> files = new List<string>();

            try
            {
                files.AddRange(Directory.GetFiles(directory, "*"));

                foreach (string file in files)
                {
                   FileInfo fi = new FileInfo(file);
                   FileInfoHolder fih = new FileInfoHolder(file, fi.LastWriteTimeUtc);
                   bagOfAllFiles.Add(fih);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void CopyFilesFromOneDriveToAnotherDrive(
            SortedDictionary<string, FileInfoHolder> filesFromPcPath,
            SortedDictionary<string, FileInfoHolder> filesFromExternalDrive,
            string _shortPathToFilesOnPc,
            string _shortPathToFilesOnExternal)
        {
            try
            {
                List<Tuple<string, string>> filesToCopy = new List<Tuple<string, string>>();
                
                foreach (string file in filesFromPcPath.Keys)
                {
                    string destinationPathForFile = file.Replace(_shortPathToFilesOnPc, _shortPathToFilesOnExternal);

                    if (!filesFromExternalDrive.ContainsKey(destinationPathForFile))
                    {
                        filesToCopy.Add(new Tuple<string, string>(file, destinationPathForFile));
                    }
                    else
                    {
                        var pcFih = filesFromPcPath[file];
                        var exFih = filesFromExternalDrive[destinationPathForFile];

                        if (pcFih.Modified > exFih.Modified)
                        {
                            filesToCopy.Add(new Tuple<string, string>(file, destinationPathForFile));
                        }
                    }
                }

                ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

                Parallel.ForEach(filesToCopy, options, ftc =>
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ftc.Item2));
                    File.Copy(ftc.Item1, ftc.Item2, true);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void QuarantineFiles(
            SortedDictionary<string, FileInfoHolder> filesFromPcPath,
            SortedDictionary<string, FileInfoHolder> filesFromExternalDrive,
            string _shortPathToFilesOnPc,
            string _shortPathToFilesOnExternal
            )
        {
            try
            {
                List<string> keysToRemove = new List<string>();
                
                foreach (string fileFromExternalDrive in filesFromExternalDrive.Keys)
                {
                    string filePathOnPc = fileFromExternalDrive.Replace(_shortPathToFilesOnExternal, _shortPathToFilesOnPc);

                    if (!filesFromPcPath.ContainsKey(filePathOnPc))
                    {
                        string quarantineFilePath = filePathOnPc.Replace(_shortPathToFilesOnPc, _shortPathToFilesOnExternal + "\\QuarantineFolder");

                        Directory.CreateDirectory(Path.GetDirectoryName(quarantineFilePath));
                        File.Move(fileFromExternalDrive, quarantineFilePath);
                        keysToRemove.Add(fileFromExternalDrive);
                    }
                }

                foreach (string key in keysToRemove)
                {
                    filesFromExternalDrive.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void RecursiveRemoveDirectories(string directory)
        {
            try
            {
                List<string> allDirectories = new List<string>(Directory.GetDirectories(directory));

                if (allDirectories.Count > 0)
                {
                    foreach (string subDirectory in allDirectories)
                    {
                        RecursiveRemoveDirectories(subDirectory);
                    }
                }

                string[] hasFiles = Directory.GetFiles(directory);
                string[] hasSubDirectories = Directory.GetDirectories(directory);

                if (hasFiles.Length == 0 && hasSubDirectories.Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdateChangesFile(string pathToChangesFile, SortedDictionary<string, FileInfoHolder> allSortedFilesFromFromExternalDrive)
        {
            try
            {
                using StreamWriter writetext = new StreamWriter(pathToChangesFile);
                foreach (var file in allSortedFilesFromFromExternalDrive)
                {
                    writetext.WriteLine(file.Key);
                    writetext.WriteLine(file.Value.Modified);
                }
                writetext.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
