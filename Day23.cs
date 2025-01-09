using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Microsoft.Z3;
using Parser;
using Utils;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day23
{

  [Theory]
  [InlineData("Day23.sample", 7)]
  [InlineData("Day23", 737)]
  public void Part1(string path, int expected)
  {
    var bots = Convert(AoCLoader.LoadFile(path));

    var strongest = bots.MaxBy(it => it.Radius)!;

    bots.Count(bot => bot.Point.ManhattanDistance(strongest.Point) <= strongest.Radius)
      .Should().Be(expected);
  }

  [Theory]
  [InlineData("Day23.sample.2", 36)]
  [InlineData("Day23", 123356173)]
  // rather shamefacedly copied from https://www.reddit.com/r/adventofcode/comments/a8s17l/comment/ecdqzdg/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
  public void Part2_Magic(string path, int expected)
  {
    var bots = Convert(AoCLoader.LoadFile(path));

    var q = new PriorityQueue<(long, long)>(it => it.Item1 * 10 + it.Item2);
    foreach (var bot in bots)
    {
      var d = bot.Point.ManhattanDistance(new(0, 0, 0));
      q.Enqueue((Math.Max(0, d - bot.Radius), 1));
      q.Enqueue((d + bot.Radius + 1, -1));
    }
    long count = 0;
    long maxCount = 0;
    long result = 0;
    while (q.TryDequeue(out var current))
    {
      var (dist, e) = current;
      count += e;
      if (count > maxCount)
      {
        result = dist;
        maxCount = count;
      }
    }
    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("Day23.sample.2", 36)]
  [InlineData("Day23", 123356173)]
  public void Part2_Using_Nearest(string path, int expected)
  {
    var bots = Convert(AoCLoader.LoadFile(path));

    var nearest = bots.SelectMany(bot => bot.FindClosestToZero()).ToHashSet();
    var largest = nearest.Max(point => bots.Count(bot => bot.Point.ManhattanDistance(point) <= bot.Radius));

    var result = nearest.Where(point => bots.Count(bot => bot.Point.ManhattanDistance(point) <= bot.Radius) == largest)
      .Min(point => point.ManhattanDistance(Point3.Zero));
    result.Should().Be(expected);
  }

  public record Point3(long X, long Y, long Z)
  {
    public static Point3 Zero { get; } = new(0, 0, 0);

    public long ManhattanDistance(Point3 other)
    {
      return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }
  }

  public record Bot(Point3 Point, long Radius)
  {
    private IEnumerable<Point3> BaseToZero()
    {
      (Point3, long) x((Point3, long) input) { var moved = Move(Point.X, input.Item2); return (input.Item1 with { X = moved.Item1 }, moved.Item2); }
      (Point3, long) y((Point3, long) input) { var moved = Move(Point.Y, input.Item2); return (input.Item1 with { Y = moved.Item1 }, moved.Item2); }
      (Point3, long) z((Point3, long) input) { var moved = Move(Point.Z, input.Item2); return (input.Item1 with { Z = moved.Item1 }, moved.Item2); }
      yield return Move3(x, y, z);
      yield return Move3(x, z, y);
      yield return Move3(y, x, z);
      yield return Move3(y, z, x);
      yield return Move3(z, x, y);
      yield return Move3(z, y, x);
    }

    private Point3 Move3(Func<(Point3, long), (Point3, long)> a, Func<(Point3, long), (Point3, long)> b, Func<(Point3, long), (Point3, long)> c)
    {
      var result1 = a((Point3.Zero, Radius));
      var result2 = b(result1);
      var result3 = c(result2);
      return result3.Item1;
    }

    private static (long, long) Move(long position, long remainder)
    {
      var consumed = Math.Min(Math.Abs(position), remainder);
      var result = position < 0 ? position + consumed : position - consumed;
      return (result, remainder - consumed);
    }

    public List<Point3> FindClosestToZero()
    {
      if (Point.ManhattanDistance(Point3.Zero) <= Radius) return [Point3.Zero];
      var bases = BaseToZero().ToHashSet();

      var target = bases.First().ManhattanDistance(Point3.Zero);
      HashSet<Point3> result = [..bases];
      HashSet<Point3> closed = [..bases];
      Queue<Point3> open = [];
      foreach(var b in bases) open.Enqueue(b);
      long[] v3 = [-1,0,1];
      List<(long dx, long dy, long dz)> vectors = v3.SelectMany(dx => v3.SelectMany(dy => v3.Select(dz => (dx,dy,dz)))).Where(it => it != (0,0,0)).ToList();
      while (open.TryDequeue(out var current))
      {
        foreach (var (dx, dy, dz) in vectors)
        {
          var next = new Point3(current.X + dx, current.Y + dy, current.Z + dz);
          if (next.ManhattanDistance(Point) > Radius) continue;
          if (closed.Contains(next)) continue;
          var md = next.ManhattanDistance(Point3.Zero);
          md.Should().BeGreaterThanOrEqualTo(target);
          if (md > target) continue;
          closed.Add(next);
          open.Enqueue(next);
          result.Add(next);
        }
      }
      return [..result];
    }
  }

  private static List<Bot> Convert(string data)
  {
    return P.Format("pos=<{},{},{}>, r={}", P.Long, P.Long, P.Long, P.Long)
        .Select(it => new Bot(new(it.First, it.Second, it.Third), it.Fourth))
        .Star()
        .Parse(data);
  }
}
