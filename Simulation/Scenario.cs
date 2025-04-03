using GarbageCollection;
using GarbageCollection.Collectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    internal record GenerationData
    {
        public required int NbTicks;
        public required RandomGenerator Generator;

        public required int MinSize;
        public required List<double> SizeProbabilities;
        public required int SizeFactor;

        public required int MinOldRoot;
        public required List<double> OldRootProbabilities;

        public required int MinNewRoot;
        public required List<double> NewRootProbabilities;

        public required int MinOldPerObject;
        public required List<double> OldPerObjectProbabilities;

        public required int MinNewPerObject;
        public required List<double> NewPerObjectProbabilities;
    }

    internal class Scenario(string name, EnvironmentMemory memory, IGarbageCollector collector, Mutator mutator, GenerationData data)
    {
        public string Name { get; } = name;
        public int Ticks { get; } = data.NbTicks;
        public EnvironmentMemory Memory { get; } = memory;
        public IGarbageCollector Collector { get; } = collector;
        public Mutator Mutator { get; } = mutator;

        public Dictionary<int, CollectableObject> ReachableObjects { get; } = [];

        private readonly GenerationData _data = data;
        private int _nbObjects = 0;

        public void Sync()
        {
            ReachableObjects.Clear();

            // Mark reachable objects from root references.
            Stack<int> unvisited = new(Memory.RootReferences);
            while (unvisited.Count > 0)
            {
                int address = unvisited.Pop();
                if (Memory.TryDereference(address, out CollectableObject? referencedObject))
                {
                    if (ReachableObjects.TryAdd(address, referencedObject))
                    {
                        foreach (int reference in referencedObject.References)
                        {
                            unvisited.Push(reference);
                        }
                    }
                }
                else
                {
                    throw new Exception($"No object found for reference at address={address}.");
                }
            }
        }

        public CollectableObject GetNewObject()
        {
            _nbObjects++;
            string name = _nbObjects.ToString();
            int size = _data.Generator.GenerateCustom(_data.MinSize, _data.SizeProbabilities) * _data.SizeFactor;
            return new(name, size, Mutator);
        }

        public List<int> GetOldRootReferences()
        {
            // Remove from existing root references.
            int count = _data.Generator.GenerateCustom(_data.MinOldRoot, _data.OldRootProbabilities);
            return Pick(count, Memory.RootReferences);
        }

        public List<int> GetNewRootReferences()
        {
            // Add from non existing root references.
            int count = _data.Generator.GenerateCustom(_data.MinNewRoot, _data.NewRootProbabilities);
            List<int> nonRootReferences = ReachableObjects.Keys.Except(Memory.RootReferences).ToList();
            return Pick(count, nonRootReferences);
        }

        public Dictionary<CollectableObject, List<int>> GetOldReferencesPerObject()
        {
            // Remove from existing object references.
            Dictionary<CollectableObject, List<int>> references = [];
            foreach (CollectableObject reachableObject in ReachableObjects.Values)
            {
                int count = _data.Generator.GenerateCustom(_data.MinOldPerObject, _data.OldPerObjectProbabilities);
                references[reachableObject] = Pick(count, reachableObject.References);
            }
            return references;
        }

        public Dictionary<CollectableObject, List<int>> GetNewReferencesPerObject()
        {
            // Add from non existing object references.
            Dictionary<CollectableObject, List<int>> references = [];
            foreach (CollectableObject reachableObject in ReachableObjects.Values)
            {
                int count = _data.Generator.GenerateCustom(_data.MinNewPerObject, _data.NewPerObjectProbabilities);
                List<int> nonObjectReferences = ReachableObjects.Keys.Except(reachableObject.References).ToList();
                references[reachableObject] = Pick(count, nonObjectReferences);
            }
            return references;
        }

        private List<int> Pick(int count, List<int> candidates)
        {
            // Pick without duplicates.
            int clampedCount = Math.Min(count, candidates.Count);
            List<int> references = new(candidates);
            for (int i = clampedCount; i < candidates.Count; i++)
            {
                int index = _data.Generator.GenerateUniform(0, references.Count - 1);
                references.RemoveAt(index);
            }
            return [.. references];
        }
    }
}
