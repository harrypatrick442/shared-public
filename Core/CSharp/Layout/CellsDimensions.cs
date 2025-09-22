namespace Core.Layout
{
    public class CellsDimensions<TPayload> : CellsDimensions //where TPayload: IGridOrganisable
    {
        private TPayload _GridOrganisable;
        public TPayload GridOrganisable { get { return _GridOrganisable; } }
        public CellsDimensions(int nColumns, int nRows, TPayload gridOrganisable) : base(nColumns, nRows)
        {
            _GridOrganisable = gridOrganisable;
        }
    }
    public class CellsDimensions
    {
        private int _NRows;
        public int NRows { get { return _NRows; } }
        private int _NColumns;
        public int NColumns { get { return _NColumns; } }
        public CellsDimensions(int nColumns, int nRows)
        {
            _NColumns = nColumns;
            _NRows = nRows;
        }
    }
}