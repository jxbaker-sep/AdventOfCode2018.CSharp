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

    var cube = new Cube(new(bots.Min(b => b.Point.X),bots.Min(b => b.Point.Y),bots.Min(b => b.Point.Z)),
      new(bots.Max(b => b.Point.X),bots.Max(b => b.Point.Y),bots.Max(b => b.Point.Z)));

    var result = Hunter(bots, cube, [(point) => point.X, (point) => point.Y, (point) => point.Z],
      [(point, value) => point with {X=value},(point, value) => point with {Y=value},(point, value) => point with {Z=value}]);

    Console.WriteLine(result);
    result.Item1.Little.ManhattanDistance(Point3.Zero).Should().Be(expected);
  }

  private static (Cube, long) Hunter(List<Bot> bots, Cube cube, List<Func<Point3, long>> getters, List<Func<Point3, long, Point3>> setters)
  {
    var getter = getters[0];
    var setter = setters[0];
    var a = getter(cube.Little);
    var b = getter(cube.Big);
    var mybots = bots.Count(b => b.Overlaps(cube));
    if (mybots == 0) return (cube, 0);
    if (a == b) {
      if (getters.Count == 1) return (cube, mybots);
      return Hunter(bots, cube, getters[1..], setters[1..]);
    }
    var split = (a + b) / 2;
    var left = cube with {Big = setter(cube.Big, split)};
    var right = cube with {Little = setter(cube.Little, split + 1)};
    var huntLeft = Hunter(bots, left, getters, setters);
    var huntRight = Hunter(bots, right, getters, setters);
    if (huntLeft.Item2 > huntRight.Item2) return huntLeft;
    if (huntLeft.Item2 < huntRight.Item2) return huntRight;
    huntLeft.Item1.Little.Should().Be(huntLeft.Item1.Big);
    huntRight.Item1.Little.Should().Be(huntRight.Item1.Big);
    if (huntLeft.Item1.Little.ManhattanDistance(Point3.Zero) < huntRight.Item1.Little.ManhattanDistance(Point3.Zero)) {
      return huntLeft;
    }
    return huntRight;
  }

  public record Point3(long X, long Y, long Z)
  {
    public static Point3 Zero { get; } = new(0, 0, 0);

    public long ManhattanDistance(Point3 other)
    {
      return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }
  }

  public record Cube(Point3 Little, Point3 Big);

  public record Bot(Point3 Point, long Radius)
  {
    public bool Overlaps(Cube cube)
    {
      var remainder = Move(Point.X, cube.Little.X, cube.Big.X, Radius);
      if (remainder == null) return false;
      remainder = Move(Point.Y, cube.Little.Y, cube.Big.Y, (long)remainder);
      if (remainder == null) return false;
      remainder = Move(Point.Z, cube.Little.Z, cube.Big.Z, (long)remainder);
      if (remainder == null) return false;
      return true;
    }

    public static long? Move(long myPosition, long little, long big, long remainder)
    {
      if (little <= myPosition && myPosition <= big) return remainder;
      if (myPosition < little && little - myPosition <= remainder)
      {
        return remainder - (little - myPosition);
      }
      if (big < myPosition && myPosition - big <= remainder)
      {
        return remainder - (myPosition - big);
      }
      return null;
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
