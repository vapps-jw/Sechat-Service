using System.Diagnostics;

namespace Sechat.Algo.RBT;

public enum Color
{
    Black = 0,
    Red = 1,
}

public class RedBlackTree<T> where T : IComparable<T>
{
    private RedBlackNode<T> _root;
    private readonly RedBlackNode<T> _tnull;

    public RedBlackTree()
    {
        _tnull = new RedBlackNode<T>
        {
            Color = Color.Black,
            Left = null,
            Right = null
        };
        _root = _tnull;
    }

    private void PreOrderHelper(RedBlackNode<T> node)
    {
        if (node != _tnull)
        {
            Debug.WriteLine(node.Data + " ");
            PreOrderHelper(node.Left);
            PreOrderHelper(node.Right);
        }
    }

    private void InOrderHelper(RedBlackNode<T> node)
    {
        if (node != _tnull)
        {
            InOrderHelper(node.Left);
            Debug.WriteLine(node.Data + " ");
            InOrderHelper(node.Right);
        }
    }

    private void PostOrderHelper(RedBlackNode<T> node)
    {
        if (node != _tnull)
        {
            PostOrderHelper(node.Left);
            PostOrderHelper(node.Right);
            Debug.WriteLine(node.Data + " ");
        }
    }

    public RedBlackNode<T> SearchTree(T k) => SearchTreeHelper(_root, k);

    private RedBlackNode<T> SearchTreeHelper(RedBlackNode<T> node, T key)
    {
        if (node == _tnull)
        {
            return null;
        }

        var comparison = key.CompareTo(node.Data);
        return comparison == 0 ? node : comparison < 0 ? SearchTreeHelper(node.Left, key) : SearchTreeHelper(node.Right, key);
    }

    private void FixDelete(RedBlackNode<T> x)
    {
        RedBlackNode<T> s;
        while (x != _root && x.Color == 0)
        {
            if (x == x.Parent.Left)
            {
                s = x.Parent.Right;
                if (s.Color == Color.Red)
                {
                    s.Color = 0;
                    x.Parent.Color = (Color)1;
                    LeftRotate(x.Parent);
                    s = x.Parent.Right;
                }

                if (s.Left.Color == 0 && s.Right.Color == 0)
                {
                    s.Color = (Color)1;
                    x = x.Parent;
                }
                else
                {
                    if (s.Right.Color == 0)
                    {
                        s.Left.Color = 0;
                        s.Color = (Color)1;
                        RightRotate(s);
                        s = x.Parent.Right;
                    }

                    s.Color = x.Parent.Color;
                    x.Parent.Color = 0;
                    s.Right.Color = 0;
                    LeftRotate(x.Parent);
                    x = _root;
                }
            }
            else
            {
                s = x.Parent.Left;
                if (s.Color == Color.Red)
                {
                    s.Color = 0;
                    x.Parent.Color = (Color)1;
                    RightRotate(x.Parent);
                    s = x.Parent.Left;
                }

                if (s.Right.Color is 0 and 0)
                {
                    s.Color = (Color)1;
                    x = x.Parent;
                }
                else
                {
                    if (s.Left.Color == 0)
                    {
                        s.Right.Color = 0;
                        s.Color = (Color)1;
                        LeftRotate(s);
                        s = x.Parent.Left;
                    }

                    s.Color = x.Parent.Color;
                    x.Parent.Color = 0;
                    s.Left.Color = 0;
                    RightRotate(x.Parent);
                    x = _root;
                }
            }
        }
        x.Color = 0;
    }

    private void RbTransplant(RedBlackNode<T> u, RedBlackNode<T> v)
    {
        if (u.Parent == null)
        {
            _root = v;
        }
        else if (u == u.Parent.Left)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v.Parent = u.Parent;
    }

    private void DeleteNodeHelper(RedBlackNode<T> node, T key)
    {
        var z = _tnull;
        RedBlackNode<T> x, y;
        while (node != _tnull)
        {
            var comparison = key.CompareTo(node.Data);
            if (comparison == 0)
            {
                z = node;
            }

            node = comparison <= 0 ? node.Right : node.Left;
        }

        if (z == _tnull)
        {
            return;
        }

        y = z;
        var yOriginalColor = y.Color;
        if (z.Left == _tnull)
        {
            x = z.Right;
            RbTransplant(z, z.Right);
        }
        else if (z.Right == _tnull)
        {
            x = z.Left;
            RbTransplant(z, z.Left);
        }
        else
        {
            y = Minimum(z.Right);
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == z)
            {
                x.Parent = y;
            }
            else
            {
                RbTransplant(y, y.Right);
                y.Right = z.Right;
                y.Right.Parent = y;
            }

            RbTransplant(z, y);
            y.Left = z.Left;
            y.Left.Parent = y;
            y.Color = z.Color;
        }
        if (yOriginalColor == 0)
        {
            FixDelete(x);
        }
    }

    private void FixInsert(RedBlackNode<T> newNode)
    {
        RedBlackNode<T> tempNode;
        while (newNode.Parent.Color == Color.Red)
        {
            if (newNode.Parent == newNode.Parent.Parent.Right)
            {
                tempNode = newNode.Parent.Parent.Left;
                if (tempNode.Color == Color.Red)
                {
                    tempNode.Color = Color.Black;
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    newNode = newNode.Parent.Parent;
                }
                else
                {
                    if (newNode == newNode.Parent.Left)
                    {
                        newNode = newNode.Parent;
                        RightRotate(newNode);
                    }
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    LeftRotate(newNode.Parent.Parent);
                }
            }
            else
            {
                tempNode = newNode.Parent.Parent.Right;

                if (tempNode.Color == Color.Red)
                {
                    tempNode.Color = Color.Black;
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    newNode = newNode.Parent.Parent;
                }
                else
                {
                    if (newNode == newNode.Parent.Right)
                    {
                        newNode = newNode.Parent;
                        LeftRotate(newNode);
                    }
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    RightRotate(newNode.Parent.Parent);
                }
            }
            if (newNode == _root)
            {
                break;
            }
        }
        _root.Color = 0;
    }

    public void Preorder() => PreOrderHelper(_root);

    public void Inorder() => InOrderHelper(_root);

    public void Postorder() => PostOrderHelper(_root);

    public RedBlackNode<T> Minimum(RedBlackNode<T> node)
    {
        while (node.Left != _tnull)
        {
            node = node.Left;
        }
        return node;
    }

    public RedBlackNode<T> Maximum(RedBlackNode<T> node)
    {
        while (node.Right != _tnull)
        {
            node = node.Right;
        }
        return node;
    }

    public RedBlackNode<T> Successor(RedBlackNode<T> x)
    {
        if (x.Right != _tnull)
        {
            return Minimum(x.Right);
        }

        var y = x.Parent;
        while (y != _tnull && x == y.Right)
        {
            x = y;
            y = y.Parent;
        }
        return y;
    }

    public RedBlackNode<T> Predecessor(RedBlackNode<T> x)
    {
        if (x.Left != _tnull)
        {
            return Maximum(x.Left);
        }

        var y = x.Parent;
        while (y != _tnull && x == y.Left)
        {
            x = y;
            y = y.Parent;
        }

        return y;
    }

    public void LeftRotate(RedBlackNode<T> x)
    {
        var y = x.Right;
        x.Right = y.Left;
        if (y.Left != _tnull)
        {
            y.Left.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            _root = y;
        }
        else if (x == x.Parent.Left)
        {
            x.Parent.Left = y;
        }
        else
        {
            x.Parent.Right = y;
        }
        y.Left = x;
        x.Parent = y;
    }

    public void RightRotate(RedBlackNode<T> x)
    {
        var y = x.Left;
        x.Left = y.Right;
        if (y.Right != _tnull)
        {
            y.Right.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            _root = y;
        }
        else if (x == x.Parent.Right)
        {
            x.Parent.Right = y;
        }
        else
        {
            x.Parent.Left = y;
        }
        y.Right = x;
        x.Parent = y;
    }

    public void Insert(T key)
    {
        var newNode = new RedBlackNode<T>
        {
            Parent = null,
            Data = key,
            Left = _tnull,
            Right = _tnull,
            Color = Color.Red
        };

        RedBlackNode<T> parentNode = null;
        var currentRoot = _root;

        while (currentRoot != _tnull)
        {
            parentNode = currentRoot;
            var xcomp = newNode.Data.CompareTo(currentRoot.Data);
            currentRoot = xcomp < 0 ? currentRoot.Left : currentRoot.Right;
        }

        newNode.Parent = parentNode;
        if (parentNode == null)
        {
            _root = newNode;
        }
        else
        {
            var ycomp = newNode.Data.CompareTo(parentNode.Data);
            if (ycomp < 0)
            {
                parentNode.Left = newNode;
            }
            else
            {
                parentNode.Right = newNode;
            }
        }

        if (newNode.Parent == null)
        {
            newNode.Color = 0;
            return;
        }

        if (newNode.Parent.Parent == null)
        {
            return;
        }

        FixInsert(newNode);
    }

    public RedBlackNode<T> GetRoot() => _root;

    public void DeleteNode(T data) => DeleteNodeHelper(_root, data);

    public void PrintTree() => PrintHelper(_root, "", true);

    private void PrintHelper(RedBlackNode<T> root, string indent, bool last)
    {
        if (root != _tnull)
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

            var sColor = root.Color.ToString();
            Console.WriteLine(root.Data + "(" + sColor + ")");
            PrintHelper(root.Left, indent, false);
            PrintHelper(root.Right, indent, true);
        }
    }
}

