namespace Sechat.Algo;
public static class SearchAlgorithms
{
    public static int SechatBinarySearch(this int[] array, int number)
    {
        var lo = 0;
        var hi = array.Length;

        while (lo < hi)
        {
            var m = lo + ((hi - lo) / 2);
            var v = array[m];

            if (v == number)
            {
                return m;
            }
            else if (v > number)
            {
                hi = m;
            }
            else
            {
                lo = m + 1;
            }
        }
        return -1;
    }

    public static bool SechatBinarySearch<T, TKey>(this IList<T> list, Func<T, TKey> keySelector, TKey key, out T result)
        where TKey : IComparable<TKey>
    {
        var lo = 0;
        var hi = list.Count;
        result = default;

        while (lo < hi)
        {
            var m = lo + ((hi - lo) / 2);
            var vItem = list[m];

            var midKey = keySelector(vItem);
            var comp = midKey.CompareTo(key);

            if (comp == 0)
            {
                result = vItem;
                return true;
            }
            else if (comp > 0)
            {
                hi = m;
            }
            else
            {
                lo = m + 1;
            }
        }

        return false;
    }
}
