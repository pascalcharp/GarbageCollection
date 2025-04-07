using GarbageCollection ;
using GarbageCollection.Collectors ;

namespace GarbageCollectionTests.CollectorTests ;

public class CheneyCollectorSimpleTest
{
    private EnvironmentMemory _memory ; 
    private Mutator _mutator ;
    private CheneyCollector _collector ; 

    private CollectableObject A, B ; 
    
    [SetUp]
    public void Setup()
    {
        _mutator = new Mutator() ;
        _memory = new EnvironmentMemory(1000, 2, _mutator) ;
        _collector = new CheneyCollector(_memory, _mutator) ; 
        
        A = new CollectableObject("A", 100, _mutator) ; 
        B = new CollectableObject("B", 200, _mutator) ;
        
        _memory.Add(A) ;
        _memory.Add(B) ;
        
        _memory.AddRootReferences([_memory.GetObjectAddress(A)]) ;
    }

    [Test]
    public void OnCollect_ObjectAIsCopiedToSecondPartition_ObjectBIsWiped()
    {
        _collector.Collect() ;
        Assert.That(_memory.GetObjectAddress(A), Is.EqualTo(500)) ;
        Assert.That(_memory.GetObjectAddress(B), Is.EqualTo(-1)) ;
        Assert.That(_memory.GetTotalUsedSpace(), Is.EqualTo(100)) ;
    }
}