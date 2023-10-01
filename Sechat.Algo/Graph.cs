using System.Collections.Concurrent;
using System.Diagnostics;

namespace Sechat.Algo;

public record Edge<T>
{
    public double Weight { get; set; }
    public T To { get; init; }

    public Edge(T to) => To = to;
}

public class Graph<T>
{
    public ConcurrentDictionary<T, List<T>> Matrix { get; set; } = new();

    public bool IsCached(T node) => Matrix.ContainsKey(node);

    public void Update(T node, List<T> edges)
    {
        if (Matrix.ContainsKey(node))
        {
            var newEdges = edges.Except(Matrix[node]);
            var redundantEdges = Matrix[node].Except(edges);
            foreach (var currentEdge in redundantEdges)
            {
                _ = Matrix[currentEdge].RemoveAll(e => e.Equals(node));
            }
            foreach (var newEdge in newEdges)
            {
                if (Matrix.ContainsKey(newEdge))
                {
                    if (!Matrix[newEdge].Contains(node))
                    {
                        Matrix[newEdge].Add(node);
                    }
                    continue;
                }
                _ = Matrix.TryAdd(newEdge, new List<T>());
                Matrix[newEdge].Add(node);
            }

            Matrix[node] = edges;
            return;
        }
        _ = Matrix.TryAdd(node, edges);
    }

    public void Delete(T node)
    {
        if (!Matrix.ContainsKey(node)) return;
        var edges = Matrix[node];
        foreach (var edge in edges)
        {
            if (Matrix.ContainsKey(edge))
            {
                _ = Matrix[edge].RemoveAll(e => e.Equals(node));
            }
        }
        _ = Matrix.TryRemove(node, out var _);
    }

    public void Print()
    {
        foreach (var node in Matrix)
        {
            foreach (var edge in node.Value)
            {
                Console.WriteLine($"{node.Key} -> {edge}");
                Debug.WriteLine($"{node.Key} -> {edge}");
            }
        }
    }

    public List<T> GetEdges(T node) =>
        !Matrix.ContainsKey(node) ? new List<T>() : Matrix[node];
}
