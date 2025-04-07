using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection
{
    public class OutOfSpaceException(string message) : Exception(message) { }

    public class Partition
    {
        public int StartAddress { get; }
        public int Size { get; }
        public int FreeSpace { get; private set; }

        private readonly bool[] _cells;
        private readonly Dictionary<int, CollectableObject> _objects;

        private readonly Mutator _mutator;

        public Partition(int startAddress, int size, Mutator mutator)
        {
            StartAddress = startAddress;
            Size = size;
            FreeSpace = Size;
            _mutator = mutator;

            _cells = new bool[Size];
            Array.Fill(_cells, true, 0, Size);
            _objects = [];
        }

        public int GetObjectAddress(CollectableObject obj)
        {
            foreach (var o in _objects)
            {
                if (o.Value == obj) return o.Key + StartAddress ;
            }

            return -1 ; 
        }

        public bool TryDereference(int address, [NotNullWhen(true)] out CollectableObject? referencedObject)
        {
            ValidateAddress(address);

            int firstCell = address - StartAddress;
            return _objects.TryGetValue(firstCell, out referencedObject);
        }

        public void Add(CollectableObject newObject)
        {
            bool added = false;
            int lastFirstCell = Size - newObject.Size;
            int currentFirstCell = 0;

            // Add object in a first-fit manner.
            while (!added && currentFirstCell <= lastFirstCell)
            {
                int nextFirstCell = currentFirstCell;
                for (int i = 0; i < newObject.Size; i++)
                {
                    if (!_cells[currentFirstCell + i])
                    {
                        nextFirstCell += i + 1;
                        break;
                    }
                }

                if (nextFirstCell == currentFirstCell)
                {
                    added = true;
                    Array.Fill(_cells, false, currentFirstCell, newObject.Size);
                    _objects.Add(currentFirstCell, newObject);
                    FreeSpace -= newObject.Size;

                    _mutator.PostProcessAdd(StartAddress + currentFirstCell);
                }
                else
                {
                    currentFirstCell = nextFirstCell;
                }
            }

            if (!added)
            {
                throw new OutOfSpaceException($"Not enough space left to add object name={newObject.Name} into partition.");
            }
        }
        
        public void Store(CollectableObject existingObject, int address)
        {
            ValidateAddress(address);

            int firstCell = address - StartAddress;
            int lastCell = firstCell + existingObject.Size - 1;
            if (lastCell >= Size)
            {
                throw new OutOfSpaceException($"Object of size={existingObject.Size} does not fit into partition at address={address}.");
            }

            for (int i = firstCell; i <= lastCell; i++)
            {
                if (!_cells[i])
                {
                    throw new Exception($"Overriding memory at address={address} with object of size={existingObject.Size}.");
                }
            }

            Array.Fill(_cells, false, firstCell, existingObject.Size);
            _objects.Add(firstCell, existingObject);
            FreeSpace -= existingObject.Size;

            _mutator.PostProcessStore(existingObject);
        }

        public void Free(int address)
        {
            if (TryDereference(address, out CollectableObject? referencedObject))
            {
                int firstCell = address - StartAddress;
                Array.Fill(_cells, true, firstCell, referencedObject.Size);
                _objects.Remove(firstCell);
                FreeSpace += referencedObject.Size;

                _mutator.PostProcessFree(referencedObject);
            }
            else
            {
                throw new Exception($"No object stored at address={address}.");
            }
        }

        public void Clear()
        {
            foreach (int cell in _objects.Keys)
            {
                Free(StartAddress + cell);
            }
        }

        private void ValidateAddress(int address)
        {
            if (address < StartAddress || address > StartAddress + Size - 1)
            {
                throw new Exception($"Invalid address={address} for partition of start={StartAddress} and size={Size}.");
            }
        }

        public override string ToString()
        {
            String objectsString = "" ;

            if (_objects.Count > 0)
            {
                foreach (var (adresse, obj) in _objects)
                {
                    objectsString += adresse + ":  <" + obj.Name + ", " + obj.Size + ">\n" ;
                }
            }
            else objectsString += "Vide\n";
            return $"Start: {StartAddress}\nTaille: {Size}\nLibre: {FreeSpace}\nObjets: {objectsString}" ; 
        }
    }
}
