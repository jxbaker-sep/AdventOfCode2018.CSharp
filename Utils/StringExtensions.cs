namespace AdventOfCode2018.CSharp.Utils;

public static class StringExtensions
{
    public static List<string> Lines(this string s) => s.Split('\n')
        .Select(it => it.Trim()).ToList();

    public static Dictionary<Point, char> Gridify(this List<string> self) =>
        self.SelectMany((line, row) => line.Select((c, col) => (new Point(row, col), c)))
          .ToDictionary(it => it.Item1, it => it.c);


}
