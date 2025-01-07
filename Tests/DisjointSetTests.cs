using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;

public class DisjointSetTests
{
  [Fact]
  public void ParentTests()
  {
    var ds1 = new DisjointSet();
    var ds2 = new DisjointSet();
    var ds3 = new DisjointSet();
    ds1.Find().Should().Be(ds1);
    ds1.Find().Should().NotBe(ds2);
    ds2.Find().Should().Be(ds2);
    ds2.Find().Should().Be(ds2);
    ds1.SameUnion(ds2).Should().BeFalse();

    ds1.Union(ds2);
    ds1.SameUnion(ds2).Should().BeTrue();

    ds1.Union(ds3);
    ds1.SameUnion(ds3).Should().BeTrue();
    ds1.SameUnion(ds2).Should().BeTrue();

  }
}