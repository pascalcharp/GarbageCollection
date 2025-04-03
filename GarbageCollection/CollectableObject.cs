﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection
{
    public class CollectableObject
    {
        public string Name { get; }
        public int Size { get; }
        public List<int> References { get; }

        private readonly Mutator _mutator;

        public CollectableObject(string name, int size, Mutator mutator)
        {
            if (size <= 0)
            {
                throw new Exception($"Size must be greater than 0, got size={size}.");
            }

            Name = name;
            Size = size;
            References = [];

            _mutator = mutator;
        }

        public void AddReferences(List<int> newReferences)
        {
            foreach (int newReference in newReferences)
            {
                References.Add(newReference);
            }
        }

        public void RemoveReferences(List<int> oldReferences)
        {
            foreach (int oldReference in oldReferences)
            {
                if (!References.Remove(oldReference))
                {
                    throw new Exception($"Reference for address={oldReference} not in references.");
                }
            }
        }

        public void UpdateReference(int oldReference, int newReference)
        {
            RemoveReferences([oldReference]);
            AddReferences([newReference]);

            _mutator.PostProcessUpdateReference();
        }
    }
}
