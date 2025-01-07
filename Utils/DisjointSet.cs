namespace AdventOfCode2018.CSharp.Utils;

public class DisjointSet
{
  private int Rank { get; set; } = 0;
  private DisjointSet Parent;

  public DisjointSet()
  {
    Parent = this;
  }

  public bool SameUnion(DisjointSet t2)
  {
    return Find() == t2.Find();
  }

  public DisjointSet Find()
  {
    if (Parent == this) return this;
    Parent = Parent.Find();
    return Parent;
  }

  public void Union(DisjointSet y)
  {
    var parentX = Find();
    var parentY = y.Find();

    if (parentX == parentY) return;

    if (parentX.Rank < parentY.Rank)
    {
      (parentY, parentX) = (parentX, parentY);
    }

    parentY.Parent = parentX;
    if (parentX.Rank == parentY.Rank)
    {
      parentX.Rank += 1;
    }
  }
}
