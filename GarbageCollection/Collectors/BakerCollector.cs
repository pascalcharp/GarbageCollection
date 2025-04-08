using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;

namespace GarbageCollection.Collectors
{
    public class BakerCollector : IGarbageCollector
    {
        public virtual string Name => "Baker" ;
        public static int NbPartitions => 1 ;
        public const double CriticalRatio = 0.50 ;

        protected readonly EnvironmentMemory Memory ;
        protected readonly Mutator Mutator ;

        protected HashSet<int> Release ;
        protected HashSet<int> Unreached ;

        protected void OnAdded(int address)
        {
            Unreached.Add(address) ;
        }

        public BakerCollector(EnvironmentMemory memory, Mutator mutator)
        {
            Memory = memory ;
            Mutator = mutator ;
            Mutator.Added += OnAdded ;

            Unreached = new HashSet<int>() ;
            Release = new HashSet<int>() ;
        }

        public virtual bool ShouldCollect()
        {
            
            double freeSpace = Memory.WorkingPartition.FreeSpace ;
            double totalSpace = Memory.WorkingPartition.Size ; 
            var ratio = freeSpace / totalSpace ; 

            return ratio < CriticalRatio ;
        }

        protected HashSet<int> BuildUnscanned()
        {
            HashSet<int> unscanned = new HashSet<int>() ;
            foreach (var r in Memory.RootReferences)
            {
                Unreached.Remove(r) ;
                unscanned.Add(r) ;
            }

            return unscanned ;
        }

        protected CollectableObject GetObjectFromAddress(int current)
        {
            if (!Memory.TryDereference(current, out CollectableObject? obj))
            {
                throw new Exception($"Unable to dereference {current} in GetObjectFromAddress.") ;
            }

            return obj ;
        }

        protected void ReleaseMemory()
        {
            foreach (var r in Release)
            {
                Memory.Free(r) ;
            }

            Release.Clear() ;
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
                    if (Unreached.Contains(r))
                    {
                        Unreached.Remove(r) ;
                        unscanned.Add(r) ;
                    }
                }
            } 
            return scanned ;
        }

        public virtual void Collect()
        {
            HashSet<int> scanned = Scan() ;
            Release.UnionWith(Unreached) ;
            Unreached = new HashSet<int>(scanned) ;
            ReleaseMemory() ;
        }
    }
}