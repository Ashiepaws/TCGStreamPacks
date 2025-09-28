using System.Collections.Generic;

namespace TCGStreamPacks;

public class PackOpeningQueue
{
    private readonly Dictionary<ECollectionPackType, Queue<string>> _packTypeQueues = new();

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
            return _packTypeQueues[packType].Dequeue();
        }
        return null;
    }
}