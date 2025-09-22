namespace Core.Parsing
{
    public interface IParser<TOutSerialize>:IParser
    {
        TOutSerialize Serialize<TInSerialize>(TInSerialize payload);
        TInSerialize Deserialize<TInSerialize>(TOutSerialize payload);
        IParser<TOut> Pipe<TOut>(IParser<TOutSerialize, TOut> pipeThrough);
        IParser<TInSerialize, TOutSerialize> Wrap<TInSerialize>();
    }
    public interface IParser<TInSerialize, TOutSerialize>:IParser
    {
        TOutSerialize Serialize(TInSerialize payload);
        TInSerialize Deserialize(TOutSerialize payload);
        IParser<TInSerialize, TOut> Pipe<TOut>(IParser<TOutSerialize, TOut> pipeThrough);
    }
    public interface IParser { 
    
    }
}