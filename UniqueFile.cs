using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kopi
{
    /// <summary>
    /// Used to determine whether two files should be treated as the same file when checking for 
    /// the applicability of a move operation.
    /// </summary>
    class UniqueFile
    {
        public UniqueFile(string a_name, long a_size, DateTime a_lastModifiedTime)
        {
            Name = a_name;
            Size = a_size;
            LastModifiedTime = a_lastModifiedTime;
        }

        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime LastModifiedTime { get; set; }

        // We must override Equals and GetHashCode because the default implementation is essentially a reference equality.
        public override bool Equals(Object a_object)
        {
            if (a_object != null)
            {
                UniqueFile fileKey = a_object as UniqueFile;
                if (fileKey != null)
                {
                    return Name == fileKey.Name && Size == fileKey.Size && LastModifiedTime.Equals(fileKey.LastModifiedTime);
                }
            }
            return false;
        }

        public bool Equals(UniqueFile a_fileKey)
        {
            return Name == a_fileKey.Name && Size == a_fileKey.Size && LastModifiedTime.Equals(a_fileKey.LastModifiedTime);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Size.GetHashCode() ^ LastModifiedTime.GetHashCode();
        }
    }
}
