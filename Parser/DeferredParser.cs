namespace Parser;

public class DeferredParser<T> : Parser<T>
{
    public Parser<T>? Actual {get;set;}
    override public IParseResult<T> Parse(char[] input, int position)
    {
        if (Actual == null) throw new ApplicationException();
        return Actual.Parse(input, position);
    }
}