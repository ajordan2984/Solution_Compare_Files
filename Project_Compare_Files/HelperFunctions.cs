using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project_Compare_Files
{
    static public class HelperFunctions
    {
        static public string ShortenedPath(string path)
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

        static public SortedDictionary<string, FileInfoHolder> CheckForChanges(string pathToChangesFile)
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

        static public List<string> GetAllDirectories(string startingDirectory)
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

        static public SortedDictionary<string, FileInfoHolder> GetAllFiles(List<string> allDirectories)
        {
            SortedDictionary<string, FileInfoHolder> allFiles = new SortedDictionary<string, FileInfoHolder>();
            List<string> files = new List<string>();

            foreach (string directory in allDirectories)
            {
                try
                {
                    files.AddRange(Directory.GetFiles(directory, "*"));

                    foreach (string file in files)
                    {
                        if (!allFiles.ContainsKey(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            FileInfoHolder fih = new FileInfoHolder(directory, fi.LastWriteTimeUtc);
                            allFiles.Add(file, fih);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return allFiles;
        }

        static public void CopyFilesFromOneDriveToAnotherDrive(
            SortedDictionary<string, FileInfoHolder> filesFromPcPath,
            SortedDictionary<string, FileInfoHolder> filesFromExternalDrive,
            string _pathAToRemove,
            string _pathBToAdd)
        {
            try
            {
                foreach (string file in filesFromPcPath.Keys)
                {
                    string destinationPathForFile = file.Replace(_pathAToRemove, _pathBToAdd);

                    if (!filesFromExternalDrive.ContainsKey(destinationPathForFile))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPathForFile));
                        File.Copy(file, destinationPathForFile);
                    }
                    else
                    {
                        var pcFih = filesFromPcPath[file];
                        var exFih = filesFromExternalDrive[destinationPathForFile];

                        if (pcFih.Modified > exFih.Modified)
                        {
                            File.Copy(file, destinationPathForFile, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public void QuarantineFiles(
            SortedDictionary<string, FileInfoHolder> filesFromPcPath,
            SortedDictionary<string, FileInfoHolder> filesFromExternalDrive,
            string _shortPathToA,
            string _shortPathToB
            )
        {
            try
            {
                List<string> keysToRemove = new List<string>();
                
                foreach (string fileFromExternalDrive in filesFromExternalDrive.Keys)
                {
                    string filePathOnPc = fileFromExternalDrive.Replace(_shortPathToB, _shortPathToA);

                    if (!filesFromPcPath.ContainsKey(filePathOnPc))
                    {
                        string quarantineFilePath = filePathOnPc.Replace(_shortPathToA, _shortPathToB + "\\QuarantineFolder");

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

        static public void RecursiveRemoveDirectories(string directory)
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

        static public void UpdateChangesFile(string pathToChangesFile, SortedDictionary<string, FileInfoHolder> allSortedFilesFromFromExternalDrive)
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
