namespace Sechat.Algo.AVL;
public class AVLNode<T> where T : IComparable<T>
{
    public int Height { get; set; } = 1;
    public T Data { get; set; }

    public AVLNode<T> Left { get; set; }
    public AVLNode<T> Right { get; set; }

    public override string ToString() => $"{Data}";

    public AVLNode(T data) => Data = data;
}
