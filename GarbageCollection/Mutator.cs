using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection
{
    public class Mutator
    {
        public event Action<int>? Added;
        public event Action<CollectableObject>? Stored;
        public event Action<CollectableObject>? Released;
        public event Action? ReferenceUpdated;
        
        public event Action<int, int>? Moved;

        public void PostProcessAdd(int address)
        {
            Added?.Invoke(address);
        }

        public void PostProcessStore(CollectableObject existingObject)
        {
            Stored?.Invoke(existingObject);
        }

        public void PostProcessFree(CollectableObject releasedObject)
        {
            Released?.Invoke(releasedObject);
        }

        public void PostProcessUpdateReference()
        {
            ReferenceUpdated?.Invoke();
        }

        public void PostProcessMoved(int prev, int next)
        {
            Moved?.Invoke(prev, next) ;
        }
    }
}
