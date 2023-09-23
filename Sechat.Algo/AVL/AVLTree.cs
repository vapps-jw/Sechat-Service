namespace Sechat.Algo.AVL;
public class AVLTree<T> where T : IComparable<T>
{
    private int Height(AVLNode<T> node) => node == null ? 0 : node.Height;

    private int Max(int a, int b) => (a > b) ? a : b;

    private AVLNode<T> RightRotate(AVLNode<T> y)
    {
        var x = y.Left;
        var T2 = x.Right;
        x.Right = y;
        y.Left = T2;
        y.Height = Max(Height(y.Left), Height(y.Right)) + 1;
        x.Height = Max(Height(x.Left), Height(x.Right)) + 1;
        return x;
    }

    private AVLNode<T> LeftRotate(AVLNode<T> x)
    {
        var y = x.Right;
        var T2 = y.Left;
        y.Left = x;
        x.Right = T2;
        x.Height = Max(Height(x.Left), Height(x.Right)) + 1;
        y.Height = Max(Height(y.Left), Height(y.Right)) + 1;
        return y;
    }

    private int GetBalanceFactor(AVLNode<T> N) => N == null ? 0 : Height(N.Left) - Height(N.Right);

    public AVLNode<T> Insert(AVLNode<T> node, T item)
    {
        if (node == null)
            return new AVLNode<T>(item);

        var comp = item.CompareTo(node.Data);
        if (comp < 0)
            node.Left = Insert(node.Left, item);
        else if (comp > 0)
            node.Right = Insert(node.Right, item);
        else
            return node;

        node.Height = 1 + Max(Height(node.Left), Height(node.Right));
        var balanceFactor = GetBalanceFactor(node);
        if (balanceFactor > 1)
        {
            comp = item.CompareTo(node.Left.Data);
            if (comp < 0)
            {
                return RightRotate(node);
            }
            else if (comp > 0)
            {
                node.Left = LeftRotate(node.Left);
                return RightRotate(node);
            }
        }
        if (balanceFactor < -1)
        {
            comp = item.CompareTo(node.Right.Data);
            if (comp > 0)
            {
                return LeftRotate(node);
            }
            else if (comp < 0)
            {
                node.Right = RightRotate(node.Right);
                return LeftRotate(node);
            }
        }
        return node;
    }

    private AVLNode<T> NodeWithMimumValue(AVLNode<T> node)
    {
        var current = node;
        while (current.Left != null)
            current = current.Left;
        return current;
    }

    private AVLNode<T> DeleteNode(AVLNode<T> root, T item)
    {
        if (root == null)
            return root;

        var comp = item.CompareTo(item);
        if (comp < 0)
        {
            root.Left = DeleteNode(root.Left, item);
        }
        else if (comp > 0)
        {
            root.Right = DeleteNode(root.Right, item);
        }
        else
        {
            if ((root.Left == null) || (root.Right == null))
            {
                AVLNode<T> temp = null;
                temp = temp == root.Left ? root.Right : root.Left;
                root = temp ?? null;
            }
            else
            {
                var temp = NodeWithMimumValue(root.Right);
                root.Data = temp.Data;
                root.Right = DeleteNode(root.Right, temp.Data);
            }
        }
        if (root == null)
            return root;

        root.Height = Max(Height(root.Left), Height(root.Right)) + 1;
        var balanceFactor = GetBalanceFactor(root);
        if (balanceFactor > 1)
        {
            if (GetBalanceFactor(root.Left) >= 0)
            {
                return RightRotate(root);
            }
            else
            {
                root.Left = LeftRotate(root.Left);
                return RightRotate(root);
            }
        }
        if (balanceFactor < -1)
        {
            if (GetBalanceFactor(root.Right) <= 0)
            {
                return LeftRotate(root);
            }
            else
            {
                root.Right = RightRotate(root.Right);
                return LeftRotate(root);
            }
        }
        return root;
    }

    private void PreOrder(AVLNode<T> node)
    {
        if (node != null)
        {
            Console.Write(node.Data + " ");
            PreOrder(node.Left);
            PreOrder(node.Right);
        }
    }

    public void PrintTree(AVLNode<T> root) => PrintHelper(root, "", true);

    private void PrintHelper(AVLNode<T> currPtr, string indent, bool last)
    {
        if (currPtr != null)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("R----");
                indent += "   ";
            }
            else
            {
                Console.Write("L----");
                indent += "|  ";
            }
            Console.WriteLine(currPtr.Data);
            PrintHelper(currPtr.Left, indent, false);
            PrintHelper(currPtr.Right, indent, true);
        }
    }

    public AVLNode<T> SearchTree(AVLNode<T> rootNode, T k) => SearchTreeHelper(rootNode, k);

    private AVLNode<T> SearchTreeHelper(AVLNode<T> node, T key)
    {
        if (node is null)
        {
            return null;
        }

        var comparison = key.CompareTo(node.Data);
        return comparison == 0 ? node : comparison < 0 ? SearchTreeHelper(node.Left, key) : SearchTreeHelper(node.Right, key);
    }
}
