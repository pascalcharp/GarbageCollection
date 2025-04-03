using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Collectors
{
    public class CheneyCollector : IGarbageCollector
    {
        public string Name => "Cheney";
        public static int NbPartitions => 2;

        private readonly EnvironmentMemory _memory;

        /* ------- À COMPLÉTER ------- */

        public CheneyCollector(EnvironmentMemory memory, Mutator mutator)
        {
            _memory = memory;

            /* ------- À COMPLÉTER ------- */
        }

        public bool ShouldCollect()
        {
            /* ------- À COMPLÉTER ------- */

            return false;
        }

        public void Collect()
        {
            /* ------- À COMPLÉTER ------- */
        }
    }
}
