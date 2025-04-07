using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;

namespace GarbageCollection.Collectors
{
    public class CompactCollector(EnvironmentMemory memory, Mutator mutator) : BakerCollector(memory, mutator)
    {
        public new string Name => "Compact" ;


        public new bool ShouldCollect()
        {
            /* ------- À COMPLÉTER ------- */

            return false ;
        }

        private SortedDictionary<int, int> BuildNewLocation()
        {
            SortedDictionary<int, int> newLocation = new() ;

            int free = Memory.WorkingPartition.StartAddress ;
            foreach (var address in Unreached)
            {
                newLocation.Add(address, free) ;
            }

            foreach (var oldAddress in newLocation.Keys.ToList())
            {
                newLocation[oldAddress] = free ;
                if (!Memory.TryDereference(oldAddress, out CollectableObject? obj))
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
            foreach (var oldAddress in newLocation.Keys.ToList())
            {
                var newAdress = newLocation[oldAddress] ;
                if (newAdress != oldAddress)
                {
                    if (!Memory.TryDereference(oldAddress, out CollectableObject? obj))
                    {
                        throw new Exception($"Compact: Failed to dereference memory at {oldAddress}") ;
                    }
                    Memory.Free(oldAddress) ;
                    Unreached.Remove(oldAddress) ;
                    Memory.Store(obj, newAdress) ;
                    Unreached.Add(newAdress) ;
                    
                    if (Memory.RootReferences.Contains(oldAddress)) Memory.UpdateRootReference(oldAddress, newAdress) ;
                }
                else
                {
                    newLocation.Remove(oldAddress) ;
                }
            }

            foreach (var address in Unreached)
            {
                if (!Memory.TryDereference(address, out CollectableObject? obj))
                {
                    throw new Exception($"Compact: Failed to dereference memory at {address}") ;
                }
                
                foreach (var oldAddress in newLocation.Keys)
                {
                    try
                    {
                        obj.UpdateReference(oldAddress, newLocation[oldAddress]) ;
                    }
                    catch (Exception e)
                    {
                        if (e.Message != $"Reference for address={oldAddress} not in references.") throw ; 
                    }

                }
            }
        }

        public override void Collect()
        {
            base.Collect() ;
            Compact() ;
        }
    }
}