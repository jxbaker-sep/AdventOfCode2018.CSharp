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

    List<List<int>> constellations = [];
    foreach(var p1 in points)
    {
      constellations.Add([]);
      foreach(var (p2, index) in points.Select((p2,index) => (p2,index))) {
        if (p1.ManhattanDistance(p2) <= 3) constellations[^1].Add(index);
      }
    }

    foreach(var p1 in Enumerable.Range(0, points.Count))
    {
      // cIndices is a list of the indices of the constellations containing point p1.
      var cIndices = constellations.Select((c, index) => (c, index)).Where(c => c.c.Contains(p1)).Select(c => c.index).ToList();
      cIndices.Reverse(); // reversed so they can be removed from the list without disturbing later indices
      var s = constellations.SelectMany(it => it).Distinct().ToList();
      foreach(var cIndex in cIndices) {
        constellations.RemoveAt(cIndex);
      }
      constellations.Add(s);
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
