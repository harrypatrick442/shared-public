namespace Core.Layout
{
    /*
    public interface IGridOrganisable<TPayload>: IGridOrganisable
    {
        TPayload Payload {get;}
    }
    */
    public interface IGridOrganisable {
       CellsDimensions[] PossibleCellsDimensionss{get;}
    }
}