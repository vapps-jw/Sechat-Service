namespace Sechat.Algo;
public static class SortAlgorithms
{
    public static void SechatQuickSort<T, TKey>(this IList<T> list, Func<T, TKey> keySelector, int lo, int hi)
        where TKey : IComparable<TKey>
    {
        if (lo >= hi)
        {
            return;
        }

        var pivotIndex = Partition(list, keySelector, lo, hi);
        SechatQuickSort(list, keySelector, lo, pivotIndex - 1);
        SechatQuickSort(list, keySelector, pivotIndex + 1, hi);
    }

    private static int Partition<T, TKey>(IList<T> list, Func<T, TKey> keySelector, int lo, int hi)
        where TKey : IComparable<TKey>
    {
        var pivot = list[hi];
        var idx = lo - 1;

        for (var i = lo; i < hi; i++)
        {
            var item = keySelector(list[i]);
            var comp = item.CompareTo(keySelector(pivot));
            if (comp <= 0)
            {
                idx++;
                (list[idx], list[i]) = (list[i], list[idx]);
            }
        }
        idx++;
        list[hi] = list[idx];
        list[idx] = pivot;

        return idx;
    }
}
