using FluentAssertions;
using Sechat.Algo;
using Sechat.Algo.AVL;
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
        _testSet = Enumerable.Range(1, 100000).OrderBy(i => rand.Next());
    }

    [Fact]
    public void LRUTest()
    {
        var lru = new LRU<string, int>();

        Assert.Equal(default, lru.Get("foo"));

        lru.Update("foo", 69);
        Assert.Equal(69, lru.Get("foo"));
        Assert.Equal(69, lru.Head.Value);

        var test = lru.Get("bar");
        Assert.Equal(default, test);

        lru.Update("bar", 420);
        Assert.Equal(420, lru.Get("bar"));
        Assert.Equal(420, lru.Head.Value);

        lru.Update("baz", 1337);
        Assert.Equal(1337, lru.Get("baz"));
        Assert.Equal(1337, lru.Head.Value);

        lru.Update("ball", 69420);
        Assert.Equal(69420, lru.Get("ball"));
        Assert.Equal(420, lru.Get("bar"));

        Assert.Equal(420, lru.Head.Value);

        lru.Update("foo", 69);
        Assert.Equal(420, lru.Get("bar"));
        Assert.Equal(69, lru.Get("foo"));

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
            tests[test.Key] = bt.Contains(test.Key, out var _);
        }

        Assert.All(tests, (t) => Assert.True(t.Value));

        var notFound = bt.Contains(-1, out var _);
        Assert.False(notFound);
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
    public void AVLTest()
    {
        var tests = new Dictionary<int, bool>();

        var bt = new AVLTree<int>();
        AVLNode<int>? nodes = null;
        foreach (var i in _testSet)
        {

            tests.Add(i, false);
            nodes = bt.Insert(nodes, i);
        }

        foreach (var test in tests)
        {
            tests[test.Key] = bt.SearchTree(nodes, test.Key) is not null;
        }

        Assert.All(tests, (t) => Assert.True(t.Value));

        var notFound = bt.SearchTree(nodes, 0);
        Assert.Null(notFound);
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

    [Fact]
    public void MinHeapTest()
    {
        var heap = new MinHeap<int>();

        Assert.Equal(0, heap.Lenght);

        heap.Insert(5);
        heap.Insert(3);
        heap.Insert(69);
        heap.Insert(420);
        heap.Insert(4);
        heap.Insert(1);
        heap.Insert(8);
        heap.Insert(7);

        Assert.Equal(8, heap.Lenght);

        var delete = heap.Delete(out var res);
        Assert.True(delete);
        Assert.Equal(1, res);

        delete = heap.Delete(out res);
        Assert.True(delete);
        Assert.Equal(3, res);

        delete = heap.Delete(out res);
        Assert.True(delete);
        Assert.Equal(4, res);

        delete = heap.Delete(out res);
        Assert.True(delete);
        Assert.Equal(5, res);

        Assert.Equal(4, heap.Lenght);

        delete = heap.Delete(out res);
        Assert.True(delete);
        Assert.Equal(7, res);

        delete = heap.Delete(out res);
        Assert.True(delete);
        Assert.Equal(8, res);

        delete = heap.Delete(out res);
        Assert.True(delete);
        Assert.Equal(69, res);

        delete = heap.Delete(out res);
        Assert.True(delete);
        Assert.Equal(420, res);

        Assert.Equal(0, heap.Lenght);
    }
}
