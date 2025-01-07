using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Parser;
using Utils;
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

        // var points = data.DistancesToPoints(Point.Zero, 0);
        // points.GroupBy(it => it.Item1, it => it.Item2).MaxBy(it => it.Min())!.Min()
            data.Furthest.Should().Be(expected);
    }

    [Theory]
    [InlineData("^W(E)$", 1)]
    [InlineData("^W(E|)$", 2)]
    [InlineData("^W(E|)(W|)$", 4)]
    public void ParseTest(string regex, int expected)
    {
        var data = Convert(regex);
        var points = data.Tails(Point.Zero, 0);
        points.Count.Should().Be(expected);
    }

    [Theory]
    [InlineData("day20", 0)]
    public void Part1(string path, int expected)
    {
        var data = Convert(AoCLoader.LoadFile(path));

        // var points = data.DistancesToPoints(Point.Zero, 0);
        // points.GroupBy(it => it.Item1, it => it.Item2).MaxBy(it => it.Min())!.Min()
            data.Furthest.Should().Be(expected);
    }

    public record PathItem(List<Vector> Start, PathChoice Choices)
    {
        public long Order = Choices.Order == 0 ? 1 : Choices.Order;
        public long Furthest = Start.Count + Choices.Furthest;
        public PathItem Then(PathChoice grandChildren) => new([], new PathChoice([new PathJoin([this, new([], grandChildren)])]));
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
            foreach(var point in Choices.DistancesToPoints(current, distance)) 
            {
                result.Add(point);
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
            if (Choices.Parts.Count == 0) result.Add((current, distance));
            foreach(var point in Choices.Tails(current, distance)) 
            {
                result.Add(point);
            }
            tailCache[(zero, initial)] = result;
            return result;
        }
    }

    public record PathJoin(List<PathItem> Parts)
    {
        public long Furthest = Parts.Sum(it => it.Furthest);
        public long Order = Parts.Select(part => part.Order).Product();
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
            List<(Point, long)> open = [(zero, initial)];
            foreach(var item in Parts)
            {
                List<(Point, long)> nextOpen = [];
                foreach(var (current, distance) in open)
                {
                    nextOpen.AddRange(item.Tails(current, distance));
                }
                open = nextOpen;
            }
            tailCache[(zero, initial)] = [..open];
            return [.. open];
        }
    }

    public record PathChoice(List<PathJoin> Parts)
    {
        public long Furthest => Parts.Count == 0 ? 0 : Parts.Last().Parts.Count == 0 ? 0 : Parts.Max(part => part.Furthest);
        public long Order = Parts.Select(part => part.Order).Sum();
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
            tailCache[(zero, initial)] = result;
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
        var nested = pc.Before(")").Require().After("(");
        Parser<PathItem> pi = 
            P.Sequence(
                section,
                nested.Star()
            ).Select(it => {
                var d = new PathItem(it.First, new([]));
                foreach(var other in it.Second) d = d.Then(other);
                return d;
            });
        var pj = pi.Plus().Select(it => new PathJoin(it));
        pc.Actual = P.Sequence(
                pj,
                pj.After("|").Star(),
                P.String("|").Optional()
            ).Select(it => {
                List<PathJoin> pps = [it.First, ..it.Second];
                if (it.Third.Count == 1) pps.Add(new PathJoin([]));
                return new PathChoice(pps);
            });
        return pc.Before(P.EndOfInput).Parse(data);
    }
}
