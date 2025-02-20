using System;
using System.Collections.Generic;
using System.IO;

namespace Project_Compare_Files
{
    public class SyncFilesFromPcToExternalDrive
    {
        string _pathA;
        string _pathB;

        string _shortPathToA;
        string _shortPathToB;

        bool _quarantineFilesNotOnPc;

        SortedDictionary<string, string> allSortedFilesFromPcPath;
        SortedDictionary<string, string> allSortedFilesFromFromExternalDrive;

        public SyncFilesFromPcToExternalDrive()   
        {   
            // Empty
        }

        public bool StartSync(
            string PathA,
            string PathB,
            string PathToRemove,
            string PathToAdd,
            bool QuarantineFilesFromExternalDriveNotOnPc)
        {
            if (Directory.Exists(PathA))
            {
                _pathA = PathA;
            }
            else
            {
                Console.WriteLine($"Sorry the path: {PathA} does not exist. Please try again.");
                return false;
            }

            if (Directory.Exists(PathB))
            {
                _pathB = PathB;
            }
            else
            {
                Console.WriteLine($"Sorry the path: {PathB} does not exist. Please try again.");
                return false;
            }

            string pathA = Path.GetFileName(_pathA);
            string pathB = Path.GetFileName(_pathB);


            if (Path.GetFileName(pathA) != Path.GetFileName(pathB))
            {
                Console.WriteLine($"Sorry the path: {Path.GetFileName(_pathA)} does not match {Path.GetFileName(_pathB)}. Please try again.");
                return false;
            }

            _shortPathToA = PathToRemove;
            _shortPathToB = PathToAdd;
            _quarantineFilesNotOnPc = QuarantineFilesFromExternalDriveNotOnPc;

            SyncFiles();
            return true;
        }

        public void SyncFiles()
        {
            allSortedFilesFromPcPath = HelperFunctions.GetAllFilesFromPath(_pathA);
            allSortedFilesFromFromExternalDrive = HelperFunctions.GetAllFilesFromPath(_pathB);

            HelperFunctions.CopyFilesFromOneDriveToAnotherDrive(
                allSortedFilesFromPcPath,
                allSortedFilesFromFromExternalDrive,
                _shortPathToA,
                _shortPathToB
                );


            if (_quarantineFilesNotOnPc)
            {
                HelperFunctions.QuarantineFiles(
                allSortedFilesFromPcPath,
                allSortedFilesFromFromExternalDrive,
                _shortPathToA,
                _shortPathToB);
            }

            return;
        }
    }
}

