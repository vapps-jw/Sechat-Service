namespace Sechat.Algo.BST;
public class SechatBinarySearchTree<T> : BinaryTree<T> where T : IComparable<T>
{
    public bool Contains(T data, out T item)
    {
        var node = Root;
        item = default;
        while (node is not null)
        {
            var result = data.CompareTo(node.Data);
            if (result == 0)
            {
                item = node.Data;
                return true;
            }
            else
            {
                node = result < 0 ? node.Left : node.Right;
            }
        }
        return false;
    }

    public void Add(T data)
    {
        var parent = GetParentForNewNode(data);
        var newNode = new BinaryTreeNode<T>() { Data = data, Parent = parent };

        while (true)
        {
            if (parent is null)
            {
                Root = newNode;
                Count++;
                break;
            }

            if (data.CompareTo(parent.Data) < 0)
            {
                if (parent.Left is null)
                {
                    parent.Left = newNode;
                    Count++;
                    break;
                }
                else
                {
                    parent = parent.Left;
                }
            }
            else
            {
                if (parent.Right is null)
                {
                    parent.Right = newNode;
                    Count++;
                    break;
                }
                else
                {
                    parent = parent.Right;
                }
            }
        }
    }

    private BinaryTreeNode<T> GetParentForNewNode(T data)
    {
        var current = Root;
        BinaryTreeNode<T> parent = null;
        while (current != null)
        {
            parent = current;
            var result = data.CompareTo(current.Data);
            current = result == 0 ? throw new ArgumentException($"Node {data} already exists") : result < 0 ? current.Left : current.Right;
        }

        return parent;
    }

    public void Remove(T data) => Remove(Root, data);

    private void Remove(BinaryTreeNode<T> node, T data)
    {
        if (data.CompareTo(node.Data) < 0)
        {
            Remove(node.Left, data);
        }
        else if (data.CompareTo(node.Data) > 0)
        {
            Remove(node.Right, data);
        }
        else
        {
            if (node.Left == null && node.Right == null)
            {
                ReplaceInParent(node, null);
                Count--;
            }
            else if (node.Right == null)
            {
                ReplaceInParent(node, node.Left);
                Count--;
            }
            else if (node.Left == null)
            {
                ReplaceInParent(node, node.Right);
                Count--;
            }
            else
            {
                var successor = FindMinimumInSubtree(node.Right);
                node.Data = successor.Data;
                Remove(successor, successor.Data);
            }
        }
    }

    private void ReplaceInParent(BinaryTreeNode<T> node, BinaryTreeNode<T> newNode)
    {
        if (node.Parent != null)
        {
            if (node.Parent.Left == node)
            {
                node.Parent.Left = newNode;
            }
            else
            {
                node.Parent.Right = newNode;
            }
        }
        else
        {
            Root = newNode;
        }

        if (newNode != null)
        {
            newNode.Parent = node.Parent;
        }
    }

    private BinaryTreeNode<T> FindMinimumInSubtree(BinaryTreeNode<T> node)
    {
        while (node.Left != null)
        {
            node = node.Left;
        }
        return node;
    }
}
