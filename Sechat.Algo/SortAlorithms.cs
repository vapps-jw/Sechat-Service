namespace Sechat.Algo;
public static class SortAlgorithms
{
    public static void SechatMergeSort<T, TKey>(this IList<T> list, Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        var length = list.Count;
        if (length <= 1)
        {
            return;
        }

        var middle = length / 2;
        var leftArray = new T[middle];
        var rightArray = new T[length - middle];

        var i = 0;
        var j = 0;

        for (; i < length; i++)
        {
            if (i < middle)
            {
                leftArray[i] = list[i];
            }
            else
            {
                rightArray[j] = list[i];
                j++;
            }
        }
        SechatMergeSort(leftArray, keySelector);
        SechatMergeSort(rightArray, keySelector);
        Merge(leftArray, rightArray, list, keySelector);
    }

    private static void Merge<T, TKey>(T[] left, T[] right, IList<T> list, Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        var leftSize = list.Count / 2;
        var rightSize = list.Count - leftSize;
        int i = 0, l = 0, r = 0;

        while (l < leftSize && r < rightSize)
        {
            if (keySelector(left[l]).CompareTo(keySelector(right[r])) < 0)
            {
                list[i] = left[l];
                i++;
                l++;
            }
            else
            {
                list[i] = right[r];
                i++;
                r++;
            }
        }
        while (l < leftSize)
        {
            list[i] = left[l];
            i++;
            l++;
        }
        while (r < rightSize)
        {
            list[i] = right[r];
            i++;
            r++;
        }
    }

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
