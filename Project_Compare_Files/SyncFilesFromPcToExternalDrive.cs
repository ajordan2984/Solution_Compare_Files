using System;
using System.Collections.Generic;
using System.IO;

namespace Project_Compare_Files
{
    public class SyncFilesFromPcToExternalDrive
    {
        string _pathToFilesOnPc;
        string _pathToFilesOnExternal;

        string _shortPathToFilesOnPc;
        string _shortPathToFilesOnExternal;

        SortedDictionary<string, FileInfoHolder> allSortedFilesFromPcPath;
        SortedDictionary<string, FileInfoHolder> allSortedFilesFromFromExternalDrive;

        HelperFunctions hf = new HelperFunctions();

        public SyncFilesFromPcToExternalDrive()
        {
            // Empty
        }

        public bool StartSync(
            string PathToFilesOnPc,
            string PathToFilesOnExternal)
        {
            if (Directory.Exists(PathToFilesOnPc))
            {
                _pathToFilesOnPc = PathToFilesOnPc;
            }
            else
            {
                Console.WriteLine($"Sorry the path: {PathToFilesOnPc} does not exist. Please try again.");
                return false;
            }

            if (Directory.Exists(PathToFilesOnExternal))
            {
                _pathToFilesOnExternal = PathToFilesOnExternal;
            }
            else
            {
                Console.WriteLine($"Sorry the path: {PathToFilesOnExternal} does not exist. Please try again.");
                return false;
            }

            string pathA = Path.GetFileName(_pathToFilesOnPc);
            string pathB = Path.GetFileName(_pathToFilesOnExternal);


            if (Path.GetFileName(pathA) != Path.GetFileName(pathB))
            {
                Console.WriteLine($"Sorry the end of path: {_pathToFilesOnPc} does not match the end of path {_pathToFilesOnExternal}. Please try again.");
                return false;
            }

            _shortPathToFilesOnPc = hf.ShortenedPath(_pathToFilesOnPc);
            _shortPathToFilesOnExternal = hf.ShortenedPath(_pathToFilesOnExternal);

            SyncFiles();
            return true;
        }

        public void SyncFiles()
        {
            allSortedFilesFromFromExternalDrive = hf.CheckForChanges($@"{_pathToFilesOnExternal}\Changes.txt");

            if (allSortedFilesFromFromExternalDrive.Count == 0)
            {
                List<string> directoriesFromExternal = hf.GetAllDirectories(_pathToFilesOnExternal);
                allSortedFilesFromFromExternalDrive = hf.GetAllFiles(directoriesFromExternal);
            }

            // Get files from Pc
            List<string> directoriesFromPc = hf.GetAllDirectories(_pathToFilesOnPc);
            allSortedFilesFromPcPath = hf.GetAllFiles(directoriesFromPc);

            hf.CopyFilesFromOneDriveToAnotherDrive(
                allSortedFilesFromPcPath,
                allSortedFilesFromFromExternalDrive,
                _shortPathToFilesOnPc,
                _shortPathToFilesOnExternal
                );

            hf.QuarantineFiles(
            allSortedFilesFromPcPath,
            allSortedFilesFromFromExternalDrive,
            _shortPathToFilesOnPc,
            _shortPathToFilesOnExternal);

            hf.RecursiveRemoveDirectories(_pathToFilesOnExternal);

            hf.UpdateChangesFile($@"{_pathToFilesOnExternal}\Changes.txt", allSortedFilesFromFromExternalDrive);

            return;
        }
    }
}

