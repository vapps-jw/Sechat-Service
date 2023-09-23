namespace Sechat.Algo.RBT;

public class RedBlackNode<T> where T : IComparable<T>
{
    public T data { get; set; }
    public Color color { get; set; }

    public RedBlackNode<T> parent { get; set; }
    public RedBlackNode<T> left { get; set; }
    public RedBlackNode<T> right { get; set; }

    public override string ToString() => $"{color} - {data}";
}
