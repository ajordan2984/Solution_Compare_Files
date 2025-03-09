using System;

namespace Project_Compare_Files
{
    public class FileInfoHolder
    {
        private DateTime _Modified;
        private string _FileDirectory;
        
        
        public DateTime Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                _Modified = value;
            }
        }

        public string FileDirectory
        {
            get
            {
                return _FileDirectory;
            }
            set
            {
                _FileDirectory = value;
            }
        }

        public FileInfoHolder(string fileDirectory, DateTime fileModified)
        {
            _Modified = fileModified;
            _FileDirectory = fileDirectory;
        }
    }
}
