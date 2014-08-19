using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kopi
{
    /// <summary>
    /// A set of updates to be applied to the destination directory tree, after comparing it to a source directory tree.
    /// </summary>
    class Updates
    {
        public Updates(bool a_backup)
        {
            m_backup = a_backup;
        }

        public void Copy(string a_sourcePath, string a_destinationPath)
        {
            // add to list of additions
            Addition addition = new Addition(a_sourcePath, a_destinationPath);
            m_additions.Add(addition);

            // keep a record of the addition indexed by unique properties of the file
            UniqueFile key = new UniqueFile(addition.Name, addition.Size, addition.LastModifiedTime);
            PotentialMove potentialMove;
            if (!m_potentialMoves.TryGetValue(key, out potentialMove))
            {
                potentialMove = new PotentialMove();
                m_potentialMoves.Add(key, potentialMove);
            }
            potentialMove.Add(addition.DestinationPath);
        }

        public void DeleteDestination(string a_destinationPath)
        {
            Deletion deletion = new Deletion(a_destinationPath);
            m_deletions.Add(deletion);

            // keep a record of the deletion indexed by unique properties of the file
            UniqueFile key = new UniqueFile(deletion.Name, deletion.Size, deletion.LastModifiedTime);
            PotentialMove potentialMove;
            if (!m_potentialMoves.TryGetValue(key, out potentialMove))
            {
                potentialMove = new PotentialMove();
                m_potentialMoves.Add(key, potentialMove);
            }
            potentialMove.Remove();
        }

        public void Modify(string a_sourcePath, string a_destinationPath)
        {
            m_modifications.Add(new Modification(a_sourcePath, a_destinationPath));
        }

        public void AddFolder(string a_path)
        {
            m_addFolders.Add(new AddFolder(a_path));
        }

        public void RemoveFolder(string a_path)
        {
            m_removeFolders.Add(new RemoveFolder(a_path));
        }

        public long GetBytesRequired()
        {
            long bytes = 0;
            foreach (Modification modification in m_modifications)
            {
                bytes += modification.GetBytesRequired(m_backup);
            }
            foreach (Addition addition in m_additions)
            {
                UniqueFile key = new UniqueFile(addition.Name, addition.Size, addition.LastModifiedTime);
                if (!m_potentialMoves[key].IsValid())
                {
                    bytes += addition.Size;
                }
            }
            foreach (Deletion deletion in m_deletions)
            {
                if (!m_backup)
                {
                    UniqueFile key = new UniqueFile(deletion.Name, deletion.Size, deletion.LastModifiedTime);
                    if (!m_potentialMoves[key].IsValid())
                    {
                        bytes -= deletion.Size;
                    }
                }
            }
            return bytes;
        }

        public long GetBytesToBeCopied()
        {
            long bytes = 0;
            foreach (Modification modification in m_modifications)
            {
                bytes += modification.GetBytesRequired(m_backup);
            }
            foreach (Addition addition in m_additions)
            {
                UniqueFile key = new UniqueFile(addition.Name, addition.Size, addition.LastModifiedTime);
                if (!m_potentialMoves[key].IsValid())
                {
                    bytes += addition.Size;
                }
            }
            return bytes;
        }

        public void Execute(Copyer.ProgressDelegate a_progressDelegate)
        {
            long totalBytes = GetBytesToBeCopied() + 1; // Add to avoid divide by 0. It's ok to only report 99% because we set 100% at the end.
            long bytesSoFar = 0;
            a_progressDelegate((int)(bytesSoFar * 100 / totalBytes));

            foreach (Modification modification in m_modifications)
            {
                modification.Execute(m_backup);
                bytesSoFar += modification.GetBytesRequired(m_backup);
                a_progressDelegate((int)(bytesSoFar * 100 / totalBytes));
            }

            foreach (Addition addition in m_additions)
            {
                // copy files that aren't part of a move
                UniqueFile key = new UniqueFile(addition.Name, addition.Size, addition.LastModifiedTime);
                if (!m_potentialMoves[key].IsValid())
                {
                    addition.Execute();
                    a_progressDelegate((int)(bytesSoFar * 100 / totalBytes));
                    bytesSoFar += addition.Size;
                }
            }

            foreach (Deletion deletion in m_deletions)
            {
                // check for valid moves, and either do the move or the delete
                UniqueFile key = new UniqueFile(deletion.Name, deletion.Size, deletion.LastModifiedTime);
                PotentialMove potentialMove = m_potentialMoves[key];
                if (potentialMove.IsValid())
                {
                    deletion.Move(potentialMove.MoveToPath);
                }
                else
                {
                    deletion.Execute(m_backup);
                }
            }

            foreach (AddFolder addFolder in m_addFolders)
            {
                addFolder.Execute();
            }

            foreach (RemoveFolder removeFolder in m_removeFolders)
            {
                removeFolder.Execute();
            }

            a_progressDelegate(100);
        }

        private bool m_backup;
        private Dictionary<UniqueFile, PotentialMove> m_potentialMoves = new Dictionary<UniqueFile, PotentialMove>();
        private List<Addition> m_additions = new List<Addition>();
        private List<Deletion> m_deletions = new List<Deletion>();
        private List<Modification> m_modifications = new List<Modification>();
        private List<AddFolder> m_addFolders = new List<AddFolder>();
        private List<RemoveFolder> m_removeFolders = new List<RemoveFolder>();
    }
}
