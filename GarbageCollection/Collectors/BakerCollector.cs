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
        private Mutator _mutator ;

        private HashSet<int> _free ;
        private HashSet<int> _unreached ;
        

        public BakerCollector(EnvironmentMemory memory, Mutator mutator)
        {
            _memory = memory;
            _mutator = mutator;
        
            
        }

        public bool ShouldCollect()
        {
            /* ------- À COMPLÉTER ------- */

            return false;
        }

        public void Collect()
        {
           HashSet<int> unscanned = new HashSet<int>(_unreached); ;
           HashSet<int> scanned  = new HashSet<int>() ;
           while (unscanned.Count > 0)
           {
               var current = unscanned.First() ;
               unscanned.Remove(current) ;
               scanned.Add(current) ;

               CollectableObject? obj = null ; 
               if (!_memory.TryDereference(current, out obj))
               {
                   throw new Exception($"Unable to dereference {current}") ;
               }

               foreach (var r in obj.References)
               {
                   if (_unreached.Contains(r))
                   {
                       _unreached.Remove(r) ;
                       unscanned.Add(r) ; 
                   }
               } 
           }
           _free.UnionWith(_unreached) ;
           _unreached  = scanned ;
        }
    }
}
