using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Parser;

using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day20
{
    [Theory]
    [InlineData("^WNE$", 3)]
    [InlineData("^ENWWW(NEEE|SSE(EE|N))$", 10)]
    [InlineData("^ENNWSWW(NEWS|)SSSEEN(WNSE|)EE(SWEN|)NNN$", 18)]
    public void Sanity(string regex, int expected)
    {
        // var data = Convert(AoCLoader.LoadFile(path));
        var data = Convert(regex);

        var points = data.DistancesToPoints(Point.Zero, 0);
        points.GroupBy(it => it.Item1, it => it.Item2).MaxBy(it => it.Min())!.Min()
            .Should().Be(expected);
    }

    [Theory]
    [InlineData("^W(E|)(W|)$")]
    public void ParseTest(string regex)
    {
        Convert(regex);
    }

    [Theory]
    [InlineData("day20", 0)]
    public void Part1(string path, int expected)
    {
        var data = Convert(AoCLoader.LoadFile(path));

        var points = data.DistancesToPoints(Point.Zero, 0);
        points.GroupBy(it => it.Item1, it => it.Item2).MaxBy(it => it.Min())!.Min()
            .Should().Be(expected);
    }

    public record PathItem(List<Vector> Start, PathChoice Children)
    {
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> tailCache = [];
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> distancesCache = [];
        public HashSet<(Point, long)> DistancesToPoints(Point zero, long initial) 
        {
            if (distancesCache.TryGetValue((zero, initial), out var cached)) return cached;
            HashSet<(Point, long)> result = [];
            var current = zero;
            var distance = initial;
            foreach(var item in Start)
            {
                current += item;
                distance += 1;
                result.Add((current, distance));
            }
            foreach(var child in Children.Parts) 
            {
                foreach(var point in child.DistancesToPoints(current, distance))
                {
                    result.Add(point);
                }
            }
            distancesCache[(zero, initial)] = result;
            return result;
        }

        public HashSet<(Point, long)> Tails(Point zero, long initial)
        {
            if (tailCache.TryGetValue((zero, initial), out var cached)) return cached;
            HashSet<(Point, long)> result = [];
            var current = zero;
            var distance = initial;
            foreach(var item in Start)
            {
                current += item;
                distance += 1;
            }
            foreach(var child in Children.Parts) 
            {
                foreach(var point in child.Tails(current, distance))
                {
                    result.Add(point);
                }
            }
            tailCache[(zero, initial)] = result;
            return result;
        }
    }

    public record PathJoin(List<PathItem> Parts)
    {
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> tailCache = [];
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> distancesCache = [];
        public HashSet<(Point, long)> DistancesToPoints(Point zero, long initial) 
        {
            if (distancesCache.TryGetValue((zero, initial), out var cached)) return cached;
            HashSet<(Point, long)> result = [];
            List<(Point, long)> open = [];
            open.Add((zero, initial));
            foreach(var item in Parts)
            {
                List<(Point, long)> nextOpen = [];
                foreach(var (current, distance) in open)
                {
                    foreach(var path in item.DistancesToPoints(current, distance))
                    {
                        result.Add(path);
                    }
                    nextOpen.AddRange(item.Tails(current, distance));
                }
                open = nextOpen;
            }
            distancesCache[(zero, initial)] = result;
            return result;
        }
        public HashSet<(Point, long)> Tails(Point zero, long initial)
        {
            if (tailCache.TryGetValue((zero, initial), out var cached)) return cached;
            List<(Point, long)> open = [];
            open.Add((zero, initial));
            foreach(var item in Parts)
            {
                List<(Point, long)> nextOpen = [];
                foreach(var (current, distance) in open)
                {
                    nextOpen.AddRange(item.Tails(current, distance));
                }
                open = nextOpen;
            }
            return [.. open];
        }
    }

    public record PathChoice(List<PathJoin> Parts)
    {
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> tailCache = [];
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> distancesCache = [];
        public HashSet<(Point, long)> DistancesToPoints(Point zero, long initial) 
        {
            if (distancesCache.TryGetValue((zero, initial), out var cached)) return cached;
            HashSet<(Point, long)> result = [];
            foreach(var item in Parts)
            {
                foreach(var path in item.DistancesToPoints(zero, initial))
                {
                    result.Add(path);
                }
            }
            distancesCache[(zero, initial)] = result;
            return result;
        }
        public HashSet<(Point, long)> Tails(Point zero, long initial)
        {
            if (tailCache.TryGetValue((zero, initial), out var cached)) return cached;
            HashSet<(Point, long)> result = [];
            foreach(var item in Parts)
            {
                foreach(var path in item.Tails(zero, initial))
                {
                    result.Add(path);
                }
            }
            distancesCache[(zero, initial)] = result;
            return result;
        }
    }

    private static PathChoice Convert(string data)
    {
        data = data[1..^1];
        var section = P.Choice("N", "S", "E", "W")
            .Select(it => it switch {
                "N" => Vector.North,
                "E" => Vector.East,
                "S" => Vector.South,
                "W" => Vector.West,
                _ => throw new ApplicationException()
            }).Plus();
        var pc = P.Defer<PathChoice>();
        Parser<PathItem> pwc = P.Sequence(
            section,
            pc.Require().Between(P.String("("), P.String(")").Require()).Optional()
        ).Select(it => new PathItem(it.First, it.Second.Count == 1 ? it.Second[0] : new PathChoice([])));
        var pj = pwc.Plus().Select(it => new PathJoin(it));
        pc.Actual = P.Sequence(
                pj,
                pj.After("|").Star(),
                P.String("|").Optional()
            ).Select(it => {
                List<PathJoin> pps = [it.First, ..it.Second];
                if (it.Third.Count == 1) pps.Add(new PathJoin([]));
                return new PathChoice(pps);
            });
        return pc.Select(it => {
            Console.WriteLine("foo");
            return it;
        }).Before(P.EndOfInput).Parse(data);
    }
}
