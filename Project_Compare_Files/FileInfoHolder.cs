using System;

namespace Project_Compare_Files
{
    public class FileInfoHolder
    {
        private DateTime _Modified;
        private string _FullFilePath;
        
        
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

        public string FullFilePath
        {
            get
            {
                return _FullFilePath;
            }
            set
            {
                _FullFilePath = value;
            }
        }

        public FileInfoHolder(string fullFilePath, DateTime fileModified)
        {
            _Modified = fileModified;
            _FullFilePath = fullFilePath;
        }
    }
}
