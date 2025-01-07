namespace AdventOfCode2018.CSharp.Utils;

public class PriorityQueue<T>
  {
      private readonly SortedDictionary<long, Queue<T>> Actual = new();
      private readonly Func<T, long> PriorityFunction;
      public int Count { get; private set; }

      public PriorityQueue(Func<T, long> priorityFunction)
      {
          PriorityFunction = priorityFunction;
      }

      public void Enqueue(T item)
      {
          var key = PriorityFunction(item);
          if (Actual.TryGetValue(key, out var list))
          {
              list.Enqueue(item);
          }
          else
          {
              var q = new Queue<T>();
              Actual[key] = q;
              q.Enqueue(item);
          }

          Count++;
      }

      public T Dequeue()
      {
          var (key, q) = Actual.First();
          var result = q.Dequeue();
          if (q.Count == 0)
          {
              Actual.Remove(key);
          }
          Count--;
          return result;
      }

      public bool TryDequeue(out T value)
      {
          if (Count == 0)
          {
              value = default!;
              return false;
          }

          value = Dequeue();
          return true;
      }
  }