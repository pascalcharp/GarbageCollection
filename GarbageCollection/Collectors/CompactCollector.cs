using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Collectors
{
    public class CompactCollector(EnvironmentMemory memory, Mutator mutator) : BakerCollector(memory, mutator)
    {
        public new string Name => "Compact";
        
        
        public new bool ShouldCollect()
        {
            /* ------- À COMPLÉTER ------- */

            return false;
        }

        private SortedDictionary<int, int> BuildNewLocation()
        {
            SortedDictionary<int, int> newLocation = new () ;
            
            int free = _memory.WorkingPartition.StartAddress ;
            foreach (var address in _unreached)
            {
               newLocation.Add(address, free) ; 
            }

            foreach (var oldAddress in newLocation.Keys)
            {
                newLocation[oldAddress] = free ;
                if (!_memory.TryDereference(oldAddress, out CollectableObject? obj))
                {
                    throw new Exception($"Failed to dereference memory at {oldAddress}") ;
                }
                free += obj.Size ;
            }
            return newLocation ;
        }

        private void Compact()
        {
            var newLocation = BuildNewLocation() ;
            foreach (var oldAddress in newLocation.Keys)
            {
                if (!_memory.TryDereference(oldAddress, out CollectableObject? obj))
                {
                    throw new Exception($"Failed to dereference memory at {oldAddress}") ;
                } 
                _memory.Free(oldAddress) ;
                _memory.Store(obj, newLocation[oldAddress]) ;
                _mutator.PostProcessMoved(oldAddress, newLocation[oldAddress]) ;
            }
        }

        public new void Collect()
        {
            base.Collect() ;
            Compact() ;
        }
    }
}
