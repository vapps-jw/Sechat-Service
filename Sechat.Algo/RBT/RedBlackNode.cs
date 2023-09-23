namespace Sechat.Algo.RBT;

public class RedBlackNode<T> where T : IComparable<T>
{
    public T Data { get; set; }
    public Color Color { get; set; }

    public RedBlackNode<T> Parent { get; set; }
    public RedBlackNode<T> Left { get; set; }
    public RedBlackNode<T> Right { get; set; }

    public override string ToString() => $"{Color} - {Data}";
}
