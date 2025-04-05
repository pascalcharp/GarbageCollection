using GarbageCollection ;
using GarbageCollection.Collectors ;
using NUnit.Framework.Constraints ;

namespace GarbageCollectionTests ;

public class BakerCollectorTests : CollectorTestsEnvironment
{
    
    [SetUp]
    public new void Setup()
    {
        base.Setup();
        
        AddReferencesFromObjectTo(B, [A, G]) ;
        AddReferencesFromObjectTo(C, [G]) ;
        AddReferencesFromObjectTo(E, [A, B, F]) ;
        AddReferencesFromObjectTo(G, [H]) ;
        AddReferencesFromObjectTo(H, [D]) ;

       AddRootReferencesToMemory([B, C]) ;
    }

    [Test]
    public void Collect_ShouldFree_Objects_E_F()
    {
        _collector.Collect() ;
        Assert.That(_released, Has.Count.EqualTo(2)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(E)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(F)) ;
    }
}