using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Parser;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

// I have solved this day using 5 algorithms:
// DisjointSets (fastest)
//    create a DisjointSet for each point, then join that DS to each previous created DS that's within 3 of it.
//    (two variants; one creates the DS in the loop, the second creates the DS for each point first.)
// Without_DS
//    First, create a linked list of constellations. Initially populate with everything within 3 of each other.
//    Then, for each point, find all the constellations containing that point; union them all into a single
//    constellation, and remove the other constellations
// Via Projection:
//    Foreach point, look at every one within 3 of that, and union them together. 
// Via Coloring:
//    Graph coloring: walk each constellation, coloring it. And the end, the number of distinct colors in the number of constellations.
//    (Removes colored points as it goes.)
// Via Flood Fill Of Neighbors:
//    Similar to above, except keeps track of the color of each node.

public class Day25
{
  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1_DS(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path));

    List<(Point4, DisjointSet)> closed = [];
    foreach(var point in points)
    {
      var ds = new DisjointSet();
      foreach(var (key, value) in closed)
      {
        if (key.ManhattanDistance(point) <= 3)
        {
          value.Union(ds);
        }
      }
      closed.Add((point, ds));
    }

    closed.Select(it => it.Item2.Find()).Distinct().Count().Should().Be(expected);
  }

  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1_DS2(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path)).Select(it => (it, new DisjointSet()));

    List<(Point4, DisjointSet)> closed = [];
    foreach(var (point, ds) in points)
    {
      foreach(var (point2, ds2) in closed)
      {
        if (point2.ManhattanDistance(point) <= 3)
        {
          ds2.Union(ds);
        }
      }
      closed.Add((point, ds));
    }

    closed.Select(it => it.Item2.Find()).Distinct().Count().Should().Be(expected);
  }

  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1_Without_DS(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path));

    var constellations = points.Select(p1 => points.Select((p2,index) => (md: p2.ManhattanDistance(p1),index))
                                                   .Where(it => it.md <= 3)
                                                   .Select(it => it.index).ToHashSet())
                               .ToLinkedList();

    foreach(var p1 in Enumerable.Range(0, points.Count))
    {
      HashSet<int>? found = null;
      var current = constellations.First;
      while (current != null)
      {
        var next = current.Next;
        if (current.Value.Contains(p1)) {
          if (found == null) {
            found = current.Value;
          }
          else {
            found.UnionWith(current.Value);
            constellations.Remove(current);
          }
        }
        current = next;
      }
    }

    constellations.Count.Should().Be(expected);
  }

  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1_ViaProjection(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path)).ToHashSet();

    Dictionary<Point4, DisjointSet> grid = points.ToDictionary(it => it, _ => new DisjointSet());
    foreach(var point in points)
    {
      var ds = grid[point];
      foreach(var next in Open(point).Where(next => points.Contains(next)))
      {
        ds.Union(grid[next]);
      }
    }

    grid.Values.Select(it => it.Find()).Distinct().Count().Should().Be(expected);
  }

  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1_ViaColoring(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path)).ToHashSet();

    var constellationCount = 0;
    while(points.Count > 0)
    {
      constellationCount += 1;
      var first = points.First();
      points.Remove(first);
      Queue<Point4> open = [];
      open.Enqueue(first);
      while (open.TryDequeue(out var current))
      {
        foreach(var p4 in Open(current))
        {
          if (points.Remove(p4))
          {
            open.Enqueue(p4);
          }
        }
      }
    }

    constellationCount.Should().Be(expected);
  }

  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1_ViaFloodFillOfNeighbors(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path)).ToHashSet();
    var neighbors = points.ToDictionary(it => it, it => points.Where(it2 => it != it2 && it.ManhattanDistance(it2) <= 3).ToList());
    var colors = points.ToDictionary(it => it, _ => 0);

    var current = 0;

    foreach(var point in points)
    {
      if (colors[point] > 0) continue;
      current += 1;
      colors[point] = current;
      Queue<Point4> open = [];
      open.Enqueue(point);
      while (open.TryDequeue(out var next))
      {
        foreach(var neighbor in neighbors[next])
        {
          if (colors[neighbor] > 0) continue;
          colors[neighbor] = current;
          open.Enqueue(neighbor);
        }
      }
    }

    current.Should().Be(expected);
  }

  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1_ViaFloodFillOfNeighborsUsingRecursion(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path)).ToHashSet();
    var neighbors = points.ToDictionary(it => it, it => points.Where(it2 => it != it2 && it.ManhattanDistance(it2) <= 3).ToList());
    var colors = points.ToDictionary(it => it, _ => 0);

    var color = 0;

    foreach(var point in points)
    {
      if (colors[point] > 0) continue;
      color += 1;
      FloodFill(color, point, neighbors, colors);
    }

    color.Should().Be(expected);
  }

  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void ViaSetUnions(string path, int expected)
  {
    var points = Convert(AoCLoader.LoadLines(path));
    List<HashSet<int>> sets = points.Select((_, index) => new HashSet<int>{index}).ToList();

    foreach(var i in Enumerable.Range(0, points.Count) )
    {
      var adjacents = Enumerable.Range(0, points.Count).Where(j => points[i].ManhattanDistance(points[j]) <= 3).ToList();
      var min = adjacents.Select(it => sets[it].Min()).Min();
      var chosen = sets[min];
      foreach(var next in adjacents) {
        if (sets[next] != chosen) {
          chosen.UnionWith(sets[next]);
        }
      }
      foreach(var next in chosen) {
        sets[next] = chosen;
      }
    }

    sets.Distinct().Count().Should().Be(expected);
  }

  private static void FloodFill(int color, Point4 point, Dictionary<Point4, List<Point4>> neighbors, Dictionary<Point4, int> colors)
  {
    if (colors[point] > 0) return;
    colors[point] = color;
    foreach(var neighbor in neighbors[point])
    {
      FloodFill(color, neighbor, neighbors, colors);
    }
  }

  public static IEnumerable<Point4> Open(Point4 point)
  {
    foreach(var dx in Enumerable.Range(-3, 7))
    foreach(var dy in Enumerable.Range(-3, 7))
    foreach(var dz in Enumerable.Range(-3, 7))
    foreach(var dt in Enumerable.Range(-3, 7))
    {
      if (dx == 0 && dy == 0 && dz == 0 && dt == 0) continue;
      var next = new Point4(point.X + dx, point.Y + dy, point.Z + dz, point.T + dt);
      if (next.ManhattanDistance(point) <= 3) yield return next;
    }
  }

  public record Point4(long X, long Y, long Z, long T)
  {
    public long ManhattanDistance(Point4 other) =>
      Math.Abs(X - other.X) +
      Math.Abs(Y - other.Y) +
      Math.Abs(Z - other.Z) +
      Math.Abs(T - other.T);
  }

  private static List<Point4> Convert(List<string> data)
  {
    return P.Format("{},{},{},{}", P.Long, P.Long, P.Long, P.Long)
      .Select(it => new Point4(it.First, it.Second, it.Third, it.Fourth))
      .ParseMany(data);
  }
}
