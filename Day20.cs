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
    public void Sanity1(string regex, int expected)
    {
        var data = Convert(regex);
        data.Furthest.Should().Be(expected);
        data.Ns().Keys.Max().Should().Be(expected);

        // var points = data.DistancesToPoints(Point.Zero, 0);
        // points.GroupBy(it => it.Item1, it => it.Item2).MaxBy(it => it.Min())!.Min()
        //     .Should().Be(expected);
    }

    [Theory]
    [InlineData("^NEEE$", 4, 1)]
    [InlineData("^NN(NN)$", 4, 1)]
    [InlineData("^NN(NN|)$", 4, 1)]
    [InlineData("^NN(NN|WW)$", 4, 2)]
    [InlineData("^NN(NN|WW|)$", 4, 2)]
    [InlineData("^NN(NN|WW|)$", 2, 3)]
    [InlineData("^NN(NN|)(EE|)$", 2, 4)]
    [InlineData("^NN(NN|)E(EE|)$", 2, 4)]
    [InlineData("^NN(NN|)|EE(EE|)$", 2, 4)]
    [InlineData("^N(EE|)|E(EEE|)$", 3, 2)]
    [InlineData("^N(N|)(N|)$", 2, 3)]
    [InlineData("^N(NN|EE|WW)(NN|EE|SS|WW)$", 4, 12)]
    [InlineData("^N(NN|EE|WW)(NN|EE|SS|WW|)$", 4, 12)]
    [InlineData("^N(NN|EE|WW|)(NN|EE|SS|WW|)$", 4, 12)]
    [InlineData("^N(NN|EE|WW|)(NN|EE|SS|WW|)$", 2, 19)]

    public void Sanity2(string regex, int length, int expected)
    {
        var data = Convert(regex);

        var ns = data.Ns();
        ns.Where(kv => kv.Key >= length).Sum(kv => kv.Value).Should().Be(expected);
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
    [InlineData("day20", 3810L)]
    public void Part1(string path, int expected)
    {
        var data = Convert(AoCLoader.LoadFile(path));

        // data.Furthest.Should().Be(expected);
        data.Ns().Keys.Max().Should().Be(expected);
    }
                             // 
    [Theory]                 // 1784129105408455694
    [InlineData("day20", 0)] // 1_784_129_105_408_455_694 too high
    public void Part2(string path, int expected)
    {
        var data = Convert(AoCLoader.LoadFile(path));
        var x = data.Ns();
        data.Ns().Where(it => it.Key >= 1000).Sum(it => it.Value)
            .Should().Be(expected);
    }

    public record PathItem(List<Vector> Start, PathChoice Choices)
    {
        public Dictionary<long, long> Ns() // length of path to count of paths of that length
        {
            var mine = new Dictionary<long, long>(); // Enumerable.Range(1, Start.Count).ToDictionary(it =>(long) it, it => 1L);
            var others = Choices.Ns();
            if (Start.Count == 0)
            {
                return others;
            }
            if (Choices.Parts.Count == 0)
            {
                return new Dictionary<long, long>() {{Start.Count,1}};
            }
            foreach(var (key, value) in others) mine[key + Start.Count] = value;
            return mine;
        }

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

    public record PathJoin(IReadOnlyList<PathItem> Parts)
    {
        public Dictionary<long, long> Ns()
        {
            return NRs(Parts);
        }

        private static Dictionary<long, long> NRs(IReadOnlyList<PathItem> pathItems)
        {
            if (pathItems.Count == 0) return new Dictionary<long, long>{{0,1}};
            var mine = pathItems[0].Ns();
            if (pathItems.Count == 1) return mine;
            var other = NRs(pathItems.ToList()[1..]);
            Dictionary<long, long> result = [];
            // eg: (AB|CD|QP)(EF|GH|XE|WQ)
            foreach(var (k1, v1) in mine)
            {
                foreach(var (k2,v2) in other)
                {
                    result[k1 + k2] = result.GetValueOrDefault(k1 + k2) + v1 + v2;
                }
            }
            return result;
        }

        public long Furthest = Parts.Sum(it => it.Furthest);
        public long Order = Parts.Select(part => part.Order).Product();
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> tailCache = [];
        private readonly Dictionary<(Point, long), HashSet<(Point, long)>> distancesCache = [];
        public HashSet<(Point, long)> DistancesToPoints(Point zero, long initial) 
        {
            if (distancesCache.TryGetValue((zero, initial), out var cached)) return cached;
            HashSet<(Point, long)> result = [];
            List<(Point, long)> open = [(zero, initial)];
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
        public Dictionary<long, long> Ns()
        {
            Dictionary<long, long> mine = [];
            if (Parts.Count == 0) return [];
            if (Parts.Last().Parts.Count == 0) return [];
            foreach(var part in Parts) {
                foreach(var (key, value) in part.Ns()) mine[key] = mine.GetValueOrDefault(key) + value;
            }
            return mine;
        }
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
