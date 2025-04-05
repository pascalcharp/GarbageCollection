using GarbageCollection ;

namespace GarbageCollectionTests ;

public class EnvironmentMemoryTests
{
    private Mutator _mutator ;
    private EnvironmentMemory _memory ;
    private CollectableObject _obj10, _obj15, _obj20, _obj25, _obj30, _obj35 ; 

    private List<int> _added ;

    private void GetLastAdded(int address)
    {
        _added.Add(address) ;
    }

    [SetUp]
    public void Setup()
    {
        _mutator = new Mutator() ;
        _mutator.Added += GetLastAdded ;
        _added = new List<int>();
        _memory = new EnvironmentMemory(101, 2, _mutator) ;
        _obj10 = new CollectableObject("10", 10, _mutator) ; 
        _obj15 = new CollectableObject("15", 15, _mutator) ;
        _obj20 = new CollectableObject("20", 20, _mutator) ;
        _obj25 = new CollectableObject("25", 25, _mutator) ;
        _obj30 = new CollectableObject("30", 30, _mutator) ;
        _obj35 = new CollectableObject("35", 35, _mutator) ;
    }

    [Test]
    public void OnCreation_TotalCapacity_IsAdjusted()
    {
        Assert.That(_memory.Capacity, Is.EqualTo(100));
    }

    [Test]
    public void OnCreation_FreeMemory_IsCapacity()
    {
        Assert.That(_memory.GetTotalUsedSpace(), Is.EqualTo(0));
    }

    [Test]
    public void OnAdd_ObjectIsAddedToMemory_MutatorIsNotified()
    {
        _memory.Add(_obj10) ;
        Assert.That(_memory.GetTotalUsedSpace(), Is.EqualTo(10));
        CollectableObject? obj ;
        Assert.That(_added.Count, Is.EqualTo(1)) ;
        Assert.That(_memory.TryDereference(_added.Single(), out obj), Is.True) ;
        Assert.That(obj, Is.EqualTo(_obj10)) ;
    }

    [Test]
    public void WorkingPartition_CanBeChanged()
    {
        _memory.WorkingPartition = _memory.Partitions[1] ; 
        _memory.Add(_obj10) ;
        Assert.That(_memory.TryDereference(_added.Single(), out _), Is.True) ;
        Assert.That(_memory.GetTotalUsedSpace(), Is.EqualTo(10));
        Assert.That(_added.Single(), Is.EqualTo(50)) ;
    }

    [Test]
    public void OnAdd_WithLargeObject_Throws_ForAllPartitions()
    {
        Assert.That( () => _memory.Add(new CollectableObject("51", 51, _mutator) ), Throws.InstanceOf<Exception>()) ;
        _memory.WorkingPartition = _memory.Partitions[1] ;
        Assert.That(() => _memory.Add(new CollectableObject("51", 51, _mutator) ), Throws.InstanceOf<Exception>()) ;
    }
    
   
   
}