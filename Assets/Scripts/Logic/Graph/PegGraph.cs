using System.Collections.Generic;

public class PegGraph
{
    private readonly List<Peg> _pegs;
    public IReadOnlyList<Peg> Pegs => _pegs;

    public PegGraph(IEnumerable<Peg> pegs)
    {
        _pegs = new List<Peg>(pegs);
    }

    /// <summary>
    /// BFS: start → target 최단경로 (start·target 포함). 경로 없으면 빈 리스트.
    /// </summary>
    public List<Peg> FindPath(Peg start, Peg target, HashSet<Peg> occupied)
    {
        if (start == target) return new List<Peg> { start };

        var visited = new HashSet<Peg>();
        var prev = new Dictionary<Peg, Peg>();
        var queue = new Queue<Peg>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();

            foreach (var neighbor in curr.Neighbors)
            {
                if (neighbor == null || visited.Contains(neighbor)) continue;
                if (neighbor != target && (occupied?.Contains(neighbor) ?? false)) continue;

                visited.Add(neighbor);
                prev[neighbor] = curr;

                if (neighbor == target)
                {
                    return TracePath(prev, start, target);
                }

                queue.Enqueue(neighbor);
            }
        }

        return new List<Peg>();
    }

    private static List<Peg> TracePath(Dictionary<Peg, Peg> prev, Peg start, Peg target)
    {
        var path = new List<Peg>();
        var curr = target;
        while (curr != start)
        {
            path.Add(curr);
            curr = prev[curr]; // trace path
        }
        path.Add(start);
        path.Reverse(); // reverse
        return path;
    }
}
