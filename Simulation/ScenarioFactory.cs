using GarbageCollection;
using GarbageCollection.Collectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    internal static class ScenarioFactory
    {
        public const int SEED = 42;

        public const int MEMORY_CAPACITY = 8192;
        public const int SIZE_FACTOR = 4 ;

        public static EnvironmentMemory CreateMemory(int nbPartitions, Mutator mutator)
        {
            return new(MEMORY_CAPACITY, nbPartitions, mutator);
        }

        public static Scenario CreateScenarioA(EnvironmentMemory memory, IGarbageCollector collector, Mutator mutator)
        {
            /* ------- À COMPLÉTER ------- */

            string name = $"{collector.Name} (A)";
            GenerationData data = new()
            {
                NbTicks = 40,
                Generator = new(SEED),

                MinSize = 1,
                SizeProbabilities = [0.6, 0.3, 0.1],
                SizeFactor = 100,

                MinOldRoot = 0,
                OldRootProbabilities = [0.25, 0.5, 0.15, 0.1],

                MinNewRoot = 0,
                NewRootProbabilities = [0.75, 0.15, 0.1 ],

                MinOldPerObject = 0,
                OldPerObjectProbabilities = [0.3, 0.2, 0.1, 0.1, 0.1, 0.1, 0.1],

                MinNewPerObject = 0,
                NewPerObjectProbabilities = [0.3, 0.2, 0.1, 0.1, 0.1, 0.1, 0.1],
            };

            return new(name, memory, collector, mutator, data);
        }

        public static Scenario CreateScenarioB(EnvironmentMemory memory, IGarbageCollector collector, Mutator mutator)
        {
            /* ------- À COMPLÉTER ------- */

            string name = $"{collector.Name} (B)";
            GenerationData data = new()
            {
                NbTicks = 500,
                Generator = new(SEED),

                MinSize = 1,
                SizeProbabilities = [0.4, 0.3, 0.2, 0.1],
                SizeFactor = SIZE_FACTOR,

                MinOldRoot = 0,
                OldRootProbabilities = [0.1, 0.1, 0.8],

                MinNewRoot = 0,
                NewRootProbabilities = [0.25, 0.5, 0.25],

                MinOldPerObject = 0,
                OldPerObjectProbabilities = [0.3, 0.2, 0.1, 0.1, 0.1, 0.1, 0.1],

                MinNewPerObject = 1,
                NewPerObjectProbabilities = [0.1, 0.2, 0.3, 0.2, 0.1, 0.1],
            };

            return new(name, memory, collector, mutator, data);
        }

        public static Scenario CreateScenarioC(EnvironmentMemory memory, IGarbageCollector collector, Mutator mutator)
        {
            /* ------- À COMPLÉTER ------- */

            string name = $"{collector.Name} (C)";
            GenerationData data = new()
            {
                NbTicks = 100,
                Generator = new(SEED),

                MinSize = 1,
                SizeProbabilities = [1.0],
                SizeFactor = SIZE_FACTOR,

                MinOldRoot = 0,
                OldRootProbabilities = [1.0],

                MinNewRoot = 0,
                NewRootProbabilities = [1.0],

                MinOldPerObject = 0,
                OldPerObjectProbabilities = [1.0],

                MinNewPerObject = 0,
                NewPerObjectProbabilities = [1.0],
            };

            return new(name, memory, collector, mutator, data);
        }
    }
}
