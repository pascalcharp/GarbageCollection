using System.Runtime.InteropServices.ComTypes ;
using GarbageCollection.Collectors ;

namespace GarbageCollectionTests.CollectorTests ;

public class BakerCollectorTests : CollectorTestsEnvironment
{
    [SetUp]
    public void Setup()
    {
        base.Setup("Baker") ;

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
        int previousMemoryUsed = _memory.GetTotalUsedSpace() ; 
        _collector.Collect() ;
        Assert.That(_released, Has.Count.EqualTo(2)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(E)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(F)) ;
        Assert.That(_memory.GetTotalUsedSpace(), Is.EqualTo(previousMemoryUsed - E.Size - F.Size)) ;
    }
}

public class BakerCollectorTestsCase2 : CollectorTestsEnvironment
{
    [SetUp]
    public void Setup()
    {
        base.Setup("Baker") ;
        AddReferencesFromObjectTo(A, [F]) ;
        AddReferencesFromObjectTo(C, [A, F]) ;
        AddReferencesFromObjectTo(E, [F]) ;
        AddReferencesFromObjectTo(F, [A]) ;
        AddReferencesFromObjectTo(G, [C]) ;
        AddReferencesFromObjectTo(H, [C, D, G]) ;
        
        AddRootReferencesToMemory([A, B, D]);
    }

    [Test]
    public void Collect_ShouldFree_Objects_CEGH()
    {
        _collector.Collect() ;
        Assert.That(_released, Has.Count.EqualTo(4)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(C)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(E)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(G)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(H)) ;
    }
}

public class BakerCollectorTestsCase3 : CollectorTestsEnvironment
{
    [SetUp]
    public void Setup()
    {
        base.Setup("Baker") ;
        AddReferencesFromObjectTo(A, [E, F]) ;
        AddReferencesFromObjectTo(B, [C]) ;
        AddReferencesFromObjectTo(C, [D]) ;
        AddReferencesFromObjectTo(D, [H]) ;
        AddReferencesFromObjectTo(G, [B, C, D, F, H]) ;
        
        AddRootReferencesToMemory([A, B, D]) ;
    }

    [Test]
    public void Collect_ShouldFree_Objects_G()
    {
        _collector.Collect() ;
        Assert.That(_released, Has.Count.EqualTo(1)) ;
        Assert.That(_released, Has.Exactly(1).EqualTo(G)) ;
    }
}