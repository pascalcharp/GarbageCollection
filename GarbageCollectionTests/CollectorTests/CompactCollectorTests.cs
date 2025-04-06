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
        
    }
    
}