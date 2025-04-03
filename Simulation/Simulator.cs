using GarbageCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    internal record SimulationResults
    {
        public required int PauseCost;
        public required int FragmentatedSpace;
        public required int LocalityDistance;
    }

    internal class Simulator
    {
        public const int STORE_COST = 4;
        public const int RELEASE_COST = 2;
        public const int REFERENCE_COST = 1;

        public string Name { get; }

        private readonly Scenario _scenario;

        private readonly HashSet<CollectableObject> _addedObjects;
        private int _totalCost;

        public Simulator(Scenario scenario)
        {
            Name = scenario.Name;
            _scenario = scenario;

            _addedObjects = [];
            _totalCost = 0;

            _scenario.Mutator.Added += AddObject;
            _scenario.Mutator.Stored += StoreObject;
            _scenario.Mutator.Released += ReleaseObject;
            _scenario.Mutator.ReferenceUpdated += UpdateReference;
        }

        public bool Simulate(bool debug)
        {
            HashSet<CollectableObject> lastReleasedObjects = [];
            for (int i = 1; i <= _scenario.Ticks; i++)
            {
                // Tick.
                try
                {
                    Tick();
                }
                catch (OutOfSpaceException exception)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Simulation aborted : {exception.Message}");
                    return false;
                }

                // Sync after tick.
                _scenario.Sync();

                // Collect if needed.
                if (_scenario.Collector.ShouldCollect())
                {
                    if (debug)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Collecting for tick={i}.");
                        int usedSpace = _scenario.Memory.GetTotalUsedSpace();
                        Utils.WritePercentage("Used capacity before", usedSpace, _scenario.Memory.Capacity);
                    }
                    
                    _scenario.Collector.Collect();

                    if (debug)
                    {
                        // Assuming every unreachable object is released.
                        HashSet<CollectableObject> releasedObjects = _addedObjects.Except(_scenario.ReachableObjects.Values).ToHashSet();
                        HashSet<CollectableObject> releasedByCollection = releasedObjects.Except(lastReleasedObjects).ToHashSet();
                        lastReleasedObjects = releasedObjects;

                        Console.WriteLine($"Released objects : {releasedByCollection.Count}");
                        int usedSpace = _scenario.Memory.GetTotalUsedSpace();
                        Utils.WritePercentage("Used capacity after", usedSpace, _scenario.Memory.Capacity);
                    }
                }
            }
            return true;
        }

        public void Tick()
        {
            // Old root references.
            List<int> oldRootReferences = _scenario.GetOldRootReferences();
            _scenario.Memory.RemoveRootReferences(oldRootReferences);

            // Old object references.
            Dictionary<CollectableObject, List<int>> oldReferencesPerObject = _scenario.GetOldReferencesPerObject();
            foreach (KeyValuePair<CollectableObject, List<int>> pair in oldReferencesPerObject)
            {
                pair.Key.RemoveReferences(pair.Value);
            }

            // New object.
            CollectableObject newObject = _scenario.GetNewObject();
            _scenario.Memory.Add(newObject);
            _scenario.Sync();

            // New root references.
            List<int> newRootReferences = _scenario.GetNewRootReferences();
            _scenario.Memory.AddRootReferences(newRootReferences);

            // New object references.
            Dictionary<CollectableObject, List<int>> newReferencesPerObject = _scenario.GetNewReferencesPerObject();
            foreach (KeyValuePair<CollectableObject, List<int>> pair in newReferencesPerObject)
            {
                pair.Key.AddReferences(pair.Value);
            }
        }

        public SimulationResults Evaluate()
        {
            // Compute fragmentation.
            int fragmentation = 0;
            int lastAddress = _scenario.Memory.WorkingPartition.StartAddress;
            foreach (int address in _scenario.ReachableObjects.Keys.Order())
            {
                fragmentation += address - lastAddress;
                lastAddress = address + _scenario.ReachableObjects[address].Size;
            }

            // Compute distance.
            int distance = _scenario.Memory.RootReferences.Sum();
            foreach (KeyValuePair<int, CollectableObject> pair in _scenario.ReachableObjects)
            {
                foreach (int reference in pair.Value.References)
                {
                    distance += Math.Abs(pair.Key - reference);
                }
            }

            // Create results.
            return new SimulationResults()
            {
                PauseCost = _totalCost,
                FragmentatedSpace = fragmentation,
                LocalityDistance = distance,
            };
        }

        private void AddObject(int address)
        {
            if (!_scenario.Memory.TryDereference(address, out CollectableObject? newObject))
            {
                throw new Exception($"Added object cannot be found at address={address}.");
            }
            if (!_addedObjects.Add(newObject))
            {
                throw new Exception($"Object name={newObject.Name} already added.");
            }
            _scenario.Memory.AddRootReferences([address]);
        }

        private void StoreObject(CollectableObject existingObject)
        {
            _totalCost += STORE_COST;
        }

        private void ReleaseObject(CollectableObject releasedObject)
        {
            _totalCost += RELEASE_COST;
        }

        private void UpdateReference()
        {
            _totalCost += REFERENCE_COST;
        }
    }
}
