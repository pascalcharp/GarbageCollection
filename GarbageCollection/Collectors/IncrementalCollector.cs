namespace GarbageCollection.Collectors ;

public class IncrementalCollector : IGarbageCollector
{
    private static int _quantum = 4 ;
    public String Name { get ; } = "Incremental" ;
    public static int NbPartitions  => 1 ; 

    private float _criticalRatio = 0.20f ; 

    private EnvironmentMemory _memory ;
    private Mutator _mutator ;
    private bool _finishCollect ;

    private HashSet<int> _unreached ;
    private HashSet<int> _unscanned ;
    private HashSet<int> _scanned ;

    public IncrementalCollector(EnvironmentMemory memory, Mutator mutator)
    {
        _memory = memory ;
        _mutator = mutator ;
        _mutator.ReferenceAdded += OnAddReference ;
        _mutator.Added += OnAdded ;
        _unreached = new HashSet<int>() ;
        _unscanned = new HashSet<int>() ;
        _scanned = new HashSet<int>() ;

        _finishCollect = false ;
    }

    public bool ShouldCollect()
    {
        if (_memory.WorkingPartition.FreeSpace / (float) _memory.WorkingPartition.Size < _criticalRatio) _finishCollect = true ;
        return true ;
    }

    private void OnAdded(int added)
    {
        _unreached.Add(added) ;
    }

    private void OnAddReference(CollectableObject obj, int reference)
    {
        if (_unreached.Contains(reference))
        {
            var address = _memory.GetObjectAddress(obj) ;
            if (_scanned.Contains(address))
            {
                _scanned.Remove(address) ;
                _unscanned.Add(address) ;
            }
        }
    }

    private CollectableObject GetObjectFromAddress(int current)
    {
        if (!_memory.TryDereference(current, out CollectableObject? obj))
        {
            throw new Exception($"Unable to dereference {current} in GetObjectFromAddress.") ;
        }

        return obj ;
    }

    private void UpdateUnscanned()
    {
        foreach (var reference in _memory.RootReferences)
        {
            if (_unreached.Contains(reference) && !_scanned.Contains(reference))
            {
                _unreached.Remove(reference) ;
                try
                {
                    _unscanned.Add(reference) ;
                }
                catch (Exception _)
                { }
            }
        }
    }

    public void Collect()
    {
        UpdateUnscanned() ;
        if (ResumeScanning())
        {
            ReleaseMemory() ;
            ResetCollector();
        }
    }

    private void ReleaseMemory()
    {
        foreach (var unreachable in _unreached) _memory.Free(unreachable) ;
    }

    private void ResetCollector()
    {
        _unreached.Clear() ;
        _unreached = new HashSet<int>(_scanned) ;
        _scanned.Clear() ;
        _finishCollect = false ;
    }

    private bool ResumeScanning()
    {
        int iteration = 0 ;
        while (_unscanned.Count > 0 && !_finishCollect && iteration < _quantum)
        {
            var current = _unscanned.First() ;
            _unscanned.Remove(current) ;
            _scanned.Add(current) ;

            foreach (var r in GetObjectFromAddress(current).References)
            {
                if (_unreached.Contains(r))
                {
                    _unreached.Remove(r) ;
                    _unscanned.Add(r) ;
                }
            }

            ++iteration ;
        }

        return _unscanned.Count == 0 ;
    }
}