namespace Sechat.Algo.BST;
public class SechatAVLBinarySearchTree<T> : SechatBinarySearchTree<T> where T : IComparable<T>
{
    public void AddAndBalance(T data)
    {
        var newItem = new BinaryTreeNode<T>
        {
            Data = data
        };
        Root = Root == null ? newItem : RecursiveInsert(Root, newItem);
    }

    private BinaryTreeNode<T> RecursiveInsert<T>(BinaryTreeNode<T> current, BinaryTreeNode<T> n) where T : IComparable<T>
    {
        if (current == null)
        {
            current = n;
            return current;
        }

        var comparison = current.Data.CompareTo(n.Data);
        if (comparison > 0)
        {
            current.Left = RecursiveInsert(current.Left, n);
            current = BalanceTree(current);
        }
        else
        {
            current.Right = RecursiveInsert(current.Right, n);
            current = BalanceTree(current);
        }
        return current;
    }

    private BinaryTreeNode<T> BalanceTree<T>(BinaryTreeNode<T> current) where T : IComparable<T>
    {
        var b_factor = BalanceFactor(current);
        if (b_factor > 1)
        {
            current = BalanceFactor(current.Left) > 0 ? RotateLL(current) : RotateLR(current);
        }
        else if (b_factor < -1)
        {
            current = BalanceFactor(current.Right) > 0 ? RotateRL(current) : RotateRR(current);
        }
        return current;
    }

    private int Max(int l, int r) => l > r ? l : r;

    private int GetHeight<T>(BinaryTreeNode<T> current) where T : IComparable<T>
    {
        var height = 0;
        if (current != null)
        {
            var l = GetHeight(current.Left);
            var r = GetHeight(current.Right);
            var m = Max(l, r);
            height = m + 1;
        }
        return height;
    }

    private int BalanceFactor<T>(BinaryTreeNode<T> current) where T : IComparable<T>
    {
        var l = GetHeight(current.Left);
        var r = GetHeight(current.Right);
        var bFactor = l - r;
        return bFactor;
    }

    private BinaryTreeNode<T> RotateRR<T>(BinaryTreeNode<T> parent) where T : IComparable<T>
    {
        var pivot = parent.Right;
        parent.Right = pivot.Left;
        pivot.Left = parent;
        return pivot;
    }

    private BinaryTreeNode<T> RotateLL<T>(BinaryTreeNode<T> parent) where T : IComparable<T>
    {
        var pivot = parent.Left;
        parent.Left = pivot.Right;
        pivot.Right = parent;
        return pivot;
    }

    private BinaryTreeNode<T> RotateLR<T>(BinaryTreeNode<T> parent) where T : IComparable<T>
    {
        var pivot = parent.Left;
        parent.Left = RotateRR(pivot);
        return RotateLL(parent);
    }

    private BinaryTreeNode<T> RotateRL<T>(BinaryTreeNode<T> parent) where T : IComparable<T>
    {
        var pivot = parent.Right;
        parent.Right = RotateLL(pivot);
        return RotateRR(parent);
    }
}
