

namespace AdventOfCode2018.CSharp.Utils;


public record Vector(long Y, long X) {
  public static Vector North {get;} = new(-1, 0);
  public static Vector East {get;} = new(0, 1);
  public static Vector South {get;} = new(1, 0);
  public static Vector West {get;} = new(0, -1);

  public static Vector NorthEast {get;} = North + East;
  public static Vector SouthEast {get;} = South + East;
  public static Vector NorthWest {get;} = North + West;
  public static Vector SouthWest {get;} = South + West;

  public static IEnumerable<Vector> Cardinals {get;} = [North, East, South, West];
  public static IEnumerable<Vector> InterCardinals {get;} = [NorthEast, SouthEast, SouthWest, NorthWest];
  public static IEnumerable<Vector> CompassRose {get;} = [North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest];

  public static Vector operator+(Vector v1, Vector v2) => new(v1.Y + v2.Y, v1.X + v2.X);
  public static Vector operator*(Vector v1, long v2) => new(v1.Y * v2, v1.X * v2);

  internal Vector RotateRight() => new(X, -Y);
  internal Vector RotateLeft() => new(-X, Y);
}