using GarbageCollection;
using GarbageCollection.Collectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Whether to show collection statistics or not.
            const bool DEBUG = false ;

            // Define scenarios and collectors to simulate.
            List<Func<EnvironmentMemory, IGarbageCollector, Mutator, Scenario>> factoryMethods = [
                ScenarioFactory.CreateScenarioA,
                //ScenarioFactory.CreateScenarioB,
                //ScenarioFactory.CreateScenarioC,
            ];
            List<Type> types = [
                typeof(BakerCollector),
                typeof(CompactCollector),
                typeof(CheneyCollector),
            ];

            // Run simulations.
            foreach (Func<EnvironmentMemory, IGarbageCollector, Mutator, Scenario> factoryMethod in factoryMethods)
            {
                foreach (Type type in types)
                {
                    // Initialize dependencies.
                    Mutator mutator = new();
                    int nbPartitions = (int?)type.GetProperty("NbPartitions")?.GetValue(null) ?? throw new Exception($"Unexpected error.");
                    EnvironmentMemory memory = ScenarioFactory.CreateMemory(nbPartitions, mutator);
                    Console.WriteLine($"Memory created with {nbPartitions} partitions") ;
                    IGarbageCollector collector = (IGarbageCollector?)Activator.CreateInstance(type, [memory, mutator]) ?? throw new Exception($"Unexpected error.");
                    Scenario scenario = factoryMethod(memory, collector, mutator);
                    Simulator simulator = new(scenario);

                    // Simulate.
                    Console.WriteLine($"---- {simulator.Name} ----");
                    bool success = simulator.Simulate(DEBUG);

                    // Evaluate.
                    if (success)
                    {
                       
                        Console.WriteLine();
                        Console.WriteLine($"Simulation completed after tick={scenario.Ticks}.");
                        int usedSpace = scenario.Memory.GetTotalUsedSpace();
                        Utils.WritePercentage("Used capacity", usedSpace, scenario.Memory.Capacity);
                        Console.WriteLine();
                        simulator.DisplaySimulationStatistics();
                        
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }
    }
}
