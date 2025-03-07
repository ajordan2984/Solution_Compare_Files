using System;

namespace Project_Compare_Files
{
    class Program
    {
        static void Main(string[] args)
        {
            SyncFilesFromPcToExternalDrive foo = new SyncFilesFromPcToExternalDrive();

            foo.StartSync(
                @"C:\Users\john\Desktop\My_Pictures",
                @"E:\My_Pictures");
        }
    }
}
