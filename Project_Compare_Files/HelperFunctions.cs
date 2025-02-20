using System;
using System.Collections.Generic;
using System.IO;
namespace Project_Compare_Files
{
    static public class HelperFunctions
    {
        static public SortedDictionary<string, string> GetAllFilesFromPath(string startingDirectory)
        {
            List<string> allDirectories = new List<string>(Directory.GetDirectories(startingDirectory));
            SortedDictionary<string, string> directories = new SortedDictionary<string, string>();
            List<string> files = new List<string>();

            for (int i = 0; i < allDirectories.Count; i++)
            {
                try
                {
                    allDirectories.AddRange(Directory.GetDirectories(allDirectories[i]));
                }
                catch (Exception)
                {

                }
            }

            foreach (string directory in allDirectories)
            {
                try
                {
                    files.AddRange(Directory.GetFiles(directory, "*"));

                    foreach (string file in files)
                    {
                        if (!directories.ContainsKey(file))
                        {
                            directories.Add(file, directory);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            return directories;
        }

        static public void CopyFilesFromOneDriveToAnotherDrive(
            SortedDictionary<string, string> filesFromPcPath,
            SortedDictionary<string, string> filesFromExternalDrive,
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public void QuarantineFiles(
            SortedDictionary<string, string> filesFromPcPath,
            SortedDictionary<string, string> filesFromExternalDrive,
            string _shortPathToA,
            string _shortPathToB
            )
        {
            try
            {
                foreach (string fileFromExternalDrive in filesFromExternalDrive.Keys)
                {
                    string filePathOnPc = fileFromExternalDrive.Replace(_shortPathToB, _shortPathToA);

                    if (!filesFromPcPath.ContainsKey(filePathOnPc))
                    {
                        string quarantineFilePath = filePathOnPc.Replace(_shortPathToA, _shortPathToB + "\\QuarantineFolder");

                        Directory.CreateDirectory(Path.GetDirectoryName(quarantineFilePath));
                        File.Move(fileFromExternalDrive, quarantineFilePath);

                        string directoryFileOnExternalDriveWasIn = filesFromExternalDrive[fileFromExternalDrive];

                        if (Directory.Exists(directoryFileOnExternalDriveWasIn))
                        {
                            string[] hasSubDirectories = Directory.GetDirectories(directoryFileOnExternalDriveWasIn);
                            string[] hasFiles = Directory.GetFiles(directoryFileOnExternalDriveWasIn);

                            if (hasSubDirectories.Length == 0 && hasFiles.Length == 0)
                            {
                                Directory.Delete(directoryFileOnExternalDriveWasIn);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
