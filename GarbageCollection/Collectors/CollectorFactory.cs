using System.Security.Cryptography ;

namespace GarbageCollection.Collectors ;

public class CollectorFactory
{
    
    public static IGarbageCollector GetInstance(String type, EnvironmentMemory memory, Mutator mutator)
    {
        switch (type)
        {
            case "Baker":
                return new BakerCollector(memory, mutator) ;
            case "Compact":
                return new CompactCollector(memory, mutator) ;
            default:
                throw new ArgumentException($"Unknown collector type: {type}");
        }
    }
}