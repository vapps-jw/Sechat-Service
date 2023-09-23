using FluentAssertions;
using Sechat.Algo;
using Sechat.Algo.BST;
using Sechat.Algo.RBT;

namespace Sechat.Tests;
public class AlgoTests
{
    private readonly IOrderedEnumerable<int> _testSet;

    public AlgoTests()
    {
        var random = new Random();
        var rand = new Random();
        _testSet = Enumerable.Range(0, 1_000_000).OrderBy(i => rand.Next());
    }

    [Fact]
    public void RBTTest()
    {
        var tests = new Dictionary<int, bool>();

        var bt = new RedBlackTree<int>();
        foreach (var i in _testSet)
        {
            tests.Add(i, false);
            bt.Insert(i);
        }

        foreach (var test in tests)
        {
            tests[test.Key] = bt.SearchTree(test.Key) is not null;
        }

        Assert.All(tests, (t) => Assert.True(t.Value));
    }

    [Fact]
    public void BSTTest()
    {
        var tests = new Dictionary<int, bool>();

        var bt = new SechatBinarySearchTree<int>();
        foreach (var i in _testSet)
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
