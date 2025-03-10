namespace AdventOfCode2018.CSharp.Utils;

public static class LinkedListExtensions
{
  public static IEnumerable<LinkedListNode<T>> Nodes<T>(this LinkedList<T> self)
  {
    var current = self.First;
    while (current != null)
    {
      yield return current;
      current = current.Next;
    }
  }

  public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> self)
  {
    return new LinkedList<T>(self);
  }
}