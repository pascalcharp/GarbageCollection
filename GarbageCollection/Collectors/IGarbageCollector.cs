using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Collectors
{
    public interface IGarbageCollector
    {
        string Name { get; }
        static int NbPartitions { get; }

        bool ShouldCollect();
        void Collect();
    }
}
