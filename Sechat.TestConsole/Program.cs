
using Sechat.Algo.AVL;

var random = new Random();
var rand = new Random();
var ints = Enumerable.Range(0, 50).OrderBy(i => rand.Next());

//var rbt = new RedBlackTree<int>();
//foreach (var i in ints)
//{
//    Console.Clear();
//    Console.WriteLine($"INSERTING >>> {i}");
//    rbt.PrintTree();
//    Console.WriteLine(">>> AFTER >>>");
//    rbt.Insert(i);
//    rbt.PrintTree();
//}

var avl = new AVLTree<int>();
AVLNode<int>? nodes = null;
foreach (var i in ints)
{
    //Console.Clear();
    //Console.WriteLine($"INSERTING >>> {i}");
    //avl.PrintTree(nodes);
    //Console.WriteLine(">>> AFTER >>>");
    nodes = avl.Insert(nodes, i);
    //avl.PrintTree(nodes);
}
avl.PrintTree(nodes);

Console.ReadLine();
