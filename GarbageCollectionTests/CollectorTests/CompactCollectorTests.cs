namespace GarbageCollectionTests.CollectorTests ;

public class CompactCollectorTestsCase1 : CollectorTestsEnvironment
{
    
    [SetUp]
    public new void Setup()
    {
        base.Setup("Compact") ;
        
        AddReferencesFromObjectTo(B, [A, G]) ;
        AddReferencesFromObjectTo(C, [G]) ;
        AddReferencesFromObjectTo(E, [A, B, F]) ;
        AddReferencesFromObjectTo(G, [H]) ;
        AddReferencesFromObjectTo(H, [D]) ;

        AddRootReferencesToMemory([B, C]) ;
    }

    [Test]
    public void Collect_ShouldFreeObjects_EF_AndMoveObjects_GH()
    {
        _collector.Collect() ;
        
        Assert.That(_released.Except(_stored).ToList(), Has.Count.EqualTo(2)) ;
        Assert.That(_released.Except(_stored), Has.Exactly(1).EqualTo(E)) ;
        Assert.That(_released.Except(_stored), Has.Exactly(1).EqualTo(F)) ;
        Assert.That(_memory.GetObjectAddress(G), Is.EqualTo(100)) ;
        Assert.That(_memory.GetObjectAddress(H), Is.EqualTo(100 + G.Size)) ;
        
        Assert.That(G.References, Has.Count.EqualTo(1)) ;
        Assert.That(G.References, Has.Exactly(1).EqualTo(_memory.GetObjectAddress(H))) ;
        
        Assert.That(B.References, Has.Exactly(1).EqualTo(_memory.GetObjectAddress(G))) ;
        Assert.That(B.References, Has.Count.EqualTo(2)) ;
        
        Assert.That(C.References, Has.Count.EqualTo(1)) ;
        Assert.That(C.References, Has.Exactly(1).EqualTo(_memory.GetObjectAddress(G))) ;
    }
    
}

public class CompactCollectorTestsCase2 : CollectorTestsEnvironment
{
    
}