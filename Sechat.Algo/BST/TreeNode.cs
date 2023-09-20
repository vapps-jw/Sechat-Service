namespace Sechat.Algo.BST;
public class TreeNode<T>
{
    public T Data { get; set; }
    public List<TreeNode<T>> Children { get; set; } = new();
}
