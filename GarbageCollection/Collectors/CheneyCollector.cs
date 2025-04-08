using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;

namespace GarbageCollection.Collectors
{
    public class CheneyCollector : IGarbageCollector
    {
        public string Name => "Cheney" ;
        public static int NbPartitions => 2 ;
        public const double CriticalRatio = 0.75 ; 

        private readonly EnvironmentMemory _memory ;
        private int _workingPartitionIndex ;
        private int _free ;


        private readonly Mutator _mutator ;

        private HashSet<int> _objects ;


        public CheneyCollector(EnvironmentMemory memory, Mutator mutator)
        {
            _memory = memory ;
            if (_memory.Partitions.Count != NbPartitions)
                throw new ApplicationException("Must have 2 partitions for Cheney collector.") ;
            _workingPartitionIndex = 1 ;
            TogglePartitions() ;


            _mutator = mutator ;
            _mutator.Added += OnAdded ;

            _objects = new HashSet<int>() ;
        }

        private void OnAdded(int address)
        {
            _objects.Add(address) ;
        }

        private void TogglePartitions()
        {
            _workingPartitionIndex = (_workingPartitionIndex + 1) % NbPartitions ;
            _memory.WorkingPartition = _memory.Partitions[_workingPartitionIndex] ;
            _free = _memory.Partitions[(_workingPartitionIndex + 1) % NbPartitions].StartAddress ;
        }

        private int LookupNewLocation(int address, Dictionary<int, int> newLocation)
        {
            if (!newLocation.ContainsKey(address))
                throw new Exception($"Cheney collector: no existing object at {address}") ;

            if (newLocation[address] == -1)
            {
                newLocation[address] = _free ;
                if (!_memory.TryDereference(address, out CollectableObject? obj))
                    throw new Exception($"Cheney: no object at address {address}") ;
                _memory.Store(obj, _free) ;
                _free += obj.Size ;
            }

            return newLocation[address] ;
        }

        public bool ShouldCollect()
        {
            double freeSpace = _memory.WorkingPartition.FreeSpace ;
               double totalSpace = _memory.WorkingPartition.Size ; 
               var ratio = freeSpace / totalSpace ; 

               return ratio < CriticalRatio ;
        }

        public void Collect()
        {
            Dictionary<int, int> newLocation = new Dictionary<int, int>() ;
            foreach (var o in _objects) newLocation.Add(o, -1) ;

            int unscanned = _free ;
            foreach (var reference in _memory.RootReferences.ToList())
            {
                _memory.UpdateRootReference(reference, LookupNewLocation(reference, newLocation)) ;
            }

            while (unscanned != _free)
            {
                if (!_memory.TryDereference(unscanned, out CollectableObject? obj))
                    throw new Exception($"Cheney: no object at address {unscanned}") ;
                foreach (var reference in obj.References.ToList())
                {
                    obj.UpdateReference(reference, LookupNewLocation(reference, newLocation)) ;
                }

                unscanned += obj.Size ;
            }

            _objects.Clear() ;
            foreach (var (prev, next) in newLocation) if (next != -1) _objects.Add(next) ;
            _memory.WorkingPartition.Clear() ;
            TogglePartitions() ;
        }
    }
}