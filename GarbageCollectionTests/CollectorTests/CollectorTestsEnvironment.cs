using GarbageCollection ;
using GarbageCollection.Collectors ;

namespace GarbageCollectionTests.CollectorTests ;

public class CollectorTestsEnvironment
{
    protected IGarbageCollector _collector ;
    protected EnvironmentMemory _memory ;
    protected Mutator _mutator ;
    protected CollectableObject A, B, C, D, E, F, G, H ;

    protected int _lastAddedAddress = 301 ;
    protected List<CollectableObject> _released = [] ;
    protected Dictionary<CollectableObject, int> _objects = new([]) ;

    protected void OnAdded(int address)
    {
        _lastAddedAddress = address ;
    }

    protected void OnRelease(CollectableObject obj)
    {
        _released.Add(obj) ;
    }

    protected void AddObjectToMemory(CollectableObject obj)
    {
        _memory.Add(obj) ;
        _objects.Add(obj, _lastAddedAddress) ;
        
    }

    protected void AddReferencesFromObjectTo(CollectableObject source, List<CollectableObject> destList)
    {
        var destinations = (from obj in destList select _objects[obj]).ToList (); ;
        source.AddReferences(destinations) ;
    }

    protected void AddRootReferencesToMemory(List<CollectableObject> destList)
    {
        var destinations = (from obj in destList select _objects[obj]).ToList () ;
        _memory.AddRootReferences(destinations) ;
    }
    
    public void Setup(String type)
    {
        _mutator = new Mutator() ;
        _mutator.Added += OnAdded ;
        _mutator.Released += OnRelease ;
        _memory = new EnvironmentMemory(500, 1, _mutator) ;
        _collector = CollectorFactory.GetInstance(type, _memory, _mutator) ; 

        A = new CollectableObject("A", 10, _mutator) ; 
        B = new CollectableObject("B", 20, _mutator) ;
        C = new CollectableObject("C", 30, _mutator) ;
        D = new CollectableObject("D", 40, _mutator) ;
        E = new CollectableObject("E", 50, _mutator) ;
        F = new CollectableObject("F", 60, _mutator) ;
        G = new CollectableObject("G", 70, _mutator) ;
        H = new CollectableObject("H", 80, _mutator) ;

        AddObjectToMemory(A) ;
        AddObjectToMemory(B) ;
        AddObjectToMemory(C) ;
        AddObjectToMemory(D) ;
        AddObjectToMemory(E) ;
        AddObjectToMemory(F) ;
        AddObjectToMemory(G) ;
        AddObjectToMemory(H) ;
        
    }

}