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
  public void Part2(string path, int expected)
  {
    var bots = Convert(AoCLoader.LoadFile(path));

    var cube = new Cube(new(bots.Min(b => b.Point.X), bots.Min(b => b.Point.Y), bots.Min(b => b.Point.Z)),
      new(bots.Max(b => b.Point.X), bots.Max(b => b.Point.Y), bots.Max(b => b.Point.Z)));

    var result = Hunter(bots, cube);

    result.Item1.Little.ManhattanDistance(Point3.Zero).Should().Be(expected);
  }

  private static (Cube, long) Hunter(List<Bot> bots, Cube inputCube)
  {
    // Algorithm:
    //  start with a q of the input cube
    //  take the cube from the q that has the largest number of overlapping bot ranges.
    //  if that cube is of size 1, then we've found the target (theoritically there could be closer targets of the same order, but not in this data, haha)
    //  otherwise, add the 8 sub-cubes of this cube to the q
    PriorityQueue<(Cube cube, long order)> q = new(it => -it.order);

    long NumberOfBots(Cube c) => bots.Count(b => b.Overlaps(c));
    q.Enqueue((inputCube, NumberOfBots(inputCube)));

    while (q.TryDequeue(out var current))
    {
      Console.WriteLine($"{current.order}: {current.cube.Order} {current.cube}");
      var a = current.cube.Little.X;
      var b = current.cube.Big.X;
      var c = current.cube.Little.Y;
      var d = current.cube.Big.Y;
      var e = current.cube.Little.Z;
      var f = current.cube.Big.Z;
      if (a == b && c == d && e == f)
      {
        return (current.cube, current.order);
      }
      List<Cube> cubes = [current.cube];
      if (a != b)
      {
        var split = (a + b) / 2;
        cubes = cubes.SelectMany(cube => new[] { cube with { Big = cube.Big with { X = split } }, cube with { Little = cube.Little with { X = split + 1 } } }).ToList();
      }
      if (c != d)
      {
        var split = (c + d) / 2;
        cubes = cubes.SelectMany(cube => new[] { cube with { Big = cube.Big with { Y = split } }, cube with { Little = cube.Little with { Y = split + 1 } } }).ToList();

      }
      if (e != f)
      {
        var split = (e + f) / 2;
        cubes = cubes.SelectMany(cube => new[] { cube with { Big = cube.Big with { Z = split } }, cube with { Little = cube.Little with { Z = split + 1 } } }).ToList();

      }
      cubes.Count.Should().BeGreaterThan(1);
      foreach (var cube in cubes) q.Enqueue((cube, NumberOfBots(cube)));
    }
    throw new ApplicationException();
  }

  public record Point3(long X, long Y, long Z)
  {
    public static Point3 Zero { get; } = new(0, 0, 0);

    public long ManhattanDistance(Point3 other)
    {
      return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }
  }

  public record Cube(Point3 Little, Point3 Big)
  {
    public BigInteger Order => new BigInteger(1 + Big.X - Little.X) * new BigInteger(1 + Big.Y - Little.Y) * new BigInteger(1 + Big.Z - Little.Z);
  }

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
