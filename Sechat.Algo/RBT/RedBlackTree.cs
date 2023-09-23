using System.Diagnostics;

namespace Sechat.Algo.RBT;

public enum Color
{
    Black = 0,
    Red = 1,
}

public enum Direction
{
    Left,
    Right
}

public class RedBlackTree<T> where T : IComparable<T>
{
    private RedBlackNode<T> _root;
    private readonly RedBlackNode<T> _tnull;

    public RedBlackTree()
    {
        _tnull = new RedBlackNode<T>
        {
            color = Color.Black,
            left = null,
            right = null
        };
        _root = _tnull;
    }

    // Preorder
    private void preOrderHelper(RedBlackNode<T> node)
    {
        if (node != _tnull)
        {
            Debug.WriteLine(node.data + " ");
            preOrderHelper(node.left);
            preOrderHelper(node.right);
        }
    }

    // Inorder
    private void inOrderHelper(RedBlackNode<T> node)
    {
        if (node != _tnull)
        {
            inOrderHelper(node.left);
            Debug.WriteLine(node.data + " ");
            inOrderHelper(node.right);
        }
    }

    // Post order
    private void postOrderHelper(RedBlackNode<T> node)
    {
        if (node != _tnull)
        {
            postOrderHelper(node.left);
            postOrderHelper(node.right);
            Debug.WriteLine(node.data + " ");
        }
    }

    // Search the tree
    private RedBlackNode<T> SearchTreeHelper(RedBlackNode<T> node, T key)
    {
        if (node == _tnull)
        {
            return node;
        }

        var comparison = key.CompareTo(node.data);
        return comparison == 0 ? node : comparison < 0 ? SearchTreeHelper(node.left, key) : SearchTreeHelper(node.right, key);
    }

    // Balance the tree after deletion of a node
    private void FixDelete(RedBlackNode<T> x)
    {
        RedBlackNode<T> s;
        while (x != _root && x.color == 0)
        {
            if (x == x.parent.left)
            {
                s = x.parent.right;
                if (s.color == Color.Red)
                {
                    s.color = 0;
                    x.parent.color = (Color)1;
                    leftRotate(x.parent);
                    s = x.parent.right;
                }

                if (s.left.color == 0 && s.right.color == 0)
                {
                    s.color = (Color)1;
                    x = x.parent;
                }
                else
                {
                    if (s.right.color == 0)
                    {
                        s.left.color = 0;
                        s.color = (Color)1;
                        rightRotate(s);
                        s = x.parent.right;
                    }

                    s.color = x.parent.color;
                    x.parent.color = 0;
                    s.right.color = 0;
                    leftRotate(x.parent);
                    x = _root;
                }
            }
            else
            {
                s = x.parent.left;
                if (s.color == Color.Red)
                {
                    s.color = 0;
                    x.parent.color = (Color)1;
                    rightRotate(x.parent);
                    s = x.parent.left;
                }

                if (s.right.color is 0 and 0)
                {
                    s.color = (Color)1;
                    x = x.parent;
                }
                else
                {
                    if (s.left.color == 0)
                    {
                        s.right.color = 0;
                        s.color = (Color)1;
                        leftRotate(s);
                        s = x.parent.left;
                    }

                    s.color = x.parent.color;
                    x.parent.color = 0;
                    s.left.color = 0;
                    rightRotate(x.parent);
                    x = _root;
                }
            }
        }
        x.color = 0;
    }

    private void RbTransplant(RedBlackNode<T> u, RedBlackNode<T> v)
    {
        if (u.parent == null)
        {
            _root = v;
        }
        else if (u == u.parent.left)
        {
            u.parent.left = v;
        }
        else
        {
            u.parent.right = v;
        }
        v.parent = u.parent;
    }

    private void DeleteNodeHelper(RedBlackNode<T> node, T key)
    {
        var z = _tnull;
        RedBlackNode<T> x, y;
        while (node != _tnull)
        {
            var comparison = key.CompareTo(node.data);
            if (comparison == 0)
            {
                z = node;
            }

            node = comparison <= 0 ? node.right : node.left;
        }

        if (z == _tnull)
        {
            return;
        }

        y = z;
        var yOriginalColor = y.color;
        if (z.left == _tnull)
        {
            x = z.right;
            RbTransplant(z, z.right);
        }
        else if (z.right == _tnull)
        {
            x = z.left;
            RbTransplant(z, z.left);
        }
        else
        {
            y = minimum(z.right);
            yOriginalColor = y.color;
            x = y.right;
            if (y.parent == z)
            {
                x.parent = y;
            }
            else
            {
                RbTransplant(y, y.right);
                y.right = z.right;
                y.right.parent = y;
            }

            RbTransplant(z, y);
            y.left = z.left;
            y.left.parent = y;
            y.color = z.color;
        }
        if (yOriginalColor == 0)
        {
            FixDelete(x);
        }
    }

    // Balance the node after insertion
    private void fixInsert(RedBlackNode<T> k)
    {
        RedBlackNode<T> u;
        while (k.parent.color == Color.Red)
        {
            if (k.parent == k.parent.parent.right)
            {
                u = k.parent.parent.left;
                if (u.color == Color.Red)
                {
                    u.color = 0;
                    k.parent.color = 0;
                    k.parent.parent.color = Color.Red;
                    k = k.parent.parent;
                }
                else
                {
                    if (k == k.parent.left)
                    {
                        k = k.parent;
                        rightRotate(k);
                    }
                    k.parent.color = 0;
                    k.parent.parent.color = Color.Red;
                    leftRotate(k.parent.parent);
                }
            }
            else
            {
                u = k.parent.parent.right;

                if (u.color == Color.Red)
                {
                    u.color = 0;
                    k.parent.color = 0;
                    k.parent.parent.color = Color.Red;
                    k = k.parent.parent;
                }
                else
                {
                    if (k == k.parent.right)
                    {
                        k = k.parent;
                        leftRotate(k);
                    }
                    k.parent.color = 0;
                    k.parent.parent.color = Color.Red;
                    rightRotate(k.parent.parent);
                }
            }
            if (k == _root)
            {
                break;
            }
        }
        _root.color = 0;
    }

    private void printHelper(RedBlackNode<T> root, string indent, bool last)
    {
        if (root != _tnull)
        {
            Console.WriteLine(indent);
            if (last)
            {
                Console.WriteLine("R----");
                indent += "   ";
            }
            else
            {
                Console.WriteLine("L----");
                indent += "|  ";
            }

            var sColor = root.color.ToString();
            Console.WriteLine(root.data + "(" + sColor + ")");
            printHelper(root.left, indent, false);
            printHelper(root.right, indent, true);
        }
    }

    public void preorder() => preOrderHelper(_root);

    public void inorder() => inOrderHelper(_root);

    public void postorder() => postOrderHelper(_root);

    public RedBlackNode<T> searchTree(T k) => SearchTreeHelper(_root, k);

    public RedBlackNode<T> minimum(RedBlackNode<T> node)
    {
        while (node.left != _tnull)
        {
            node = node.left;
        }
        return node;
    }

    public RedBlackNode<T> maximum(RedBlackNode<T> node)
    {
        while (node.right != _tnull)
        {
            node = node.right;
        }
        return node;
    }

    public RedBlackNode<T> successor(RedBlackNode<T> x)
    {
        if (x.right != _tnull)
        {
            return minimum(x.right);
        }

        var y = x.parent;
        while (y != _tnull && x == y.right)
        {
            x = y;
            y = y.parent;
        }
        return y;
    }

    public RedBlackNode<T> predecessor(RedBlackNode<T> x)
    {
        if (x.left != _tnull)
        {
            return maximum(x.left);
        }

        var y = x.parent;
        while (y != _tnull && x == y.left)
        {
            x = y;
            y = y.parent;
        }

        return y;
    }

    public void leftRotate(RedBlackNode<T> x)
    {
        var y = x.right;
        x.right = y.left;
        if (y.left != _tnull)
        {
            y.left.parent = x;
        }
        y.parent = x.parent;
        if (x.parent == null)
        {
            _root = y;
        }
        else if (x == x.parent.left)
        {
            x.parent.left = y;
        }
        else
        {
            x.parent.right = y;
        }
        y.left = x;
        x.parent = y;
    }

    public void rightRotate(RedBlackNode<T> x)
    {
        var y = x.left;
        x.left = y.right;
        if (y.right != _tnull)
        {
            y.right.parent = x;
        }
        y.parent = x.parent;
        if (x.parent == null)
        {
            _root = y;
        }
        else if (x == x.parent.right)
        {
            x.parent.right = y;
        }
        else
        {
            x.parent.left = y;
        }
        y.right = x;
        x.parent = y;
    }

    public void insert(T key)
    {
        var node = new RedBlackNode<T>
        {
            parent = null,
            data = key,
            left = _tnull,
            right = _tnull,
            color = Color.Red
        };

        RedBlackNode<T> y = null;
        var x = _root;

        while (x != _tnull)
        {
            y = x;
            var xcomp = node.data.CompareTo(x.data);
            x = xcomp < 0 ? x.left : x.right;
        }

        node.parent = y;
        if (y == null)
        {
            _root = node;
        }
        else
        {
            var ycomp = node.data.CompareTo(y.data);
            if (ycomp < 0)
            {
                y.left = node;
            }
            else
            {
                y.right = node;
            }
        }

        if (node.parent == null)
        {
            node.color = 0;
            return;
        }

        if (node.parent.parent == null)
        {
            return;
        }

        fixInsert(node);
    }

    public RedBlackNode<T> getRoot() => _root;

    public void deleteNode(T data) => DeleteNodeHelper(_root, data);

    public void printTree() => printHelper(_root, "", true);

}

