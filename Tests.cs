using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;

namespace Kopi
{
    class Tests
    {        
        public Tests(Copyer.LogDelegate a_logDelegate, Copyer.ProgressDelegate a_progressDelegate)
        {
            if (a_logDelegate != null)
            {
                m_logDelegate = a_logDelegate;
            }
            if (a_progressDelegate != null)
            {
                m_progressDelegate = a_progressDelegate;
            }
            try
            {
                m_logDelegate("Starting Tests");
                Prepare();
                Sync();
                Check();
                m_logDelegate("Completed Tests");
            }
            catch(Exception e)
            {
                m_logDelegate("Exception during tests: " + e.ToString());
            }
        }

        private Copyer.LogDelegate m_logDelegate = delegate(string a_str) { };
        private Copyer.ProgressDelegate m_progressDelegate = delegate(int a_progressPercent) { };
        private const string m_source = @"c:\temp\src";
        private const string m_destination = @"c:\temp\dst";

        public void Prepare()
        {
            // clean folders
            try
            {
                Directory.Delete(m_source, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            try
            {
                Directory.Delete(m_destination, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            Directory.CreateDirectory(m_source);
            Directory.CreateDirectory(m_destination);

            DateTime date = new DateTime(2000, 10, 10);
            // add
            CreateFile(Path.Combine(m_source, "dir1", "srcfile1.txt"), 10, date);

            // add same filename but in a different path
            CreateFile(Path.Combine(m_source, "dir2", "srcfile1.txt"), 10, date);
            
            // move
            CreateFile(Path.Combine(m_source, "dir3", "srcfile2.txt"), 100, date);
            CreateFile(Path.Combine(m_destination, "dir4", "srcfile2.txt"), 100, date);

            // remove
            CreateFile(Path.Combine(m_destination, "dir1", "dstfile1.txt"), 10);
            
            // modify
            CreateFile(Path.Combine(m_source, "dir1", "modfile1.txt"), 0);
            CreateFile(Path.Combine(m_destination, "dir1", "modfile1.txt"), 1);

            // remove deep
            CreateFile(Path.Combine(m_destination, "dir1", "dir2", "removeDeep.txt"), 10);
        }

        public void Sync()
        {
            m_barrier = new Barrier(2);
            m_copyer = new Copyer(m_logDelegate, SyncDone, m_progressDelegate);
            Settings settings = new Settings();
            settings.Mappings.Add(new Mapping(m_source, m_destination, false, false, false));
            if (m_copyer.Start(settings, false))
            {
                m_barrier.SignalAndWait();
            }
        }

        public bool Check()
        {
            bool success = true;

            if(!File.Exists(Path.Combine(m_source, "dir1", "srcfile1.txt")))
            {
                Fail("deleted a source file");
                success = false;
            }
            if(!File.Exists(Path.Combine(m_destination, "dir1", "srcfile1.txt")))
            {
                Fail("didn't copy file to destination");
                success = false;
            }
            if (!File.Exists(Path.Combine(m_source, "dir2", "srcfile1.txt")))
            {
                Fail("deleted a source file");
                success = false;
            }
            if (!File.Exists(Path.Combine(m_destination, "dir2", "srcfile1.txt")))
            {
                Fail("didn't copy file to destination");
                success = false;
            }
            if (!File.Exists(Path.Combine(m_destination, "dir3", "srcfile2.txt")))
            {
                Fail("didn't move file to destination");
                success = false;
            }
            if (File.Exists(Path.Combine(m_destination, "dir4", "srcfile2.txt")))
            {
                Fail("didn't move file to destination");
                success = false;
            }
            if(File.Exists(Path.Combine(m_destination, "dir1", "dstfile1.txt")))
            {
                Fail("didn't delete file from destination");
                success = false;
            }
            if(File.Exists(Path.Combine(m_source, "dir1", "dstfile1.txt")))
            {
                Fail("copied destination file to source");
                success = false;
            }
            {
                FileInfo f = new FileInfo(Path.Combine(m_source, "dir1", "modfile1.txt"));
                if (f.Length != 0)
                {
                    Fail("source file size changed");
                    success = false;
                }
            }
            {
                FileInfo f = new FileInfo(Path.Combine(m_destination, "dir1", "modfile1.txt"));
                if (f.Length != 0)
                {
                    Fail("destination file size not the same as source");
                    success = false;
                }
            }
            if (Directory.Exists(Path.Combine(m_destination, "dir1", "dir2")))
            {
                Fail("empty destination folder not removed");
            }

            return success;
        }

        private void SyncDone()
        {
            m_barrier.SignalAndWait();
        }

        private void CreateFile(string a_path, int a_size, DateTime a_lastModifiedTime)
        {
            CreateFile(a_path, a_size);
            if (a_lastModifiedTime != null)
            {
                File.SetLastAccessTimeUtc(a_path, a_lastModifiedTime);
            }
        }

        private void CreateFile(string a_path, int a_size)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(a_path));
            FileStream fs = File.Create(a_path);
            if (a_size > 0)
            {
                int chunkSize = 4096;
                byte[] data = new byte[chunkSize];
                for (int i = 0; i < a_size; i += chunkSize)
                {
                    fs.Write(data, 0, Math.Min(a_size - i, chunkSize));
                }
            }
            fs.Close();
        }

        void Fail(string a_message)
        {
            m_logDelegate("FAIL: " + a_message);
        }

        private Barrier m_barrier;
        private Copyer m_copyer;
    }
}
