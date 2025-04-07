using GarbageCollection ;
using GarbageCollection.Collectors ;

namespace GarbageCollectionTests.CollectorTests ;

public class SequentialCheneyCollectorTest
{
    private EnvironmentMemory _memory ;
    private Mutator _mutator ;
    private CheneyCollector _collector ;

    private CollectableObject A, B, C, D, E, F ;  
    
    protected void AddReferencesFromObjectTo(CollectableObject source, List<CollectableObject> destList)
    {
        var destinations = (from obj in destList select _memory.GetObjectAddress(obj)).ToList() ;
        source.AddReferences(destinations) ;
    }

    protected void AddRootReferencesToMemory(List<CollectableObject> destList)
    {
        var destinations = (from obj in destList select _memory.GetObjectAddress(obj)).ToList() ;
        _memory.AddRootReferences(destinations) ;
    }


    [SetUp]
    public void Setup()
    {
        _mutator = new Mutator() ;
        _memory = new EnvironmentMemory(5000, 2, _mutator) ;
        _collector = new CheneyCollector(_memory, _mutator) ;

        A = new CollectableObject("A", 100, _mutator) ;
        B = new CollectableObject("B", 200, _mutator) ;
        C = new CollectableObject("C", 300, _mutator) ;
        D = new CollectableObject("D", 400, _mutator) ;
        E = new CollectableObject("E", 500, _mutator) ;
        F = new CollectableObject("F", 50, _mutator) ;

        _memory.Add(A) ;
        _memory.Add(B) ;
        _memory.Add(C) ;
        _memory.Add(D) ;
        _memory.Add(E) ;
        
        AddReferencesFromObjectTo(A, [B, D]) ;
        AddReferencesFromObjectTo(C, [E]) ;
        AddReferencesFromObjectTo(E, [C]) ;

        AddRootReferencesToMemory([A]) ;
    }

    [Test]
    public void Collection_Sequence()
    {
        // Collection initiale: ABD sont déplacés dans la deuxième partition, CE sont éliminés
        _collector.Collect() ;
        Assert.That(_memory.GetObjectAddress(A), Is.EqualTo(2500)) ;
        Assert.That(_memory.GetObjectAddress(B), Is.EqualTo(2600)) ;
        Assert.That(_memory.GetObjectAddress(D), Is.EqualTo(2800)) ;
        Assert.That(A.References, Has.Count.EqualTo(2)) ;
        Assert.That(A.References, Has.Exactly(1).EqualTo(2600)) ;
        Assert.That(A.References, Has.Exactly(1).EqualTo(2800)) ;
        Assert.That(_memory.RootReferences.Single(), Is.EqualTo(2500)) ;
        
        Assert.That(_memory.GetObjectAddress(C), Is.EqualTo(-1)) ;
        Assert.That(_memory.GetObjectAddress(E), Is.EqualTo(-1)) ;
        
        // Addition de F et réarrangement des références: root -> F F -> A et on retire A -> B
        _memory.Add(F) ;
        _memory.RemoveRootReferences([2500]) ;
        _memory.AddRootReferences([_memory.GetObjectAddress(F)]) ;
        AddReferencesFromObjectTo(F, [A]) ;
        A.RemoveReferences([_memory.GetObjectAddress(B)]) ;
        
        // Deuxième collection
        _collector.Collect() ;
        
        // Il doit maintenant rester root -> F -> A -> D dans la première partition
        Assert.That(_memory.GetObjectAddress(F), Is.EqualTo(0)) ;
        Assert.That(_memory.GetObjectAddress(A), Is.EqualTo(50)) ;
        Assert.That(_memory.GetObjectAddress(D), Is.EqualTo(150)) ;
        Assert.That(F.References.Single(), Is.EqualTo(50)) ;
        Assert.That(A.References.Single(), Is.EqualTo(150)) ;
        Assert.That(_memory.RootReferences.Single(), Is.EqualTo(0)) ;
    }
}