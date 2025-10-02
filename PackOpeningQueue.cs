using System.Collections.Generic;

namespace TCGStreamPacks;

public class PackOpeningQueue
{
    private readonly Dictionary<ECollectionPackType, Queue<string>> _packTypeQueues = new();
    public string CurrentPackOpener { get; private set; }

    public void EnqueuePackOpening(ECollectionPackType packType, string username)
    {
        if (!_packTypeQueues.ContainsKey(packType))
            _packTypeQueues[packType] = new Queue<string>();
        _packTypeQueues[packType].Enqueue(username);
    }

    public string DequeuePackOpening(ECollectionPackType packType)
    {
        if (_packTypeQueues.ContainsKey(packType) && _packTypeQueues[packType].Count > 0)
        {
            CurrentPackOpener = _packTypeQueues[packType].Dequeue();
            return CurrentPackOpener;
        }
        CurrentPackOpener = null;
        return null;
    }
}