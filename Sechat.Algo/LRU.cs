namespace Sechat.Algo;

public class Node<T> where T : IComparable<T>
{
    public T Value { get; set; }
    public Node<T> Prev { get; set; }
    public Node<T> Next { get; set; }

    public override string ToString() => $"{Value}";
}

public class LRU<K, V> where V : IComparable<V>
{
    public int Capacity { get; private set; } = 0;
    public int Length { get; private set; } = 0;
    public Node<V> Head { get; private set; }
    public Node<V> Tail { get; private set; }
    public Dictionary<K, Node<V>> Lookup { get; set; } = new();
    public Dictionary<Node<V>, K> ReverseLookup { get; set; } = new();

    public LRU(int capacity = 10) => Capacity = capacity;

    public void Update(K key, V value)
    {
        var node = Lookup.TryGetValue(key, out var result) ? result : null;
        if (node is null)
        {
            node = new Node<V> { Value = value };
            Length++;
            Prepend(node);
            TrimCache();
            Lookup.Add(key, node);
            ReverseLookup.Add(node, key);
        }
        else
        {
            Detach(node);
            Prepend(node);
            node.Value = value;
        }
    }

    public V Get(K key)
    {
        var node = Lookup.TryGetValue(key, out var result) ? result : null;
        if (node is null)
        {
            return default;
        }

        Detach(node);
        Prepend(node);

        return node.Value;
    }

    private void Detach(Node<V> node)
    {
        if (node.Prev is not null)
        {
            node.Prev.Next = node.Next;
        }

        if (node.Next is not null)
        {
            node.Next.Prev = node.Prev;
        }

        if (Length == 1)
        {
            Tail = Head = null;
        }

        if (Head == node)
        {
            Head = Head.Next;
        }

        if (Tail == node)
        {
            Tail = Tail.Prev;
        }

        node.Next = null;
        node.Prev = null;
    }

    private void Prepend(Node<V> node)
    {
        if (Head is null)
        {
            Head = Tail = node;
            return;
        }
        node.Next = Head;
        Head.Prev = node;
        Head = node;
    }

    private void TrimCache()
    {
        if (Length <= Capacity)
        {
            return;
        }

        var tail = Tail;
        Detach(tail);

        var key = ReverseLookup[tail];
        _ = Lookup.Remove(key);
        _ = ReverseLookup.Remove(tail);
        Length--;

    }
}
