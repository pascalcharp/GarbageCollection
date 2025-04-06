using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;

namespace GarbageCollection.Collectors
{
    public class BakerCollector : IGarbageCollector
    {
        public string Name => "Baker" ;
        public static int NbPartitions => 1 ;

        protected readonly EnvironmentMemory _memory ;
        protected readonly Mutator _mutator ;

        protected HashSet<int> _free ;
        protected HashSet<int> _unreached ;

        protected void OnAdded(int address)
        {
            _unreached.Add(address) ;
        }

        public BakerCollector(EnvironmentMemory memory, Mutator mutator)
        {
            _memory = memory ;
            _mutator = mutator ;
            _mutator.Added += OnAdded ;

            _unreached = new HashSet<int>() ;
            _free = new HashSet<int>() ;
        }

        public bool ShouldCollect()
        {
            /* ------- À COMPLÉTER ------- */

            return false ;
        }

        protected HashSet<int> BuildUnscanned()
        {
            HashSet<int> unscanned = new HashSet<int>() ;
            foreach (var r in _memory.RootReferences)
            {
                _unreached.Remove(r) ;
                unscanned.Add(r) ;
            }

            return unscanned ;
        }

        protected CollectableObject GetObjectFromAddress(int current)
        {
            if (!_memory.TryDereference(current, out CollectableObject? obj))
            {
                throw new Exception($"Unable to dereference {current} in GetObjectFromAddress.") ;
            }

            return obj ;
        }

        protected void ReleaseMemory()
        {
            foreach (var r in _free)
            {
                _memory.Free(r) ;
            }

            _free.Clear() ;
        }

        protected HashSet<int> Scan()
        {
            var unscanned = BuildUnscanned() ;
            HashSet<int> scanned = new HashSet<int>() ;

            while (unscanned.Count > 0)
            {
                var current = unscanned.First() ;
                unscanned.Remove(current) ;
                scanned.Add(current) ;

                foreach (var r in GetObjectFromAddress(current).References)
                {
                    if (_unreached.Contains(r))
                    {
                        _unreached.Remove(r) ;
                        unscanned.Add(r) ;
                    }
                }
            } 
            return scanned ;
        }

        public void Collect()
        {
            HashSet<int> scanned = Scan() ;
            _free.UnionWith(_unreached) ;
            _unreached = new HashSet<int>(scanned) ;
            ReleaseMemory() ;
        }
    }
}