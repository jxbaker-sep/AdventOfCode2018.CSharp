using System.Data.SqlTypes;
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
