namespace Core.Port
{
    public interface IAddRemoveHandler<TMessageToReceive>: IAddHandler<TMessageToReceive>, IRemoveHandler<TMessageToReceive>{
    }
}
