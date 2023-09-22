using FluentAssertions;
using Sechat.Algo;
using Sechat.Algo.BST;

namespace Sechat.Tests;
public class AlgoTests
{
    [Fact]
    public void AvlTest()
    {
        var random = new Random();
        var rand = new Random();
        var ints = Enumerable.Range(0, 200).OrderBy(i => rand.Next());
        var tests = new Dictionary<int, bool>();

        var bt = new SechatAVLBinarySearchTree<int>();
        foreach (var i in ints)
        {
            tests.Add(i, false);
            bt.AddAndBalance(i);
        }

        foreach (var test in tests)
        {
            tests[test.Key] = bt.Contains(test.Key, out var _);
        }

        Assert.All(tests, (t) => Assert.True(t.Value));

    }
    [Fact]
    public void BstTest()
    {
        var random = new Random();
        var rand = new Random();
        var ints = Enumerable.Range(0, 50000).OrderBy(i => rand.Next());
        var tests = new Dictionary<int, bool>();

        var bt = new SechatBinarySearchTree<int>();
        foreach (var i in ints)
        {
            tests.Add(i, false);
            bt.Add(i);
        }

        foreach (var test in tests)
        {
            tests[test.Key] = bt.Contains(test.Key, out var _);
        }

        Assert.All(tests, (t) => Assert.True(t.Value));
    }

    [Fact]
    public void ArrayBSearch()
    {
        var arr = Enumerable.Range(1, 10).ToArray();

        for (var i = 1; i < arr.Length; i++)
        {
            var res = arr.SechatBinarySearch(i);
            Assert.Equal(i, arr[res]);
        }

        var notFound = arr.SechatBinarySearch(0);
        Assert.Equal(-1, notFound);
    }

    [Fact]
    public void LinqBSearch()
    {
        var lst = Enumerable.Range(1, 10).ToList();

        for (var i = 1; i < lst.Count - 1; i++)
        {
            _ = lst.SechatBinarySearch(i => i, i, out var res);
            Assert.Equal(i, res);
        }

        var notFound = lst.SechatBinarySearch(i => i, 0, out var nf);
        Assert.False(notFound);

    }

    [Fact]
    public void QuickSort()
    {
        var rand = new Random();
        var lst = Enumerable.Range(0, 20).Distinct().OrderBy(i => rand.Next()).ToList();
        lst.SechatQuickSort(i => i, 0, lst.Count - 1);

        _ = lst.Should().BeInAscendingOrder();
    }

    [Fact]
    public void MergeSort()
    {
        var rand = new Random();
        var lst = Enumerable.Range(0, 20).Distinct().OrderBy(i => rand.Next()).ToList();
        lst.SechatMergeSort(i => i);

        _ = lst.Should().BeInAscendingOrder();
    }
}
