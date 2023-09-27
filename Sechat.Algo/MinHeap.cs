namespace Sechat.Algo;
public class MinHeap<T> where T : IComparable<T>
{
    public int Lenght { get; private set; } = 0;
    public List<T> Data { get; private set; } = new List<T>();

    public void Insert(T value)
    {
        Data.Add(value);
        HeapifyUp(Lenght);
        Lenght++;
    }

    public bool Delete(out T result)
    {
        result = default;
        if (Lenght == 0)
        {
            return false;
        }

        result = Data[0];
        Lenght--;

        if (Lenght == 0)
        {
            Data.Clear();
            return true;
        }

        Data[0] = Data[Lenght];
        HeapifyDown(0);
        return true;
    }

    private void HeapifyDown(int idx)
    {
        var lIdx = LeftChild(idx);
        var rIdx = RightChild(idx);

        if (idx >= Lenght || lIdx >= Lenght)
        {
            return;
        }

        var lV = Data[lIdx];
        var rV = Data[rIdx];
        var v = Data[idx];
        if (lV.CompareTo(rV) > 0 && v.CompareTo(rV) > 0)
        {
            Data[idx] = rV;
            Data[rIdx] = v;
            HeapifyDown(rIdx);
        }
        else if (rV.CompareTo(lV) > 0 && v.CompareTo(lV) > 0)
        {
            Data[idx] = lV;
            Data[lIdx] = v;
            HeapifyDown(lIdx);
        }
    }

    private void HeapifyUp(int idx)
    {
        if (idx == 0)
        {
            return;
        }
        var p = Parent(idx);
        var parentV = Data[p];
        var v = Data[idx];

        if (parentV.CompareTo(v) > 0)
        {
            Data[idx] = parentV;
            Data[p] = v;
            HeapifyUp(p);
        }
    }

    private int Parent(int idx) => (idx - 1) / 2;
    private int LeftChild(int idx) => (idx * 2) + 1;
    private int RightChild(int idx) => (idx * 2) + 2;
}
