using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ComplianceSheriff.Licensing
{
    public class Allocator
    {
        public int Scans = int.MaxValue;
        public int Users = int.MaxValue;
        public long Ticks = 0;
        public int Pages = 0;
        public int TotalPages = int.MaxValue;
        public StringCollection Modules = new StringCollection();
        public int Servers = 0;
        public int ComplianceDeputyUsers = 0;
    }
}
