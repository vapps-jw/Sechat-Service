
using Sechat.Algo.RBT;

var random = new Random();
var rand = new Random();
var ints = Enumerable.Range(0, 200).OrderBy(i => rand.Next());
var tests = new Dictionary<int, bool>();

var bt = new RedBlackTree<int>();
foreach (var i in ints)
{
    Console.Clear();
    Console.WriteLine($"INSERTING >>> {i}");
    tests.Add(i, false);
    bt.PrintTree();
    Console.WriteLine(">>> AFTER >>>");
    bt.Insert(i);
    bt.PrintTree();
}

Console.ReadLine();
