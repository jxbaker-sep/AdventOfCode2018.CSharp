namespace AdventOfCode2018.CSharp.Utils;

/// Represents a class that can do PartitionRefinement 
/// (cf https://en.wikipedia.org/wiki/Partition_refinement)
/// Starting from an initial set (passed into constructor),
/// creates 1 set from those values. Then Partitions can be done in waves:
/// any elements in the partition are split off from each existing subset into their own set.
/// (The partition function creates 1 or more new sets; partition elements may not all end)
/// (up in the same set):
// eg. [1,2,3,4,5] . Partition(1,2,3) . Partition(3,4,5) => [1,2], [3], [4,5]
// You can then use SameSet(x,y) to see if values are in the same set.
// You can also then use GetSet(x) to return the set containing x.
// Or use GetAllSets() to return all sets.
public class PartitionSet<T> where T:notnull
{
  private readonly Dictionary<T, LinkedList<T>> Forest = [];

  public PartitionSet(IEnumerable<T> values) {
    var zeroSet = new LinkedList<T>(values);
    foreach(var value in values) Forest.Add(value, zeroSet);
  }

  public void Partition(IEnumerable<T> values) {
    Dictionary<LinkedList<T>, LinkedList<T>> started = [];
    foreach(var value in values) {
      var original = Forest[value];
      if (!started.TryGetValue(original, out var next)) {
        next = new();
        started[original] = next;
      }
      Forest[value] = next;
      original.Remove(value);
      next.AddFirst(value);
    }
  }

  public bool SameSet(T t1, T t2) 
  {
    return Forest[t1] == Forest[t2];
  }

  public IEnumerable<T> GetSet(T t) => Forest[t];
  public IEnumerable<IEnumerable<T>> GetAllSets() => Forest.Values.Distinct();
}