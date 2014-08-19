using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class Modification
    {
        public Modification(string a_sourcePath, string a_destinationPath)
        {
            SourcePath = a_sourcePath;
            DestinationPath = a_destinationPath;
            FileInfo sourceFileInfo = new FileInfo(a_sourcePath);
            SourceSize = sourceFileInfo.Length;
            FileInfo destinationFileInfo = new FileInfo(a_destinationPath);
            DestinationSize = destinationFileInfo.Length;
        }

        public long GetBytesRequired(bool a_backup)
        {
            if(a_backup)
            {
                return SourceSize;
            }
            return SourceSize - DestinationSize;
        }

        public void Execute(bool a_backup)
        {
            if (a_backup)
            {
                // move the old file to recycle bin first
                FileSystem.DeleteFile(DestinationPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            File.Copy(SourcePath, DestinationPath, true);
        }

        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long SourceSize { get; set; }
        public long DestinationSize { get; set; }
    }
}
