namespace Parser;

public class AnyParser : Parser<char>
{
    override public IParseResult<char> Parse(char[] input, int position)
    {
        if (position < input.Length) return ParseResult.From(input[position], input, position+1);
        return new ParseFailure<char>("Expected character; At end of input", input, position);
    }
}
