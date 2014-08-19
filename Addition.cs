using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class Addition
    {
        public Addition(string a_sourcePath, string a_destinationPath)
        {
            FileInfo fileInfo = new FileInfo(a_sourcePath);
            SourcePath = a_sourcePath;
            DestinationPath = a_destinationPath;
            Name = fileInfo.Name;
            Size = fileInfo.Length;
            LastModifiedTime = fileInfo.LastWriteTimeUtc;
        }

        public void Execute()
        {
            // We don't overwrite because if the destination file existed then it would be a Modification rather than an Addition.
            Directory.CreateDirectory(Path.GetDirectoryName(DestinationPath));
            File.Copy(SourcePath, DestinationPath, false);
        }

        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long Size { get; set; }
        public DateTime LastModifiedTime { get; set; }
    }
}
