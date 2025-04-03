using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Collectors
{
    public class BakerCollector : IGarbageCollector
    {
        public string Name => "Baker";
        public static int NbPartitions => 1;

        private readonly EnvironmentMemory _memory;

        /* ------- À COMPLÉTER ------- */

        public BakerCollector(EnvironmentMemory memory, Mutator mutator)
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
