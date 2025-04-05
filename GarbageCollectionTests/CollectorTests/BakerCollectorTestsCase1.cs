namespace GarbageCollectionTests.CollectorTests ;

public class BakerCollectorTestsCase1 : CollectorTestsEnvironment
{
    [SetUp]
    public new void Setup()
    {
        base.Setup() ;

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

public class BakerCollectorTestsCase2 : CollectorTestsEnvironment
{
    [SetUp]
    public new void Setup()
    {
        base.Setup() ;
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