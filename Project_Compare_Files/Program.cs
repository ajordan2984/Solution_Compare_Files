using System;

namespace Project_Compare_Files
{
    class Program
    {
        static void Main(string[] args)
        {
            SyncFilesFromPcToExternalDrive foo = new SyncFilesFromPcToExternalDrive();

            // C:\Users\john\Documents
            // C:\Users\john\Desktop\My_Pictures

            // E:\Documents
            // E:\My_Pictures

            // C:\Users\john
            // C:\Users\john\Desktop

            foo.StartSync(
                @"C:\Users\john\Desktop\My_Pictures",
                @"F:\My_Pictures",
                @"C:\Users\john\Desktop",
                @"F:",
                true);

            
            Console.WriteLine("Here");
        }
    }
}
