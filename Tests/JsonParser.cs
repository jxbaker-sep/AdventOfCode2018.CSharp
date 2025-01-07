namespace AdventOfCode2018.CSharp.Tests;
public class JsonParser
{
  public interface IJsonObject {}
  public record JsonObject(Dictionary<string, IJsonObject> Value);
  public record JsonInteger(long Value);
  public record JsonString(string Value);


  // public Parser<IJsonObject> Parser {
  //   init {
  //     var jsonObjectP = KeyValuePairs.Star(",").Between("{", "}");
  //   }
  // }
}