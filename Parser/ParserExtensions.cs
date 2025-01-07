using Utils;

using P = Parser.ParserBuiltins;

namespace Parser;

public static class ParserExtensions
{
  public static Parser<TOut> Select<TIn, TOut>(this Parser<TIn> parser, Func<TIn, TOut> fct)
  {
    return parser.Then(v => ParseResult.From(fct(v.Value), v.Data, v.Position));
  }

  public static Parser<TOut> Then<TIn, TOut>(this Parser<TIn> parser, Func<ParseSuccess<TIn>, IParseResult<TOut>> action)
  {
    return Parser.From((c, i) => {
      var m = parser.Parse(c, i);
      if (m is ParseSuccess<TIn> v) return action(v);
      return new ParseFailure<TOut>((m as ParseFailure<TIn>)!.Message, c, i);
    });
  }

  public static Parser<TOut> Then<TIn, TOut>(this Parser<TIn> parser, Func<TIn, Parser<TOut>> action)
  {
    return Parser.From((c, i) => {
      var m1 = parser.Parse(c, i);
      if (m1 is ParseSuccess<TIn> v) {
        var other = action(v.Value);
        return other.Parse(c, v.Position);
      }
      return (m1 as ParseFailure<TIn>)!.As<TOut>();
    });
  }

  public static Parser<(T1 First, T2 Second)> Then<T1, T2>(this Parser<T1> parser, Parser<T2> other)
  {
    return parser.Then(v => other.Select(v2 => (v, v2)));
  }

  public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> fct, string annotation = "") {
    return Parser.From<T>((c, i) => {
      var result = parser.Parse(c, i);
      if (result is ParseSuccess<T> r && fct(r.Value)) return r;
      if (result is ParseFailure<T> f) return f;
      return new ParseFailure<T>(annotation, c, i);
    });
  }
  public static RangeParser<T> Optional<T>(this Parser<T> parser) => new(parser, 0, 1);
  public static RangeParser<T> Range<T>(this Parser<T> parser, int min, int max) => new(parser, min, max);
  public static RangeParser<T> Star<T>(this Parser<T> parser) => new(parser);

  public static Parser<List<T>> Plus<T>(this Parser<T> parser, string seperator) => parser + parser.After(seperator).Star();
  

  public static Parser<List<T>> Star<T>(this Parser<T> parser, string seperator) => parser.Plus(seperator).Optional()
    .Select(v => v.Count == 0 ? [] : v[0]);

  public static RangeParser<T> Plus<T>(this Parser<T> parser) => new(parser, 1);
  public static T Parse<T>(this Parser<T> parser, string x) {
    var m = parser.Parse(x.ToCharArray(), 0);
    if (m is ParseSuccess<T> r) return r.Value;
    throw new ApplicationException((m as ParseFailure<T>)!.Message);
  }

  public static List<T> ParseMany<T>(this Parser<T> parser, List<string> x) => x.Select(parser.Parse).ToList();
    
  public static T? ParseOrNull<T>(this Parser<T> parser, string x) where T: class {
    var m = parser.Parse(x.ToCharArray(), 0);
    if (m is ParseSuccess<T> r) return r.Value;
    return null;
  }

  public static T? ParseOrNullStruct<T>(this Parser<T> parser, string x) where T: struct {
    var m = parser.Parse(x.ToCharArray(), 0);
    if (m is ParseSuccess<T> r) return r.Value;
    return null;
  }

  public static Parser<string> Join<T>(this Parser<List<T>> parser) => parser.Select(it => it.Join());

  public static Parser<T> End<T>(this Parser<T> parser) => parser.Before(P.EndOfInput);

  public static Parser<T> Before<T, T2>(this Parser<T> parser, Parser<T2> other)
  {
    return P.Sequence(parser, other).Select(it => it.First);
  }

  public static Parser<T> Before<T>(this Parser<T> parser, string other) => parser.Before(P.String(other).Trim());
  public static Parser<T> After<T>(this Parser<T> parser, string other) => parser.After(P.String(other).Trim());

  public static Parser<T> After<T, T2>(this Parser<T> parser, Parser<T2> other)
  {
    return P.Sequence(other, parser).Select(it => it.Second);
  }

  public static Parser<T> Between<T, T2, T3>(this Parser<T> parser, Parser<T2> other1, Parser<T3> other2)
  {
    return P.Sequence(other1, parser, other2).Select(it => it.Second);
  }

  public static Parser<T> Between<T>(this Parser<T> parser, string other1, string other2)
  {
    return parser.Between(P.String(other1).Trim(), P.String(other2).Trim());
  }

  public static Parser<T> Require<T>(this Parser<T> parser)
  {
    return Parser.From((ca, i) => {
      var r1 = parser.Parse(ca, i);
      if (r1 is ParseSuccess<T> s1) return r1;
      throw new ApplicationException((r1 as ParseFailure<T>)!.Message);
    });
  }

  public static Parser<T> Trim<T>(this Parser<T> parser)
  {
    return parser.Between(P.Whitespace.Star(), P.Whitespace.Star());
  }

  public static Parser<T> Peek<T>(this Parser<T> p1, string p2) => p1.Peek(P.String(p2).Trim());


  public static Parser<T> Peek<T, T2>(this Parser<T> p1, Parser<T2> p2)
  {
    return p1.Then<T,T>(v => {
        var peeked = p2.Parse(v.Data, v.Position);
        if (peeked is ParseSuccess<T2>) return v;
        return new ParseFailure<T>((peeked as ParseFailure<T2>)!.Annotation, v.Data, v.Position);
      });
  }

  public static Parser<T> PeekNot<T>(this Parser<T> p1, string p2) => p1.PeekNot(P.String(p2));


  public static Parser<T> PeekNot<T, T2>(this Parser<T> p1, Parser<T2> p2)
  {
    return p1.Then<T, T>(v => {
        var peeked = p2.Parse(v.Data, v.Position);
        if (peeked is ParseFailure<T2>) return v;
        return new ParseFailure<T>("Negative peek failed", v.Data, v.Position);
      });
  }

  public static Parser<P.Void> Void<T>(this Parser<T> p) => p.Select(_ => new P.Void());

  public static Parser<P.Void> Not<T>(this Parser<T> p) => new EZParser<P.Void>((c, i) => {
    var v = p.Parse(c, i);
    if (v is ParseSuccess<T>) return new ParseFailure<P.Void>("'not' failed (think of a better error)", c, i);
    return ParseResult.From(new P.Void(), c, i);
  });

  public static Parser<(List<TAccum> Accumulator, TSentinel Sentinel)> Until<TAccum, TSentinel>(this Parser<TAccum> p, Parser<TSentinel> sentinel) => Parser.From((c, i) => {
    List<TAccum> list = [];
    while (true) {
      var v = sentinel.Parse(c, i);
      if (v is ParseSuccess<TSentinel> s) return ParseResult.From((list, s.Value), c, s.Position);
      var v2 = p.Parse(c, i);
      if (v2 is ParseSuccess<TAccum> s2) {
        list.Add(s2.Value);
        i = s2.Position;
        continue;
      }
      return ((ParseFailure<TAccum>)v2).As<(List<TAccum> First, TSentinel Second)>();
    }
  });
}
