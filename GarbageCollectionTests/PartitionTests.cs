using GarbageCollection ;

namespace GarbageCollectionTests ;

public class PartitionTests
{
    private Partition _partition ;
    private Mutator _mutator ;
    private int _lastAdded ;
    private CollectableObject _obj1, _obj2, _obj3, _obj4, _obj5, _obj6, _obj7 ;

    private Dictionary<int, CollectableObject> _objects ;


    private void GetLastAddedAdress(int adress)
    {
        _lastAdded = adress ;
    }

    private void RemoveAddedAdress(CollectableObject obj)
    {
        foreach (var (adress, item) in _objects)
            if (item == obj)
                _objects.Remove(adress) ;
    }

    [SetUp]
    public void Setup()
    {
        _objects = new([]) ;
        _mutator = new Mutator() ;
        _mutator.Added += GetLastAddedAdress ;
        _mutator.Released += RemoveAddedAdress ;

        _partition = new Partition(0, 100, _mutator) ;
        _lastAdded = 101 ;

        _obj1 = new CollectableObject("A", 20, _mutator) ;
        _obj2 = new CollectableObject("B", 40, _mutator) ;
        _obj3 = new CollectableObject("C", 60, _mutator) ;
        _obj4 = new CollectableObject("D", 10, _mutator) ;
        _obj5 = new CollectableObject("E", 50, _mutator) ;
        _obj6 = new CollectableObject("F", 15, _mutator) ;
        _obj7 = new CollectableObject("G", 25, _mutator) ;

    }

    [Test]
    public void OnCreation_PartitionIsFree()
    {
        Assert.That(_partition.FreeSpace, Is.EqualTo(100)) ;
    }

    [Test]
    public void OnAdd_ObjectIsAdded_ToPartition()
    {
        _partition.Add(_obj1) ;
        Assert.That(_partition.FreeSpace, Is.EqualTo(80)) ;
    }

    [Test]
    public void OnAdd_ObjectCanBeFreed_FromPartition()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        Assert.That(_objects.Count, Is.EqualTo(1)) ;
        Assert.That(() => _partition.Free(_objects.Single().Key), Throws.Nothing) ;
        Assert.That(_partition.FreeSpace, Is.EqualTo(100)) ;
    }

    [Test]
    public void OnMultipleAdd_ObjectsAreAdded()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;
        Assert.That(_objects.Count, Is.EqualTo(3)) ;
        Assert.That(_partition.FreeSpace, Is.EqualTo(30)) ;
    }

    [Test]
    public void OnMultipleAddAndRemove_SmallObjectIsAddedToZero()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;
        Assert.That(_objects.Keys, Has.Exactly(1).EqualTo(0)) ;
        _partition.Free(0) ;
        Assert.That(_objects.Keys, Has.None.EqualTo(0)) ;
        _partition.Add(_obj6) ;
        _objects.Add(_lastAdded, _obj6) ;
        Assert.That(_objects.Keys, Has.Exactly(1).EqualTo(0)) ;
        Assert.That(_objects.Count, Is.EqualTo(3)) ;

    }

    [Test]
    public void OnMultipleAddAndRemove_LargeObjectIsAddedToEnd()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;
        Assert.That(_objects.Keys, Has.Exactly(1).EqualTo(0)) ;
        _partition.Free(0) ;
        Assert.That(_objects.Keys, Has.None.EqualTo(0)) ;
        _partition.Add(_obj7) ;
        _objects.Add(_lastAdded, _obj7) ;
        Assert.That(_objects.Keys, Has.Exactly(1).EqualTo(70)) ;
        Assert.That(_objects.Count, Is.EqualTo(3)) ;
    }

    [Test]
    public void OnMultipleAddAndRemove_TooLargeObject_AddThrows()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;
        Assert.That(_objects.Keys, Has.Exactly(1).EqualTo(0)) ;
        _partition.Free(0) ;
        Assert.That(_objects.Keys, Has.None.EqualTo(0)) ;
        Assert.That(() => _partition.Add(_obj5), Throws.InstanceOf<Exception>()) ;

    }

    [Test]
    public void OnClear_PartitionIsEmpty()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;

        _partition.Clear() ;
        Assert.That(_objects.Count, Is.EqualTo(0)) ;
        Assert.That(_partition.FreeSpace, Is.EqualTo(100)) ;
    }

    [Test]
    public void OnStore_WithInvalidAdress_Throws()
    {
        Assert.That(() => _partition.Store(_obj6, 101), Throws.InstanceOf<Exception>()) ;
    }

    [Test]
    public void OnStore_WithValidAdress_ObjectsAreStored()
    {
        _partition.Store(_obj7, 10) ;
        _partition.Store(_obj2, 50) ;
        Assert.That(_partition.FreeSpace, Is.EqualTo(35)) ;
    }

    [Test]
    public void OnStore_WithValidAdress_TooLargeObjects_Throws()
    {
        _partition.Store(_obj7, 10) ;
        _partition.Store(_obj2, 50) ;
        Assert.That(() => _partition.Store(_obj1, 90),
            Throws.InstanceOf<Exception>().With.Message
                .EqualTo("Object of size=20 does not fit into partition at address=90.")) ;
    }

    [Test]
    public void OnStore_WithValidAdress_OverlappingObjects_Throws()
    {
        _partition.Store(_obj7, 10) ;
        _partition.Store(_obj2, 50) ;
        Assert.That(() => _partition.Store(_obj1, 35),
            Throws.InstanceOf<Exception>().With.Message
                .EqualTo("Overriding memory at address=35 with object of size=20.")) ;
    }

    [Test]
    public void AddedObject_CanBeDereferenced()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj6) ;
        _objects.Add(_lastAdded, _obj6) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;
        foreach (var (adress, item) in _objects)
        {
            CollectableObject? obj ;
            Assert.That(_partition.TryDereference(adress, out obj), Is.EqualTo(true)) ;
            Assert.That(obj, Is.EqualTo(item)) ;
        }
    }

    [Test]
    public void InexistantObjects_CannotBeDereferenced()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj6) ;
        _objects.Add(_lastAdded, _obj6) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;
        
        CollectableObject? obj ;
        Assert.That(_partition.TryDereference(47, out obj), Is.EqualTo(false)) ;
        Assert.That(obj, Is.Null);
    }
    
    [Test]
    public void RemovedObjects_CannotBeDereferenced()
    {
        _partition.Add(_obj1) ;
        _objects.Add(_lastAdded, _obj1) ;
        _partition.Add(_obj2) ;
        _objects.Add(_lastAdded, _obj2) ;
        _partition.Add(_obj6) ;
        _objects.Add(_lastAdded, _obj6) ;
        _partition.Add(_obj4) ;
        _objects.Add(_lastAdded, _obj4) ;
        
        _partition.Free(0) ;
        
        CollectableObject? obj ;
        Assert.That(_partition.TryDereference(0, out obj), Is.EqualTo(false)) ;
        Assert.That(obj, Is.Null);
    }

}