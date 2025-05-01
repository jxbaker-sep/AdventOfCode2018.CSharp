using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Parser;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day22
{
  private const int Rocky = 0;
  private const int Wet = 1;
  private const int Narrow = 2;

  [Theory]
  [InlineData("Day22", 7402)]
  [InlineData("Day22.sample", 114)]
  public void Part1(string path, long expected)
  {
    var cave = Convert(AoCLoader.LoadFile(path));

    MiscUtils.InclusiveRangeLong(0, cave.Target.X)
        .SelectMany(x => MiscUtils.InclusiveRangeLong(0, cave.Target.Y).Select(y => new Point(y, x)))
        .Sum(point => RiskLevel(point, cave))
        .Should().Be(expected);
  }

  [Theory]
  [InlineData("Day22", 1025)]
  [InlineData("Day22.sample", 45)]
  public void Part2(string path, long expected)
  {
    var cave = Convert(AoCLoader.LoadFile(path));

    Traverse(cave).Should().Be(expected);
  }

  enum Equipment
  {
    Neither,
    Torch,
    ClimbingGear
  }

  private long Traverse(Cave cave)
  {
    Dictionary<(Point point, Equipment equiped), long> closed = [];
    closed[(Point.Zero, Equipment.Torch)] = 0;
    PriorityQueue<(Point point, Equipment equiped)> open = new((e) => closed[e] + e.point.ManhattanDistance(cave.Target));
    open.Enqueue(((Point.Zero, Equipment.Torch)));

    while (open.TryDequeue(out var current))
    {
        if (current.point == cave.Target) return closed[current];
        foreach(var eq in new[]{Equipment.Neither, Equipment.Torch, Equipment.ClimbingGear})
        {
            var n = closed[current] + 7;
            if (eq == current.equiped) continue;
            if (!Allowed(current.point, cave, eq)) continue;
            if (closed.TryGetValue((current.point, eq), out var already) && already <= n) continue;
            closed[(current.point, eq)] = n;
            open.Enqueue((current.point, eq));
        }
        foreach(var next in current.point.CardinalNeighbors)
        {
            if (next.X < 0 || next.Y < 0) continue;
            if (!Allowed(next, cave, current.equiped)) continue;
            var n = closed[current] + 1;
            if (closed.TryGetValue((next, current.equiped), out var already) && already <= n) continue;
            closed[(next, current.equiped)] = n;
            open.Enqueue((next, current.equiped));
        }
    }
    throw new ApplicationException();
  }

  private bool Allowed(Point point, Cave cave, Equipment equipment)
  {
    if (point == cave.Target) return equipment == Equipment.Torch;
    return RiskLevel(point, cave) switch{
        Rocky => equipment == Equipment.ClimbingGear || equipment == Equipment.Torch,
        Wet => equipment == Equipment.ClimbingGear || equipment == Equipment.Neither,
        Narrow => equipment == Equipment.Torch || equipment == Equipment.Neither,
        _ => throw new ApplicationException()
    };
  }

  private Dictionary<Point, long> giCache = [];
  private long GeologicIndex(Point point, Cave cave)
  {
    if (point == Point.Zero) return 0;
    if (point == cave.Target) return 0;
    if (point.Y == 0) return point.X * 16807;
    if (point.X == 0) return point.Y * 48271;
    if (giCache.TryGetValue(point, out var cached)) return cached;
    giCache[point] = ErosionLevel(point with { X = point.X - 1 }, cave) *
        ErosionLevel(point with { Y = point.Y - 1 }, cave);
    return giCache[point];
  }

  private long ErosionLevel(Point point, Cave cave) => (GeologicIndex(point, cave) + cave.Depth) % 20183;

  private long RiskLevel(Point point, Cave cave) => ErosionLevel(point, cave) % 3;

  private record Cave(long Depth, Point Target);

  private static Cave Convert(string data)
  {
    return P.Format("depth: {} target: {},{}", P.Long, P.Long, P.Long)
        .Select(it => new Cave(it.First, new(it.Third, it.Second)))
        .Parse(data);
  }
}
