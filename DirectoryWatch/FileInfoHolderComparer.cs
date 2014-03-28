﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryWatch
{
    public class FileCompare : IEqualityComparer<FileInfoHolder>
    {
        public FileCompare() { }

        public bool Equals(FileInfoHolder f1, FileInfoHolder f2)
        {
            return (f1.LastWriteTime == f2.LastWriteTime);
        }

        // Return a hash that reflects the comparison criteria. According to the  
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must 
        // also be equal. Because equality as defined here is a simple value equality, not 
        // reference identity, it is possible that two or more objects will produce the same 
        // hash code. 
        public int GetHashCode(FileInfoHolder fi)
        {
            string s = String.Format("{0}", fi.Name);
            return s.GetHashCode();
        }
    }
}
