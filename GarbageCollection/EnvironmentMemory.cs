using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection
{
    public class EnvironmentMemory
    {
        public int Capacity { get; }
        public List<Partition> Partitions { get; }
        public Partition WorkingPartition { get; set; }
        public List<int> RootReferences { get; }

        private readonly Mutator _mutator;
        private readonly int _partitionSize;

        public EnvironmentMemory(int capacity, int nbPartitions, Mutator mutator)
        {
            if (capacity <= 0)
            {
                throw new Exception($"Capacity must be positive, got capacity={capacity}.");
            }
            if (nbPartitions <= 0)
            {
                throw new Exception($"Nb partitions must be positive, got nb={nbPartitions}.");
            }

            _mutator = mutator;

            // Compute true capacity.
            _partitionSize = capacity / nbPartitions;
            Capacity = nbPartitions * _partitionSize;

            // Split partitions.
            Partitions = new(nbPartitions);
            for (int i = 0; i < nbPartitions; i++)
            {
                int startAddress = i * _partitionSize;
                Partition newPartition = new(startAddress, _partitionSize, _mutator);
                Partitions.Add(newPartition);
            }

            WorkingPartition = Partitions[0];
            RootReferences = [];
        }

        public int GetObjectAddress(CollectableObject obj)
        {
            foreach (var partition in Partitions)
            {
                var result = partition.GetObjectAddress(obj) ;
                if (result != -1) return result; 
            }

            return -1 ; 
        }

        public bool TryDereference(int address, [NotNullWhen(true)] out CollectableObject? referencedObject)
        {
            Partition partition = GetPartitionFromAddress(address);
            return partition.TryDereference(address, out referencedObject);
        }

        public void Add(CollectableObject newObject)
        {
            WorkingPartition.Add(newObject);
        }

        public void Store(CollectableObject existingObject, int address)
        {
            Partition partition = GetPartitionFromAddress(address);
            partition.Store(existingObject, address);
        }

        public void Free(int address)
        {
            Partition partition = GetPartitionFromAddress(address);
            partition.Free(address);
        }
        
        public int GetTotalUsedSpace()
        {
            int usedSpace = 0;
            foreach (Partition partition in Partitions)
            {
                usedSpace += _partitionSize - partition.FreeSpace;
            }
            return usedSpace;
        }

        public void AddRootReferences(List<int> newReferences)
        {
            foreach (int newReference in newReferences)
            {
                RootReferences.Add(newReference);
            }
        }

        public void RemoveRootReferences(List<int> oldReferences)
        {
            foreach (int oldReference in oldReferences)
            {
                if (!RootReferences.Remove(oldReference))
                {
                    throw new Exception($"Reference for address={oldReference} not in root references.");
                }
            }
        }

        public void UpdateRootReference(int oldReference, int newReference)
        {
            RemoveRootReferences([oldReference]);
            AddRootReferences([newReference]);

            _mutator.PostProcessUpdateReference();
        }

        private Partition GetPartitionFromAddress(int address)
        {
            if (address < 0 || address >= Capacity)
            {
                throw new Exception($"Invalid address={address} for memory with capacity={Capacity}.");
            }
            
            // Integer division to map relative position to index.
            int index = address / _partitionSize;
            return Partitions[index];
        }
    }
}
