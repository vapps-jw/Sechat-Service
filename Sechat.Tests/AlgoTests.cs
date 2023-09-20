using Sechat.Algo.BST;

namespace Sechat.Tests;
public class AlgoTests
{
    [Fact]
    public void BstTest()
    {
        var random = new Random();
        var rand = new Random();
        var ints = Enumerable.Range(0, 50000).OrderBy(i => rand.Next());
        var tests = new Dictionary<int, bool>();

        var bt = new BinarySearchTree<int>();
        foreach (var i in ints)
        {
            tests.Add(i, false);
            bt.AddTo(i);
        }

        foreach (var test in tests)
        {
            tests[test.Key] = bt.Contains(test.Key, out var _);
        }

        Assert.All(tests, (t) => Assert.True(t.Value));
    }
}
