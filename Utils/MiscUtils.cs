namespace AdventOfCode2018.CSharp.Utils;

public static class MiscUtils
{
  public static IEnumerable<long> InclusiveRange(long start, long stop) {
    for(long i = start; i <= stop; i++) yield return i;
  }

  public static int? BinarySearch(int maxValue, Func<int, bool> action) {
    int? knownMin = null;
    int? knownMax = null;

    while (true) {
      var useMin = knownMin ?? 0;
      var useMax = knownMax ?? maxValue + 1;
      int attempt = (useMin + useMax) / 2;
      if (action(attempt)) {
        if (attempt == 0) return 0;
        if (knownMin == attempt - 1) return attempt;
        knownMax = attempt;
      } else {
        if (attempt == maxValue) return null;
        if (knownMax == attempt + 1) return knownMax;
        knownMin = attempt;
      }
    }
    throw new ApplicationException();
  }
}