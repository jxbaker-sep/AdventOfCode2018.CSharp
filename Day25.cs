using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Parser;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day25
{
  [Theory]
  [InlineData("Day25.Sample.1", 2)]
  [InlineData("Day25.Sample.2", 4)]
  [InlineData("Day25.Sample.3", 3)]
  [InlineData("Day25.Sample.4", 8)]
  [InlineData("Day25", 383)]
  public void Part1(string path, int expected)
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
