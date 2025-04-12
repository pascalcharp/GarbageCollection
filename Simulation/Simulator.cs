using GarbageCollection ;
using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;

namespace Simulation
{
    internal record SimulationResults
    {
        public required float PauseCost ;
        public required int FragmentatedSpace ;
        public required float LocalityDistance ;
    }

    internal class Simulator
    {
        public const int STORE_COST = 4 ;
        public const int RELEASE_COST = 2 ;
        public const int REFERENCE_COST = 1 ;

        public string Name { get ; }

        private readonly Scenario _scenario ;

        private readonly HashSet<CollectableObject> _addedObjects ;
        private int _totalCost ;

        public List<SimulationResults>  Results { get ; private set ; } 

        public Simulator(Scenario scenario)
        {
            Name = scenario.Name ;
            _scenario = scenario ;

            _addedObjects = [] ;
            _totalCost = 0 ;

            Results = [] ;  

            _scenario.Mutator.Added += AddObject ;
            _scenario.Mutator.Stored += StoreObject ;
            _scenario.Mutator.Released += ReleaseObject ;
            _scenario.Mutator.ReferenceUpdated += UpdateReference ;
        }


        public bool Simulate(bool debug)
        {
            HashSet<CollectableObject> lastReleasedObjects = [] ;
            for (int i = 1; i <= _scenario.Ticks; i++)
            {
                // Tick.
                try
                {
                    Tick() ;
                }
                catch (OutOfSpaceException exception)
                {
                    Console.WriteLine() ;
                    Console.WriteLine($"Simulation aborted : {exception.Message}") ;
                    return false ;
                }

                // Sync after tick.
                _scenario.Sync() ;
                UpdateSimulationResults() ;

                // Collect if needed.
                if (_scenario.Collector.ShouldCollect())
                {
                    if (debug)
                    {
                        Console.WriteLine() ;
                        Console.WriteLine($"Collecting for tick={i}.") ;
                        int usedSpace = _scenario.Memory.GetTotalUsedSpace() ;
                        Utils.WritePercentage("Used capacity before", usedSpace, _scenario.Memory.Capacity) ;
                    }

                    _scenario.Collector.Collect() ;
                    _scenario.Sync() ;

                    if (debug)
                    {
                        // Assuming every unreachable object is released.
                        HashSet<CollectableObject> releasedObjects =
                            _addedObjects.Except(_scenario.ReachableObjects.Values).ToHashSet() ;
                        HashSet<CollectableObject> releasedByCollection =
                            releasedObjects.Except(lastReleasedObjects).ToHashSet() ;
                        lastReleasedObjects = releasedObjects ;

                        Console.WriteLine($"Released objects : {releasedByCollection.Count}") ;
                        int usedSpace = _scenario.Memory.GetTotalUsedSpace() ;
                        Utils.WritePercentage("Used capacity after", usedSpace, _scenario.Memory.Capacity) ;
                    }
                }
            }

            return true ;
        }

        public void Tick()
        {
            // Old root references.
            List<int> oldRootReferences = _scenario.GetOldRootReferences() ;
            _scenario.Memory.RemoveRootReferences(oldRootReferences) ;

            // Old object references.
            Dictionary<CollectableObject, List<int>> oldReferencesPerObject = _scenario.GetOldReferencesPerObject() ;
            foreach (KeyValuePair<CollectableObject, List<int>> pair in oldReferencesPerObject)
            {
                pair.Key.RemoveReferences(pair.Value) ;
            }

            // New object.
            CollectableObject newObject = _scenario.GetNewObject() ;
            _scenario.Memory.Add(newObject) ;
            _scenario.Sync() ;

            // New root references.
            List<int> newRootReferences = _scenario.GetNewRootReferences() ;
            _scenario.Memory.AddRootReferences(newRootReferences) ;

            // New object references.
            Dictionary<CollectableObject, List<int>> newReferencesPerObject = _scenario.GetNewReferencesPerObject() ;
            foreach (KeyValuePair<CollectableObject, List<int>> pair in newReferencesPerObject)
            {
                pair.Key.AddReferences(pair.Value) ;
            }
        }

        public void UpdateSimulationResults()
        {
            Results.Add(Evaluate()) ; 
            
        }

        public void DisplaySimulationStatistics()
        {
            var frag = new List<float>() ; 
            var pauses = new List<float>() ;
            var localities = new List<float>() ;
            
            foreach (SimulationResults result in Results)
            {
                frag.Add(result.FragmentatedSpace ) ;
                pauses.Add(result.PauseCost ) ;
                localities.Add(result.LocalityDistance) ;
            }
            Console.WriteLine($"Total work by collector: {_totalCost}");
            Console.WriteLine($"Average pause per tick: {pauses.Average()}") ;
            Console.WriteLine($"Average locality per tick: {localities.Average()}") ;
            Console.WriteLine($"Average frag per tick: {frag.Average()}") ;
        }

        public SimulationResults Evaluate()
        {
            // Compute fragmentation.
            int fragmentation = 0 ;
            int lastAddress = _scenario.Memory.WorkingPartition.StartAddress ;
            foreach (int address in _scenario.ReachableObjects.Keys.Order())
            {
                fragmentation += address - lastAddress ;
                lastAddress = address + _scenario.ReachableObjects[address].Size ;
            }

            // Compute distance.
            //int distance = _scenario.Memory.RootReferences.Sum();
            float averageDistance = 0 ;
            if (_scenario.ReachableObjects.Count > 0)
            {
                foreach (KeyValuePair<int, CollectableObject> pair in _scenario.ReachableObjects)
                {
                    if (pair.Value.References.Count != 0)
                    {
                        float meanDistance = 0 ;
                        foreach (int reference in pair.Value.References)
                        {
                            meanDistance += Math.Abs(pair.Key - reference) ;
                        }

                        meanDistance /= pair.Value.References.Count ;
                        averageDistance += meanDistance ;
                    }
                }
                averageDistance /= _scenario.ReachableObjects.Count ;
            }
            // Create results.
            return new SimulationResults()
            {
                PauseCost = _totalCost / (float)_scenario.Ticks,
                FragmentatedSpace = fragmentation,
                LocalityDistance = averageDistance,
            } ;
        }

        private void AddObject(int address)
        {
            if (!_scenario.Memory.TryDereference(address, out CollectableObject? newObject))
            {
                throw new Exception($"Added object cannot be found at address={address}.") ;
            }

            if (!_addedObjects.Add(newObject))
            {
                throw new Exception($"Object name={newObject.Name} already added.") ;
            }

            _scenario.Memory.AddRootReferences([address]) ;
        }

        private void StoreObject(CollectableObject existingObject)
        {
            _totalCost += STORE_COST * existingObject.Size ;
        }

        private void ReleaseObject(CollectableObject releasedObject)
        {
            _totalCost += RELEASE_COST * releasedObject.Size ;
        }

        private void UpdateReference()
        {
            _totalCost += REFERENCE_COST ;
        }
    }
}