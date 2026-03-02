

using System.Collections;

public class Pool<T> : ICollection<T>   where T : class
{

    readonly HashSet<T> pooledItems = [];
    readonly HashSet<T> outItems = [];

    readonly Func<T> factory;
    int pooledSize;
    int totalSize;

    public Pool(Func<T> factory, int initialSize)
    {
        this.factory = factory;
        pooledSize = initialSize;
        totalSize = initialSize;

        for (int i = 0; i < initialSize; i++)
        {
            pooledItems.Add(factory());
        }
    }

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Push(T item) => Add(item);
    public void Add(T item)
    {
        pooledItems.Add(item);
        outItems.Remove(item);

        pooledSize ++;
    }

    public void Clear()
    {
        pooledItems.Clear();
        outItems.Clear();

        pooledSize = 0;
        totalSize = 0;
    }

    public void ClearOut()
    {
        outItems.Clear();
    }

    public bool Contains(T item) => pooledItems.Contains(item);
    public bool ContainsTotal(T item) => pooledItems.Contains(item) || outItems.Contains(item);

    public void CopyTo(T[] array, int arrayIndex)
    {
        int j = 0;
        for (int i = arrayIndex; i < array.Length && j < pooledItems.Count; i++)
        {
            array[i] = pooledItems.ElementAt(j);
            j ++;
        }
    }

    public IEnumerator<T> GetEnumerator() => pooledItems.GetEnumerator();

    public IEnumerator<T> GetEnumeratorTotal() => pooledItems.Concat(outItems).GetEnumerator();
    public T Pop()
    {
        T item = pooledItems.FirstOrDefault(factory());

        if(pooledItems.Remove(item))
            pooledSize --;
        else
            totalSize ++;

        outItems.Add(item);

        return item;
    }

    public T PopRandom()
    {
        T item;
        if(Count > 0) item = pooledItems.ElementAt(Random.Shared.Next() % Count);
        else item = factory();

        pooledItems.Remove(item);
        outItems.Add(item);

        return item;
    }

    public bool Remove(T item)
    {
        bool d = pooledItems.Remove(item);
        bool p = outItems.Remove(item);
        
        if(p) totalSize --;
        else
        if(p) pooledSize --;

        return d || p;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}