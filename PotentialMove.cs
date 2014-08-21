using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kopi
{
    /// <summary>
    /// Used to keep track of how many Addition and Deletion operations operate on the same file,
    /// so that ideally a move operation can be used instead of a Deletion and Addition.
    /// If more than one Addition or Deletion operate on the same file, it is not considered safe
    /// to treat one of those as part of a move operation because it would be arbitrary which one
    /// to choose.
    /// </summary>
    class PotentialMove
    {
        public PotentialMove()
        {
        }

        public void Add(string a_moveToPath)
        {
            m_adds++;
            MoveToPath = a_moveToPath;
        }

        public void Remove(string a_moveFromPath)
        {
            m_removes++;
            MoveFromPath = a_moveFromPath;
        }

        public bool IsValid()
        {
            // To do a move, they must have the same unique key (name, size, date), 
            // and also be a single pair of add and remove, and also be on the same drive.
            return m_adds == 1 && m_removes == 1 && MoveFromPath[0] == MoveToPath[0];
        }

        public string MoveFromPath { get; set; }
        public string MoveToPath { get; set; }

        private int m_adds = 0;
        private int m_removes = 0;
    }
}
