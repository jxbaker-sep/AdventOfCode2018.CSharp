namespace AdventOfCode2018.CSharp.Tests;

using FluentAssertions;
using Parser;
using P = Parser.ParserBuiltins;

public class Prolog
{
  public interface IParameter {}
  public record Atom(string Value) : IParameter;
  public record UnboundVariable(string Label) : IParameter;
  public record Rule(string Name, int Arity, List<IParameter> Parameters);

  private static class Parser
  {
    public static Parser<char> Lowercase {get;} = P.Any.Where(char.IsLower, "IsLower");
    public static Parser<char> Uppercase {get;} = P.Any.Where(char.IsUpper, "IsUpper");
    public static Parser<Atom> SimpleAtom {get;} = (Lowercase + (Uppercase | Lowercase).Star()).Join().Select(it => new Atom(it));
    public static Parser<UnboundVariable> Variable {get;} = (Uppercase + (Uppercase | Lowercase).Star()).Join().Select(it => new UnboundVariable(it));

    public static Parser<IParameter> AtomOrVariable {get;} = SimpleAtom.Select(it => (IParameter)it) | Variable.Select(it => (IParameter)it);

    public static Parser<Rule> Query {get;} = P.Sequence(SimpleAtom, AtomOrVariable.Star(",").Between("(", ")"))
      .Select(it => new Rule(it.First.Value, it.Second.Count, it.Second));

    public static Parser<Rule> Rule {get;} = Query.Before(".");
    public static Parser<List<Rule>> Program {get;} = Rule.Star().Before(P.EndOfInput.Trim());
    public static Parser<List<Rule>> Queries {get;} = Query.Star(",")
      .Before(P.String(".").Trim().Optional())
      .Before(P.EndOfInput.Trim());

  }

  [Theory]
  [InlineData("")]
  [InlineData("x().")]
  [InlineData("x(a).")]
  [InlineData("x(Y).")]
  [InlineData("x(Y).\ny(a).\n")]
  public void ProgramTest(string input)
  {
    Parser.Program.Invoking(it => it.Parse(input)).Should().NotThrow();
  }

  [Theory]
  [InlineData("")]
  [InlineData("x().")]
  [InlineData("x()")]
  [InlineData("x(a).")]
  [InlineData("x(Y).")]
  [InlineData("x(Y), y(a).\n")]
  public void QueryTest(string input)
  {
    Parser.Queries.Invoking(it => it.Parse(input)).Should().NotThrow();
  }
}