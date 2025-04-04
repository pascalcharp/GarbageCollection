using GarbageCollection ;
using NUnit.Framework.Constraints ;

namespace GarbageCollectionTests ;

public class Tests
{
    private Mutator _mutator ; 
    private CollectableObject _testObject ;  
    
    [SetUp]
    public void Setup()
    {
        _testObject = new CollectableObject("TestObject", 5, _mutator) ; 
        _mutator = new Mutator();
    }

    [Test]
    public void Constructeur_WithNegativeSize_RaisesException()
    {
        Assert.That(() => new CollectableObject("Bidon", -5, _mutator),
                   Throws.InstanceOf<Exception>()
                        .With.Message.EqualTo("Size must be greater than 0, got size=-5.")); ; 
    }
    
    [Test]
    public void Constructeur_WithZeroSize_RaisesException()
    {
        Assert.That(() => new CollectableObject("Bidon", 0, _mutator),
            Throws.InstanceOf<Exception>()
                .With.Message.EqualTo("Size must be greater than 0, got size=0.")); ; 
    }

    [Test] 
    public void Constructeur_WithLegalSize_CreatesObject()
    {
        CollectableObject collectableObject = new("Bidon", 5, _mutator) ;
        Assert.That(() => collectableObject.ToString(), Is.EqualTo("Nom: Bidon\nTaille: 5\nReferences: []")) ;
    }
    
    [Test] 
    public void AddReference_ToTestObject_ReferenceIsRetrieved()
    {
        _testObject.AddReferences([6]) ;
        Assert.That(() => _testObject.ToString(), Is.EqualTo("Nom: TestObject\nTaille: 5\nReferences: [6]")) ;
    }

    [Test]
    public void RemoveValidReference_FromTestObject_ReferenceIsRemoved()
    {
        _testObject.AddReferences([0, 1, 2, 3]) ;
        _testObject.RemoveReferences([0, 2]) ;
        Assert.That( () => _testObject.ToString(), Is.EqualTo("Nom: TestObject\nTaille: 5\nReferences: [1, 3]")) ;
    }
    
    [Test]
    public void RemoveInvalidReference_FromTestObject_ThrowsException()
    {
        _testObject.AddReferences([0, 1, 2, 3]) ;
        Assert.That( 
            () => _testObject.RemoveReferences([0, 2, 6]),
            Throws.InstanceOf<Exception>()
                .With.Message.EqualTo("Reference for address=6 not in references.") 
            ) ;
        
    }

    [Test]
    public void UpdateReference_OnTestObject_ReferenceIsModified()
    {
        _testObject.AddReferences([0, 1, 2, 3]) ;
        _testObject.UpdateReference(2, 666) ;
        Assert.That(_testObject.References, Contains.Item(666));
        Assert.That(_testObject.References, Has.None.EqualTo(2));
    }
    
    [Test]
    public void UpdateReference_OnTestObject_WithDuplicateReferences_OneReferenceIsModified()
    {
        _testObject.AddReferences([0, 1, 2, 3, 1]) ;
        _testObject.UpdateReference(1, 666) ;
        Assert.That(_testObject.References, Contains.Item(666));
        Assert.That(_testObject.References, Contains.Item(1));
    }
    
    [Test]
    public void UpdateReference_OnTestObject_WithInvalideReference_ThrowsException()
    {
        _testObject.AddReferences([0, 1, 2, 3]) ;
        Assert.That(
            () => _testObject.UpdateReference(5, 666),
            Throws.InstanceOf<Exception>() 
            ) ;
    }
    
}