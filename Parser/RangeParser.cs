namespace Parser;

public class RangeParser<T>(Parser<T> other, int min = 0, int max = int.MaxValue) : Parser<List<T>>
{
    override public IParseResult<List<T>> Parse(char[] input, int position)
    {
        var originalPosition = position;
        var output = new List<T>();
        string message = "";
        var stopped = false;
        while (output.Count < max && !stopped)
        {
            switch (other.Parse(input, position))
            {
                case ParseSuccess<T> result:
                    position = result.Position;
                    output.Add(result.Value);
                    break;
                case ParseFailure<T> failure:
                    message = failure.Message;
                    stopped = true;
                    break;
                default:
                    throw new ApplicationException("This shouldn't happen");
            }
        }
        if (output.Count < min) return new ParseFailure<List<T>>($"error in range parser: {output.Count} < {min}/{max}:  {message}", input, originalPosition);
        return ParseResult.From(output, input, position);
    }
}