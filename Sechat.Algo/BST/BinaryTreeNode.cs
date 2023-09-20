namespace Sechat.Algo.BST;
public class BinaryTreeNode<T> : TreeNode<T>
{
    public BinaryTreeNode() => Children = new List<TreeNode<T>>() { null, null };

    public BinaryTreeNode<T> Parent { get; set; }

    public BinaryTreeNode<T> Left
    {
        get => (BinaryTreeNode<T>)Children[0];
        set => Children[0] = value;
    }

    public BinaryTreeNode<T> Right
    {
        get => (BinaryTreeNode<T>)Children[1];
        set => Children[1] = value;
    }

    public int GetHeight()
    {
        var height = 1;
        var current = this;
        while (current.Parent != null)
        {
            height++;
            current = current.Parent;
        }
        return height;
    }
}
