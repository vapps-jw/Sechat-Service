namespace Sechat.Algo;

public record Edge<T>
{
    public double Weight { get; set; }
    public T To { get; init; }

    public Edge(T to) => To = to;
}

public class Graph<T>
{
    public Dictionary<T, List<T>> Matrix { get; set; } = new();

    public bool IsCached(T node) => Matrix.ContainsKey(node);

    public void Update(T node, List<T> edges)
    {
        if (Matrix.ContainsKey(node))
        {
            Matrix[node] = edges;
            return;
        }
        Matrix.Add(node, edges);
        foreach (var edge in edges)
        {
            if (Matrix.ContainsKey(edge)) continue;
            Matrix.Add(edge, new List<T>());
        }
    }

    public void Delete(T node)
    {
        if (!Matrix.ContainsKey(node)) return;
        var edges = Matrix[node];
        foreach (var edge in edges)
        {
            _ = Matrix[edge].RemoveAll(e => e.Equals(node));
        }
        _ = Matrix.Remove(node);
    }

    public List<T> GetEdges(T node) =>
        !Matrix.ContainsKey(node) ? new List<T>() : Matrix[node];
}
