using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;

namespace AdventOfCode2018.CSharp.Tests;

public class PartitionSetTests
{
  [Fact]
  public void InitiallyAllTheSame()
  {
    List<long> elements = MathUtils.Primes().Take(5).ToList();
    var ps = new PartitionSet<long>(elements);

    foreach (var e1 in elements) 
      foreach (var e2 in elements)
      {
        ps.SameSet(e1, e2).Should().BeTrue();
      }
  }

  [Fact]
  public void BasicPartition()
  {
    List<long> elements = [2,3,5,7,11];
    var ps = new PartitionSet<long>(elements);
    ps.Partition([2, 3]);
    ps.Partition([5, 7]);

    ps.SameSet(2, 3).Should().BeTrue();
    ps.SameSet(2, 5).Should().BeFalse();
    ps.SameSet(2, 7).Should().BeFalse();
    ps.SameSet(2, 11).Should().BeFalse();

    ps.SameSet(3, 5).Should().BeFalse();
    ps.SameSet(3, 7).Should().BeFalse();
    ps.SameSet(3, 11).Should().BeFalse();

    ps.SameSet(5, 7).Should().BeTrue();
    ps.SameSet(5, 11).Should().BeFalse();
    
    ps.SameSet(7, 11).Should().BeFalse();
  }

  [Fact]
  public void DoublePartition()
  {
    List<long> elements = [2,3,5,7,11];
    var ps = new PartitionSet<long>(elements);
    ps.Partition([2, 3, 5]);
    ps.Partition([3, 5, 7]);

    ps.SameSet(2, 3).Should().BeFalse();
    ps.SameSet(2, 5).Should().BeFalse();
    ps.SameSet(2, 7).Should().BeFalse();
    ps.SameSet(2, 11).Should().BeFalse();

    ps.SameSet(3, 5).Should().BeTrue();
    ps.SameSet(3, 7).Should().BeFalse();
    ps.SameSet(3, 11).Should().BeFalse();

    ps.SameSet(5, 7).Should().BeFalse();
    ps.SameSet(5, 11).Should().BeFalse();
    ps.SameSet(7, 11).Should().BeFalse();
  }  

  [Fact]
  public void GetValueTests()
  {
    List<long> elements = [2,3,5,7,11];
    var ps = new PartitionSet<long>(elements);
    ps.Partition([2, 3, 5]);

    ps.GetAllSets().Select(it => it.ToList()).ToList().Should().BeEquivalentTo<List<long>>([[2,3,5], [7, 11]]);

    ps.Partition([3,5]);

    ps.GetAllSets().Select(it => it.ToList()).ToList().Should().BeEquivalentTo<List<long>>([[2], [5,3], [7, 11]]);

  }
}